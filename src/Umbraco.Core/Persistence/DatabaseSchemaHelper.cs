using System;
using System.Linq;
using NPoco;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;

namespace Umbraco.Core.Persistence
{
    public class DatabaseSchemaHelper
    {
        private readonly UmbracoDatabase _database;
        private readonly ILogger _logger;
        private readonly BaseDataCreation _baseDataCreation;

        public DatabaseSchemaHelper(UmbracoDatabase database, ILogger logger)
        {
            _database = database;
            _logger = logger;
            _baseDataCreation = new BaseDataCreation(database, logger);
        }

        private ISqlSyntaxProvider SqlSyntax => _database.SqlSyntax;

        public bool TableExist(string tableName)
        {
            return SqlSyntax.DoesTableExist(_database, tableName);
        }

        internal void UninstallDatabaseSchema()
        {
            // fixme
            // weird to create a DatabaseSchemaCreation here, since it creates
            // a circular dependency with DatabaseSchemaHelper?
            var creation = new DatabaseSchemaCreation(_database, _logger);
            creation.UninstallDatabaseSchema();
        }

        /// <summary>
        /// Creates the Umbraco db schema in the Database of the current Database.
        /// Safe method that is only able to create the schema in non-configured
        /// umbraco instances.
        /// </summary>
        public void CreateDatabaseSchema(ApplicationContext applicationContext)
        {
            if (applicationContext == null) throw new ArgumentNullException(nameof(applicationContext));
            CreateDatabaseSchema(true, applicationContext);
        }

        /// <summary>
        /// Creates the Umbraco db schema in the Database of the current Database
        /// with the option to guard the db from having the schema created
        /// multiple times.
        /// </summary>
        /// <param name="guardConfiguration"></param>
        /// <param name="applicationContext"></param>
        public void CreateDatabaseSchema(bool guardConfiguration, ApplicationContext applicationContext)
        {
            if (applicationContext == null) throw new ArgumentNullException(nameof(applicationContext));

            if (guardConfiguration && applicationContext.IsConfigured)
                throw new Exception("Umbraco is already configured!");

            CreateDatabaseSchemaDo(applicationContext.Services.MigrationEntryService);
        }

        internal void CreateDatabaseSchemaDo(bool guardConfiguration, ApplicationContext applicationContext)
        {
            if (guardConfiguration && applicationContext.IsConfigured)
                throw new Exception("Umbraco is already configured!");

            CreateDatabaseSchemaDo(applicationContext.Services.MigrationEntryService);
        }

        internal void CreateDatabaseSchemaDo(IMigrationEntryService migrationEntryService)
        {
            _logger.Info<Database>("Initializing database schema creation");

            // fixme
            // weird to create a DatabaseSchemaCreation here, since it creates
            // a circular dependency with DatabaseSchemaHelper?
            var creation = new DatabaseSchemaCreation(_database, _logger);
            creation.InitializeDatabaseSchema();

            _logger.Info<Database>("Finalized database schema creation");
        }

        public void CreateTable<T>(bool overwrite)
           where T : new()
        {
            var tableType = typeof (T);
            CreateTable(overwrite, tableType);
        }

        public void CreateTable<T>()
          where T : new()
        {
            var tableType = typeof (T);
            CreateTable(false, tableType);
        }

        public void CreateTable(bool overwrite, Type modelType)
        {
            var tableDefinition = DefinitionFactory.GetTableDefinition(modelType, SqlSyntax);
            var tableName = tableDefinition.Name;

            var createSql = SqlSyntax.Format(tableDefinition);
            var createPrimaryKeySql = SqlSyntax.FormatPrimaryKey(tableDefinition);
            var foreignSql = SqlSyntax.Format(tableDefinition.ForeignKeys);
            var indexSql = SqlSyntax.Format(tableDefinition.Indexes);

            var tableExist = TableExist(tableName);
            if (overwrite && tableExist)
            {
                DropTable(tableName);
                tableExist = false;
            }

            if (tableExist == false)
            {
                using (var transaction = _database.GetTransaction())
                {
                    //Execute the Create Table sql
                    var created = _database.Execute(new Sql(createSql));
                    _logger.Info<Database>($"Create Table sql {created}:\n {createSql}");

                    //If any statements exists for the primary key execute them here
                    if (string.IsNullOrEmpty(createPrimaryKeySql) == false)
                    {
                        var createdPk = _database.Execute(new Sql(createPrimaryKeySql));
                        _logger.Info<Database>($"Primary Key sql {createdPk}:\n {createPrimaryKeySql}");
                    }

                    //Turn on identity insert if db provider is not mysql
                    if (SqlSyntax.SupportsIdentityInsert() && tableDefinition.Columns.Any(x => x.IsIdentity))
                        _database.Execute(new Sql($"SET IDENTITY_INSERT {SqlSyntax.GetQuotedTableName(tableName)} ON "));

                    //Call the NewTable-event to trigger the insert of base/default data
                    //OnNewTable(tableName, _db, e, _logger);

                    _baseDataCreation.InitializeBaseData(tableName);

                    //Turn off identity insert if db provider is not mysql
                    if (SqlSyntax.SupportsIdentityInsert() && tableDefinition.Columns.Any(x => x.IsIdentity))
                        _database.Execute(new Sql($"SET IDENTITY_INSERT {SqlSyntax.GetQuotedTableName(tableName)} OFF;"));

                    //Special case for MySql
                    if (SqlSyntax is MySqlSyntaxProvider && tableName.Equals("umbracoUser"))
                    {
                        _database.Update<UserDto>("SET id = @IdAfter WHERE id = @IdBefore AND userLogin = @Login", new { IdAfter = 0, IdBefore = 1, Login = "admin" });
                    }

                    //Loop through index statements and execute sql
                    foreach (var sql in indexSql)
                    {
                        var createdIndex = _database.Execute(new Sql(sql));
                        _logger.Info<Database>($"Create Index sql {createdIndex}:\n {sql}");
                    }

                    //Loop through foreignkey statements and execute sql
                    foreach (var sql in foreignSql)
                    {
                        var createdFk = _database.Execute(new Sql(sql));
                        _logger.Info<Database>($"Create Foreign Key sql {createdFk}:\n {sql}");
                    }

                    transaction.Complete();
                }
            }

            _logger.Info<Database>($"New table '{tableName}' was created");
        }

        public void DropTable<T>()
            where T : new()
        {
            var type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            if (tableNameAttribute == null)
                throw new Exception($"The Type '{type.Name}' does not contain a TableNameAttribute, which is used"
                    + " to find the name of the table to drop. The operation could not be completed.");

            var tableName = tableNameAttribute.Value;
            DropTable(tableName);
        }

        public void DropTable(string tableName)
        {
            var sql = new Sql(string.Format(
                SqlSyntax.DropTable,
                SqlSyntax.GetQuotedTableName(tableName)));
            _database.Execute(sql);
        }
    }
}