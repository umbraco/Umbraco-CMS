using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;

namespace Umbraco.Core.Persistence
{

    /// <summary>
    /// Helper class for working with databases and schemas.
    /// </summary>
    public class DatabaseSchemaHelper
    {
        private readonly Database _db;
        private readonly ILogger _logger;
        private readonly ISqlSyntaxProvider _syntaxProvider;
        private readonly BaseDataCreation _baseDataCreation;

        /// <summary>
        /// Intializes a new helper instance.
        /// </summary>
        /// <param name="db">The database to be used.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="syntaxProvider">The syntax provider.</param>
        /// <example>
        /// A new instance could be initialized like:
        /// <code>
        /// var schemaHelper = new DatabaseSchemaHelper(
        ///     ApplicationContext.Current.DatabaseContext.Database,
        ///     ApplicationContext.Current.ProfilingLogger.Logger,
        ///     ApplicationContext.Current.DatabaseContext.SqlSyntax
        /// );
        /// </code>
        /// </example>
        public DatabaseSchemaHelper(Database db, ILogger logger, ISqlSyntaxProvider syntaxProvider)
        {
            _db = db;
            _logger = logger;
            _syntaxProvider = syntaxProvider;
            _baseDataCreation = new BaseDataCreation(db, logger);
        }

        /// <summary>
        /// Returns whether a table with the specified <paramref name="tableName"/> exists in the database.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns><c>true</c> if the table exists; otherwise <c>false</c>.</returns>
        /// <example>
        /// <code>
        /// if (schemaHelper.TableExist("MyTable"))
        /// {
        ///     // do something when the table exists
        /// }
        /// </code>
        /// </example>
        public bool TableExist(string tableName)
        {
            return _syntaxProvider.DoesTableExist(_db, tableName);
        }

        /// <summary>
        /// Returns whether the table for the specified <typeparamref name="T"/> exists in the database.
        /// 
        /// If <typeparamref name="T"/> has been decorated with an <see cref="TableNameAttribute"/>, the name from that
        /// attribute will be used for the table name. If the attribute is not present, the name
        /// <typeparamref name="T"/> will be used instead.
        /// </summary>
        /// <typeparam name="T">The type representing the DTO/table.</typeparam>
        /// <returns><c>true</c> if the table exists; otherwise <c>false</c>.</returns>
        /// <example>
        /// <code>
        /// if (schemaHelper.TableExist&lt;MyDto&gt;)
        /// {
        ///     // do something when the table exists
        /// }
        /// </code>
        /// </example>
        public bool TableExist<T>()
        {
            var poco = Database.PocoData.ForType(typeof(T));
            var tableName = poco.TableInfo.TableName;
            return TableExist(tableName);
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

            var creation = new DatabaseSchemaCreation(_db, _logger, _syntaxProvider);
            creation.InitializeDatabaseSchema();

            _logger.Info<Database>("Finalized database schema creation");
        }

        /// Creates a new table in the database based on the type of <typeparamref name="T"/>.
        /// 
        /// If <typeparamref name="T"/> has been decorated with an <see cref="TableNameAttribute"/>, the name from that
        /// attribute will be used for the table name. If the attribute is not present, the name
        /// <typeparamref name="T"/> will be used instead.
        /// 
        /// If a table with the same name already exists, the <paramref name="overwrite"/> parameter will determine
        /// whether the table is overwritten. If <c>true</c>, the table will be overwritten, whereas this method will
        /// not do anything if the parameter is <c>false</c>.
        /// <typeparam name="T">The type representing the DTO/table.</typeparam>
        /// <param name="overwrite">Whether the table should be overwritten if it already exists.</param>
        public void CreateTable<T>(bool overwrite)
           where T : new()
        {
            var tableType = typeof(T);
            CreateTable(overwrite, tableType);
        }

