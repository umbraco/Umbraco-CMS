using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents the Unit of Work implementation for working with files
    /// </summary>
    internal class FileUnitOfWork : IUnitOfWork
    {
        private Guid _key;
        private readonly List<Operation> _operations = new List<Operation>();

        public FileUnitOfWork()
        {
            _key = Guid.NewGuid();
        }

        #region Implementation of IUnitOfWork

        /// <summary>
        /// Registers an <see cref="IEntity" /> instance to be added through this <see cref="UnitOfWork" />
        /// </summary>
        /// <param name="entity">The <see cref="IEntity" /></param>
        /// <param name="repository">The <see cref="IUnitOfWorkRepository" /> participating in the transaction</param>
        public void RegisterAdded(IEntity entity, IUnitOfWorkRepository repository)
        {
            _operations.Add(
                new Operation
                {
                    Entity = entity,
                    ProcessDate = DateTime.Now,
                    Repository = repository,
                    Type = TransactionType.Insert
                });
        }

        /// <summary>
        /// Registers an <see cref="IEntity" /> instance to be changed through this <see cref="UnitOfWork" />
        /// </summary>
        /// <param name="entity">The <see cref="IEntity" /></param>
        /// <param name="repository">The <see cref="IUnitOfWorkRepository" /> participating in the transaction</param>
        public void RegisterChanged(IEntity entity, IUnitOfWorkRepository repository)
        {
            _operations.Add(
                new Operation
                {
                    Entity = entity,
                    ProcessDate = DateTime.Now,
                    Repository = repository,
                    Type = TransactionType.Update
                });
        }

        /// <summary>
        /// Registers an <see cref="IEntity" /> instance to be removed through this <see cref="UnitOfWork" />
        /// </summary>
        /// <param name="entity">The <see cref="IEntity" /></param>
        /// <param name="repository">The <see cref="IUnitOfWorkRepository" /> participating in the transaction</param>
        public void RegisterRemoved(IEntity entity, IUnitOfWorkRepository repository)
        {
            _operations.Add(
                new Operation
                {
                    Entity = entity,
                    ProcessDate = DateTime.Now,
                    Repository = repository,
                    Type = TransactionType.Delete
                });
        }

        public void Commit()
        {
            using(var scope = new TransactionScope())
            {
                foreach (var operation in _operations.OrderBy(o => o.ProcessDate))
                {
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
                // Commit the transaction
                scope.Complete();
            }

            // Clear everything
            _operations.Clear();
            _key = Guid.NewGuid();
        }

        public object Key
        {
            get { return _key; }
        }

        #endregion

        #region Operation

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
            /// Gets or sets the process date.
            /// </summary>
            /// <value>The process date.</value>
            public DateTime ProcessDate { get; set; }

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

        #endregion

    }
}