using System.Data;
using System.Text;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Scoping
{
    /// <summary>
    ///     Implements <see cref="IScope" />.
    /// </summary>
    /// <remarks>Not thread-safe obviously.</remarks>
    internal class Scope : ICoreScope, IScope, Core.Scoping.IScope
    {
        private readonly bool _autoComplete;
        private readonly CoreDebugSettings _coreDebugSettings;

        private readonly object _dictionaryLocker;
        private readonly IEventAggregator _eventAggregator;
        private readonly IsolationLevel _isolationLevel;
        private readonly object _lockQueueLocker = new();
        private readonly ILogger<Scope> _logger;
        private readonly MediaFileManager _mediaFileManager;
        private readonly RepositoryCacheMode _repositoryCacheMode;
        private readonly bool? _scopeFileSystem;

        private readonly ScopeProvider _scopeProvider;
        private bool? _completed;
        private IUmbracoDatabase? _database;

        private bool _disposed;
        private IEventDispatcher? _eventDispatcher;
        private ICompletable? _fscope;

        private EventMessages? _messages;
        private IsolatedCaches? _isolatedCaches;
        private IScopedNotificationPublisher? _notificationPublisher;

        private StackQueue<(DistributedLockType lockType, TimeSpan timeout, Guid instanceId, int lockId)>? _queuedLocks;

        // This is all used to safely track read/write locks at given Scope levels so that
        // when we dispose we can verify that everything has been cleaned up correctly.
        private HashSet<int>? _readLocks;
        private Dictionary<Guid, Dictionary<int, int>>? _readLocksDictionary;
        private HashSet<int>? _writeLocks;
        private Dictionary<Guid, Dictionary<int, int>>? _writeLocksDictionary;
        private Queue<IDistributedLock>? _acquiredLocks;

        // initializes a new scope
        private Scope(
            ScopeProvider scopeProvider,
            CoreDebugSettings coreDebugSettings,
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
        {
            _scopeProvider = scopeProvider;
            _coreDebugSettings = coreDebugSettings;
            _mediaFileManager = mediaFileManager;
            _eventAggregator = eventAggregator;
            _logger = logger;
            Context = scopeContext;

            _isolationLevel = isolationLevel;
            _repositoryCacheMode = repositoryCacheMode;
            _eventDispatcher = eventDispatcher;
            _notificationPublisher = notificationPublisher;
            _scopeFileSystem = scopeFileSystems;
            _autoComplete = autoComplete;
            Detachable = detachable;
            _dictionaryLocker = new object();

#if DEBUG_SCOPES
            _scopeProvider.RegisterScope(this);
#endif
            logger.LogTrace("Create {InstanceId} on thread {ThreadId}", InstanceId.ToString("N").Substring(0, 8), Thread.CurrentThread.ManagedThreadId);

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

                // see note below
                if (scopeFileSystems == true)
                {
                    _fscope = fileSystems.Shadow();
                }

                _acquiredLocks = new Queue<IDistributedLock>();

                return;
            }

            if (parent != null)
            {
                ParentScope = parent;

                // cannot specify a different mode!
                // TODO: means that it's OK to go from L2 to None for reading purposes, but writing would be BAD!
                // this is for XmlStore that wants to bypass caches when rebuilding XML (same for NuCache)
                if (repositoryCacheMode != RepositoryCacheMode.Unspecified &&
                    parent.RepositoryCacheMode > repositoryCacheMode)
                {
                    throw new ArgumentException(
                        $"Value '{repositoryCacheMode}' cannot be lower than parent value '{parent.RepositoryCacheMode}'.", nameof(repositoryCacheMode));
                }

                // cannot specify a dispatcher!
                if (_eventDispatcher != null)
                {
                    throw new ArgumentException("Value cannot be specified on nested scope.", nameof(eventDispatcher));
                }

                // Only the outermost scope can specify the notification publisher
                if (_notificationPublisher != null)
                {
                    throw new ArgumentException("Value cannot be specified on nested scope.", nameof(notificationPublisher));
                }

                // cannot specify a different fs scope!
                // can be 'true' only on outer scope (and false does not make much sense)
                if (scopeFileSystems != null && parent._scopeFileSystem != scopeFileSystems)
                {
                    throw new ArgumentException(
                        $"Value '{scopeFileSystems.Value}' be different from parent value '{parent._scopeFileSystem}'.", nameof(scopeFileSystems));
                }
            }
            else
            {
                _acquiredLocks = new Queue<IDistributedLock>();

                // the FS scope cannot be "on demand" like the rest, because we would need to hook into
                // every scoped FS to trigger the creation of shadow FS "on demand", and that would be
                // pretty pointless since if scopeFileSystems is true, we *know* we want to shadow
                if (scopeFileSystems == true)
                {
                    _fscope = fileSystems.Shadow();
                }
            }
        }

        // initializes a new scope
        public Scope(
            ScopeProvider scopeProvider,
            CoreDebugSettings coreDebugSettings,
            MediaFileManager mediaFileManager,
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

        public bool ScopedFileSystems
        {
            get
            {
                if (ParentScope != null)
                {
                    return ParentScope.ScopedFileSystems;
                }

                return _fscope != null;
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

        public Guid InstanceId { get; } = Guid.NewGuid();

        public int CreatedThreadId { get; } = Thread.CurrentThread.ManagedThreadId;

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
        public RepositoryCacheMode RepositoryCacheMode
        {
            get
            {
                if (_repositoryCacheMode != RepositoryCacheMode.Unspecified)
                {
                    return _repositoryCacheMode;
                }

                if (ParentScope != null)
                {
                    return ParentScope.RepositoryCacheMode;
                }

                return RepositoryCacheMode.Default;
            }
        }

        /// <inheritdoc />
        public IsolatedCaches IsolatedCaches
        {
            get
            {
                if (ParentScope != null)
                {
                    return ParentScope.IsolatedCaches;
                }

                return _isolatedCaches ??= new IsolatedCaches(type => new DeepCloneAppCache(new ObjectCacheAppCache()));
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
                    EnsureDbLocks();
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
                    EnsureDbLocks();
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

        public int Depth
        {
            get
            {
                if (ParentScope == null)
                {
                    return 0;
                }

                return ParentScope.Depth + 1;
            }
        }

        public IScopedNotificationPublisher Notifications
        {
            get
            {
                EnsureNotDisposed();
                if (ParentScope != null)
                {
                    return ParentScope.Notifications;
                }

                return _notificationPublisher ??
                       (_notificationPublisher = new ScopedNotificationPublisher(_eventAggregator));
            }
        }

        /// <inheritdoc />
        public bool Complete()
        {
            if (_completed.HasValue == false)
            {
                _completed = true;
            }

            return _completed.Value;
        }

        public void Dispose()
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

            // Decrement the lock counters on the parent if any.
            ClearLocks(InstanceId);
            if (ParentScope is null)
            {
                while (!_acquiredLocks?.IsCollectionEmpty() ?? false)
                {
                    _acquiredLocks?.Dequeue().Dispose();
                }

                // We're the parent scope, make sure that locks of all scopes has been cleared
                // Since we're only reading we don't have to be in a lock
                if (_readLocksDictionary?.Count > 0 || _writeLocksDictionary?.Count > 0)
                {
                    var exception = new InvalidOperationException(
                        $"All scopes has not been disposed from parent scope: {InstanceId}, see log for more details.");
                    _logger.LogError(exception, GenerateUnclearedScopesLogMessage());
                    throw exception;
                }
            }

            _scopeProvider.PopAmbientScope(); // might be null = this is how scopes are removed from context objects

#if DEBUG_SCOPES
            _scopeProvider.Disposed(this);
#endif

            if (_autoComplete && _completed == null)
            {
                _completed = true;
            }

            if (ParentScope != null)
            {
                ParentScope.ChildCompleted(_completed);
            }
            else
            {
                DisposeLastScope();
            }

            lock (_lockQueueLocker)
            {
                _queuedLocks?.Clear();
            }

            _disposed = true;
        }

        public void EagerReadLock(params int[] lockIds) => EagerReadLockInner(InstanceId, null, lockIds);

        /// <inheritdoc />
        public void ReadLock(params int[] lockIds) => LazyReadLockInner(InstanceId, lockIds);

        public void EagerReadLock(TimeSpan timeout, int lockId) =>
            EagerReadLockInner(InstanceId, timeout, lockId);

        /// <inheritdoc />
        public void ReadLock(TimeSpan timeout, int lockId) => LazyReadLockInner(InstanceId, timeout, lockId);

        public void EagerWriteLock(params int[] lockIds) => EagerWriteLockInner(InstanceId, null, lockIds);

        /// <inheritdoc />
        public void WriteLock(params int[] lockIds) => LazyWriteLockInner(InstanceId, lockIds);

        public void EagerWriteLock(TimeSpan timeout, int lockId) =>
            EagerWriteLockInner(InstanceId, timeout, lockId);

        /// <inheritdoc />
        public void WriteLock(TimeSpan timeout, int lockId) => LazyWriteLockInner(InstanceId, timeout, lockId);

        /// <summary>
        ///     Used for testing. Ensures and gets any queued read locks.
        /// </summary>
        /// <returns></returns>
        internal Dictionary<Guid, Dictionary<int, int>>? GetReadLocks()
        {
            EnsureDbLocks();
            // always delegate to root/parent scope.
            if (ParentScope is not null)
            {
                return ParentScope.GetReadLocks();
            }

            return _readLocksDictionary;
        }

        /// <summary>
        ///     Used for testing. Ensures and gets and queued write locks.
        /// </summary>
        /// <returns></returns>
        internal Dictionary<Guid, Dictionary<int, int>>? GetWriteLocks()
        {
            EnsureDbLocks();
            // always delegate to root/parent scope.
            if (ParentScope is not null)
            {
                return ParentScope.GetWriteLocks();
            }

            return _writeLocksDictionary;
        }

        public void Reset() => _completed = null;

        public void ChildCompleted(bool? completed)
        {
            // if child did not complete we cannot complete
            if (completed.HasValue == false || completed.Value == false)
            {
                if (_coreDebugSettings.LogIncompletedScopes)
                {
                    _logger.LogWarning("Uncompleted Child Scope at\r\n {StackTrace}", Environment.StackTrace);
                }

                _completed = false;
            }
        }

        /// <summary>
        ///     When we require a ReadLock or a WriteLock we don't immediately request these locks from the database,
        ///     instead we only request them when necessary (lazily).
        ///     To do this, we queue requests for read/write locks.
        ///     This is so that if there's a request for either of these
        ///     locks, but the service/repository returns an item from the cache, we don't end up making a DB call to make the
        ///     read/write lock.
        ///     This executes the queue of requested locks in order in an efficient way lazily whenever the database instance is
        ///     resolved.
        /// </summary>
        private void EnsureDbLocks()
        {
            // always delegate to the root parent
            if (ParentScope is not null)
            {
                ParentScope.EnsureDbLocks();
            }
            else
            {
                lock (_lockQueueLocker)
                {
                    if (_queuedLocks?.Count > 0)
                    {
                        DistributedLockType currentType = DistributedLockType.ReadLock;
                        TimeSpan currentTimeout = TimeSpan.Zero;
                        Guid currentInstanceId = InstanceId;
                        var collectedIds = new HashSet<int>();

                        var i = 0;
                        while (_queuedLocks.Count > 0)
                        {
                            (DistributedLockType lockType, TimeSpan timeout, Guid instanceId, var lockId) = _queuedLocks.Dequeue();

                            if (i == 0)
                            {
                                currentType = lockType;
                                currentTimeout = timeout;
                                currentInstanceId = instanceId;
                            }
                            else if (lockType != currentType || timeout != currentTimeout ||
                                     instanceId != currentInstanceId)
                            {
                                // the lock type, instanceId or timeout switched.
                                // process the lock ids collected
                                switch (currentType)
                                {
                                    case DistributedLockType.ReadLock:
                                        EagerReadLockInner(currentInstanceId, currentTimeout == TimeSpan.Zero ? null : currentTimeout, collectedIds.ToArray());
                                        break;
                                    case DistributedLockType.WriteLock:
                                        EagerWriteLockInner(currentInstanceId, currentTimeout == TimeSpan.Zero ? null : currentTimeout, collectedIds.ToArray());
                                        break;
                                }

                                // clear the collected and set new type
                                collectedIds.Clear();
                                currentType = lockType;
                                currentTimeout = timeout;
                                currentInstanceId = instanceId;
                            }

                            collectedIds.Add(lockId);
                            i++;
                        }

                        // process the remaining
                        switch (currentType)
                        {
                            case DistributedLockType.ReadLock:
                                EagerReadLockInner(currentInstanceId, currentTimeout == TimeSpan.Zero ? null : currentTimeout, collectedIds.ToArray());
                                break;
                            case DistributedLockType.WriteLock:
                                EagerWriteLockInner(currentInstanceId, currentTimeout == TimeSpan.Zero ? null : currentTimeout, collectedIds.ToArray());
                                break;
                        }
                    }
                }
            }
        }

        private void EnsureNotDisposed()
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

        /// <summary>
        ///     Generates a log message with all scopes that hasn't cleared their locks, including how many, and what locks they
        ///     have requested.
        /// </summary>
        /// <returns>Log message.</returns>
        private string GenerateUnclearedScopesLogMessage()
        {
            // Dump the dicts into a message for the locks.
            var builder = new StringBuilder();
            builder.AppendLine(
                $"Lock counters aren't empty, suggesting a scope hasn't been properly disposed, parent id: {InstanceId}");
            WriteLockDictionaryToString(_readLocksDictionary!, builder, "read locks");
            WriteLockDictionaryToString(_writeLocksDictionary!, builder, "write locks");
            return builder.ToString();
        }

        /// <summary>
        ///     Writes a locks dictionary to a <see cref="StringBuilder" /> for logging purposes.
        /// </summary>
        /// <param name="dict">Lock dictionary to report on.</param>
        /// <param name="builder">String builder to write to.</param>
        /// <param name="dictName">The name to report the dictionary as.</param>
        private void WriteLockDictionaryToString(Dictionary<Guid, Dictionary<int, int>> dict, StringBuilder builder, string dictName)
        {
            if (dict?.Count > 0)
            {
                builder.AppendLine($"Remaining {dictName}:");
                foreach (KeyValuePair<Guid, Dictionary<int, int>> instance in dict)
                {
                    builder.AppendLine($"Scope {instance.Key}");
                    foreach (KeyValuePair<int, int> lockCounter in instance.Value)
                    {
                        builder.AppendLine($"\tLock ID: {lockCounter.Key} - times requested: {lockCounter.Value}");
                    }
                }
            }
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

            void HandleScopedFileSystems()
            {
                if (_scopeFileSystem == true)
                {
                    if (completed)
                    {
                        _fscope?.Complete();
                    }

                    _fscope?.Dispose();
                    _fscope = null;
                }
            }

            void HandleScopedNotifications()
            {
                if (onException == false)
                {
                    _eventDispatcher?.ScopeExit(completed);
                    _notificationPublisher?.ScopeExit(completed);
                }
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

        /// <summary>
        ///     Increment the counter of a locks dictionary, either ReadLocks or WriteLocks,
        ///     for a specific scope instance and lock identifier. Must be called within a lock.
        /// </summary>
        /// <param name="lockId">Lock ID to increment.</param>
        /// <param name="instanceId">Instance ID of the scope requesting the lock.</param>
        /// <param name="locks">Reference to the dictionary to increment on</param>
        private void IncrementLock(int lockId, Guid instanceId, ref Dictionary<Guid, Dictionary<int, int>>? locks)
        {
            // Since we've already checked that we're the parent in the WriteLockInner method, we don't need to check again.
            // If it's the very first time a lock has been requested the WriteLocks dict hasn't been instantiated yet.
            locks ??= new Dictionary<Guid, Dictionary<int, int>>();

            // Try and get the dict associated with the scope id.
            var locksDictFound = locks.TryGetValue(instanceId, out Dictionary<int, int>? locksDict);
            if (locksDictFound)
            {
                locksDict!.TryGetValue(lockId, out var value);
                locksDict[lockId] = value + 1;
            }
            else
            {
                // The scope hasn't requested a lock yet, so we have to create a dict for it.
                locks.Add(instanceId, new Dictionary<int, int>());
                locks[instanceId][lockId] = 1;
            }
        }

        /// <summary>
        ///     Clears all lock counters for a given scope instance, signalling that the scope has been disposed.
        /// </summary>
        /// <param name="instanceId">Instance ID of the scope to clear.</param>
        private void ClearLocks(Guid instanceId)
        {
            if (ParentScope is not null)
            {
                ParentScope.ClearLocks(instanceId);
            }
            else
            {
                lock (_dictionaryLocker)
                {
                    _readLocksDictionary?.Remove(instanceId);
                    _writeLocksDictionary?.Remove(instanceId);

                    // remove any queued locks for this instance that weren't used.
                    while (_queuedLocks?.Count > 0)
                    {
                        // It's safe to assume that the locks on the top of the stack belong to this instance,
                        // since any child scopes that might have added locks to the stack must be disposed before we try and dispose this instance.
                        (DistributedLockType lockType, TimeSpan timeout, Guid instanceId, int lockId) top =
                            _queuedLocks.PeekStack();
                        if (top.instanceId == instanceId)
                        {
                            _queuedLocks.Pop();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        public void LazyReadLockInner(Guid instanceId, params int[] lockIds)
        {
            if (ParentScope != null)
            {
                ParentScope.LazyReadLockInner(instanceId, lockIds);
            }
            else
            {
                LazyLockInner(DistributedLockType.ReadLock, instanceId, lockIds);
            }
        }

        public void LazyReadLockInner(Guid instanceId, TimeSpan timeout, int lockId)
        {
            if (ParentScope != null)
            {
                ParentScope.LazyReadLockInner(instanceId, timeout, lockId);
            }
            else
            {
                LazyLockInner(DistributedLockType.ReadLock, instanceId, timeout, lockId);
            }
        }

        public void LazyWriteLockInner(Guid instanceId, params int[] lockIds)
        {
            if (ParentScope != null)
            {
                ParentScope.LazyWriteLockInner(instanceId, lockIds);
            }
            else
            {
                LazyLockInner(DistributedLockType.WriteLock, instanceId, lockIds);
            }
        }

        public void LazyWriteLockInner(Guid instanceId, TimeSpan timeout, int lockId)
        {
            if (ParentScope != null)
            {
                ParentScope.LazyWriteLockInner(instanceId, timeout, lockId);
            }
            else
            {
                LazyLockInner(DistributedLockType.WriteLock, instanceId, timeout, lockId);
            }
        }

        private void LazyLockInner(DistributedLockType lockType, Guid instanceId, params int[] lockIds)
        {
            lock (_lockQueueLocker)
            {
                if (_queuedLocks == null)
                {
                    _queuedLocks = new StackQueue<(DistributedLockType, TimeSpan, Guid, int)>();
                }

                foreach (var lockId in lockIds)
                {
                    _queuedLocks.Enqueue((lockType, TimeSpan.Zero, instanceId, lockId));
                }
            }
        }

        private void LazyLockInner(DistributedLockType lockType, Guid instanceId, TimeSpan timeout, int lockId)
        {
            lock (_lockQueueLocker)
            {
                if (_queuedLocks == null)
                {
                    _queuedLocks = new StackQueue<(DistributedLockType, TimeSpan, Guid, int)>();
                }


                _queuedLocks.Enqueue((lockType, timeout, instanceId, lockId));
            }
        }

        /// <summary>
        ///     Handles acquiring a read lock, will delegate it to the parent if there are any.
        /// </summary>
        /// <param name="instanceId">Instance ID of the requesting scope.</param>
        /// <param name="timeout">Optional database timeout in milliseconds.</param>
        /// <param name="lockIds">Array of lock object identifiers.</param>
        private void EagerReadLockInner(Guid instanceId, TimeSpan? timeout, params int[] lockIds)
        {
            if (ParentScope is not null)
            {
                // If we have a parent we delegate lock creation to parent.
                ParentScope.EagerReadLockInner(instanceId, timeout, lockIds);
            }
            else
            {
                lock (_dictionaryLocker)
                {
                    foreach (var lockId in lockIds)
                    {
                        IncrementLock(lockId, instanceId, ref _readLocksDictionary);

                        // We are the outermost scope, handle the lock request.
                        LockInner(
                            instanceId,
                            ref _readLocksDictionary!,
                            ref _readLocks!,
                            ObtainReadLock,
                            timeout,
                            lockId);
                    }
                }
            }
        }

        /// <summary>
        ///     Handles acquiring a write lock with a specified timeout, will delegate it to the parent if there are any.
        /// </summary>
        /// <param name="instanceId">Instance ID of the requesting scope.</param>
        /// <param name="timeout">Optional database timeout in milliseconds.</param>
        /// <param name="lockIds">Array of lock object identifiers.</param>
        private void EagerWriteLockInner(Guid instanceId, TimeSpan? timeout, params int[] lockIds)
        {
            if (ParentScope is not null)
            {
                // If we have a parent we delegate lock creation to parent.
                ParentScope.EagerWriteLockInner(instanceId, timeout, lockIds);
            }
            else
            {
                lock (_dictionaryLocker)
                {
                    foreach (var lockId in lockIds)
                    {
                        IncrementLock(lockId, instanceId, ref _writeLocksDictionary);

                        // We are the outermost scope, handle the lock request.
                        LockInner(
                            instanceId,
                            ref _writeLocksDictionary!,
                            ref _writeLocks!,
                            ObtainWriteLock,
                            timeout,
                            lockId);
                    }
                }
            }
        }

        /// <summary>
        ///     Handles acquiring a lock, this should only be called from the outermost scope.
        /// </summary>
        /// <param name="instanceId">Instance ID of the scope requesting the lock.</param>
        /// <param name="locks">Reference to the applicable locks dictionary (ReadLocks or WriteLocks).</param>
        /// <param name="locksSet">Reference to the applicable locks hashset (_readLocks or _writeLocks).</param>
        /// <param name="obtainLock">Delegate used to request the lock from the locking mechanism.</param>
        /// <param name="timeout">Optional timeout parameter to specify a timeout.</param>
        /// <param name="lockId">Lock identifier.</param>
        private void LockInner(
            Guid instanceId,
            ref Dictionary<Guid, Dictionary<int, int>> locks,
            ref HashSet<int> locksSet,
            Action<int, TimeSpan?> obtainLock,
            TimeSpan? timeout,
            int lockId)
        {
            locksSet ??= new HashSet<int>();

            // Only acquire the lock if we haven't done so yet.
            if (locksSet.Contains(lockId))
            {
                return;
            }

            locksSet.Add(lockId);
            try
            {
                obtainLock(lockId, timeout);
            }
            catch
            {
                // Something went wrong and we didn't get the lock
                // Since we at this point have determined that we haven't got any lock with an ID of LockID, it's safe to completely remove it instead of decrementing.
                locks[instanceId].Remove(lockId);

                // It needs to be removed from the HashSet as well, because that's how we determine to acquire a lock.
                locksSet.Remove(lockId);
                throw;
            }
        }

        /// <summary>
        ///     Obtains a read lock with a custom timeout.
        /// </summary>
        /// <param name="lockId">Lock object identifier to lock.</param>
        /// <param name="timeout">TimeSpan specifying the timout period.</param>
        private void ObtainReadLock(int lockId, TimeSpan? timeout)
        {
            if (_acquiredLocks == null)
            {
                throw new InvalidOperationException($"Cannot obtain a read lock as the {nameof(_acquiredLocks)} queue is null.");
            }

            _acquiredLocks.Enqueue(_scopeProvider.DistributedLockingMechanismFactory.DistributedLockingMechanism.ReadLock(lockId, timeout));
        }

        /// <summary>
        ///     Obtains a write lock with a custom timeout.
        /// </summary>
        /// <param name="lockId">Lock object identifier to lock.</param>
        /// <param name="timeout">TimeSpan specifying the timout period.</param>
        private void ObtainWriteLock(int lockId, TimeSpan? timeout)
        {
            if (_acquiredLocks == null)
            {
                throw new InvalidOperationException($"Cannot obtain a write lock as the {nameof(_acquiredLocks)} queue is null.");
            }

            _acquiredLocks.Enqueue(_scopeProvider.DistributedLockingMechanismFactory.DistributedLockingMechanism.WriteLock(lockId, timeout));
        }
    }
}
