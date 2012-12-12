using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents the Unit of Work implementation for PetaPoco
    /// </summary>
	internal class PetaPocoUnitOfWork : DisposableObject, IDatabaseUnitOfWork
    {

	    /// <summary>
	    /// Used for testing
	    /// </summary>
		internal Guid InstanceId { get; private set; }

        private Guid _key;
        private readonly List<Operation> _operations = new List<Operation>();


		/// <summary>
		/// Creates a new unit of work instance
		/// </summary>
		/// <param name="database"></param>
		/// <remarks>
		/// The Database instance used for this unit of work should not be shared with other unit's of work, other repositories, etc...
		/// as it will get disposed of when this unit of work is disposed.
		/// </remarks>
		public PetaPocoUnitOfWork(UmbracoDatabase database)
        {
	        Database = database;
	        _key = Guid.NewGuid();
	        InstanceId = Guid.NewGuid();
        }	    

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

        /// <summary>
        /// Commits all batched changes within the scope of a PetaPoco transaction <see cref="Transaction"/>
        /// </summary>
        public void Commit()
        {
			using(Transaction transaction = Database.GetTransaction())			
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
                transaction.Complete();
            }

            // Clear everything
            _operations.Clear();
            _key = Guid.NewGuid();
        }

        public object Key
        {
            get { return _key; }
        }

		public UmbracoDatabase Database { get; private set; }

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

		/// <summary>
		/// Ensures disposable objects are disposed
		/// </summary>		
		/// <remarks>
		/// Ensures that the Database instance is disposed of. As per the constructor documentation, the database instance
		/// used to construct this object should not be shared as it will get disposed of when this object is disposed.
		/// </remarks>
	    protected override void DisposeResources()
	    {
			_operations.Clear();			
			Database.Dispose();
	    }
    }
}