using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{

    public class DatabaseSchemaHelper
    {
        private readonly Database _db;
        private readonly ILogger _logger;
        private readonly ISqlSyntaxProvider _syntaxProvider;
        private readonly BaseDataCreation _baseDataCreation;

        public DatabaseSchemaHelper(Database db, ILogger logger, ISqlSyntaxProvider syntaxProvider)
        {
            _db = db;
            _logger = logger;
            _syntaxProvider = syntaxProvider;
            _baseDataCreation = new BaseDataCreation(db, logger);
        }

        public bool TableExist(string tableName)
        {
            return _syntaxProvider.DoesTableExist(_db, tableName);
        }

        internal void UninstallDatabaseSchema()
        {
            var creation = new DatabaseSchemaCreation(_db, _logger, _syntaxProvider);
            creation.UninstallDatabaseSchema();
        }

        /// <summary>
        /// Creates the Umbraco db schema in the Database of the current Database.
        /// Safe method that is only able to create the schema in non-configured
        /// umbraco instances.
        /// </summary>
        public void CreateDatabaseSchema(ApplicationContext applicationContext)
        {
            if (applicationContext == null) throw new ArgumentNullException("applicationContext");
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
            if (applicationContext == null) throw new ArgumentNullException("applicationContext");

            if (guardConfiguration && applicationContext.IsConfigured)
                throw new Exception("Umbraco is already configured!");

            CreateDatabaseSchemaDo();
        }

        internal void CreateDatabaseSchemaDo(bool guardConfiguration, ApplicationContext applicationContext)
        {
            if (guardConfiguration && applicationContext.IsConfigured)
                throw new Exception("Umbraco is already configured!");

            CreateDatabaseSchemaDo();
        }

        internal void CreateDatabaseSchemaDo()
        {
            _logger.Info<Database>("Initializing database schema creation");

            var creation = new DatabaseSchemaCreation(_db, _logger, _syntaxProvider);
            creation.InitializeDatabaseSchema();

            _logger.Info<Database>("Finalized database schema creation");
        }

        public void CreateTable<T>(bool overwrite)
           where T : new()
        {
            var tableType = typeof(T);
            CreateTable(overwrite, tableType);
        }

        public void CreateTable<T>()
          where T : new()
        {
            var tableType = typeof(T);           
            CreateTable(false, tableType);
        }

        public void CreateTable(bool overwrite, Type modelType)
        {
            var tableDefinition = DefinitionFactory.GetTableDefinition(modelType);
            var tableName = tableDefinition.Name;

            string createSql = _syntaxProvider.Format(tableDefinition);
            string createPrimaryKeySql = _syntaxProvider.FormatPrimaryKey(tableDefinition);
            var foreignSql = _syntaxProvider.Format(tableDefinition.ForeignKeys);
            var indexSql = _syntaxProvider.Format(tableDefinition.Indexes);

            var tableExist = _db.TableExist(tableName);
            if (overwrite && tableExist)
            {
                _db.DropTable(tableName);
                tableExist = false;
            }

            if (tableExist == false)
            {
                using (var transaction = _db.GetTransaction())
                {
                    //Execute the Create Table sql
                    int created = _db.Execute(new Sql(createSql));
                    _logger.Info<Database>(string.Format("Create Table sql {0}:\n {1}", created, createSql));

                    //If any statements exists for the primary key execute them here
                    if (!string.IsNullOrEmpty(createPrimaryKeySql))
                    {
                        int createdPk = _db.Execute(new Sql(createPrimaryKeySql));
                        _logger.Info<Database>(string.Format("Primary Key sql {0}:\n {1}", createdPk, createPrimaryKeySql));
                    }

                    //Turn on identity insert if db provider is not mysql
                    if (_syntaxProvider.SupportsIdentityInsert() && tableDefinition.Columns.Any(x => x.IsIdentity))
                        _db.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", _syntaxProvider.GetQuotedTableName(tableName))));

                    //Call the NewTable-event to trigger the insert of base/default data
                    //OnNewTable(tableName, _db, e, _logger);

                    _baseDataCreation.InitializeBaseData(tableName);

                    //Turn off identity insert if db provider is not mysql
                    if (_syntaxProvider.SupportsIdentityInsert() && tableDefinition.Columns.Any(x => x.IsIdentity))
                        _db.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF;", _syntaxProvider.GetQuotedTableName(tableName))));

                    //Special case for MySql
                    if (_syntaxProvider is MySqlSyntaxProvider && tableName.Equals("umbracoUser"))
                    {
                        _db.Update<UserDto>("SET id = @IdAfter WHERE id = @IdBefore AND userLogin = @Login", new { IdAfter = 0, IdBefore = 1, Login = "admin" });
                    }

                    //Loop through foreignkey statements and execute sql
                    foreach (var sql in foreignSql)
                    {
                        int createdFk = _db.Execute(new Sql(sql));
                        _logger.Info<Database>(string.Format("Create Foreign Key sql {0}:\n {1}", createdFk, sql));
                    }

                    //Loop through index statements and execute sql
                    foreach (var sql in indexSql)
                    {
                        int createdIndex = _db.Execute(new Sql(sql));
                        _logger.Info<Database>(string.Format("Create Index sql {0}:\n {1}", createdIndex, sql));
                    }

                    transaction.Complete();
                }
            }

            _logger.Info<Database>(string.Format("New table '{0}' was created", tableName));
        }

        public void DropTable<T>()
            where T : new()
        {
            Type type = typeof(T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            if (tableNameAttribute == null)
                throw new Exception(
                    string.Format(
                        "The Type '{0}' does not contain a TableNameAttribute, which is used to find the name of the table to drop. The operation could not be completed.",
                        type.Name));

            string tableName = tableNameAttribute.Value;
            DropTable(tableName);
        }

        public void DropTable(string tableName)
        {
            var sql = new Sql(string.Format(
                _syntaxProvider.DropTable,
                _syntaxProvider.GetQuotedTableName(tableName)));
            _db.Execute(sql);
        }
    }
}