using System;
using System.Collections.Generic;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a <see cref="IDatabaseUnitOfWork"/> provider that creates <see cref="NPocoUnitOfWork"/> instances.
    /// </summary>
    public class NPocoUnitOfWorkProvider : IDatabaseUnitOfWorkProvider
    {
        private readonly IDatabaseFactory _databaseFactory;
        private readonly RepositoryFactory _repositoryFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NPocoUnitOfWorkProvider"/> class with a database factory and a repository factory.
        /// </summary>
        /// <param name="databaseFactory">A database factory.</param>
        /// <param name="repositoryFactory">A repository factory.</param>
        public NPocoUnitOfWorkProvider(IDatabaseFactory databaseFactory, RepositoryFactory repositoryFactory)
        {
            Mandate.ParameterNotNull(databaseFactory, nameof(databaseFactory));
            Mandate.ParameterNotNull(repositoryFactory, nameof(repositoryFactory));
            _databaseFactory = databaseFactory;
            _repositoryFactory = repositoryFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NPocoUnitOfWorkProvider"/> class with a logger.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <param name="mappingResolver"></param>
        /// <remarks>
        /// <para>FOR UNIT TESTS ONLY</para>
        /// <para>Creates a new <see cref="IDatabaseFactory"/> each time it is called, by initializing a new
        /// <see cref="DefaultDatabaseFactory"/> with the default connection name, and default sql syntax providers.</para>
        /// </remarks>
        internal NPocoUnitOfWorkProvider(ILogger logger, IMappingResolver mappingResolver)
        {
            _databaseFactory = new DefaultDatabaseFactory(GlobalSettings.UmbracoConnectionName, GetDefaultSqlSyntaxProviders(logger), logger, new DefaultScopeContextAdapter(), mappingResolver);
            // careful, _repositoryFactory remains null!
        }

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
        public IDatabaseUnitOfWork CreateUnitOfWork()
        {
            // get a database from the factory - might be the "ambient" database eg
            // the one that's enlisted with the HttpContext - so it's not always a
            // "new" database.
            var database = _databaseFactory.GetDatabase();
            return new NPocoUnitOfWork(database, _repositoryFactory);
        }

        #endregion
    }
}