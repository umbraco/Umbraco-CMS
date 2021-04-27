using System.Collections.Generic;
using System.Linq;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence
{
    /// <summary>
    /// A provider that just generates insert commands
    /// </summary>
    public class BasicBulkSqlInsertProvider : IBulkSqlInsertProvider
    {
        public string ProviderName => Cms.Core.Constants.DatabaseProviders.SqlServer;

        public int BulkInsertRecords<T>(IUmbracoDatabase database, IEnumerable<T> records)
        {
            var recordsA = records.ToArray();
            if (recordsA.Length == 0) return 0;

            return BulkInsertRecordsWithCommands(database, recordsA);
        }

        /// <summary>
        /// Bulk-insert records using commands.
        /// </summary>
        /// <typeparam name="T">The type of the records.</typeparam>
        /// <param name="database">The database.</param>
        /// <param name="records">The records.</param>
        /// <returns>The number of records that were inserted.</returns>
        internal static int BulkInsertRecordsWithCommands<T>(IUmbracoDatabase database, T[] records)
        {
            foreach (var command in database.GenerateBulkInsertCommands(records))
                command.ExecuteNonQuery();

            return records.Length; // what else?
        }
    }
}
