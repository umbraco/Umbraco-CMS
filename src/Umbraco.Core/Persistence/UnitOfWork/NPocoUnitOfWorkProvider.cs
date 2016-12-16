using System;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a <see cref="IDatabaseUnitOfWork"/> provider that creates <see cref="NPocoUnitOfWork"/> instances.
    /// </summary>
    public class NPocoUnitOfWorkProvider : IDatabaseUnitOfWorkProvider
    {        
        private readonly RepositoryFactory _repositoryFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NPocoUnitOfWorkProvider"/> class with a database factory and a repository factory.
        /// </summary>
        /// <param name="databaseFactory">A database factory.</param>
        /// <param name="repositoryFactory">A repository factory.</param>
        public NPocoUnitOfWorkProvider(IUmbracoDatabaseFactory databaseFactory, RepositoryFactory repositoryFactory)
        {
            if (databaseFactory == null) throw new ArgumentNullException(nameof(databaseFactory));
            if (repositoryFactory == null) throw new ArgumentNullException(nameof(repositoryFactory));

            DatabaseFactory = databaseFactory;
            _repositoryFactory = repositoryFactory;
        }

        /// <inheritdoc />
        public IUmbracoDatabaseFactory DatabaseFactory { get; }

        /// <inheritdoc />
        public IDatabaseUnitOfWork CreateUnitOfWork()
        {
            return new NPocoUnitOfWork(DatabaseFactory.Database, _repositoryFactory);
        }
    }
}