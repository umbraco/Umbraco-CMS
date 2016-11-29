using Umbraco.Core.DI;

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

        /// <inheritdoc />
        public DatabaseContext DatabaseContext => Current.DatabaseContext; // fixme inject!

        /// <inheritdoc />
        public IDatabaseUnitOfWork CreateUnitOfWork()
        {
            // get a database from the factory - might be the "ambient" database eg
            // the one that's enlisted with the HttpContext - so it's *not* necessary a
            // "new" database.
            var database = _databaseFactory.GetDatabase();
            var databaseContext = Current.DatabaseContext; // fixme - inject!
            return new NPocoUnitOfWork(databaseContext, database, _repositoryFactory);
        }
    }
}