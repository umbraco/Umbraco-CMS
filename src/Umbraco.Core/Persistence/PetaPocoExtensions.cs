using System;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.SqlSyntax.ModelDefinitions;

namespace Umbraco.Core.Persistence
{
    public static class PetaPocoExtensions
    {
        internal delegate void CreateTableEventHandler(string tableName, Database db, TableCreationEventArgs e);

        internal static event CreateTableEventHandler NewTable;

        public static void CreateTable<T>(this Database db)
           where T : new()
        {
            var tableType = typeof(T);
            CreateTable(db, false, tableType);
        }

        public static void CreateTable<T>(this Database db, bool overwrite)
           where T : new()
        {
            var tableType = typeof(T);
            CreateTable(db, overwrite, tableType);
        }

        public static void CreateTable(this Database db, bool overwrite, Type modelType)
        {
            //TODO The line below should be refactored to use 'Umbraco.Core.Persistence.DatabaseModelDefinitions.DefinitionFactory.GetTableDefinition(modelType)'
            //But first the sql syntax provider should be updated/refactored to format sql statements using the 'new' definitions from the DatabaseModelDefinitions-namespace.
            var tableDefinition = DefinitionFactory.GetTableDefinition(modelType);
            var tableName = tableDefinition.TableName;

            string createSql = SyntaxConfig.SqlSyntaxProvider.ToCreateTableStatement(tableDefinition);
            string createPrimaryKeySql = SyntaxConfig.SqlSyntaxProvider.ToCreatePrimaryKeyStatement(tableDefinition);
            var foreignSql = SyntaxConfig.SqlSyntaxProvider.ToCreateForeignKeyStatements(tableDefinition);
            var indexSql = SyntaxConfig.SqlSyntaxProvider.ToCreateIndexStatements(tableDefinition);

            var tableExist = db.TableExist(tableName);
            if (overwrite && tableExist)
            {
                db.DropTable(tableName);
            }

            if (!tableExist)
            {
                using (var transaction = db.GetTransaction())
                {
                    //Execute the Create Table sql
                    int created = db.Execute(new Sql(createSql));

                    //If any statements exists for the primary key execute them here
                    if (!string.IsNullOrEmpty(createPrimaryKeySql))
                        db.Execute(new Sql(createPrimaryKeySql));

                    //Fires the NewTable event, which is used internally to insert base data before adding constrants to the schema
                    if (NewTable != null)
                    {
                        var e = new TableCreationEventArgs();

                        //Turn on identity insert if db provider is not mysql
						if (ApplicationContext.Current.DatabaseContext.ProviderName.Contains("MySql") == false && tableDefinition.IsIdentity)
                            db.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(tableName))));
                        
                        //Call the NewTable-event to trigger the insert of base/default data
                        NewTable(tableName, db, e);

                        //Turn off identity insert if db provider is not mysql
						if (ApplicationContext.Current.DatabaseContext.ProviderName.Contains("MySql") == false && tableDefinition.IsIdentity)
                            db.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF;", SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(tableName))));
                    }

                    //Loop through foreignkey statements and execute sql
                    foreach (var sql in foreignSql)
                    {
                        int createdFk = db.Execute(new Sql(sql));
                    }

                    //Loop through index statements and execute sql
                    foreach (var sql in indexSql)
                    {
                        int createdIndex = db.Execute(new Sql(sql));
                    }

                    //Specific to Sql Ce - look for changes to Identity Seed
					if (ApplicationContext.Current.DatabaseContext.ProviderName.Contains("SqlServerCe"))
                    {
                        var seedSql = SyntaxConfig.SqlSyntaxProvider.ToAlterIdentitySeedStatements(tableDefinition);
                        foreach (var sql in seedSql)
                        {
                            int createdSeed = db.Execute(new Sql(sql));
                        }
                    }

                    transaction.Complete();
                }
            }

            LogHelper.Info<Database>(string.Format("New table '{0}' was created", tableName));
        }

        public static void DropTable<T>(this Database db)
            where T : new()
        {
            Type type = typeof (T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            if (tableNameAttribute == null)
                throw new Exception(
                    string.Format(
                        "The Type '{0}' does not contain a TableNameAttribute, which is used to find the name of the table to drop. The operation could not be completed.",
                        type.Name));

            string tableName = tableNameAttribute.Value;
            DropTable(db, tableName);
        }

        public static void DropTable(this Database db, string tableName)
        {
            var sql = new Sql(string.Format("DROP TABLE {0}", SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(tableName)));
            db.Execute(sql);
        }

        public static bool TableExist(this Database db, string tableName)
        {
            return SyntaxConfig.SqlSyntaxProvider.DoesTableExist(db, tableName);
        }

        public static bool TableExist(this UmbracoDatabase db, string tableName)
        {
            return SyntaxConfig.SqlSyntaxProvider.DoesTableExist(db, tableName);
        }

        public static void CreateDatabaseSchema(this Database db)
        {
            NewTable += PetaPocoExtensions_NewTable;

            LogHelper.Info<Database>("Initializing database schema creation");

            var creation = new DatabaseSchemaCreation(db);
            creation.InitializeDatabaseSchema();

            NewTable -= PetaPocoExtensions_NewTable;
        }

        private static void PetaPocoExtensions_NewTable(string tableName, Database db, TableCreationEventArgs e)
        {
            var baseDataCreation = new BaseDataCreation(db);
            baseDataCreation.InitializeBaseData(tableName);
        }
    }

    internal class TableCreationEventArgs : System.ComponentModel.CancelEventArgs{}
}