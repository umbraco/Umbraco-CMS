using System;
using LightInject;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Instanciates repositories.
    /// </summary>
    public class RepositoryFactory
    {
        private readonly IServiceContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryFactory"/> class with a container.
        /// </summary>
        /// <param name="container">A container.</param>
        public RepositoryFactory(IServiceContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            _container = container;
        }

        /// <summary>
        /// Creates a repository.
        /// </summary>
        /// <typeparam name="TRepository">The type of the repository.</typeparam>
        /// <param name="uow">A unit of work.</param>
        /// <param name="name">The optional name of the repository.</param>
        /// <returns>The created repository for the unit of work.</returns>
        public virtual TRepository CreateRepository<TRepository>(IDatabaseUnitOfWork uow, string name = null)
            where TRepository : IRepository
        {
            return string.IsNullOrWhiteSpace(name)
                ? _container.GetInstance<IDatabaseUnitOfWork, TRepository>(uow)
                : _container.GetInstance<IDatabaseUnitOfWork, TRepository>(uow, name);
        }

        /// <summary>
        /// Creates a repository.
        /// </summary>
        /// <typeparam name="TRepository">The type of the repository.</typeparam>
        /// <param name="uow">A unit of work.</param>
        /// <param name="name">The optional name of the repository.</param>
        /// <returns>The created repository for the unit of work.</returns>
        internal virtual TRepository CreateRepository<TRepository>(FileUnitOfWork uow, string name = null)
            where TRepository : IRepository
        {
            return string.IsNullOrWhiteSpace(name)
                ? _container.GetInstance<FileUnitOfWork, TRepository>(uow)
                : _container.GetInstance<FileUnitOfWork, TRepository>(uow, name);
        }
    }
}