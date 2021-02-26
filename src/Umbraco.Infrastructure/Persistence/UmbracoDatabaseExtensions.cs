using System;
using System.Linq;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence
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


        /// <summary>
        /// Returns true if the database contains the specified table
        /// </summary>
        /// <param name="database"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool HasTable(this IUmbracoDatabase database, string tableName)
        {
            try
            {
                return database.SqlContext.SqlSyntax.GetTablesInSchema(database).Any(table => table.InvariantEquals(tableName));
            }
            catch (Exception)
            {
                return false; // will occur if the database cannot connect
            }
        }

        /// <summary>
        /// Returns true if the database contains no tables
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static bool IsDatabaseEmpty(this IUmbracoDatabase database)
            => database.SqlContext.SqlSyntax.GetTablesInSchema(database).Any() == false;

    }
}
