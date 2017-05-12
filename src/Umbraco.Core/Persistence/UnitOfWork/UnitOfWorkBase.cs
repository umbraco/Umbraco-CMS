using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    public abstract class UnitOfWorkBase : DisposableObject, IUnitOfWork
    {
        private readonly Queue<Operation> _operations = new Queue<Operation>();

        // fixme - explain readonly
        // it means that the unit of work *will* complete no matter what
        // but if an exception is thrown from within the 'using' block
        // the calling code still can deal with it and cancel everything

        protected UnitOfWorkBase(RepositoryFactory repositoryFactory, bool readOnly = false)
        {
            RepositoryFactory = repositoryFactory;
            ReadOnly = readOnly;
        }

        protected bool ReadOnly { get; }

        protected RepositoryFactory RepositoryFactory { get; }

        public abstract TRepository CreateRepository<TRepository>(string name = null)
            where TRepository : IRepository;

        /// <summary>
        /// Registers an entity to be added as part of this unit of work.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="repository">The repository in charge of the entity.</param>
        public void RegisterCreated(IEntity entity, IUnitOfWorkRepository repository)
        {
            if (ReadOnly)
                throw new NotSupportedException("This unit of work is read-only.");

            Completed = false;

            _operations.Enqueue(new Operation
            {
                Entity = entity,
                Repository = repository,
                Type = OperationType.Insert
            });
        }

        /// <summary>
        /// Registers an entity to be updated as part of this unit of work.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="repository">The repository in charge of the entity.</param>
        public void RegisterUpdated(IEntity entity, IUnitOfWorkRepository repository)
        {
            if (ReadOnly)
                throw new NotSupportedException("This unit of work is read-only.");

            Completed = false;

            _operations.Enqueue(new Operation
            {
                Entity = entity,
                Repository = repository,
                Type = OperationType.Update
            });
        }

        /// <summary>
        /// Registers an entity to be deleted as part of this unit of work.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="repository">The repository in charge of the entity.</param>
        public void RegisterDeleted(IEntity entity, IUnitOfWorkRepository repository)
        {
            if (ReadOnly)
                throw new NotSupportedException("This unit of work is read-only.");

            Completed = false;

            _operations.Enqueue(new Operation
            {
                Entity = entity,
                Repository = repository,
                Type = OperationType.Delete
            });
        }

        // fixme - we don't need Begin, really, or do we?
        public virtual void Begin()
        { }

        public virtual void Flush()
        {
            if (ReadOnly)
                throw new NotSupportedException("This unit of work is read-only.");

            Begin();

            while (_operations.Count > 0)
            {
                var operation = _operations.Dequeue();
                switch (operation.Type)
                {
                    case OperationType.Insert:
                        operation.Repository.PersistNewItem(operation.Entity);
                        break;
                    case OperationType.Delete:
                        operation.Repository.PersistDeletedItem(operation.Entity);
                        break;
                    case OperationType.Update:
                        operation.Repository.PersistUpdatedItem(operation.Entity);
                        break;
                }
            }

            // fixme - if any operation throws, the uow still contains the remaining ops
            // should we clear the ops?
        }

        public virtual void Complete()
        {
            if (ReadOnly)
                throw new NotSupportedException("This unit of work is read-only.");

            if (Completed)
                throw new InvalidOperationException("This unit of work has already been completed.");

            Flush();
            Completed = true;
        }

        protected bool Completed { get; private set; }

        protected override void DisposeResources()
        {
            // whatever hasn't been commited is lost
            // not sure we need this as we are being disposed...
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
            public OperationType Type { get; set; }
        }

        /// <summary>
        /// The types of unit of work operation.
        /// </summary>
        private enum OperationType
        {
            Insert,
            Update,
            Delete
        }
    }
}
