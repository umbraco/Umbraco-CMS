using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text.RegularExpressions;
using NPoco;
using StackExchange.Profiling.Data;
using Umbraco.Core.Persistence.FaultHandling;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Provides extension methods to NPoco Database class.
    /// </summary>
    public static class NPocoDatabaseExtensions
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
        // todo: use the proper database syntax, not this kludge

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
        internal static RecordPersistenceType InsertOrUpdate<T>(this IDatabase db, T poco)
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
        internal static RecordPersistenceType InsertOrUpdate<T>(this IDatabase db,
            T poco,
            string updateCommand,
            object updateArgs)
            where T : class
        {
            if (poco == null)
                throw new ArgumentNullException(nameof(poco));

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
                catch (SqlException) // assuming all db engines will throw that exception
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
        /// This will escape single @ symbols for npoco values so it doesn't think it's a parameter
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EscapeAtSymbols(string value)
        {
            if (value.Contains("@") == false) return value;

            //this fancy regex will only match a single @ not a double, etc...
            var regex = new Regex("(?<!@)@(?!@)");
            return regex.Replace(value, "@@");
        }

        // todo: review NPoco native InsertBulk to replace the code below

        /// <summary>
        /// Bulk-inserts records within a transaction.
        /// </summary>
        /// <typeparam name="T">The type of the records.</typeparam>
        /// <param name="database">The database.</param>
        /// <param name="records">The records.</param>
        /// <param name="useNativeBulkInsert">Whether to use native bulk insert when available.</param>
        public static void BulkInsertRecordsWithTransaction<T>(this Database database, IEnumerable<T> records, bool useNativeBulkInsert = true)
        {
            var recordsA = records.ToArray();
            if (recordsA.Length == 0)
                return;

            // no need to "try...catch", if the transaction is not completed it will rollback!
            using (var tr = database.GetTransaction())
            {
                database.BulkInsertRecords(recordsA, useNativeBulkInsert);
                tr.Complete();
            }
        }

        /// <summary>
        /// Bulk-inserts records.
        /// </summary>
        /// <typeparam name="T">The type of the records.</typeparam>
        /// <param name="database">The database.</param>
        /// <param name="records">The records.</param>
        /// <param name="useNativeBulkInsert">Whether to use native bulk insert when available.</param>
        /// <returns>The number of records that were inserted.</returns>
        public static int BulkInsertRecords<T>(this Database database, IEnumerable<T> records, bool useNativeBulkInsert = true)
        {
            var recordsA = records.ToArray();
            if (recordsA.Length == 0) return 0;

            var pocoData = database.PocoDataFactory.ForType(typeof(T));
            if (pocoData == null) throw new InvalidOperationException("Could not find PocoData for " + typeof(T));

            if (database.DatabaseType.IsSqlCe())
            {
                if (useNativeBulkInsert) return BulkInsertRecordsSqlCe(database, pocoData, recordsA);
                // else, no other choice
                foreach (var record in recordsA)
                    database.Insert(record);
                return recordsA.Length;
            }

            if (database.DatabaseType.IsSqlServer())
            {
                return useNativeBulkInsert && database.DatabaseType.IsSqlServer2008OrLater()
                    ? BulkInsertRecordsSqlServer(database, pocoData, recordsA)
                    : BulkInsertRecordsWithCommands(database, recordsA);
            }

            if (database.DatabaseType.IsMySql())
                return BulkInsertRecordsWithCommands(database, recordsA);

            throw new NotSupportedException();
        }

        /// <summary>
        /// Bulk-insert records using commands.
        /// </summary>
        /// <typeparam name="T">The type of the records.</typeparam>
        /// <param name="database">The database.</param>
        /// <param name="records">The records.</param>
        /// <returns>The number of records that were inserted.</returns>
        private static int BulkInsertRecordsWithCommands<T>(Database database, T[] records)
        {
            foreach (var command in database.GenerateBulkInsertCommands(records))
                command.ExecuteNonQuery();

            return records.Length; // what else?
        }

        /// <summary>
        /// Creates bulk-insert commands.
        /// </summary>
        /// <typeparam name="T">The type of the records.</typeparam>
        /// <param name="database">The database.</param>
        /// <param name="records">The records.</param>
        /// <returns>The sql commands to execute.</returns>
        internal static IDbCommand[] GenerateBulkInsertCommands<T>(this Database database, T[] records)
        {
            var pocoData = database.PocoDataFactory.ForType(typeof(T));

            // get columns to include, = number of parameters per row
            var columns = pocoData.Columns.Where(c => IncludeColumn(pocoData, c)).ToArray();
            var paramsPerRecord = columns.Length;

            // format columns to sql
            var tableName = database.DatabaseType.EscapeTableName(pocoData.TableInfo.TableName);
            var columnNames = string.Join(", ", columns.Select(c => tableName + "." + database.DatabaseType.EscapeSqlIdentifier(c.Key)));

            // example:
            // assume 4168 records, each record containing 8 fields, ie 8 command parameters
            // max 2100 parameter per command
            // Math.Floor(2100 / 8) = 262 record per command
            // 4168 / 262 = 15.908... = there will be 16 command in total
            // (if we have disabled db parameters, then all records will be included, in only one command)
            var recordsPerCommand = paramsPerRecord == 0 ? int.MaxValue : Convert.ToInt32(Math.Floor(2000.00 / paramsPerRecord));
            var commandsCount = Convert.ToInt32(Math.Ceiling((double)records.Length / recordsPerCommand));

            var commands = new IDbCommand[commandsCount];
            var recordsIndex = 0;
            var recordsLeftToInsert = records.Length;
            var prefix = database.DatabaseType.GetParameterPrefix(database.ConnectionString);
            for (var commandIndex = 0; commandIndex < commandsCount; commandIndex++)
            {
                var command = database.CreateCommand(database.Connection, CommandType.Text, string.Empty);
                var parameterIndex = 0;
                var commandRecords = Math.Min(recordsPerCommand, recordsLeftToInsert);
                var recordsValues = new string[commandRecords];
                for (var commandRecordIndex = 0; commandRecordIndex < commandRecords; commandRecordIndex++, recordsIndex++, recordsLeftToInsert--)
                {
                    var record = records[recordsIndex];
                    var recordValues = new string[columns.Length];
                    for (var columnIndex = 0; columnIndex < columns.Length; columnIndex++)
                    {
                        database.AddParameter(command, columns[columnIndex].Value.GetValue(record));
                        recordValues[columnIndex] = prefix + parameterIndex++;
                    }
                    recordsValues[commandRecordIndex] = "(" + string.Join(",", recordValues) + ")";
                }

                command.CommandText = $"INSERT INTO {tableName} ({columnNames}) VALUES {string.Join(", ", recordsValues)}";
                commands[commandIndex] = command;
            }

            return commands;
        }

        /// <summary>
        /// Determines whether a column should be part of a bulk-insert.
        /// </summary>
        /// <param name="pocoData">The PocoData object corresponding to the record's type.</param>
        /// <param name="column">The column.</param>
        /// <returns>A value indicating whether the column should be part of the bulk-insert.</returns>
        /// <remarks>Columns that are primary keys and auto-incremental, or result columns, are excluded from bulk-inserts.</remarks>
        private static bool IncludeColumn(PocoData pocoData, KeyValuePair<string, PocoColumn> column)
        {
            return column.Value.ResultColumn == false
                   && (pocoData.TableInfo.AutoIncrement == false || column.Key != pocoData.TableInfo.PrimaryKey);
        }

        /// <summary>
        /// Bulk-insert records using SqlCE TableDirect method.
        /// </summary>
        /// <typeparam name="T">The type of the records.</typeparam>
        /// <param name="database">The database.</param>
        /// <param name="pocoData">The PocoData object corresponding to the record's type.</param>
        /// <param name="records">The records.</param>
        /// <returns>The number of records that were inserted.</returns>
        internal static int BulkInsertRecordsSqlCe<T>(Database database, PocoData pocoData, IEnumerable<T> records)
        {
            var columns = pocoData.Columns.ToArray();

            // create command against the original database.Connection
            using (var command = database.CreateCommand(database.Connection, CommandType.TableDirect, string.Empty))
            {
                command.CommandText = pocoData.TableInfo.TableName;
                command.CommandType = CommandType.TableDirect; // fixme - why repeat?
                // fixme - not supporting transactions?
                //cmd.Transaction = GetTypedTransaction<SqlCeTransaction>(db.Connection.);

                var count = 0;
                var tCommand = GetTypedCommand<SqlCeCommand>(command); // execute on the real command

                // seems to cause problems, I think this is primarily used for retrieval, not inserting.
                // see: https://msdn.microsoft.com/en-us/library/system.data.sqlserverce.sqlcecommand.indexname%28v=vs.100%29.aspx?f=255&MSPPError=-2147217396
                //tCommand.IndexName = pd.TableInfo.PrimaryKey;

                using (var resultSet = tCommand.ExecuteResultSet(ResultSetOptions.Updatable))
                {
                    var updatableRecord = resultSet.CreateRecord();
                    foreach (var record in records)
                    {
                        for (var i = 0; i < columns.Length; i++)
                        {
                            // skip the index if this shouldn't be included (i.e. PK)
                            if (IncludeColumn(pocoData, columns[i]))
                            {
                                var val = columns[i].Value.GetValue(record);
                                updatableRecord.SetValue(i, val);
                            }
                        }
                        resultSet.Insert(updatableRecord);
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>
        /// Bulk-insert records using SqlServer BulkCopy method.
        /// </summary>
        /// <typeparam name="T">The type of the records.</typeparam>
        /// <param name="database">The database.</param>
        /// <param name="pocoData">The PocoData object corresponding to the record's type.</param>
        /// <param name="records">The records.</param>
        /// <returns>The number of records that were inserted.</returns>
        internal static int BulkInsertRecordsSqlServer<T>(Database database, PocoData pocoData, IEnumerable<T> records)
        {
            // create command against the original database.Connection
            using (var command = database.CreateCommand(database.Connection, CommandType.Text, string.Empty))
            {
                // use typed connection and transactionf or SqlBulkCopy
                var tConnection = GetTypedConnection<SqlConnection>(database.Connection);
                var tTransaction = GetTypedTransaction<SqlTransaction>(command.Transaction);
                var tableName = pocoData.TableInfo.TableName;

                var umbracoDatabase = database as UmbracoDatabase;
                if (umbracoDatabase == null) throw new NotSupportedException("Database must be UmbracoDatabase.");
                var syntax = umbracoDatabase.SqlSyntax as SqlServerSyntaxProvider;
                if (syntax == null) throw new NotSupportedException("SqlSyntax must be SqlServerSyntaxProvider.");

                using (var copy = new SqlBulkCopy(tConnection, SqlBulkCopyOptions.Default, tTransaction) { BulkCopyTimeout = 10000, DestinationTableName = tableName })
                using (var bulkReader = new PocoDataDataReader<T, SqlServerSyntaxProvider>(records, pocoData, syntax))
                {
                    copy.WriteToServer(bulkReader);
                    return bulkReader.RecordsAffected;
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
            return profiled == null ? connection as TConnection : profiled.InnerConnection as TConnection;
        }

        /// <summary>
        /// Returns the underlying transaction as a typed transaction - this is used to unwrap the profiled mini profiler stuff
        /// </summary>
        /// <typeparam name="TTransaction"></typeparam>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private static TTransaction GetTypedTransaction<TTransaction>(IDbTransaction transaction)
            where TTransaction : class, IDbTransaction
        {
            var profiled = transaction as ProfiledDbTransaction;
            return profiled == null ? transaction as TTransaction : profiled.WrappedTransaction as TTransaction;
        }

        /// <summary>
        /// Returns the underlying command as a typed command - this is used to unwrap the profiled mini profiler stuff
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        private static TCommand GetTypedCommand<TCommand>(IDbCommand command)
            where TCommand : class, IDbCommand
        {
            var faultHandling = command as FaultHandlingDbCommand;
            if (faultHandling != null) command = faultHandling.Inner;
            var profiled = command as ProfiledDbCommand;
            if (profiled != null) command = profiled.InternalCommand;
            return command as TCommand;
        }

        public static void TruncateTable(this IDatabase db, ISqlSyntaxProvider sqlSyntax, string tableName)
        {
            var sql = new Sql(string.Format(
                sqlSyntax.TruncateTable,
                sqlSyntax.GetQuotedTableName(tableName)));
            db.Execute(sql);
        }

        public static IsolationLevel GetCurrentTransactionIsolationLevel(this IDatabase database)
        {
            var transaction = database.Transaction;
            return transaction?.IsolationLevel ?? IsolationLevel.Unspecified;
        }

        public static IEnumerable<TResult> FetchByGroups<TResult, TSource>(this IDatabase db, IEnumerable<TSource> source, int groupSize, Func<IEnumerable<TSource>, Sql<SqlContext>> sqlFactory)
        {
            return source.SelectByGroups(x => db.Fetch<TResult>(sqlFactory(x)), groupSize);
        }
    }
}