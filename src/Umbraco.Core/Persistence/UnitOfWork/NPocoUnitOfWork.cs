using System;
using System.Collections.Generic;
using System.Data;
using LightInject;
using NPoco;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Core.Persistence.UnitOfWork
{
	/// <summary>
	/// Represents the Unit of Work implementation for NPoco.
	/// </summary>
	internal class NPocoUnitOfWork : DisposableObject, IDatabaseUnitOfWork
	{
        private readonly Queue<Operation> _operations = new Queue<Operation>();
        private readonly RepositoryFactory _factory;
        private ITransaction _transaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="NPocoUnitOfWork"/> class with a database and a repository factory.
        /// </summary>
        /// <param name="database">A database.</param>
        /// <param name="factory">A repository factory.</param>
        /// <remarks>This should be used by the NPocoUnitOfWorkProvider exclusively.</remarks>
        internal NPocoUnitOfWork(UmbracoDatabase database, RepositoryFactory factory)
		{
			Database = database;
            _factory = factory;
		}

        /// <summary>
        /// Gets the unit of work underlying database.
        /// </summary>
        public UmbracoDatabase Database { get; }

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

        /// <summary>
        ///  Ensures that we have a transaction.
        /// </summary>
        /// <remarks>Isolation level is determined by the database, see UmbracoDatabase.DefaultIsolationLevel. Should be
        /// at least IsolationLevel.RepeatablRead else the node locks will not work correctly.</remarks>
        public void EnsureTransaction()
	    {
	        if (_transaction == null)
	            _transaction = Database.GetTransaction();
	    }

		/// <summary>
		/// Commits all batched changes.
		/// </summary>
		/// <remarks>
		/// Unlike a typical unit of work, this UOW will let you commit more than once since a new transaction is creaed per
		/// Commit() call instead of having one transaction per UOW. 
		/// </remarks>
		public void Commit()
		{
		    Commit(null);
		}

        /// <summary>
        /// Commits all batched changes.
        /// </summary>
        /// <param name="completing">A callback which is executed after the operations have been processed and
        /// before the transaction is completed.</param>
        internal void Commit(Action<UmbracoDatabase> completing)
        {
            EnsureTransaction();

            try
            {
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

                // execute the callback if there is one
                completing?.Invoke(Database);
                _transaction.Complete();
            }
            finally
            {
                _transaction.Dispose(); // will rollback if not completed
                _transaction = null;
            }

            // clear everything
            // in case the Dequeue loop was aborted
            // fixme - but the, exception and this is never reached?
            _operations.Clear();
        }

        protected override void DisposeResources()
        {
            _operations.Clear();

            if (_transaction == null) return;
            _transaction.Dispose();
            _transaction = null;
        }

	    public void ReadLockNodes(params int[] lockIds)
	    {
	        EnsureTransaction();
            if (Database.Transaction.IsolationLevel < IsolationLevel.RepeatableRead)
                throw new InvalidOperationException("A transaction with minimum RepeatableRead isolation level is required.");
            foreach (var lockId in lockIds)
                Database.ExecuteScalar<int>("SELECT sortOrder FROM umbracoNode WHERE id=@id",
                    new { @id = lockId });
        }

        public void WriteLockNodes(params int[] lockIds)
	    {
            EnsureTransaction();
            if (Database.Transaction.IsolationLevel < IsolationLevel.RepeatableRead)
                throw new InvalidOperationException("A transaction with minimum RepeatableRead isolation level is required.");
            foreach (var lockId in lockIds)
                Database.Execute("UPDATE umbracoNode SET sortOrder = (CASE WHEN (sortOrder=1) THEN -1 ELSE 1 END) WHERE id=@id",
                    new { @id = lockId });
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