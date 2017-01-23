using System;
using System.Collections.Generic;
using System.Data;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Implements <see cref="IScope"/>.
    /// </summary>
    /// <remarks>Not thread-safe obviously.</remarks>
    internal class Scope : IScope
    {
        private readonly ScopeProvider _scopeProvider;
        private readonly IsolationLevel _isolationLevel;
        private readonly RepositoryCacheMode _repositoryCacheMode;
        private bool _disposed;
        private bool? _completed;

        private IsolatedRuntimeCache _isolatedRuntimeCache;
        private UmbracoDatabase _database;
        private IList<EventMessage> _messages;
        private IDictionary<string, Action<bool>> _exitActions;
        private IEventManager _eventManager;

        // this is v7, in v8 this has to change to RepeatableRead
        private const IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadCommitted;

        // initializes a new scope
        public Scope(ScopeProvider scopeProvider, IsolationLevel isolationLevel = IsolationLevel.Unspecified, RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified, bool detachable = false)
        {
            _scopeProvider = scopeProvider;
            _isolationLevel = isolationLevel;
            _repositoryCacheMode = repositoryCacheMode;
            Detachable = detachable;
#if DEBUG_SCOPES
            _scopeProvider.Register(this);
#endif
        }

        // initializes a new scope in a nested scopes chain, with its parent
        public Scope(ScopeProvider scopeProvider, Scope parent, IsolationLevel isolationLevel = IsolationLevel.Unspecified, RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified)
            : this(scopeProvider, isolationLevel, repositoryCacheMode)
        {
            ParentScope = parent;

            // cannot specify a different mode!
            if (repositoryCacheMode != RepositoryCacheMode.Unspecified && parent.RepositoryCacheMode != repositoryCacheMode)
                throw new ArgumentException("Cannot be different from parent.", "repositoryCacheMode");
        }

        // initializes a new scope, replacing a NoScope instance
        public Scope(ScopeProvider scopeProvider, NoScope noScope, IsolationLevel isolationLevel = IsolationLevel.Unspecified, RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified)
            : this(scopeProvider, isolationLevel, repositoryCacheMode)
        {
            // steal everything from NoScope
            _database = noScope.DatabaseOrNull;
            _messages = noScope.MessagesOrNull;

            // make sure the NoScope can be replaced ie not in a transaction
            if (_database != null && _database.InTransaction)
                    throw new Exception("NoScope instance is not free.");
        }

#if DEBUG_SCOPES
        private readonly Guid _instanceId = Guid.NewGuid();
        public Guid InstanceId { get { return _instanceId; } }
#endif

        /// <inheritdoc />
        public RepositoryCacheMode RepositoryCacheMode
        {
            get
            {
                if (_repositoryCacheMode != RepositoryCacheMode.Unspecified) return _repositoryCacheMode;
                if (ParentScope != null) return ParentScope.RepositoryCacheMode;
                return RepositoryCacheMode.Default;
            }
        }

        /// <inheritdoc />
        public IsolatedRuntimeCache IsolatedRuntimeCache
        {
            get
            {
                if (ParentScope != null) return ParentScope.IsolatedRuntimeCache;

                return _isolatedRuntimeCache ?? (_isolatedRuntimeCache
                    = new IsolatedRuntimeCache(type => new DeepCloneRuntimeCacheProvider(new ObjectCacheRuntimeCacheProvider())));
            }
        }

        // a value indicating whether the scope is detachable
        // ie whether it was created by CreateDetachedScope
        public bool Detachable { get; private set; }

        // the parent scope (in a nested scopes chain)
        public Scope ParentScope { get; set; }

        // the original scope (when attaching a detachable scope)
        public IScope OrigScope { get; set; }

        private IsolationLevel IsolationLevel
        {
            get
            {
                if (_isolationLevel != IsolationLevel.Unspecified) return _isolationLevel;
                if (ParentScope != null) return ParentScope.IsolationLevel;
                return DefaultIsolationLevel;
            }
        }

        /// <inheritdoc />
        public UmbracoDatabase Database
        {
            get
            {
                EnsureNotDisposed();
                if (ParentScope != null)
                {
                    var database = ParentScope.Database;
                    if (_isolationLevel > IsolationLevel.Unspecified && database.CurrentTransactionIsolationLevel < _isolationLevel)
                        throw new Exception("Scope requires isolation level " + _isolationLevel + ", but got " + database.CurrentTransactionIsolationLevel + " from parent.");
                    _database = database;
                }

                if (_database != null)
                {
                    // if the database has been created by a Scope instance it has to be
                    // in a transaction, however it can be a database that was stolen from
                    // a NoScope instance, in which case we need to enter a transaction, as
                    // a scope implies a transaction, always
                    if (_database.InTransaction)
                        return _database;
                }
                else
                {
                    // create a new database
                    _database = _scopeProvider.DatabaseFactory.CreateNewDatabase();
                }

                // enter a transaction, as a scope implies a transaction, always
                try
                {
                    _database.BeginTransaction(IsolationLevel);
                    return _database;
                }
                catch
                {
                    _database.Dispose();
                    _database = null;
                    throw;
                }
            }
        }

        //public UmbracoDatabase DatabaseOrNull
        //{
        //    get
        //    {
        //        EnsureNotDisposed();
        //        return ParentScope == null ? _database : ParentScope.DatabaseOrNull;
        //    }
        //}

        /// <inheritdoc />
        public IList<EventMessage> Messages
        {
            get
            {
                EnsureNotDisposed();
                if (ParentScope != null) return ParentScope.Messages;
                return _messages ?? (_messages = new List<EventMessage>());
            }
        }

        //public IList<EventMessage> MessagesOrNull
        //{
        //    get
        //    {
        //        EnsureNotDisposed();
        //        return ParentScope == null ? _messages : ParentScope.MessagesOrNull;
        //    }
        //}

        /// <inheritdoc />
        public IEventManager Events
        {
            get
            {
                EnsureNotDisposed();
                if (ParentScope != null) return ParentScope.Events;
                return _eventManager ?? (_eventManager = new ScopedEventManager());
            }
        }

        /// <inheritdoc />
        public void Complete()
        {
            if (_completed.HasValue == false)
                _completed = true;
        }

        public void Reset()
        {
            _completed = null;
        }

        public void ChildCompleted(bool? completed)
        {
            // if child did not complete we cannot complete
            if (completed.HasValue == false || completed.Value == false)
                _completed = false;
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("this");
        }

        public void Dispose()
        {
            EnsureNotDisposed();

            if (this != _scopeProvider.AmbientScope)
                throw new InvalidOperationException("Not the ambient scope.");

#if DEBUG_SCOPES
            _scopeProvider.Disposed(this);
#endif

            var parent = ParentScope;
            _scopeProvider.AmbientScope = parent;

            if (parent != null)
                parent.ChildCompleted(_completed);
            else
                DisposeLastScope();

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private void DisposeLastScope()
        {
            // note - messages
            // at the moment we are totally not filtering the messages based on completion
            // status, so whether the scope is committed or rolled back makes no difference

            var completed = _completed.HasValue && _completed.Value;

            if (_database != null)
            {
                try
                {
                    if (completed)
                        _database.CompleteTransaction();
                    else
                        _database.AbortTransaction();
                }
                finally
                {
                    _database.Dispose();
                    _database = null;
                }
            }

            // run everything we need to run when completing
            List<Exception> exceptions = null;
            foreach (var action in ExitActions.Values)
            {
                try
                {
                    action(completed);
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                        exceptions = new List<Exception>();
                    exceptions.Add(e);
                }
            }
            if (exceptions != null)
                throw new AggregateException("Exceptions were throws by complete actions.", exceptions);

            // now trigger every events
            // fixme - should some of them trigger within the transaction?
            if (_eventManager != null)
                _eventManager.Dispose();
        }

        private IDictionary<string, Action<bool>> ExitActions
        {
            get
            {
                if (ParentScope != null) return ParentScope.ExitActions;

                return _exitActions ?? (_exitActions
                    = new Dictionary<string, Action<bool>>());
            }
        }

        /// <inheritdoc />
        public void OnExit(string key, Action action)
        {
            ExitActions[key] = completed => { if (completed) action(); };
        }

        /// <inheritdoc />
        public void OnExit(string key, Action<bool> action)
        {
            ExitActions[key] = action;
        }

        public T Enlist<T>(string key, Func<T> creator, Action<ActionTime, bool, T> action)
        {
            return creator();
        }

        public enum ActionTime
        {
            BeforeCommit,
            BeforeEvent,
            BeforeDispose
        }
    }
}
