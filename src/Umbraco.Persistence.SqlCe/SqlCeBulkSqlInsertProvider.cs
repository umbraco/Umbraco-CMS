using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using NPoco;
using Umbraco.Core.Persistence;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Persistence.SqlCe
{
    public class SqlCeBulkSqlInsertProvider : IBulkSqlInsertProvider
    {
        public string ProviderName => Constants.DatabaseProviders.SqlCe;

        public int BulkInsertRecords<T>(IUmbracoDatabase database, IEnumerable<T> records)
        {
            var recordsA = records.ToArray();
            if (recordsA.Length == 0) return 0;

            var pocoData = database.PocoDataFactory.ForType(typeof(T));
            if (pocoData == null) throw new InvalidOperationException("Could not find PocoData for " + typeof(T));

            return BulkInsertRecordsSqlCe(database, pocoData, recordsA);

        }

         /// <summary>
        /// Bulk-insert records using SqlCE TableDirect method.
        /// </summary>
        /// <typeparam name="T">The type of the records.</typeparam>
        /// <param name="database">The database.</param>
        /// <param name="pocoData">The PocoData object corresponding to the record's type.</param>
        /// <param name="records">The records.</param>
        /// <returns>The number of records that were inserted.</returns>
        private static int BulkInsertRecordsSqlCe<T>(IUmbracoDatabase database, PocoData pocoData, IEnumerable<T> records)
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
                var tCommand = NPocoDatabaseExtensions.GetTypedCommand<SqlCeCommand>(command); // execute on the real command

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
                            if (NPocoDatabaseExtensions.IncludeColumn(pocoData, columns[i]))
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
    }
}
