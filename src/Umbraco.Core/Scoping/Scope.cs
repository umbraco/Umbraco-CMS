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
        private readonly EventsDispatchMode _dispatchMode;
        private readonly ScopeContext _scopeContext;
        private bool _disposed;
        private bool? _completed;

        private IsolatedRuntimeCache _isolatedRuntimeCache;
        private UmbracoDatabase _database;
        private IEventDispatcher _eventDispatcher;        

        // this is v7, in v8 this has to change to RepeatableRead
        private const IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadCommitted;

        // initializes a new scope
        public Scope(ScopeProvider scopeProvider,
            ScopeContext scopeContext,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            EventsDispatchMode dispatchMode = EventsDispatchMode.Unspecified,
            bool detachable = false)
        {
            _scopeProvider = scopeProvider;
            _scopeContext = scopeContext;
            _isolationLevel = isolationLevel;
            _repositoryCacheMode = repositoryCacheMode;
            _dispatchMode = dispatchMode;
            Detachable = detachable;
#if DEBUG_SCOPES
            _scopeProvider.Register(this);
            Console.WriteLine("create " + _instanceId.ToString("N").Substring(0, 8));
#endif
        }

        // initializes a new scope in a nested scopes chain, with its parent
        public Scope(ScopeProvider scopeProvider, Scope parent,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified, 
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            EventsDispatchMode dispatchMode = EventsDispatchMode.Unspecified)
            : this(scopeProvider, (ScopeContext) null, isolationLevel, repositoryCacheMode, dispatchMode)
        {
            ParentScope = parent;

            // cannot specify a different mode!
            if (repositoryCacheMode != RepositoryCacheMode.Unspecified && parent.RepositoryCacheMode != repositoryCacheMode)
                throw new ArgumentException("Cannot be different from parent.", "repositoryCacheMode");

            // cannot specify a different mode!
            if (_dispatchMode != EventsDispatchMode.Unspecified && parent._dispatchMode != dispatchMode)
                throw new ArgumentException("Cannot be different from parent.", "dispatchMode");
        }

        // initializes a new scope, replacing a NoScope instance
        public Scope(ScopeProvider scopeProvider, NoScope noScope,
            ScopeContext scopeContext,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified, 
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            EventsDispatchMode dispatchMode = EventsDispatchMode.Unspecified)
            : this(scopeProvider, scopeContext, isolationLevel, repositoryCacheMode, dispatchMode)
        {
            // steal everything from NoScope
            _database = noScope.DatabaseOrNull;

            // make sure the NoScope can be replaced ie not in a transaction
            if (_database != null && _database.InTransaction)
                    throw new Exception("NoScope instance is not free.");
        }

#if DEBUG_SCOPES
        private readonly Guid _instanceId = Guid.NewGuid();
        public Guid InstanceId { get { return _instanceId; } }
#endif

        private EventsDispatchMode DispatchMode
        {
            get
            {
                if (_dispatchMode != EventsDispatchMode.Unspecified) return _dispatchMode;
                if (ParentScope != null) return ParentScope.DispatchMode;
                return EventsDispatchMode.Scope;
            }
        }

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

        public UmbracoDatabase DatabaseOrNull
        {
            get
            {
                EnsureNotDisposed();
                return ParentScope == null ? _database : ParentScope.DatabaseOrNull;
            }
        }

        /// <inheritdoc />
        public EventMessages Messages
        {
            get
            {
                EnsureNotDisposed();
                if (ParentScope != null) return ParentScope.Messages;
                //return _messages ?? (_messages = new EventMessages());

                // ok, this isn't pretty, but it works
                // TODO kill the message factory and let the scope manage it all
                return ApplicationContext.Current.Services.EventMessagesFactory.Get();
            }
        }

        public EventMessages MessagesOrNull
        {
            get
            {
                EnsureNotDisposed();
                return ParentScope == null ? null : ParentScope.MessagesOrNull;
            }
        }

        /// <inheritdoc />
        public IEventDispatcher Events
        {
            get
            {
                EnsureNotDisposed();
                if (ParentScope != null) return ParentScope.Events;
                return _eventDispatcher ?? (_eventDispatcher = new ScopeEventDispatcher(DispatchMode));
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

            // deal with events
            if (_eventDispatcher != null)
                _eventDispatcher.ScopeExit(completed);

            // if *we* created it, then get rid of it
            if (_scopeProvider.AmbientContext == _scopeContext)
            {
                try
                {
                    _scopeProvider.AmbientContext.ScopeExit(completed);
                }
                finally
                {
                    _scopeProvider.AmbientContext = null;
                }
            }
        }
    }
}
