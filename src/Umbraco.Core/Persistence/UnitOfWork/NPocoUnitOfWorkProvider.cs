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