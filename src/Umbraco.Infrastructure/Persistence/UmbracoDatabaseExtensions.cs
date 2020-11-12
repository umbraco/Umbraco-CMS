using System;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Runtime;

namespace Umbraco.Core.Persistence
{
    internal static class UmbracoDatabaseExtensions
    {
        public static UmbracoDatabase AsUmbracoDatabase(this IUmbracoDatabase database)
        {
            var asDatabase = database as UmbracoDatabase;
            if (asDatabase == null) throw new Exception("oops: database.");
            return asDatabase;
        }

        /// <summary>
        /// Gets a key/value directly from the database, no scope, nothing.
        /// </summary>
        /// <remarks>Used by <see cref="CoreRuntimeBootstrapper"/> to determine the runtime state.</remarks>
        public static string GetFromKeyValueTable(this IUmbracoDatabase database, string key)
        {
            if (database is null) return null;

            var sql = database.SqlContext.Sql()
                .Select<KeyValueDto>()
                .From<KeyValueDto>()
                .Where<KeyValueDto>(x => x.Key == key);
            return database.FirstOrDefault<KeyValueDto>(sql)?.Value;
        }
    }
}
