using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.Querying;
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
            //don't do anything if there are no records.
            if (collection.Any() == false)
                return;

            using (var tr = db.GetTransaction())
            {
                db.BulkInsertRecords(collection, tr);
            }
        }

        public static void BulkInsertRecords<T>(this Database db, IEnumerable<T> collection, Transaction tr)
        {
            //don't do anything if there are no records.
            if (collection.Any() == false)
                return;

            try
            {
                //if it is sql ce or it is a sql server version less than 2008, we need to do individual inserts.
                var sqlServerSyntax = SqlSyntaxContext.SqlSyntaxProvider as SqlServerSyntaxProvider;
                if ((sqlServerSyntax != null && (int)sqlServerSyntax.VersionName.Value < (int)SqlServerVersionName.V2008) 
                    || SqlSyntaxContext.SqlSyntaxProvider is SqlCeSyntaxProvider)
                {
                    //SqlCe doesn't support bulk insert statements!

                    foreach (var poco in collection)
                    {
                        db.Insert(poco);
                    }
                }
                else
                {
                    string[] sqlStatements;
                    var cmds = db.GenerateBulkInsertCommand(collection, db.Connection, out sqlStatements);
                    for (var i = 0; i < sqlStatements.Length; i++)
                    {
                        using (var cmd = cmds[i])
                        {
                            cmd.CommandText = sqlStatements[i];
                            cmd.ExecuteNonQuery();
                        }
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

        /// <summary>
        /// Creates a bulk insert command
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="collection"></param>
        /// <param name="connection"></param>        
        /// <param name="sql"></param>
        /// <returns>Sql commands with populated command parameters required to execute the sql statement</returns>
        /// <remarks>
        /// The limits for number of parameters are 2100 (in sql server, I think there's many more allowed in mysql). So 
        /// we need to detect that many params and split somehow. 
        /// For some reason the 2100 limit is not actually allowed even though the exception from sql server mentions 2100 as a max, perhaps it is 2099 
        /// that is max. I've reduced it to 2000 anyways.
        /// </remarks>
        internal static IDbCommand[] GenerateBulkInsertCommand<T>(
            this Database db, 
            IEnumerable<T> collection, 
            IDbConnection connection,             
            out string[] sql)
        {
            //A filter used below a few times to get all columns except result cols and not the primary key if it is auto-incremental
            Func<Database.PocoData, KeyValuePair<string, Database.PocoColumn>, bool> includeColumn = (data, column) =>
                {
                    if (column.Value.ResultColumn) return false;
                    if (data.TableInfo.AutoIncrement && column.Key == data.TableInfo.PrimaryKey) return false;
                    return true;
                };

            var pd = Database.PocoData.ForType(typeof(T));
            var tableName = db.EscapeTableName(pd.TableInfo.TableName);

            //get all columns to include and format for sql
            var cols = string.Join(", ", 
                pd.Columns
                .Where(c => includeColumn(pd, c))
                .Select(c => tableName + "." + db.EscapeSqlIdentifier(c.Key)).ToArray());

            var itemArray = collection.ToArray();

            //calculate number of parameters per item
            var paramsPerItem = pd.Columns.Count(i => includeColumn(pd, i));
            
            //Example calc:
            // Given: we have 4168 items in the itemArray, each item contains 8 command parameters (values to be inserterted)                
            // 2100 / 8 = 262.5
            // Math.Floor(2100 / 8) = 262 items per trans
            // 4168 / 262 = 15.908... = there will be 16 trans in total

            //all items will be included if we have disabled db parameters
            var itemsPerTrans = Math.Floor(2000.00 / paramsPerItem);
            //there will only be one transaction if we have disabled db parameters
            var numTrans = Math.Ceiling(itemArray.Length / itemsPerTrans);

            var sqlQueries = new List<string>();
            var commands = new List<IDbCommand>();

            for (var tIndex = 0; tIndex < numTrans; tIndex++)
            {
                var itemsForTrans = itemArray
                    .Skip(tIndex * (int)itemsPerTrans)
                    .Take((int)itemsPerTrans);

                var cmd = db.CreateCommand(connection, "");
                var pocoValues = new List<string>();
                var index = 0;
                foreach (var poco in itemsForTrans)
                {
                    var values = new List<string>();
                    //get all columns except result cols and not the primary key if it is auto-incremental
                    foreach (var i in pd.Columns.Where(x => includeColumn(pd, x)))
                    {
                        db.AddParam(cmd, i.Value.GetValue(poco), "@");
                        values.Add(string.Format("{0}{1}", "@", index++));
                    }
                    pocoValues.Add("(" + string.Join(",", values.ToArray()) + ")");
                }

                var sqlResult = string.Format("INSERT INTO {0} ({1}) VALUES {2}", tableName, cols, string.Join(", ", pocoValues)); 
                sqlQueries.Add(sqlResult);
                commands.Add(cmd);
            }

            sql = sqlQueries.ToArray();

            return commands.ToArray();    
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