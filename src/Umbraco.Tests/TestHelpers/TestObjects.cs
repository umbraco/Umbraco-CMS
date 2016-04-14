using System;
using System.Collections.Generic;
using System.Data.Common;
using Moq;
using NPoco;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// Provides objects for tests.
    /// </summary>
    static partial class TestObjects
    {
        /// <summary>
        /// Gets the default ISqlSyntaxProvider objects.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <param name="lazyFactory">A (lazy) database factory.</param>
        /// <returns>The default ISqlSyntaxProvider objects.</returns>
        public static IEnumerable<ISqlSyntaxProvider> GetDefaultSqlSyntaxProviders(ILogger logger, Lazy<IDatabaseFactory> lazyFactory = null)
        {
            return new ISqlSyntaxProvider[]
            {
                new MySqlSyntaxProvider(logger),
                new SqlCeSyntaxProvider(),
                new SqlServerSyntaxProvider(lazyFactory ?? new Lazy<IDatabaseFactory>(() => null))
            };
        }

        /// <summary>
        /// Gets an UmbracoDatabase.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <returns>An UmbracoDatabase.</returns>
        /// <remarks>This is just a void database that has no actual database but pretends to have an open connection
        /// that can begin a transaction.</remarks>
        public static UmbracoDatabase GetUmbracoSqlCeDatabase(ILogger logger)
        {
            var syntax = new SqlCeSyntaxProvider();
            var dbProviderFactory = DbProviderFactories.GetFactory(Constants.DbProviderNames.SqlCe);
            var connection = TestObjects.GetDbConnection();
            return new UmbracoDatabase(connection, syntax, DatabaseType.SQLCe, dbProviderFactory, logger);
        }

        /// <summary>
        /// Gets an UmbracoDatabase.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <returns>An UmbracoDatabase.</returns>
        /// <remarks>This is just a void database that has no actual database but pretends to have an open connection
        /// that can begin a transaction.</remarks>
        public static UmbracoDatabase GetUmbracoSqlServerDatabase(ILogger logger)
        {
            var syntax = new SqlServerSyntaxProvider(new Lazy<IDatabaseFactory>(() => null)); // do NOT try to get the server's version!
            var dbProviderFactory = DbProviderFactories.GetFactory(Constants.DbProviderNames.SqlServer);
            var connection = TestObjects.GetDbConnection();
            return new UmbracoDatabase(connection, syntax, DatabaseType.SqlServer2008, dbProviderFactory, logger);
        }
    }
}
