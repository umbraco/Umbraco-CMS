using System.Data;
using System.Text;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Scoping
{
    /// <summary>
    ///     Implements <see cref="IScope" />.
    /// </summary>
    /// <remarks>Not thread-safe obviously.</remarks>
    internal class Scope : CoreScope, ICoreScope, IScope, Core.Scoping.IScope
    {
        private readonly bool _autoComplete;
        private readonly CoreDebugSettings _coreDebugSettings;
        private readonly IsolationLevel _isolationLevel;
        private readonly ILogger<Scope> _logger;
        private readonly MediaFileManager _mediaFileManager;

        private readonly ScopeProvider _scopeProvider;
        private IUmbracoDatabase? _database;

        private bool _disposed;
        private IEventDispatcher? _eventDispatcher;

        private EventMessages? _messages;

        // initializes a new scope
        private Scope(
            ScopeProvider scopeProvider,
            CoreDebugSettings coreDebugSettings,
            IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
            ILoggerFactory loggerFactory,
            MediaFileManager mediaFileManager,
            IEventAggregator eventAggregator,
            ILogger<Scope> logger,
            FileSystems fileSystems,
            Scope? parent,
            IScopeContext? scopeContext,
            bool detachable,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher? eventDispatcher = null,
            IScopedNotificationPublisher? notificationPublisher = null,
            bool? scopeFileSystems = null,
            bool callContext = false,
            bool autoComplete = false)
            : base(
                parent,
                distributedLockingMechanismFactory,
                loggerFactory,
                fileSystems,
                eventAggregator,
                repositoryCacheMode,
                scopeFileSystems,
                notificationPublisher)
        {
            _scopeProvider = scopeProvider;
            _coreDebugSettings = coreDebugSettings;
            _mediaFileManager = mediaFileManager;
            _logger = logger;
            Context = scopeContext;

            _isolationLevel = isolationLevel;
            _eventDispatcher = eventDispatcher;
            _autoComplete = autoComplete;
            Detachable = detachable;

#if DEBUG_SCOPES
            _scopeProvider.RegisterScope(this);
#endif
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
            {
                logger.LogTrace("Create {InstanceId} on thread {ThreadId}", InstanceId.ToString("N").Substring(0, 8), Thread.CurrentThread.ManagedThreadId);
            }

            if (detachable)
            {
                if (parent != null)
                {
                    throw new ArgumentException("Cannot set parent on detachable scope.", nameof(parent));
                }

                if (scopeContext != null)
                {
                    throw new ArgumentException("Cannot set context on detachable scope.", nameof(scopeContext));
                }

                if (autoComplete)
                {
                    throw new ArgumentException("Cannot auto-complete a detachable scope.", nameof(autoComplete));
                }

                // detachable creates its own scope context
                Context = new ScopeContext();

                return;
            }

            if (parent != null)
            {
                ParentScope = parent;

                // cannot specify a dispatcher!
                if (_eventDispatcher != null)
                {
                    throw new ArgumentException("Value cannot be specified on nested scope.", nameof(eventDispatcher));
                }
            }
        }

        // initializes a new scope
        public Scope(
            ScopeProvider scopeProvider,
            CoreDebugSettings coreDebugSettings,
            MediaFileManager mediaFileManager,
            IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
            ILoggerFactory loggerFactory,
            IEventAggregator eventAggregator,
            ILogger<Scope> logger,
            FileSystems fileSystems,
            bool detachable,
            IScopeContext? scopeContext,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher? eventDispatcher = null,
            IScopedNotificationPublisher? scopedNotificationPublisher = null,
            bool? scopeFileSystems = null,
            bool callContext = false,
            bool autoComplete = false)
            : this(
                scopeProvider,
                coreDebugSettings,
                distributedLockingMechanismFactory,
                loggerFactory,
                mediaFileManager,
                eventAggregator,
                logger,
                fileSystems,
                null,
                scopeContext,
                detachable,
                isolationLevel,
                repositoryCacheMode,
                eventDispatcher,
                scopedNotificationPublisher,
                scopeFileSystems,
                callContext,
                autoComplete)
        {
        }

        // initializes a new scope in a nested scopes chain, with its parent
        public Scope(
            ScopeProvider scopeProvider,
            CoreDebugSettings coreDebugSettings,
            MediaFileManager mediaFileManager,
            IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
            ILoggerFactory loggerFactory,
            IEventAggregator eventAggregator,
            ILogger<Scope> logger,
            FileSystems fileSystems,
            Scope parent,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher? eventDispatcher = null,
            IScopedNotificationPublisher? notificationPublisher = null,
            bool? scopeFileSystems = null,
            bool callContext = false,
            bool autoComplete = false)
            : this(
                scopeProvider,
                coreDebugSettings,
                distributedLockingMechanismFactory,
                loggerFactory,
                mediaFileManager,
                eventAggregator,
                logger,
                fileSystems,
                parent,
                null,
                false,
                isolationLevel,
                repositoryCacheMode,
                eventDispatcher,
                notificationPublisher,
                scopeFileSystems,
                callContext,
                autoComplete)
        {
        }

        [Obsolete("Scopes are never stored on HttpContext.Items anymore, so CallContext is always true.")]
        // a value indicating whether to force call-context
        public bool CallContext
        {
            get => true;
            set
            {
                // NOOP - always true.
            }
        }

        // a value indicating whether the scope is detachable
        // ie whether it was created by CreateDetachedScope
        public bool Detachable { get; }

        // the parent scope (in a nested scopes chain)
        public Scope? ParentScope { get; set; }

        public bool Attached { get; set; }

        // the original scope (when attaching a detachable scope)
        public Scope? OrigScope { get; set; }

        // the original context (when attaching a detachable scope)
        public IScopeContext? OrigContext { get; set; }

        // the context (for attaching & detaching only)
        public IScopeContext? Context { get; }

        public IsolationLevel IsolationLevel
        {
            get
            {
                if (_isolationLevel != IsolationLevel.Unspecified)
                {
                    return _isolationLevel;
                }

                if (ParentScope != null)
                {
                    return ParentScope.IsolationLevel;
                }

                return SqlContext.SqlSyntax.DefaultIsolationLevel;
            }
        }

        // true if Umbraco.CoreDebugSettings.LogUncompletedScope appSetting is set to "true"
        private bool LogUncompletedScopes => _coreDebugSettings.LogIncompletedScopes;

        public ISqlContext SqlContext
        {
            get
            {
                if (_scopeProvider.SqlContext == null)
                {
                    throw new InvalidOperationException(
                        $"The {nameof(_scopeProvider.SqlContext)} on the scope provider is null");
                }

                return _scopeProvider.SqlContext;
            }
        }

        /// <inheritdoc />
        public IUmbracoDatabase Database
        {
            get
            {
                EnsureNotDisposed();

                if (_database != null)
                {
                    // If the database has already been resolved, we are already in a
                    // transaction, but it's possible that more locks have been requested
                    // so acquire them.

                    // TODO: This is the issue we face with non-eager locks. If locks are
                    // requested after the Database property is resolved, those locks may
                    // not get executed because the developer may be using their local Database variable
                    // instead of accessing via scope.Database.
                    // In our case within Umbraco, I don't think this ever occurs, all locks are requested
                    // up-front, however, that might not be the case for others.
                    // The other way to deal with this would be to bake this callback somehow into the
                    // UmbracoDatabase instance directly and ensure it's called when OnExecutingCommand
                    // (so long as the executing command isn't a lock command itself!)
                    // If we could do that, that would be the ultimate lazy executed locks.
                    Locks.EnsureLocks(InstanceId);
                    return _database;
                }

                if (ParentScope != null)
                {
                    IUmbracoDatabase database = ParentScope.Database;
                    IsolationLevel currentLevel = database.GetCurrentTransactionIsolationLevel();
                    if (_isolationLevel > IsolationLevel.Unspecified && currentLevel < _isolationLevel)
                    {
                        throw new Exception("Scope requires isolation level " + _isolationLevel + ", but got " +
                                            currentLevel + " from parent.");
                    }

                    return _database = database;
                }

                // create a new database
                _database = _scopeProvider.DatabaseFactory.CreateDatabase();

                // enter a transaction, as a scope implies a transaction, always
                try
                {
                    _database.BeginTransaction(IsolationLevel);
                    Locks.EnsureLocks(InstanceId);
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

        /// <inheritdoc />
        public EventMessages Messages
        {
            get
            {
                EnsureNotDisposed();
                if (ParentScope != null)
                {
                    return ParentScope.Messages;
                }

                return _messages ??= new EventMessages();

                // TODO: event messages?
                // this may be a problem: the messages collection will be cleared at the end of the scope
                // how shall we process it in controllers etc? if we don't want the global factory from v7?
                // it'd need to be captured by the controller
                //
                // + rename // EventMessages = ServiceMessages or something
            }
        }

        /// <inheritdoc />
        [Obsolete("Will be removed in 14, please use notifications instead")]
        public IEventDispatcher Events
        {
            get
            {
                EnsureNotDisposed();
                if (ParentScope != null)
                {
                    return ParentScope.Events;
                }

                return _eventDispatcher ??= new QueuingEventDispatcher(_mediaFileManager);
            }
        }

        public override void Dispose()
        {
            EnsureNotDisposed();

            if (this != _scopeProvider.AmbientScope)
            {
                var failedMessage =
                    $"The {nameof(Scope)} {InstanceId} being disposed is not the Ambient {nameof(Scope)} {_scopeProvider.AmbientScope?.InstanceId.ToString() ?? "NULL"}. This typically indicates that a child {nameof(Scope)} was not disposed, or flowed to a child thread that was not awaited, or concurrent threads are accessing the same {nameof(Scope)} (Ambient context) which is not supported. If using Task.Run (or similar) as a fire and forget tasks or to run threads in parallel you must suppress execution context flow with ExecutionContext.SuppressFlow() and ExecutionContext.RestoreFlow().";

#if DEBUG_SCOPES
                Scope ambient = _scopeProvider.AmbientScope;
                _logger.LogWarning("Dispose error (" + (ambient == null ? "no" : "other") + " ambient)");
                if (ambient == null)
                {
                    throw new InvalidOperationException("Not the ambient scope (no ambient scope).");
                }

                ScopeInfo ambientInfos = _scopeProvider.GetScopeInfo(ambient);
                ScopeInfo disposeInfos = _scopeProvider.GetScopeInfo(this);
                throw new InvalidOperationException($"{failedMessage} (see ctor stack traces).\r\n"
                                                    + "- ambient ->\r\n" + ambientInfos.ToString() + "\r\n"
                                                    + "- dispose ->\r\n" + disposeInfos.ToString() + "\r\n");
#else
                throw new InvalidOperationException(failedMessage);
#endif
            }

            Locks.ClearLocks(InstanceId);

            if (ParentScope is null)
            {
                Locks.EnsureLocksCleared(InstanceId);
            }

            _scopeProvider.PopAmbientScope(); // might be null = this is how scopes are removed from context objects

#if DEBUG_SCOPES
            _scopeProvider.Disposed(this);
#endif

            if (_autoComplete && Completed == null)
            {
                Completed = true;
            }

            // CoreScope.Dispose will handle file systems and notifications, as well as notifying any parent scope of the child scope's completion.
            // In this overridden class, we re-use that functionality and also handle scope context (including enlisted actions) and detached scopes.
            // We retain order of events behaviour from Umbraco 11:
            // - handle file systems (in CoreScope)
            // - handle scoped notifications (in CoreScope)
            // - handle scope context (in Scope)
            // - handle detatched scopes (in Scope)
            if (ParentScope is null)
            {
                DisposeLastScope();
            }
            else
            {
                ParentScope.ChildCompleted(Completed);
            }

            _disposed = true;
        }

        /// <summary>
        ///     Used for testing. Ensures and gets any queued read locks.
        /// </summary>
        /// <returns></returns>
        internal Dictionary<Guid, Dictionary<int, int>>? GetReadLocks()
        {
            Locks.EnsureLocks(InstanceId);
            return ((LockingMechanism)Locks).GetReadLocks();
        }

        /// <summary>
        ///     Used for testing. Ensures and gets and queued write locks.
        /// </summary>
        /// <returns></returns>
        internal Dictionary<Guid, Dictionary<int, int>>? GetWriteLocks()
        {
            Locks.EnsureLocks(InstanceId);
            return ((LockingMechanism)Locks).GetWriteLocks();
        }

        public void Reset() => Completed = null;

        internal void EnsureNotDisposed()
        {
            // We can't be disposed
            if (_disposed)
            {
                throw new ObjectDisposedException($"The {nameof(Scope)} ({this.GetDebugInfo()}) is already disposed");
            }

            // And neither can our ancestors if we're trying to be disposed since
            // a child must always be disposed before it's parent.
            // This is a safety check, it's actually not entirely possible that a parent can be
            // disposed before the child since that will end up with a "not the Ambient" exception.
            ParentScope?.EnsureNotDisposed();

            // TODO: safer?
            //if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
            //    throw new ObjectDisposedException(GetType().FullName);
        }

        private void DisposeLastScope()
        {
            // figure out completed
            var completed = Completed.HasValue && Completed.Value;

            // deal with database
            var databaseException = false;
            if (_database != null)
            {
                try
                {
                    if (completed)
                    {
                        _database.CompleteTransaction();
                    }
                    else
                    {
                        _database.AbortTransaction();
                    }
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
                    {
                        RobustExit(false, true);
                    }
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
            if (onException)
            {
                completed = false;
            }

            void HandleScopeContext()
            {
                // if *we* created it, then get rid of it
                if (_scopeProvider.AmbientContext == Context)
                {
                    try
                    {
                        _scopeProvider.AmbientContext?.ScopeExit(completed);
                    }
                    finally
                    {
                        // removes the ambient context (ambient scope already gone)
                        _scopeProvider.PopAmbientScopeContext();
                    }
                }
            }

            void HandleDetachedScopes()
            {
                if (Detachable)
                {
                    // get out of the way, restore original

                    // TODO: Difficult to know if this is correct since this is all required
                    // by Deploy which I don't fully understand since there is limited tests on this in the CMS
                    if (OrigScope != _scopeProvider.AmbientScope)
                    {
                        _scopeProvider.PopAmbientScope();
                    }

                    if (OrigContext != _scopeProvider.AmbientContext)
                    {
                        _scopeProvider.PopAmbientScopeContext();
                    }

                    Attached = false;
                    OrigScope = null;
                    OrigContext = null;
                }
            }

            TryFinally(
                HandleScopedFileSystems,
                HandleScopedNotifications,
                HandleScopeContext,
                HandleDetachedScopes);
        }

        private static void TryFinally(params Action[] actions)
        {
            var exceptions = new List<Exception>();

            foreach (Action action in actions)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
