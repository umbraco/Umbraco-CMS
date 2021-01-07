using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Install;

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

        /// <summary>
        /// Returns the <see cref="DatabaseSchemaResult"/> for the database
        /// </summary>
        /// <param name="database"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static DatabaseSchemaResult ValidateSchema(this IUmbracoDatabase database, ILogger logger)
        {
            if (database is null) throw new ArgumentNullException(nameof(database));
            if (logger is null) throw new ArgumentNullException(nameof(logger));

            var dbSchema = new DatabaseSchemaCreator(database, logger);
            var databaseSchemaValidationResult = dbSchema.ValidateSchema();            
            return databaseSchemaValidationResult;
        }

        /// <summary>
        /// Returns true if Umbraco database tables are detected to be installed
        /// </summary>
        /// <param name="database"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static bool IsUmbracoInstalled(this IUmbracoDatabase database, ILogger logger)
        {
            if (database is null) throw new ArgumentNullException(nameof(database));
            if (logger is null) throw new ArgumentNullException(nameof(logger));

            var databaseSchemaValidationResult = database.ValidateSchema(logger);
            return databaseSchemaValidationResult.DetermineHasInstalledVersion();
        }

    }
}
