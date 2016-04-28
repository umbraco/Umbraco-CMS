using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents the Unit of Work implementation for working with files
    /// </summary>
    internal class FileUnitOfWork : DisposableObject, IUnitOfWork
    {
        private readonly Queue<Operation> _operations = new Queue<Operation>();
        private readonly RepositoryFactory _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NPocoUnitOfWork"/> class with a a repository factory.
        /// </summary>
        /// <param name="factory">A repository factory.</param>
        /// <remarks>This should be used by the FileUnitOfWorkProvider exclusively.</remarks>
        public FileUnitOfWork(RepositoryFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Creates a repository.
        /// </summary>
        /// <typeparam name="TRepository">The type of the repository.</typeparam>
        /// <param name="name">The optional name of the repository.</param>
        /// <returns>The created repository for the unit of work.</returns>
	    public TRepository CreateRepository<TRepository>(string name = null)
            where TRepository : IRepository
        {
            return _factory.CreateRepository<TRepository>(this, name);
        }

        /// <summary>
        /// Registers an <see cref="IEntity" /> instance to be added through this <see cref="IUnitOfWork" />.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="repository">The repository participating in the transaction.</param>
        public void RegisterAdded(IEntity entity, IUnitOfWorkRepository repository)
        {
            _operations.Enqueue(new Operation
            {
                Entity = entity,
                Repository = repository,
                Type = TransactionType.Insert
            });
        }

        /// <summary>
        /// Registers an <see cref="IEntity" /> instance to be changed through this <see cref="IUnitOfWork" />.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="repository">The repository participating in the transaction.</param>
        public void RegisterChanged(IEntity entity, IUnitOfWorkRepository repository)
        {
            _operations.Enqueue(new Operation
            {
                Entity = entity,
                Repository = repository,
                Type = TransactionType.Update
            });
        }

        /// <summary>
        /// Registers an <see cref="IEntity" /> instance to be removed through this <see cref="IUnitOfWork" />.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="repository">The repository participating in the transaction.</param>
        public void RegisterRemoved(IEntity entity, IUnitOfWorkRepository repository)
        {
            _operations.Enqueue(new Operation
            {
                Entity = entity,
                Repository = repository,
                Type = TransactionType.Delete
            });
        }

        public void Commit()
        {
            //NOTE: I'm leaving this in here for reference, but this is useless, transaction scope + Files doesn't do anything,
            // the closest you can get is transactional NTFS, but that requires distributed transaction coordinator and some other libs/wrappers,
            // plus MS has not deprecated it anyways. To do transactional IO we'd have to write this ourselves using temporary files and then 
            // on committing move them to their correct place.
            //using(var scope = new TransactionScope())
            //{
            //    // Commit the transaction
            //    scope.Complete();
            //}

            while (_operations.Count > 0)
            {
                var operation = _operations.Dequeue();
                switch (operation.Type)
                {
                    case TransactionType.Insert:
                        operation.Repository.PersistNewItem(operation.Entity);
                        break;
                    case TransactionType.Delete:
                        operation.Repository.PersistDeletedItem(operation.Entity);
                        break;
                    case TransactionType.Update:
                        operation.Repository.PersistUpdatedItem(operation.Entity);
                        break;
                }
            }

            // clear everything
            // fixme - why? everything should have been dequeued?
            _operations.Clear();
        }

        protected override void DisposeResources()
        {
            _operations.Clear();
        }

        /// <summary>
        /// Provides a snapshot of an entity and the repository reference it belongs to.
        /// </summary>
        private sealed class Operation
        {
            /// <summary>
            /// Gets or sets the entity.
            /// </summary>
            /// <value>The entity.</value>
            public IEntity Entity { get; set; }

            /// <summary>
            /// Gets or sets the repository.
            /// </summary>
            /// <value>The repository.</value>
            public IUnitOfWorkRepository Repository { get; set; }

            /// <summary>
            /// Gets or sets the type of operation.
            /// </summary>
            /// <value>The type of operation.</value>
            public TransactionType Type { get; set; }
        }
    }
}