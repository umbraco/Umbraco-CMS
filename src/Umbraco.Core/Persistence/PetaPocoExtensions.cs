using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.SqlSyntax;

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

        public static void BulkInsertRecords<T>(this Database db, IEnumerable<T> collection)
        {
            using (var tr = db.GetTransaction())
            {
                try
                {
                    if (SqlSyntaxContext.SqlSyntaxProvider is SqlCeSyntaxProvider)
                    {
                        //SqlCe doesn't support bulk insert statements!

                        foreach (var poco in collection)
                        {
                            db.Insert(poco);
                        }

                    }
                    else
                    {
                        string sql;
                        using (var cmd = db.GenerateBulkInsertCommand(collection, db.Connection, out sql))
                        {
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tr.Complete();
                }
                catch
                {
                    tr.Dispose();
                    throw;
                }
            }
        }

        internal static IDbCommand GenerateBulkInsertCommand<T>(this Database db, IEnumerable<T> collection, IDbConnection connection, out string sql)
        {
            var pd = Database.PocoData.ForType(typeof(T));
            var tableName = db.EscapeTableName(pd.TableInfo.TableName);

            //get all columns but not the primary key if it is auto-incremental
            var cols = string.Join(", ", (
                from c in pd.Columns
                where 
                    //don't return ResultColumns
                    !c.Value.ResultColumn
                    //if the table is auto-incremental, don't return the primary key
                    && (pd.TableInfo.AutoIncrement && c.Key != pd.TableInfo.PrimaryKey)
                select tableName + "." + db.EscapeSqlIdentifier(c.Key))
                .ToArray());

            var cmd = db.CreateCommand(connection, "");

            var pocoValues = new List<string>();
            var index = 0;
            foreach (var poco in collection)
            {
                var values = new List<string>();
                foreach (var i in pd.Columns)
                {
                    if (pd.TableInfo.AutoIncrement && i.Key == pd.TableInfo.PrimaryKey)
                    {
                        continue;
                    }
                    values.Add(string.Format("{0}{1}", "@", index++));
                    db.AddParam(cmd, i.Value.GetValue(poco), "@");
                }
                pocoValues.Add("(" + string.Join(",", values.ToArray()) + ")");
            }
            sql = string.Format("INSERT INTO {0} ({1}) VALUES {2}", tableName, cols, string.Join(", ", pocoValues));
            return cmd;
        }

        public static void CreateTable(this Database db, bool overwrite, Type modelType)
        {
            var tableDefinition = DefinitionFactory.GetTableDefinition(modelType);
            var tableName = tableDefinition.Name;

            string createSql = SqlSyntaxContext.SqlSyntaxProvider.Format(tableDefinition);
            string createPrimaryKeySql = SqlSyntaxContext.SqlSyntaxProvider.FormatPrimaryKey(tableDefinition);
            var foreignSql = SqlSyntaxContext.SqlSyntaxProvider.Format(tableDefinition.ForeignKeys);
            var indexSql = SqlSyntaxContext.SqlSyntaxProvider.Format(tableDefinition.Indexes);

            var tableExist = db.TableExist(tableName);
            if (overwrite && tableExist)
            {
                db.DropTable(tableName);
            }

            if (tableExist == false)
            {
                using (var transaction = db.GetTransaction())
                {
                    //Execute the Create Table sql
                    int created = db.Execute(new Sql(createSql));
                    LogHelper.Info<Database>(string.Format("Create Table sql {0}:\n {1}", created, createSql));

                    //If any statements exists for the primary key execute them here
                    if (!string.IsNullOrEmpty(createPrimaryKeySql))
                    {
                        int createdPk = db.Execute(new Sql(createPrimaryKeySql));
                        LogHelper.Info<Database>(string.Format("Primary Key sql {0}:\n {1}", createdPk, createPrimaryKeySql));
                    }

                    //Fires the NewTable event, which is used internally to insert base data before adding constrants to the schema
                    if (NewTable != null)
                    {
                        var e = new TableCreationEventArgs();

                        //Turn on identity insert if db provider is not mysql
                        if (SqlSyntaxContext.SqlSyntaxProvider.SupportsIdentityInsert() && tableDefinition.Columns.Any(x => x.IsIdentity))
                            db.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} ON ", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(tableName))));

                        //Call the NewTable-event to trigger the insert of base/default data
                        NewTable(tableName, db, e);

                        //Turn off identity insert if db provider is not mysql
                        if (SqlSyntaxContext.SqlSyntaxProvider.SupportsIdentityInsert() && tableDefinition.Columns.Any(x => x.IsIdentity))
                            db.Execute(new Sql(string.Format("SET IDENTITY_INSERT {0} OFF;", SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(tableName))));

                        //Special case for MySql
                        if (SqlSyntaxContext.SqlSyntaxProvider is MySqlSyntaxProvider && tableName.Equals("umbracoUser"))
                        {
                            db.Update<UserDto>("SET id = @IdAfter WHERE id = @IdBefore AND userLogin = @Login", new { IdAfter = 0, IdBefore = 1, Login = "admin" });
                        }
                    }

                    //Loop through foreignkey statements and execute sql
                    foreach (var sql in foreignSql)
                    {
                        int createdFk = db.Execute(new Sql(sql));
                        LogHelper.Info<Database>(string.Format("Create Foreign Key sql {0}:\n {1}", createdFk, sql));
                    }

                    //Loop through index statements and execute sql
                    foreach (var sql in indexSql)
                    {
                        int createdIndex = db.Execute(new Sql(sql));
                        LogHelper.Info<Database>(string.Format("Create Index sql {0}:\n {1}", createdIndex, sql));
                    }

                    transaction.Complete();
                }
            }

            LogHelper.Info<Database>(string.Format("New table '{0}' was created", tableName));
        }

        public static void DropTable<T>(this Database db)
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
            DropTable(db, tableName);
        }

        public static void DropTable(this Database db, string tableName)
        {
            var sql = new Sql(string.Format(
                SqlSyntaxContext.SqlSyntaxProvider.DropTable,
                SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(tableName)));
            db.Execute(sql);
        }

        public static void TruncateTable(this Database db, string tableName)
        {
            var sql = new Sql(string.Format(
                SqlSyntaxContext.SqlSyntaxProvider.TruncateTable,
                SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(tableName)));
            db.Execute(sql);
        }

        public static bool TableExist(this Database db, string tableName)
        {
            return SqlSyntaxContext.SqlSyntaxProvider.DoesTableExist(db, tableName);
        }

        public static bool TableExist(this UmbracoDatabase db, string tableName)
        {
            return SqlSyntaxContext.SqlSyntaxProvider.DoesTableExist(db, tableName);
        }

        public static void CreateDatabaseSchema(this Database db)
        {
            CreateDatabaseSchema(db, true);
        }

        internal static void UninstallDatabaseSchema(this Database db)
        {
            var creation = new DatabaseSchemaCreation(db);
            creation.UninstallDatabaseSchema();
        }

        internal static void CreateDatabaseSchema(this Database db, bool guardConfiguration)
        {
            if (guardConfiguration && ApplicationContext.Current.IsConfigured)
                throw new Exception("Umbraco is already configured!");

            NewTable += PetaPocoExtensions_NewTable;

            LogHelper.Info<Database>("Initializing database schema creation");

            var creation = new DatabaseSchemaCreation(db);
            creation.InitializeDatabaseSchema();

            LogHelper.Info<Database>("Finalized database schema creation");

            NewTable -= PetaPocoExtensions_NewTable;
        }

        public static DatabaseProviders GetDatabaseProvider(this Database db)
        {
            return ApplicationContext.Current.DatabaseContext.DatabaseProvider;
        }

        private static void PetaPocoExtensions_NewTable(string tableName, Database db, TableCreationEventArgs e)
        {
            var baseDataCreation = new BaseDataCreation(db);
            baseDataCreation.InitializeBaseData(tableName);
        }
    }

    internal class TableCreationEventArgs : System.ComponentModel.CancelEventArgs { }
}