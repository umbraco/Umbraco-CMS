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
    internal sealed class Scope : CoreScope, ICoreScope, IScope, Core.Scoping.IScope
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Scope"/> class, which manages the lifetime and transactional context of operations within Umbraco.
        /// </summary>
        /// <remarks>initializes a new scope</remarks>
        /// <param name="scopeProvider">The provider responsible for managing scope lifecycles.</param>
        /// <param name="coreDebugSettings">The settings used to control core debugging behavior.</param>
        /// <param name="mediaFileManager">The manager used for handling media file operations within the scope.</param>
        /// <param name="distributedLockingMechanismFactory">Factory for creating distributed locking mechanisms to ensure concurrency control.</param>
        /// <param name="loggerFactory">Factory for creating logger instances.</param>
        /// <param name="eventAggregator">The event aggregator for publishing and subscribing to events within the scope.</param>
        /// <param name="logger">The logger instance for logging scope-related operations.</param>
        /// <param name="fileSystems">The collection of file system providers used by the scope.</param>
        /// <param name="detachable">If set to <c>true</c>, the scope can be detached and re-attached to different contexts.</param>
        /// <param name="scopeContext">The context object that holds scope-specific data, or <c>null</c> if not provided.</param>
        /// <param name="isolationLevel">The transaction isolation level to use for this scope. Defaults to <see cref="IsolationLevel.Unspecified"/>.</param>
        /// <param name="repositoryCacheMode">The cache mode to use for repository operations within this scope.</param>
        /// <param name="eventDispatcher">Optional event dispatcher for handling domain events within the scope.</param>
        /// <param name="scopedNotificationPublisher">Optional notification publisher for publishing notifications within the scope.</param>
        /// <param name="scopeFileSystems">If set, determines whether file systems should be scoped to this instance.</param>
        /// <param name="callContext">If <c>true</c>, uses call context for scope management; otherwise, uses async local context.</param>
        /// <param name="autoComplete">If <c>true</c>, the scope will automatically complete when disposed.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Scoping.Scope"/> class, representing a unit of work and transaction scope within Umbraco's infrastructure.
        /// </summary>
        /// <remarks>initializes a new scope in a nested scopes chain, with its parent</remarks>
        /// <param name="scopeProvider">The provider responsible for managing scope lifecycles.</param>
        /// <param name="coreDebugSettings">Configuration settings for core debugging features.</param>
        /// <param name="mediaFileManager">Manages media file operations within the scope.</param>
        /// <param name="distributedLockingMechanismFactory">Factory for creating distributed locking mechanisms to coordinate access across multiple processes or servers.</param>
        /// <param name="loggerFactory">Factory for creating logger instances.</param>
        /// <param name="eventAggregator">Aggregates and dispatches domain events within the scope.</param>
        /// <param name="logger">Logger instance for logging scope-related operations.</param>
        /// <param name="fileSystems">Provides access to the file systems used by Umbraco.</param>
        /// <param name="parent">The parent scope, if this is a nested scope; otherwise, <c>null</c>.</param>
        /// <param name="isolationLevel">The transaction isolation level to use for this scope.</param>
        /// <param name="repositoryCacheMode">The caching mode for repository operations within the scope.</param>
        /// <param name="eventDispatcher">Optional event dispatcher for handling domain events.</param>
        /// <param name="notificationPublisher">Optional notification publisher for publishing scoped notifications.</param>
        /// <param name="scopeFileSystems">If <c>true</c>, file systems are scoped to this instance; otherwise, they are shared.</param>
        /// <param name="callContext">If <c>true</c>, associates the scope with the call context for ambient scope management.</param>
        /// <param name="autoComplete">If <c>true</c>, the scope will automatically complete when disposed; otherwise, completion must be explicit.</param>
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

        /// <summary>
        /// Gets a value indicating whether this scope was created by <see cref="CreateDetachedScope"/>, making it detachable.
        /// </summary>
        /// <remarks>
        /// a value indicating whether the scope is detachable
        /// ie whether it was created by CreateDetachedScope
        /// </remarks>
        public bool Detachable { get; }

        /// <summary>
        /// Gets or sets the parent <see cref="Scope"/> in a chain of nested scopes.
        /// Returns <c>null</c> if this scope is the root.
        /// </summary>
        /// <remarks>the parent scope (in a nested scopes chain)</remarks>
        public Scope? ParentScope { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this scope is currently attached to a parent or ambient scope.
        /// </summary>

        public bool Attached { get; set; }

        /// <summary>
        /// Gets or sets the original <see cref="Scope"/> instance when attaching a detachable scope.
        /// This property is used to keep track of the scope that was active before a detachable scope was attached.
        /// </summary>
        /// <remarks>the original scope (when attaching a detachable scope)</remarks>
        public Scope? OrigScope { get; set; }

        /// <summary>
        /// Gets or sets the original <see cref="IScopeContext"/> that was present before attaching a detachable scope.
        /// This is used to restore the previous context when the detachable scope is detached.
        /// </summary>
        /// <remarks>the original context (when attaching a detachable scope)</remarks>
        public IScopeContext? OrigContext { get; set; }

        /// <summary>
        /// Gets the <see cref="IScopeContext"/> associated with this scope, which is used for attaching and detaching resources or state during the scope's lifetime.
        /// </summary>
        /// <remarks>the context (for attaching & detaching only)</remarks>
        public IScopeContext? Context { get; }

        /// <summary>
        /// Gets the isolation level used by this scope.
        /// If not explicitly set, it inherits from the parent scope or defaults to the SQL context's default isolation level.
        /// </summary>
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

        /// <summary>
        /// Gets the current <see cref="ISqlContext"/> instance from the scope provider.
        /// The SQL context provides access to SQL-related operations and metadata within the current scope.
        /// </summary>
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
        [Obsolete("Please use notifications instead. Scheduled for removal in Umbraco 18.")]
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

        /// <summary>
        /// Disposes the current scope instance, performing cleanup and validation.
        /// Throws an exception if this scope is not the ambient scope, which indicates improper scope usage such as incorrect disposal order or concurrent access.
        /// Clears locks, manages the ambient scope stack, and handles completion and disposal logic for the scope and its parent if applicable.
        /// </summary>
        public override void Dispose()
        {
            EnsureNotDisposed();

            if (this != _scopeProvider.AmbientScope)
            {
                var failedMessage =
                    $"The {nameof(Scope)} {InstanceId} being disposed is not the Ambient {nameof(Scope)} {_scopeProvider.AmbientScope?.InstanceId.ToString() ?? "NULL"}. This typically indicates that a child {nameof(Scope)} was not disposed, or flowed to a child thread that was not awaited, or concurrent threads are accessing the same {nameof(Scope)} (Ambient context) which is not supported. If using Task.Run (or similar) as a fire and forget tasks or to run threads in parallel you must suppress execution context flow with ExecutionContext.SuppressFlow() and ExecutionContext.RestoreFlow().";

#if DEBUG_SCOPES
                Scope? ambient = _scopeProvider.AmbientScope;
                _logger.LogWarning("Dispose error (" + (ambient == null ? "no" : "other") + " ambient)");
                if (ambient == null)
                {
                    throw new InvalidOperationException("Not the ambient scope (no ambient scope).");
                }

                ScopeInfo ambientInfos = _scopeProvider.GetScopeInfo(ambient);
                ScopeInfo disposeInfos = _scopeProvider.GetScopeInfo(this);
                throw new InvalidOperationException($"{failedMessage} (see ctor stack traces).\r\n"
                                                    + "- ambient ->\r\n" + ambientInfos + "\r\n"
                                                    + "- dispose ->\r\n" + disposeInfos + "\r\n");
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

        /// <summary>
        /// Resets the scope by setting the <c>Completed</c> property to <c>null</c>, allowing the scope to be reused or re-evaluated.
        /// </summary>
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
                HandleScopeContext,
                HandleScopedNotifications,
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
