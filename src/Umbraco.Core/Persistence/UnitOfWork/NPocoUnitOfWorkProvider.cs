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
        /// <param name="databaseContext">A database context.</param>
        /// <param name="repositoryFactory">A repository factory.</param>
        public NPocoUnitOfWorkProvider(DatabaseContext databaseContext, RepositoryFactory repositoryFactory)
        {
            if (databaseContext == null) throw new ArgumentNullException(nameof(databaseContext));
            if (repositoryFactory == null) throw new ArgumentNullException(nameof(repositoryFactory));

            DatabaseContext = databaseContext;
            _repositoryFactory = repositoryFactory;
        }

        /// <inheritdoc />
        public DatabaseContext DatabaseContext { get; }

        /// <inheritdoc />
        public IDatabaseUnitOfWork CreateUnitOfWork()
        {
            return new NPocoUnitOfWork(DatabaseContext, _repositoryFactory);
        }
    }
}