        /// <summary>
        /// Creates a new table in the database based on the type of <typeparamref name="T"/>.
        /// 
        /// If <typeparamref name="T"/> has been decorated with an <see cref="TableNameAttribute"/>, the name from that
        /// attribute will be used for the table name. If the attribute is not present, the name
        /// <typeparamref name="T"/> will be used instead.
        /// 
        /// If a table with the same name already exists, this method will not do anything.
        /// </summary>
        /// <typeparam name="T">The type representing the DTO/table.</typeparam>
        public void CreateTable<T>()
          where T : new()
        {
            var tableType = typeof(T);           
            CreateTable(false, tableType);
        }

        /// <summary>
        /// Creates a new table in the database for the specified <paramref name="modelType"/>.
        /// 
        /// If <paramref name="modelType"/> has been decorated with an <see cref="TableNameAttribute"/>, the name from
        /// that  attribute will be used for the table name. If the attribute is not present, the name
        /// <paramref name="modelType"/> will be used instead.
        /// 
        /// If a table with the same name already exists, the <paramref name="overwrite"/> parameter will determine
        /// whether the table is overwritten. If <c>true</c>, the table will be overwritten, whereas this method will
        /// not do anything if the parameter is <c>false</c>.
        /// </summary>
        /// <param name="overwrite">Whether the table should be overwritten if it already exists.</param>
        /// <param name="modelType">The the representing the table.</param>
        public void CreateTable(bool overwrite, Type modelType)
        {
            var tableDefinition = DefinitionFactory.GetTableDefinition(_syntaxProvider, modelType);
            var tableName = tableDefinition.Name;

            string createSql = _syntaxProvider.Format(tableDefinition);
            string createPrimaryKeySql = _syntaxProvider.FormatPrimaryKey(tableDefinition);
            var foreignSql = _syntaxProvider.Format(tableDefinition.ForeignKeys);
            var indexSql = _syntaxProvider.Format(tableDefinition.Indexes);

            var tableExist = TableExist(tableName);
            if (overwrite && tableExist)
            {
                _logger.Info<Database>(string.Format("Table '{0}' already exists, but will be recreated", tableName));

                DropTable(tableName);
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

                    //Loop through index statements and execute sql
                    foreach (var sql in indexSql)
                    {
                        int createdIndex = _db.Execute(new Sql(sql));
                        _logger.Info<Database>(string.Format("Create Index sql {0}:\n {1}", createdIndex, sql));
                    }

                    //Loop through foreignkey statements and execute sql
                    foreach (var sql in foreignSql)
                    {
                        int createdFk = _db.Execute(new Sql(sql));
                        _logger.Info<Database>(string.Format("Create Foreign Key sql {0}:\n {1}", createdFk, sql));
                    }

                    transaction.Complete();
                    if (overwrite)
                    {
                        _logger.Info<Database>(string.Format("Table '{0}' was recreated", tableName));
                    }
                    else
                    {
                        _logger.Info<Database>(string.Format("New table '{0}' was created", tableName));
                    }
                }
            }
            else
            {
                // The table exists and was not recreated/overwritten.
                _logger.Info<Database>(string.Format("Table '{0}' already exists - no changes were made", tableName));
            }
        }

        /// <summary>
        /// Drops the table for the specified <typeparamref name="T"/>.
        /// 
        /// If <typeparamref name="T"/> has been decorated with an <see cref="TableNameAttribute"/>, the name from that
        /// attribute will be used for the table name. If the attribute is not present, the name
        /// <typeparamref name="T"/> will be used instead.
        /// </summary>
        /// <typeparam name="T">The type representing the DTO/table.</typeparam>
        /// <example>
        /// <code>
        /// schemaHelper.DropTable&lt;MyDto&gt;);
        /// </code>
        /// </example>
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

        /// <summary>
        /// Drops the table with the specified <paramref name="tableName"/>.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <example>
        /// <code>
        /// schemaHelper.DropTable("MyTable");
        /// </code>
        /// </example>
        public void DropTable(string tableName)
        {
            var sql = new Sql(string.Format(
                _syntaxProvider.DropTable,
                _syntaxProvider.GetQuotedTableName(tableName)));
            _db.Execute(sql);
        }
    }
}
