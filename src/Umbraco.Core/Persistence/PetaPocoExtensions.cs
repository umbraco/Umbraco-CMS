using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text.RegularExpressions;
using StackExchange.Profiling.Data;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    public static class PetaPocoExtensions
    {
        // NOTE
        //
        // proper way to do it with TSQL and SQLCE
        //   IF EXISTS (SELECT ... FROM table WITH (UPDLOCK,HOLDLOCK)) WHERE ...)
        //   BEGIN
        //     UPDATE table SET ... WHERE ...
        //   END
        //   ELSE
        //   BEGIN
        //     INSERT INTO table (...) VALUES (...)
        //   END
        //
        // works in READ COMMITED, TSQL & SQLCE lock the constraint even if it does not exist, so INSERT is OK
        //
        // proper way to do it with MySQL
        //   IF EXISTS (SELECT ... FROM table WHERE ... FOR UPDATE)
        //   BEGIN
        //     UPDATE table SET ... WHERE ...
        //   END
        //   ELSE
        //   BEGIN
        //     INSERT INTO table (...) VALUES (...)
        //   END
        //
        // MySQL locks the constraint ONLY if it exists, so INSERT may fail...
        //   in theory, happens in READ COMMITTED but not REPEATABLE READ
        //   http://www.percona.com/blog/2012/08/28/differences-between-read-committed-and-repeatable-read-transaction-isolation-levels/
        //   but according to
        //   http://dev.mysql.com/doc/refman/5.0/en/set-transaction.html
        //   it won't work for exact index value (only ranges) so really...
        //
        // MySQL should do
        //   INSERT INTO table (...) VALUES (...) ON DUPLICATE KEY UPDATE ...
        //
        // also the lock is released when the transaction is committed
        // not sure if that can have unexpected consequences on our code?
        //
        // so... for the time being, let's do with that somewhat crazy solution below...

        /// <summary>
        /// Safely inserts a record, or updates if it exists, based on a unique constraint.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="poco"></param>
        /// <returns>The action that executed, either an insert or an update. If an insert occurred and a PK value got generated, the poco object
        /// passed in will contain the updated value.</returns>
        /// <remarks>
        /// <para>We cannot rely on database-specific options such as MySql ON DUPLICATE KEY UPDATE or MSSQL MERGE WHEN MATCHED because SQLCE
        /// does not support any of them. Ideally this should be achieved with proper transaction isolation levels but that would mean revisiting
        /// isolation levels globally. We want to keep it simple for the time being and manage it manually.</para>
        /// <para>We handle it by trying to update, then insert, etc. until something works, or we get bored.</para>
        /// <para>Note that with proper transactions, if T2 begins after T1 then we are sure that the database will contain T2's value
        /// once T1 and T2 have completed. Whereas here, it could contain T1's value.</para>
        /// </remarks>
        internal static RecordPersistenceType InsertOrUpdate<T>(this Database db, T poco)
            where T : class
        {
            return db.InsertOrUpdate(poco, null, null);
        }

        /// <summary>
        /// Safely inserts a record, or updates if it exists, based on a unique constraint.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="poco"></param>
        /// <param name="updateArgs"></param>
        /// <param name="updateCommand">If the entity has a composite key they you need to specify the update command explicitly</param>
        /// <returns>The action that executed, either an insert or an update. If an insert occurred and a PK value got generated, the poco object
        /// passed in will contain the updated value.</returns>
        /// <remarks>
        /// <para>We cannot rely on database-specific options such as MySql ON DUPLICATE KEY UPDATE or MSSQL MERGE WHEN MATCHED because SQLCE
        /// does not support any of them. Ideally this should be achieved with proper transaction isolation levels but that would mean revisiting
        /// isolation levels globally. We want to keep it simple for the time being and manage it manually.</para>
        /// <para>We handle it by trying to update, then insert, etc. until something works, or we get bored.</para>
        /// <para>Note that with proper transactions, if T2 begins after T1 then we are sure that the database will contain T2's value
        /// once T1 and T2 have completed. Whereas here, it could contain T1's value.</para>
        /// </remarks>
        internal static RecordPersistenceType InsertOrUpdate<T>(this Database db,
            T poco,
            string updateCommand,
            object updateArgs)
            where T : class
        {
            if (poco == null)
                throw new ArgumentNullException("poco");

            // try to update
            var rowCount = updateCommand.IsNullOrWhiteSpace()
                ? db.Update(poco)
                : db.Update<T>(updateCommand, updateArgs);
            if (rowCount > 0)
                return RecordPersistenceType.Update;

            // failed: does not exist, need to insert
            // RC1 race cond here: another thread may insert a record with the same constraint

            var i = 0;
            while (i++ < 4)
            {
                try
                {
                    // try to insert
                    db.Insert(poco);
                    return RecordPersistenceType.Insert;
                }
                catch (SqlException) // TODO: need to find out if all db will throw that exception - probably OK
                {
                    // failed: exists (due to race cond RC1)
                    // RC2 race cond here: another thread may remove the record

                    // try to update
                    rowCount = updateCommand.IsNullOrWhiteSpace()
                        ? db.Update(poco)
                        : db.Update<T>(updateCommand, updateArgs);
                    if (rowCount > 0)
                        return RecordPersistenceType.Update;

                    // failed: does not exist (due to race cond RC2), need to insert
                    // loop
                }
            }

            // this can go on forever... have to break at some point and report an error.
            throw new DataException("Record could not be inserted or updated.");
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

        /// <summary>
        /// Performs the bulk insertion in the context of a current transaction with an optional parameter to complete the transaction
        /// when finished
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="collection"></param>
        [Obsolete("Use the method that specifies an SqlSyntaxContext instance instead")]
        public static void BulkInsertRecords<T>(this Database db, IEnumerable<T> collection)
        {
            db.BulkInsertRecords(collection, null, SqlSyntaxContext.SqlSyntaxProvider, true, false);
        }


        /// <summary>
        /// Performs the bulk insertion
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="collection"></param>
        /// <param name="syntaxProvider"></param>
        /// <param name="useNativeSqlPlatformBulkInsert">
        /// If this is false this will try to just generate bulk insert statements instead of using the current SQL platform's bulk
        /// insert logic. For SQLCE, bulk insert statements do not work so if this is false it will insert one at a time.
        /// </param>
        /// <returns>The number of items inserted</returns>
        public static int BulkInsertRecords<T>(this Database db,
            IEnumerable<T> collection,
            ISqlSyntaxProvider syntaxProvider,
            bool useNativeSqlPlatformBulkInsert = true)
        {
            return BulkInsertRecords<T>(db, collection, null, syntaxProvider, useNativeSqlPlatformBulkInsert, false);
        }

        /// <summary>
        /// Performs the bulk insertion in the context of a current transaction with an optional parameter to complete the transaction
        /// when finished
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="collection"></param>
        /// <param name="tr"></param>
        /// <param name="syntaxProvider"></param>
        /// <param name="useNativeSqlPlatformBulkInsert">
        /// If this is false this will try to just generate bulk insert statements instead of using the current SQL platform's bulk
        /// insert logic. For SQLCE, bulk insert statements do not work so if this is false it will insert one at a time.
        /// </param>
        /// <param name="commitTrans"></param>
        /// <returns>The number of items inserted</returns>
        public static int BulkInsertRecords<T>(this Database db,
            IEnumerable<T> collection,
            Transaction tr,
            ISqlSyntaxProvider syntaxProvider,
            bool useNativeSqlPlatformBulkInsert = true,
            bool commitTrans = false)
        {
            db.OpenSharedConnection();
            try
            {
                return BulkInsertRecordsTry(db, collection, tr, syntaxProvider, useNativeSqlPlatformBulkInsert, commitTrans);
            }
            finally
            {
                db.CloseSharedConnection();
            }
        }

        public static int BulkInsertRecordsTry<T>(this Database db,
            IEnumerable<T> collection,
            Transaction tr,
            ISqlSyntaxProvider syntaxProvider,
            bool useNativeSqlPlatformBulkInsert = true,
            bool commitTrans = false)
        {
            if (commitTrans && tr == null)
                throw new ArgumentNullException("tr", "The transaction cannot be null if commitTrans is true.");

            //don't do anything if there are no records.
            if (collection.Any() == false)
            {
                return 0;
            }

            var pd = Database.PocoData.ForType(typeof (T));
            if (pd == null) throw new InvalidOperationException("Could not find PocoData for " + typeof (T));

            try
            {
                int processed = 0;

                var usedNativeSqlPlatformInserts = useNativeSqlPlatformBulkInsert
                                                   && NativeSqlPlatformBulkInsertRecords(db, syntaxProvider, pd, collection, out processed);

                if (usedNativeSqlPlatformInserts == false)
                {
                    //if it is sql ce or it is a sql server version less than 2008, we need to do individual inserts.
                    var sqlServerSyntax = syntaxProvider as SqlServerSyntaxProvider;
                    if ((sqlServerSyntax != null && (int) sqlServerSyntax.GetVersionName(db) < (int) SqlServerVersionName.V2008)
                        || syntaxProvider is SqlCeSyntaxProvider)
                    {
                        //SqlCe doesn't support bulk insert statements!
                        foreach (var poco in collection)
                        {
                            db.Insert(poco);
                        }
                    }
                    else
                    {
                        //we'll need to generate insert statements instead

                        string[] sqlStatements;
                        var cmds = db.GenerateBulkInsertCommand(pd, collection, out sqlStatements);
                        for (var i = 0; i < sqlStatements.Length; i++)
                        {
                            using (var cmd = cmds[i])
                            {
                                cmd.CommandText = sqlStatements[i];
                                cmd.ExecuteNonQuery();
                                processed++;
                            }
                        }
                    }
                }

                if (commitTrans)
                    tr.Complete();
                return processed;
            }
            catch
            {
                if (commitTrans)
                    tr.Dispose();
                throw;
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
        [Obsolete("Use the method that specifies an SqlSyntaxContext instance instead")]
        public static void BulkInsertRecords<T>(this Database db, IEnumerable<T> collection, Transaction tr, bool commitTrans = false)
        {
            db.BulkInsertRecords<T>(collection, tr, SqlSyntaxContext.SqlSyntaxProvider, commitTrans);
        }

        /// <summary>
        /// Creates a bulk insert command
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="collection"></param>
        /// <param name="sql"></param>
        /// <param name="pd"></param>
        /// <returns>Sql commands with populated command parameters required to execute the sql statement</returns>
        /// <remarks>
        /// The limits for number of parameters are 2100 (in sql server, I think there's many more allowed in mysql). So
        /// we need to detect that many params and split somehow.
        /// For some reason the 2100 limit is not actually allowed even though the exception from sql server mentions 2100 as a max, perhaps it is 2099
        /// that is max. I've reduced it to 2000 anyways.
        /// </remarks>
        internal static IDbCommand[] GenerateBulkInsertCommand<T>(
            this Database db,
            Database.PocoData pd,
            IEnumerable<T> collection,
            out string[] sql)
        {
            if (db == null) throw new ArgumentNullException("db");
            if (db.Connection == null) throw new ArgumentException("db.Connection is null.");

            var tableName = db.EscapeTableName(pd.TableInfo.TableName);

            //get all columns to include and format for sql
            var cols = string.Join(", ",
                pd.Columns
                    .Where(c => IncludeColumn(pd, c))
                    .Select(c => tableName + "." + db.EscapeSqlIdentifier(c.Key)).ToArray());

            var itemArray = collection.ToArray();

            //calculate number of parameters per item
            var paramsPerItem = pd.Columns.Count(i => IncludeColumn(pd, i));

            //Example calc:
            // Given: we have 4168 items in the itemArray, each item contains 8 command parameters (values to be inserterted)
            // 2100 / 8 = 262.5
            // Math.Floor(2100 / 8) = 262 items per trans
            // 4168 / 262 = 15.908... = there will be 16 trans in total

            //all items will be included if we have disabled db parameters
            var itemsPerTrans = Math.Floor(2000.00/paramsPerItem);
            //there will only be one transaction if we have disabled db parameters
            var numTrans = Math.Ceiling(itemArray.Length/itemsPerTrans);

            var sqlQueries = new List<string>();
            var commands = new List<IDbCommand>();

            for (var tIndex = 0; tIndex < numTrans; tIndex++)
            {
                var itemsForTrans = itemArray
                    .Skip(tIndex*(int) itemsPerTrans)
                    .Take((int) itemsPerTrans);

                var cmd = db.CreateCommand(db.Connection, string.Empty);
                var pocoValues = new List<string>();
                var index = 0;
                foreach (var poco in itemsForTrans)
                {
                    var values = new List<string>();
                    //get all columns except result cols and not the primary key if it is auto-incremental
                    foreach (var i in pd.Columns.Where(x => IncludeColumn(pd, x)))
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

        /// <summary>
        /// A filter used below a few times to get all columns except result cols and not the primary key if it is auto-incremental
        /// </summary>
        /// <param name="data"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private static bool IncludeColumn(Database.PocoData data, KeyValuePair<string, Database.PocoColumn> column)
        {
            if (column.Value.ResultColumn) return false;
            if (data.TableInfo.AutoIncrement && column.Key == data.TableInfo.PrimaryKey) return false;
            return true;
        }

        /// <summary>
        /// Bulk insert records with Sql BulkCopy or TableDirect or whatever sql platform specific bulk insert records should be used
        /// </summary>
        /// <param name="db"></param>
        /// <param name="syntaxProvider"></param>
        /// <param name="pd"></param>
        /// <param name="collection"></param>
        /// <param name="processed">The number of records inserted</param>
        private static bool NativeSqlPlatformBulkInsertRecords<T>(Database db, ISqlSyntaxProvider syntaxProvider, Database.PocoData pd, IEnumerable<T> collection, out int processed)
        {
            var dbConnection = db.Connection;

            //unwrap the profiled connection if there is one
            var profiledConnection = dbConnection as ProfiledDbConnection;
            if (profiledConnection != null)
            {
                dbConnection = profiledConnection.InnerConnection;
            }

            //check if it's SQL or SqlCe

            var sqlConnection = dbConnection as SqlConnection;
            if (sqlConnection != null)
            {
                processed = BulkInsertRecordsSqlServer(db, (SqlServerSyntaxProvider)syntaxProvider, pd, collection);
                return true;
            }

            var sqlCeConnection = dbConnection as SqlCeConnection;
            if (sqlCeConnection != null)
            {
                processed = BulkInsertRecordsSqlCe(db, pd, collection);
                return true;
            }

            //could not use the SQL server's specific bulk insert operations
            processed = 0;
            return false;
        }

        /// <summary>
        /// Logic used to perform bulk inserts with SqlCe's TableDirect
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="pd"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        internal static int BulkInsertRecordsSqlCe<T>(Database db,
            Database.PocoData pd,
            IEnumerable<T> collection)
        {
            var cols = pd.Columns.ToArray();

            using (var cmd = db.CreateCommand(db.Connection, string.Empty))
            {
                cmd.CommandText = pd.TableInfo.TableName;
                cmd.CommandType = CommandType.TableDirect;
                //cmd.Transaction = GetTypedTransaction<SqlCeTransaction>(db.Connection.);

                //get the real command
                using (var sqlCeCommand = GetTypedCommand<SqlCeCommand>(cmd))
                {
                    // This seems to cause problems, I think this is primarily used for retrieval, not
                    // inserting. see: https://msdn.microsoft.com/en-us/library/system.data.sqlserverce.sqlcecommand.indexname%28v=vs.100%29.aspx?f=255&MSPPError=-2147217396
                    //sqlCeCommand.IndexName = pd.TableInfo.PrimaryKey;

                    var count = 0;
                    using (var rs = sqlCeCommand.ExecuteResultSet(ResultSetOptions.Updatable))
                    {
                        var rec = rs.CreateRecord();

                        foreach (var item in collection)
                        {
                            for (var i = 0; i < cols.Length; i++)
                            {
                                //skip the index if this shouldn't be included (i.e. PK)
                                if (IncludeColumn(pd, cols[i]))
                                {
                                    var val = cols[i].Value.GetValue(item);
                                    rec.SetValue(i, val);
                                }
                            }
                            rs.Insert(rec);
                            count++;
                        }
                    }
                    return count;
                }

            }
        }

        /// <summary>
        /// Logic used to perform bulk inserts with SqlServer's BulkCopy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="sqlSyntaxProvider"></param>
        /// <param name="pd"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        internal static int BulkInsertRecordsSqlServer<T>(Database db, SqlServerSyntaxProvider sqlSyntaxProvider,
            Database.PocoData pd, IEnumerable<T> collection)
        {
            //NOTE: We need to use the original db.Connection here to create the command, but we need to pass in the typed
            // connection below to the SqlBulkCopy
            using (var cmd = db.CreateCommand(db.Connection, string.Empty))
            {
                using (var copy = new SqlBulkCopy(
                    GetTypedConnection<SqlConnection>(db.Connection),
                    SqlBulkCopyOptions.Default,
                    GetTypedTransaction<SqlTransaction>(cmd.Transaction))
                {
                    BulkCopyTimeout = 10000,
                    DestinationTableName = pd.TableInfo.TableName
                })
                {
                    //var cols = pd.Columns.Where(x => IncludeColumn(pd, x)).Select(x => x.Value).ToArray();

                    using (var bulkReader = new PocoDataDataReader<T, SqlServerSyntaxProvider>(collection, pd, sqlSyntaxProvider))
                    {
                        copy.WriteToServer(bulkReader);

                        return bulkReader.RecordsAffected;
                    }
                }
            }
        }


        /// <summary>
        /// Returns the underlying connection as a typed connection - this is used to unwrap the profiled mini profiler stuff
        /// </summary>
        /// <typeparam name="TConnection"></typeparam>
        /// <param name="connection"></param>
        /// <returns></returns>
        private static TConnection GetTypedConnection<TConnection>(IDbConnection connection)
            where TConnection : class, IDbConnection
        {
            var profiled = connection as ProfiledDbConnection;
            if (profiled != null)
            {
                return profiled.InnerConnection as TConnection;
            }
            return connection as TConnection;
        }

        /// <summary>
        /// Returns the underlying connection as a typed connection - this is used to unwrap the profiled mini profiler stuff
        /// </summary>
        /// <typeparam name="TTransaction"></typeparam>
        /// <param name="connection"></param>
        /// <returns></returns>
        private static TTransaction GetTypedTransaction<TTransaction>(IDbTransaction connection)
            where TTransaction : class, IDbTransaction
        {
            var profiled = connection as ProfiledDbTransaction;
            if (profiled != null)
            {
                return profiled.WrappedTransaction as TTransaction;
            }
            return connection as TTransaction;
        }

        /// <summary>
        /// Returns the underlying connection as a typed connection - this is used to unwrap the profiled mini profiler stuff
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        private static TCommand GetTypedCommand<TCommand>(IDbCommand command)
            where TCommand : class, IDbCommand
        {
            var profiled = command as ProfiledDbCommand;
            if (profiled != null)
            {
                return profiled.InternalCommand as TCommand;
            }
            return command as TCommand;
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