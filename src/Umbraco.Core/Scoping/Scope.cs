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
        private EventMessages _messages;
        private IDictionary<string, IEnlistedObject> _enlisted;
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
                return ParentScope == null ? _messages : ParentScope.MessagesOrNull;
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
            // note - messages
            // at the moment we are totally not filtering the messages based on completion
            // status, so whether the scope is committed or rolled back makes no difference

            // note - scope
            // at that point, there is *no* ambient scope anymore, which means that every
            // enlisted action, triggered event, *anything*, is not scoped and is free to
            // do whatever needed.
            //
            // fixme - what does this mean for XML scope?!

            var completed = _completed.HasValue && _completed.Value;

            // run enlisted actions
            RunEnlisted(ActionTime.BeforeCommit, completed);

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

            // run enlisted actions
            RunEnlisted(ActionTime.BeforeEvents, completed);

            // deal with events
            if (_eventDispatcher != null)
                _eventDispatcher.ScopeExit(completed);

            // run enlisted actions
            RunEnlisted(ActionTime.BeforeDispose, completed);

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

        private void RunEnlisted(ActionTime actionTime, bool completed)
        {
            List<Exception> exceptions = null;
            foreach (var enlisted in Enlisted.Values)
            {
                try
                {
                    enlisted.Execute(actionTime, completed);
                }
                catch (Exception e)
                {
                    if (exceptions == null)
                        exceptions = new List<Exception>();
                    exceptions.Add(e);
                }
            }
            if (exceptions != null)
                throw new AggregateException("Exceptions were thrown by listed actions at ActionTime " + actionTime + ".", exceptions);
        }

        private IDictionary<string, IEnlistedObject> Enlisted
        {
            get
            {
                if (ParentScope != null) return ParentScope.Enlisted;

                return _enlisted ?? (_enlisted
                    = new Dictionary<string, IEnlistedObject>());
            }
        }

        private interface IEnlistedObject
        {
            void Execute(ActionTime actionTime, bool completed);
        }

        private class EnlistedObject<T> : IEnlistedObject
        {
            private readonly ActionTime _actionTimes;
            private readonly Action<ActionTime, bool, T> _action;

            public EnlistedObject(T item)
            {
                Item = item;
                _actionTimes = ActionTime.None;
            }

            public EnlistedObject(T item, ActionTime actionTimes, Action<ActionTime, bool, T> action)
            {
                Item = item;
                _actionTimes = actionTimes;
                _action = action;
            }

            public T Item { get; private set; }

            public void Execute(ActionTime actionTime, bool completed)
            {
                if (_actionTimes.HasFlag(actionTime))
                    _action(actionTime, completed, Item);
            }
        }

        /// <inheritdoc />
        public T Enlist<T>(string key, Func<T> creator)
        {
            IEnlistedObject enlisted;
            if (Enlisted.TryGetValue(key, out enlisted))
            {
                var enlistedAs = enlisted as EnlistedObject<T>;
                if (enlistedAs == null) throw new Exception("An item with a different type has already been enlisted with the same key.");
                return enlistedAs.Item;
            }
            var enlistedOfT = new EnlistedObject<T>(creator());
            Enlisted[key] = enlistedOfT;
            return enlistedOfT.Item;
        }

        /// <inheritdoc />
        public T Enlist<T>(string key, Func<T> creator, ActionTime actionTimes, Action<ActionTime, bool, T> action)
        {
            IEnlistedObject enlisted;
            if (Enlisted.TryGetValue(key, out enlisted))
            {
                var enlistedAs = enlisted as EnlistedObject<T>;
                if (enlistedAs == null) throw new Exception("An item with a different type has already been enlisted with the same key.");
                return enlistedAs.Item;
            }
            var enlistedOfT = new EnlistedObject<T>(creator(), actionTimes, action);
            Enlisted[key] = enlistedOfT;
            return enlistedOfT.Item;
        }

        /// <inheritdoc />
        public void Enlist(string key, ActionTime actionTimes, Action<ActionTime, bool> action)
        {
            IEnlistedObject enlisted;
            if (Enlisted.TryGetValue(key, out enlisted))
            {
                var enlistedAs = enlisted as EnlistedObject<object>;
                if (enlistedAs == null) throw new Exception("An item with a different type has already been enlisted with the same key.");
                return;
            }
            var enlistedOfT = new EnlistedObject<object>(null, actionTimes, (actionTime, completed, item) => action(actionTime, completed));
            Enlisted[key] = enlistedOfT;
        }
    }
}
