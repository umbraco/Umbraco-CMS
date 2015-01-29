using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    public static class PetaPocoExtensions
    {
        

        /// <summary>
        /// This will handle the issue of inserting data into a table when there can be a violation of a primary key or unique constraint which
        /// can occur when two threads are trying to insert data at the exact same time when the data violates this constraint. 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="poco"></param>       
        /// <returns>
        /// Returns the action that executed, either an insert or an update
        /// 
        /// NOTE: If an insert occurred and a PK value got generated, the poco object passed in will contain the updated value.
        /// </returns>
        /// <remarks>
        /// In different databases, there are a few raw SQL options like MySql's ON DUPLICATE KEY UPDATE or MSSQL's MERGE WHEN MATCHED, but since we are 
        /// also supporting SQLCE for which this doesn't exist we cannot simply rely on the underlying database to help us here. So we'll actually need to 
        /// try to be as proficient as possible when we know this can occur and manually handle the issue. 
        /// 
        /// We do this by first trying to Update the record, this will return the number of rows affected. If it is zero then we insert, if it is one, then
        /// we know the update was successful and the row was already inserted by another thread. If the rowcount is zero and we insert and get an exception, 
        /// that's due to a race condition, in which case we need to retry and update.
        /// </remarks>
        internal static RecordPersistenceType InsertOrUpdate<T>(this Database db, T poco)
            where T : class
        {
            return db.InsertOrUpdate(poco, null, null);
        }

        /// <summary>
        /// This will handle the issue of inserting data into a table when there can be a violation of a primary key or unique constraint which
        /// can occur when two threads are trying to insert data at the exact same time when the data violates this constraint. 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="poco"></param>
        /// <param name="updateArgs"></param>      
        /// <param name="updateCommand">If the entity has a composite key they you need to specify the update command explicitly</param>
        /// <returns>
        /// Returns the action that executed, either an insert or an update
        /// 
        /// NOTE: If an insert occurred and a PK value got generated, the poco object passed in will contain the updated value.
        /// </returns>
        /// <remarks>
        /// In different databases, there are a few raw SQL options like MySql's ON DUPLICATE KEY UPDATE or MSSQL's MERGE WHEN MATCHED, but since we are 
        /// also supporting SQLCE for which this doesn't exist we cannot simply rely on the underlying database to help us here. So we'll actually need to 
        /// try to be as proficient as possible when we know this can occur and manually handle the issue. 
        /// 
        /// We do this by first trying to Update the record, this will return the number of rows affected. If it is zero then we insert, if it is one, then
        /// we know the update was successful and the row was already inserted by another thread. If the rowcount is zero and we insert and get an exception, 
        /// that's due to a race condition, in which case we need to retry and update.
        /// </remarks>
        internal static RecordPersistenceType InsertOrUpdate<T>(this Database db,
            T poco, 
            string updateCommand, 
            object updateArgs)
            where T : class
        {
            if (poco == null) throw new ArgumentNullException("poco");

            var rowCount = updateCommand.IsNullOrWhiteSpace()
                    ? db.Update(poco)
                    : db.Update<T>(updateCommand, updateArgs); 

            if (rowCount > 0) return RecordPersistenceType.Update;

            try
            {
                db.Insert(poco);
                return RecordPersistenceType.Insert;
            }
            //TODO: Need to find out if this is the same exception that will occur for all databases... pretty sure it will be
            catch (SqlException ex)
            {
                //This will occur if the constraint was violated and this record was already inserted by another thread, 
                //at this exact same time, in this case we need to do an update

                rowCount = updateCommand.IsNullOrWhiteSpace() 
                    ? db.Update(poco) 
                    : db.Update<T>(updateCommand, updateArgs);                

                if (rowCount == 0)
                {
                    //this would be strange! in this case the only circumstance would be that at the exact same time, 3 threads executed, one
                    // did the insert and the other somehow managed to do a delete precisely before this update was executed... now that would
                    // be real crazy. In that case we need to throw an exception.
                    throw new DataException("Record could not be inserted or updated");
                }

                return RecordPersistenceType.Update;
            }
        }

        /// <summary>
        /// This will escape single @ symbols for peta poco values so it doesn't think it's a parameter
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EscapeAtSymbols(string value)
        {
            if (value.Contains("@"))
            {
                //this fancy regex will only match a single @ not a double, etc...
                var regex = new Regex("(?<!@)@(?!@)");
                return regex.Replace(value, "@@");    
            }
            return value;

        }

        [Obsolete("Use the DatabaseSchemaHelper instead")]
        public static void CreateTable<T>(this Database db)
          where T : new()
        {
            var creator = new DatabaseSchemaHelper(db, LoggerResolver.Current.Logger, SqlSyntaxContext.SqlSyntaxProvider);
            creator.CreateTable<T>();
        }

        [Obsolete("Use the DatabaseSchemaHelper instead")]
        public static void CreateTable<T>(this Database db, bool overwrite)
           where T : new()
        {
            var creator = new DatabaseSchemaHelper(db, LoggerResolver.Current.Logger, SqlSyntaxContext.SqlSyntaxProvider);
            creator.CreateTable<T>(overwrite);
        }

        public static void BulkInsertRecords<T>(this Database db, IEnumerable<T> collection)
        {
            //don't do anything if there are no records.
            if (collection.Any() == false)
                return;

            using (var tr = db.GetTransaction())
            {
                db.BulkInsertRecords(collection, tr, true);
            }
        }

        /// <summary>
        /// Performs the bulk insertion in the context of a current transaction with an optional parameter to complete the transaction
        /// when finished
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="collection"></param>
        /// <param name="tr"></param>
        /// <param name="commitTrans"></param>
        public static void BulkInsertRecords<T>(this Database db, IEnumerable<T> collection, Transaction tr, bool commitTrans = false)
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

                if (commitTrans)
                {
                    tr.Complete();    
                }
            }
            catch
            {
                if (commitTrans)
                {
                    tr.Dispose();    
                }
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

        [Obsolete("Use the DatabaseSchemaHelper instead")]
        public static void CreateTable(this Database db, bool overwrite, Type modelType)
        {
            var creator = new DatabaseSchemaHelper(db, LoggerResolver.Current.Logger, SqlSyntaxContext.SqlSyntaxProvider);
            creator.CreateTable(overwrite, modelType);
        }

        [Obsolete("Use the DatabaseSchemaHelper instead")]
        public static void DropTable<T>(this Database db)
            where T : new()
        {
            var helper = new DatabaseSchemaHelper(db, LoggerResolver.Current.Logger, SqlSyntaxContext.SqlSyntaxProvider);
            helper.DropTable<T>();
        }

        [Obsolete("Use the DatabaseSchemaHelper instead")]
        public static void DropTable(this Database db, string tableName)
        {
            var helper = new DatabaseSchemaHelper(db, LoggerResolver.Current.Logger, SqlSyntaxContext.SqlSyntaxProvider);
            helper.DropTable(tableName);
        }

        public static void TruncateTable(this Database db, string tableName)
        {
            var sql = new Sql(string.Format(
                SqlSyntaxContext.SqlSyntaxProvider.TruncateTable,
                SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(tableName)));
            db.Execute(sql);
        }

        [Obsolete("Use the DatabaseSchemaHelper instead")]
        public static bool TableExist(this Database db, string tableName)
        {
            return SqlSyntaxContext.SqlSyntaxProvider.DoesTableExist(db, tableName);
        }

        [Obsolete("Use the DatabaseSchemaHelper instead")]
        public static bool TableExist(this UmbracoDatabase db, string tableName)
        {
            return SqlSyntaxContext.SqlSyntaxProvider.DoesTableExist(db, tableName);
        }

        /// <summary>
        /// Creates the Umbraco db schema in the Database of the current Database.
        /// Safe method that is only able to create the schema in non-configured
        /// umbraco instances.
        /// </summary>
        /// <param name="db">Current PetaPoco <see cref="Database"/> object</param>
        [Obsolete("Use the DatabaseSchemaHelper instead")]
        public static void CreateDatabaseSchema(this Database db)
        {
            CreateDatabaseSchema(db, true);
        }

        /// <summary>
        /// Creates the Umbraco db schema in the Database of the current Database
        /// with the option to guard the db from having the schema created
        /// multiple times.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="guardConfiguration"></param>
        [Obsolete("Use the DatabaseSchemaHelper instead")]
        public static void CreateDatabaseSchema(this Database db, bool guardConfiguration)
        {
            var helper = new DatabaseSchemaHelper(db, LoggerResolver.Current.Logger, SqlSyntaxContext.SqlSyntaxProvider);
            helper.CreateDatabaseSchema(guardConfiguration, ApplicationContext.Current);
        }

        //TODO: What the heck? This makes no sense at all
        public static DatabaseProviders GetDatabaseProvider(this Database db)
        {
            return ApplicationContext.Current.DatabaseContext.DatabaseProvider;
        }

        
    }

    
}