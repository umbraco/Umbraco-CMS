using System;
using System.Data;
using NPoco;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Implements IDatabaseUnitOfWork for NPoco.
    /// </summary>
    internal class NPocoUnitOfWork : UnitOfWorkBase, IDatabaseUnitOfWork
    {
        private ITransaction _transaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="NPocoUnitOfWork"/> class with a database and a repository factory.
        /// </summary>
        /// <param name="database">A database.</param>
        /// <param name="factory">A repository factory.</param>
        /// <remarks>This should be used by the NPocoUnitOfWorkProvider exclusively.</remarks>
        internal NPocoUnitOfWork(UmbracoDatabase database, RepositoryFactory factory)
            : base(factory)
        {
            Database = database;
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
        public override TRepository CreateRepository<TRepository>(string name = null)
        {
            return Factory.CreateRepository<TRepository>(this, name);
        }

        /// <summary>
        /// Ensures that we have a transaction.
        /// </summary>
        /// <remarks>Isolation level is determined by the database, see UmbracoDatabase.DefaultIsolationLevel. Should be
        /// at least IsolationLevel.RepeatablRead else the node locks will not work correctly.</remarks>
        public override void Begin()
        {
            base.Begin();

            if (_transaction == null)
                _transaction = Database.GetTransaction();
        }

        protected override void DisposeResources()
        {
            base.DisposeResources();

            // no transaction, nothing to do
            if (_transaction == null) return;

            // will either complete or abort NPoco transaction
            // which means going one level up in the transaction stack
            // and commit or rollback only if at top of stack
            if (Completed)
                _transaction.Complete(); // complete the transaction
            else
                _transaction.Dispose(); // abort the transaction

            _transaction = null;
        }

        public void ReadLock(params int[] lockIds)
        {
            Begin(); // we need a transaction

            if (Database.Transaction.IsolationLevel < IsolationLevel.RepeatableRead)
                throw new InvalidOperationException("A transaction with minimum RepeatableRead isolation level is required.");
            // *not* using a unique 'WHERE IN' query here because the *order* of lockIds is important to avoid deadlocks
            foreach (var lockId in lockIds)
            {
                var i = Database.ExecuteScalar<int?>("SELECT value FROM umbracoLock WHERE id=@id",
                    new { @id = lockId });
                if (i == null) // ensure we are actually locking!
                    throw new Exception($"LockObject with id={lockId} does not exist.");
            }
        }

        public void WriteLock(params int[] lockIds)
        {
            Begin(); // we need a transaction

            if (Database.Transaction.IsolationLevel < IsolationLevel.RepeatableRead)
                throw new InvalidOperationException("A transaction with minimum RepeatableRead isolation level is required.");
            // *not* using a unique 'WHERE IN' query here because the *order* of lockIds is important to avoid deadlocks
            foreach (var lockId in lockIds)
            {
                var i = Database.Execute("UPDATE umbracoLock SET value = (CASE WHEN (value=1) THEN -1 ELSE 1 END) WHERE id=@id",
                    new { @id = lockId });
                if (i == 0) // ensure we are actually locking!
                    throw new Exception($"LockObject with id={lockId} does not exist.");
            }
        }
    }
}