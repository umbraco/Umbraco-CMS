using System;
using System.Data;
using Umbraco.Core.Events;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a scoped unit of work.
    /// </summary>
    internal class ScopeUnitOfWork : UnitOfWorkBase, IScopeUnitOfWork
    {
        private readonly IsolationLevel _isolationLevel;
        private readonly IScopeProvider _scopeProvider;
        private bool _completeScope;
        private IScope _scope;
        private Guid _key;

        /// <summary>
        /// Used for testing
        /// </summary>
        internal Guid InstanceId { get; }

        /// <summary>
        /// Creates a new unit of work instance
        /// </summary>
        /// <param name="scopeProvider"></param>
        /// <param name="databaseContext"></param>
        /// <param name="repositoryFactory"></param>
        /// <param name="isolationLevel"></param>
        /// <param name="readOnly"></param>
        /// <param name="immediate"></param>
        /// <remarks>
        /// This should normally not be used directly and should be created with the UnitOfWorkProvider
        /// </remarks>
        internal ScopeUnitOfWork(IScopeProvider scopeProvider, ISqlContext sqlContext, RepositoryFactory repositoryFactory, IsolationLevel isolationLevel = IsolationLevel.Unspecified, bool readOnly = false, bool immediate = false)
            : base(repositoryFactory, readOnly, immediate)
        {
            _scopeProvider = scopeProvider;
            SqlContext = sqlContext;
            _isolationLevel = isolationLevel;

            // fixme only 1!
            _key = Guid.NewGuid();
            InstanceId = Guid.NewGuid();

            // be false by default
            // if set to true... the UnitOfWork is "auto-commit" which means that even in the case of
            // an exception, the scope would still be completed - ppl should use it with great care!
            _completeScope = readOnly;
        }

        #region IDatabaseContext

        /// <inheritdoc />
        public ISqlContext SqlContext { get; }

        #endregion

        public override TRepository CreateRepository<TRepository>(string name = null)
        {
            return RepositoryFactory.CreateRepository<TRepository>(this, name);
        }

        public override void Begin()
        {
            base.Begin();

            // soon as we get Database, a transaction is started
            var unused = Database;
        }

        /// <inheritdoc />
        public void ReadLock(params int[] lockIds)
        {
            Scope.ReadLock(lockIds);
        }

        /// <inheritdoc />
        public void WriteLock(params int[] lockIds)
        {
            if (ReadOnly)
                throw new NotSupportedException("This unit of work is read-only.");

            Scope.WriteLock(lockIds);
        }

        public override void Complete()
        {
            base.Complete();
            _completeScope = true;
            _key = Guid.NewGuid(); // fixme kill!
        }

        public object Key => _key;

        // fixme v8
        // once we are absolutely sure that our UOW cannot be disposed more than once,
        // this should throw if the UOW has already been disposed, NOT recreate a scope!
        public IScope Scope => _scope ?? (_scope = _scopeProvider.CreateScope(_isolationLevel));

        public IUmbracoDatabase Database => Scope.Database;

        public EventMessages Messages => Scope.Messages;

        public IEventDispatcher Events => Scope.Events;

        /// <summary>
        /// Ensures disposable objects are disposed
        /// </summary>
        /// <remarks>
        /// Ensures that the Transaction instance is disposed of
        /// </remarks>
        protected override void DisposeResources()
        {
            // base deals with the operation's queue
            base.DisposeResources();

            if (_scope == null) return;
            if (_completeScope) _scope.Complete();
            _scope.Dispose();
            _scope = null;
        }
    }
}
