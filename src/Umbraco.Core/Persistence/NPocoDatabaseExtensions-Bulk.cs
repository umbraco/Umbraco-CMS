using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Data.SqlTypes;
using System.Linq;
using NPoco;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Provides extension methods to NPoco Database class.
    /// </summary>
    public static partial class NPocoDatabaseExtensions
    {
        /// <summary>
        /// Configures NPoco's SqlBulkCopyHelper to use the correct SqlConnection and SqlTransaction instances from the underlying RetryDbConnection and ProfiledDbTransaction
        /// </summary>
        /// <remarks>
        /// This is required to use NPoco's own <see cref="Database.InsertBulk{T}(IEnumerable{T})" /> method because we use wrapped DbConnection and DbTransaction instances.
        /// NPoco's InsertBulk method only caters for efficient bulk inserting records for Sql Server, it does not cater for bulk inserting of records for
        /// any other database type and in which case will just insert records one at a time.
        /// NPoco's InsertBulk method also deals with updating the passed in entity's PK/ID once it's inserted whereas our own BulkInsertRecords methods
        /// do not handle this scenario.
        /// </remarks>
        public static void ConfigureNPocoBulkExtensions()
        {
            SqlBulkCopyHelper.SqlConnectionResolver = dbConn => GetTypedConnection<SqlConnection>(dbConn);
            SqlBulkCopyHelper.SqlTransactionResolver = dbTran => GetTypedTransaction<SqlTransaction>(dbTran);
        }

        /// <summary>
        /// Bulk-inserts records within a transaction.
        /// </summary>
        /// <typeparam name="T">The type of the records.</typeparam>
        /// <param name="database">The database.</param>
        /// <param name="records">The records.</param>
        /// <param name="useNativeBulkInsert">Whether to use native bulk insert when available.</param>
        public static void BulkInsertRecordsWithTransaction<T>(this IUmbracoDatabase database, IEnumerable<T> records, bool useNativeBulkInsert = true)
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
        public static int BulkInsertRecords<T>(this IUmbracoDatabase database, IEnumerable<T> records, bool useNativeBulkInsert = true)
        {
            if (!records.Any()) return 0;

            var pocoData = database.PocoDataFactory.ForType(typeof(T));
            if (pocoData == null) throw new InvalidOperationException("Could not find PocoData for " + typeof(T));

            if (database.DatabaseType.IsSqlCe())
            {
                if (useNativeBulkInsert)
                {
                    return BulkInsertRecordsSqlCe(database, pocoData, records);
                }

                // else, no other choice
                var count = 0;                
                foreach (var record in records)
                {
                    database.Insert(record);
                    count++;
                }   
                return count;
            }

            if (database.DatabaseType.IsSqlServer())
            {
                return useNativeBulkInsert && database.DatabaseType.IsSqlServer2008OrLater()
                    ? BulkInsertRecordsSqlServer(database, pocoData, records)
                    : BulkInsertRecordsWithCommands(database, records.ToArray());
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Bulk-insert records using commands.
        /// </summary>
        /// <typeparam name="T">The type of the records.</typeparam>
        /// <param name="database">The database.</param>
        /// <param name="records">The records.</param>
        /// <returns>The number of records that were inserted.</returns>
        private static int BulkInsertRecordsWithCommands<T>(IUmbracoDatabase database, T[] records)
        {
            foreach (var command in database.GenerateBulkInsertCommands(records))
            {
                command.ExecuteNonQuery();
            }   

            return records.Length; // what else?
        }

        /// <summary>
        /// Creates bulk-insert commands.
        /// </summary>
        /// <typeparam name="T">The type of the records.</typeparam>
        /// <param name="database">The database.</param>
        /// <param name="records">The records.</param>
        /// <returns>The sql commands to execute.</returns>
        internal static IDbCommand[] GenerateBulkInsertCommands<T>(this IUmbracoDatabase database, T[] records)
        {
            if (database?.Connection == null) throw new ArgumentException("Null database?.connection.", nameof(database));

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
            var recordsPerCommand = paramsPerRecord == 0 ? int.MaxValue : Convert.ToInt32(Math.Floor((double)Constants.Sql.MaxParameterCount / paramsPerRecord));
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
        internal static int BulkInsertRecordsSqlCe<T>(IUmbracoDatabase database, PocoData pocoData, IEnumerable<T> records)
        {
            var columns = pocoData.Columns.ToArray();

            // create command against the original database.Connection
            using (var command = database.CreateCommand(database.Connection, CommandType.TableDirect, string.Empty))
            {
                command.CommandText = pocoData.TableInfo.TableName;
                command.CommandType = CommandType.TableDirect; // TODO: why repeat?
                // TODO: not supporting transactions?
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
                                if (val is byte[])
                                {
                                    var bytes = val as byte[];
                                    updatableRecord.SetSqlBinary(i, new SqlBinary(bytes));
                                }
                                else
                                {
                                    updatableRecord.SetValue(i, val);
                                }
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
        internal static int BulkInsertRecordsSqlServer<T>(IUmbracoDatabase database, PocoData pocoData, IEnumerable<T> records)
        {
            // TODO: The main reason this exists is because the NPoco InsertBulk method doesn't return the number of items.
            // It is worth investigating the performance of this vs NPoco's because we use a custom BulkDataReader
            // which in theory should be more efficient than NPocos way of building up an in-memory DataTable.

            // create command against the original database.Connection
            using (var command = database.CreateCommand(database.Connection, CommandType.Text, string.Empty))
            {
                // use typed connection and transactionf or SqlBulkCopy
                var tConnection = GetTypedConnection<SqlConnection>(database.Connection);
                var tTransaction = GetTypedTransaction<SqlTransaction>(command.Transaction);
                var tableName = pocoData.TableInfo.TableName;

                var syntax = database.SqlContext.SqlSyntax as SqlServerSyntaxProvider;
                if (syntax == null) throw new NotSupportedException("SqlSyntax must be SqlServerSyntaxProvider.");

                using (var copy = new SqlBulkCopy(tConnection, SqlBulkCopyOptions.Default, tTransaction)
                {
                    BulkCopyTimeout = 0, // 0 = no bulk copy timeout. If a timeout occurs it will be an connection/command timeout.
                    DestinationTableName = tableName,
                    // be consistent with NPoco: https://github.com/schotime/NPoco/blob/5117a55fde57547e928246c044fd40bd00b2d7d1/src/NPoco.SqlServer/SqlBulkCopyHelper.cs#L50
                    BatchSize = 4096
                })
                using (var bulkReader = new PocoDataDataReader<T, SqlServerSyntaxProvider>(records, pocoData, syntax))
                {
                    //we need to add column mappings here because otherwise columns will be matched by their order and if the order of them are different in the DB compared
                    //to the order in which they are declared in the model then this will not work, so instead we will add column mappings by name so that this explicitly uses
                    //the names instead of their ordering.
                    foreach (var col in bulkReader.ColumnMappings)
                    {
                        copy.ColumnMappings.Add(col.DestinationColumn, col.DestinationColumn);
                    }

                    copy.WriteToServer(bulkReader);
                    return bulkReader.RecordsAffected;
                }
            }
        }

    }
}
