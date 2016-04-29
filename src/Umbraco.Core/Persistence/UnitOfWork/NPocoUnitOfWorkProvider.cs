using System;
using System.Collections.Generic;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a <see cref="IDatabaseUnitOfWork"/> provider that creates <see cref="NPocoUnitOfWork"/> instances.
    /// </summary>
    public class NPocoUnitOfWorkProvider : IDatabaseUnitOfWorkProvider
    {
        private readonly IDatabaseFactory _dbFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NPocoUnitOfWorkProvider"/> class with a database factory.
        /// </summary>
        /// <param name="dbFactory">A database factory implementation.</param>
        public NPocoUnitOfWorkProvider(IDatabaseFactory dbFactory)
        {
            Mandate.ParameterNotNull(dbFactory, nameof(dbFactory));
            _dbFactory = dbFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NPocoUnitOfWorkProvider"/> class with a logger.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <remarks>
        /// <para>FOR UNIT TESTS ONLY</para>
        /// <para>Creates a new <see cref="IDatabaseFactory"/> each time it is called, by initializing a new
        /// <see cref="DefaultDatabaseFactory"/> with the default connection name, and default sql syntax providers.</para>
        /// </remarks>
        internal NPocoUnitOfWorkProvider(ILogger logger)
            : this(new DefaultDatabaseFactory(GlobalSettings.UmbracoConnectionName, GetDefaultSqlSyntaxProviders(logger), logger))
        { }

        // this should NOT be here, all tests should supply the appropriate providers,
        // however the above ctor is used in hundreds of tests at the moment, so...
        // will refactor later
        private static IEnumerable<ISqlSyntaxProvider> GetDefaultSqlSyntaxProviders(ILogger logger)
        {
            return new ISqlSyntaxProvider[]
            {
                new MySqlSyntaxProvider(logger),
                new SqlCeSyntaxProvider(),
                new SqlServerSyntaxProvider(new Lazy<IDatabaseFactory>(() => null))
            };
        }

        #region Implement IUnitOfWorkProvider

        /// <summary>
        /// Creates a unit of work around a database obtained from the database factory.
        /// </summary>
        /// <returns>A unit of work.</returns>
        /// <remarks>The unit of work will execute on the database returned by the database factory.</remarks>
        public IDatabaseUnitOfWork GetUnitOfWork()
        {
            // get a database from the factory - might be the "ambient" database eg
            // the one that's enlisted with the HttpContext - so it's not always a
            // "new" database.
            var database = _dbFactory.GetDatabase();
            return new NPocoUnitOfWork(database);
        }

        #endregion
    }
}