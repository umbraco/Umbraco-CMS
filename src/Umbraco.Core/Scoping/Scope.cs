using System;
using System.Data;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Implements <see cref="IScope"/>.
    /// </summary>
    /// <remarks>Not thread-safe obviously.</remarks>
    internal class Scope : IScopeInternal
    {
        private readonly ScopeProvider _scopeProvider;
        private readonly IsolationLevel _isolationLevel;
        private readonly RepositoryCacheMode _repositoryCacheMode;
        private readonly bool? _scopeFileSystem;
        private readonly ScopeContext _scopeContext;
        private bool _callContext;
        private bool _disposed;
        private bool? _completed;

        private IsolatedRuntimeCache _isolatedRuntimeCache;
        private UmbracoDatabase _database;
        private EventMessages _messages;
        private ICompletable _fscope;
        private IEventDispatcher _eventDispatcher;

        // this is v7, in v8 this has to change to RepeatableRead
        private const IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadCommitted;

        // initializes a new scope
        private Scope(ScopeProvider scopeProvider,
            Scope parent, ScopeContext scopeContext, bool detachable,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            bool? scopeFileSystems = null,
            bool callContext = false)
        {
            _scopeProvider = scopeProvider;
            _scopeContext = scopeContext;
            _isolationLevel = isolationLevel;
            _repositoryCacheMode = repositoryCacheMode;
            _eventDispatcher = eventDispatcher;
            _scopeFileSystem = scopeFileSystems;
            _callContext = callContext;
            Detachable = detachable;

#if DEBUG_SCOPES
            _scopeProvider.RegisterScope(this);
            Console.WriteLine("create " + _instanceId.ToString("N").Substring(0, 8));
#endif

            if (detachable)
            {
                if (parent != null) throw new ArgumentException("Cannot set parent on detachable scope.", "parent");
                if (scopeContext != null) throw new ArgumentException("Cannot set context on detachable scope.", "scopeContext");

                // detachable creates its own scope context
                _scopeContext = new ScopeContext();

                // see note below
                if (scopeFileSystems == true)
                    _fscope = FileSystemProviderManager.Current.Shadow(Guid.NewGuid());

                return;
            }

            if (parent != null)
            {
                ParentScope = parent;

                // cannot specify a different mode!
                if (repositoryCacheMode != RepositoryCacheMode.Unspecified && parent.RepositoryCacheMode != repositoryCacheMode)
                    throw new ArgumentException("Cannot be different from parent.", "repositoryCacheMode");

                // cannot specify a dispatcher!
                if (_eventDispatcher != null)
                    throw new ArgumentException("Cannot be specified on nested scope.", "eventDispatcher");

                // cannot specify a different fs scope!
                if (scopeFileSystems != null && parent._scopeFileSystem != scopeFileSystems)
                    throw new ArgumentException("Cannot be different from parent.", "scopeFileSystems");
            }
            else
            {
                // the FS scope cannot be "on demand" like the rest, because we would need to hook into
                // every scoped FS to trigger the creation of shadow FS "on demand", and that would be
                // pretty pointless since if scopeFileSystems is true, we *know* we want to shadow
                if (scopeFileSystems == true)
                    _fscope = FileSystemProviderManager.Current.Shadow(Guid.NewGuid());
            }
        }

        // initializes a new scope
        public Scope(ScopeProvider scopeProvider, bool detachable,
            ScopeContext scopeContext,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            bool? scopeFileSystems = null,
            bool callContext = false)
            : this(scopeProvider, null, scopeContext, detachable, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems, callContext)
        { }

        // initializes a new scope in a nested scopes chain, with its parent
        public Scope(ScopeProvider scopeProvider, Scope parent,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            bool? scopeFileSystems = null,
            bool callContext = false)
            : this(scopeProvider, parent, null, false, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems, callContext)
        { }

        // initializes a new scope, replacing a NoScope instance
        public Scope(ScopeProvider scopeProvider, NoScope noScope,
            ScopeContext scopeContext,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            bool? scopeFileSystems = null,
            bool callContext = false)
            : this(scopeProvider, null, scopeContext, false, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems, callContext)
        {
            // steal everything from NoScope
            _database = noScope.DatabaseOrNull;
            _messages = noScope.MessagesOrNull;

            // make sure the NoScope can be replaced ie not in a transaction
            if (_database != null && _database.InTransaction)
                throw new Exception("NoScope instance is not free.");
        }

        private readonly Guid _instanceId = Guid.NewGuid();
        public Guid InstanceId { get { return _instanceId; } }

        // a value indicating whether to force call-context
        public bool CallContext
        {
            get
            {
                if (_callContext) return true;
                if (ParentScope != null) return ParentScope.CallContext;
                return false;
            }
            set { _callContext = value; }
        }

        public bool ScopedFileSystems
        {
            get
            {
                if (ParentScope != null) return ParentScope.ScopedFileSystems;
                return _fscope != null;
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
        public IScopeInternal ParentScope { get; set; }

        public bool Attached { get; set; }

        // the original scope (when attaching a detachable scope)
        public IScopeInternal OrigScope { get; set; }

        // the original context (when attaching a detachable scope)
        public ScopeContext OrigContext { get; set; }

        // the context (for attaching & detaching only)
        public ScopeContext Context
        {
            get { return _scopeContext; }
        }

        public IsolationLevel IsolationLevel
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

                if (_messages != null) return _messages;

                // this is ugly - in v7 for backward compatibility reasons, EventMessages need
                // to survive way longer that the scopes, and kinda resides on its own in http
                // context, but must also be in scopes for when we do async and lose http context
                // TODO refactor in v8

                var factory = ScopeLifespanMessagesFactory.Current;
                if (factory == null)
                {
                    _messages = new EventMessages();
                }
                else
                {
                    _messages = factory.GetFromHttpContext();
                    if (_messages == null)
                        factory.Set(_messages = new EventMessages());
                }

                return _messages;
            }
        }

        public EventMessages MessagesOrNull
        {
            get
            {
                EnsureNotDisposed();
                if (ParentScope != null) return ParentScope.MessagesOrNull;

                // see comments in Messages

                if (_messages != null) return _messages;

                var factory = ScopeLifespanMessagesFactory.Current;
                return factory == null ? null : factory.GetFromHttpContext();
            }
        }

        /// <inheritdoc />
        public IEventDispatcher Events
        {
            get
            {
                EnsureNotDisposed();
                if (ParentScope != null) return ParentScope.Events;
                return _eventDispatcher ?? (_eventDispatcher = new ScopeEventDispatcher());
            }
        }

        /// <inheritdoc />
        public bool Complete()
        {
            if (_completed.HasValue == false)
                _completed = true;
            return _completed.Value;
        }

        public void Reset()
        {
            _completed = null;
        }

        public void ChildCompleted(bool? completed)
        {
            // if child did not complete we cannot complete
            if (completed.HasValue == false || completed.Value == false)
            {
                if (LogUncompletedScopes)
                    Logging.LogHelper.Debug<Scope>("Uncompleted Child Scope at\r\n" + Environment.StackTrace);
                _completed = false;
            }
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
            {
#if DEBUG_SCOPES
                var ambient = _scopeProvider.AmbientScope;
                Logging.LogHelper.Debug<Scope>("Dispose error (" + (ambient == null ? "no" : "other") + " ambient)");
                if (ambient == null)
                    throw new InvalidOperationException("Not the ambient scope (no ambient scope).");
                var infos = _scopeProvider.GetScopeInfo(ambient);
                throw new InvalidOperationException("Not the ambient scope (see current ambient ctor stack trace).\r\n" + infos.CtorStack);
#else
                throw new InvalidOperationException("Not the ambient scope.");
#endif
            }

            var parent = ParentScope;
            _scopeProvider.AmbientScope = parent; // might be null = this is how scopes are removed from context objects

#if DEBUG_SCOPES
            _scopeProvider.Disposed(this);
#endif

            if (parent != null)
                parent.ChildCompleted(_completed);
            else
                DisposeLastScope();

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private void DisposeLastScope()
        {
            // figure out completed
            var completed = _completed.HasValue && _completed.Value;

            // deal with database
            var databaseException = false;
            if (_database != null)
            {
                try
                {
                    if (completed)
                        _database.CompleteTransaction();
                    else
                        _database.AbortTransaction();
                }
                catch
                {
                    databaseException = true;
                    throw;
                }
                finally
                {
                    _database.Dispose();
                    _database = null;

                    if (databaseException)
                        RobustExit(false, true);
                }
            }

            RobustExit(completed, false);
        }

        // this chains some try/finally blocks to
        // - complete and dispose the scoped filesystems
        // - deal with events if appropriate
        // - remove the scope context if it belongs to this scope
        // - deal with detachable scopes
        // here,
        // - completed indicates whether the scope has been completed
        //    can be true or false, but in both cases the scope is exiting
        //    in a normal way
        // - onException indicates whether completing/aborting the database
        //    transaction threw an exception, in which case 'completed' has
        //    to be false + events don't trigger and we just to some cleanup
        //    to ensure we don't leave a scope around, etc
        private void RobustExit(bool completed, bool onException)
        {
            if (onException) completed = false;

            TryFinally(() =>
            {
                if (_scopeFileSystem == true)
                {
                    if (completed)
                        _fscope.Complete();
                    _fscope.Dispose();
                    _fscope = null;
                }
            }, () =>
            {
                // deal with events
                if (onException == false && _eventDispatcher != null)
                    _eventDispatcher.ScopeExit(completed);
            }, () =>
            {
                // if *we* created it, then get rid of it
                if (_scopeProvider.AmbientContext == _scopeContext)
                {
                    try
                    {
                        _scopeProvider.AmbientContext.ScopeExit(completed);
                    }
                    finally
                    {
                        // removes the ambient context (ambient scope already gone)
                        _scopeProvider.SetAmbient(null);
                    }
                }
            }, () =>
            {
                if (Detachable)
                {
                    // get out of the way, restore original
                    _scopeProvider.SetAmbient(OrigScope, OrigContext);
                    Attached = false;
                    OrigScope = null;
                    OrigContext = null;
                }
            });
        }

        private static void TryFinally(params Action[] actions)
        {
            TryFinally(0, actions);
        }

        private static void TryFinally(int index, Action[] actions)
        {
            if (index == actions.Length) return;
            try
            {
                actions[index]();
            }
            finally
            {
                TryFinally(index + 1, actions);
            }
        }

        // backing field for LogUncompletedScopes
        private static bool? _logUncompletedScopes;

        // caching config
        // true if Umbraco.CoreDebug.LogUncompletedScope appSetting is set to "true"
        private static bool LogUncompletedScopes
        {
            get { return (_logUncompletedScopes ?? (_logUncompletedScopes = UmbracoConfig.For.CoreDebug().LogUncompletedScopes)).Value; }
        }
    }
}
