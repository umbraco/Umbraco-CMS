using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Microsoft.Data.Sqlite;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence
{
    /// <summary>
    /// A bulk sql insert provider for Sqlite
    /// </summary>
    public class SqliteBulkSqlInsertProvider : IBulkSqlInsertProvider
    {
        public string ProviderName => Cms.Core.Constants.DatabaseProviders.SQLite;

        public int BulkInsertRecords<T>(IUmbracoDatabase database, IEnumerable<T> records)
        {
            var recordsA = records.ToArray();
            if (recordsA.Length == 0) return 0;

            var pocoData = database.PocoDataFactory.ForType(typeof(T));
            if (pocoData == null) throw new InvalidOperationException("Could not find PocoData for " + typeof(T));

            return BulkInsertRecordsSqlite(database, pocoData, recordsA);
        }

        /// <summary>
        /// Bulk-insert records using SqlServer BulkCopy method.
        /// </summary>
        /// <typeparam name="T">The type of the records.</typeparam>
        /// <param name="database">The database.</param>
        /// <param name="pocoData">The PocoData object corresponding to the record's type.</param>
        /// <param name="records">The records.</param>
        /// <returns>The number of records that were inserted.</returns>
        private int BulkInsertRecordsSqlite<T>(IUmbracoDatabase database, PocoData pocoData, IEnumerable<T> records)
        {
            var count = 0;
            var inTrans = database.InTransaction;

            if (!inTrans)
            {
                database.BeginTransaction();
            }

            foreach (var record in records)
            {
                database.Insert(record);
                count++;
            }

            if (!inTrans)
            {
                database.CompleteTransaction();
            }

            return count;
        }
    }
}
