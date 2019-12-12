using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NPoco;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    public class SqlServerBulkSqlInsertProvider : IBulkSqlInsertProvider
    {
        public int BulkInsertRecords<T>(IUmbracoDatabase database, IEnumerable<T> records, bool useNativeBulkInsert)
        {
            var recordsA = records.ToArray();
            if (recordsA.Length == 0) return 0;

            var pocoData = database.PocoDataFactory.ForType(typeof(T));
            if (pocoData == null) throw new InvalidOperationException("Could not find PocoData for " + typeof(T));

            return useNativeBulkInsert && database.DatabaseType.IsSqlServer2008OrLater()
                ? BulkInsertRecordsSqlServer(database, pocoData, recordsA)
                : BulkInsertRecordsWithCommands(database, recordsA);
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
                command.ExecuteNonQuery();

            return records.Length; // what else?
        }

        /// <summary>
        /// Bulk-insert records using SqlServer BulkCopy method.
        /// </summary>
        /// <typeparam name="T">The type of the records.</typeparam>
        /// <param name="database">The database.</param>
        /// <param name="pocoData">The PocoData object corresponding to the record's type.</param>
        /// <param name="records">The records.</param>
        /// <returns>The number of records that were inserted.</returns>
        private int BulkInsertRecordsSqlServer<T>(IUmbracoDatabase database, PocoData pocoData, IEnumerable<T> records)
        {
            // create command against the original database.Connection
            using (var command = database.CreateCommand(database.Connection, CommandType.Text, string.Empty))
            {
                // use typed connection and transaction or SqlBulkCopy
                var tConnection = NPocoDatabaseExtensions.GetTypedConnection<SqlConnection>(database.Connection);
                var tTransaction = NPocoDatabaseExtensions.GetTypedTransaction<SqlTransaction>(command.Transaction);
                var tableName = pocoData.TableInfo.TableName;

                var syntax = database.SqlContext.SqlSyntax as SqlServerSyntaxProvider;
                if (syntax == null) throw new NotSupportedException("SqlSyntax must be SqlServerSyntaxProvider.");

                using (var copy = new SqlBulkCopy(tConnection, SqlBulkCopyOptions.Default, tTransaction) { BulkCopyTimeout = 10000, DestinationTableName = tableName })
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
