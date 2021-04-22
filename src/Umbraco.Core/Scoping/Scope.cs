using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Implements <see cref="IScope"/>.
    /// </summary>
    /// <remarks>Not thread-safe obviously.</remarks>
    internal class Scope : IScope2
    {
        private enum LockType
        {
            ReadLock,
            WriteLock
        }

        private readonly ScopeProvider _scopeProvider;
        private readonly ILogger _logger;

        private readonly IsolationLevel _isolationLevel;
        private readonly RepositoryCacheMode _repositoryCacheMode;
        private readonly bool? _scopeFileSystem;
        private readonly bool _autoComplete;
        private bool _callContext;

        private bool _disposed;
        private bool? _completed;

        private IsolatedCaches _isolatedCaches;
        private IUmbracoDatabase _database;
        private EventMessages _messages;
        private ICompletable _fscope;
        private IEventDispatcher _eventDispatcher;

        private object _dictionaryLocker;
        private readonly object _lockQueueLocker = new object();
        private HashSet<int> _readLocks;
        private HashSet<int> _writeLocks;
        internal Dictionary<Guid, Dictionary<int, int>> ReadLocks;
        internal Dictionary<Guid, Dictionary<int, int>> WriteLocks;
        private List<(LockType lockType, TimeSpan timeout, int lockId)> _queuedLocks;

        // initializes a new scope
        private Scope(ScopeProvider scopeProvider,
            ILogger logger, FileSystems fileSystems, Scope parent, ScopeContext scopeContext, bool detachable,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            bool? scopeFileSystems = null,
            bool callContext = false,
            bool autoComplete = false)
        {
            _scopeProvider = scopeProvider;
            _logger = logger;

            Context = scopeContext;

            _isolationLevel = isolationLevel;
            _repositoryCacheMode = repositoryCacheMode;
            _eventDispatcher = eventDispatcher;
            _scopeFileSystem = scopeFileSystems;
            _callContext = callContext;
            _autoComplete = autoComplete;

            Detachable = detachable;

            _dictionaryLocker = new object();

#if DEBUG_SCOPES
            _scopeProvider.RegisterScope(this);
            Console.WriteLine("create " + InstanceId.ToString("N").Substring(0, 8));
#endif

            if (detachable)
            {
                if (parent != null) throw new ArgumentException("Cannot set parent on detachable scope.", nameof(parent));
                if (scopeContext != null) throw new ArgumentException("Cannot set context on detachable scope.", nameof(scopeContext));
                if (autoComplete) throw new ArgumentException("Cannot auto-complete a detachable scope.", nameof(autoComplete));

                // detachable creates its own scope context
                Context = new ScopeContext();

                // see note below
                if (scopeFileSystems == true)
                    _fscope = fileSystems.Shadow();

                return;
            }

            if (parent != null)
            {
                ParentScope = parent;

                // cannot specify a different mode!
                // TODO: means that it's OK to go from L2 to None for reading purposes, but writing would be BAD!
                // this is for XmlStore that wants to bypass caches when rebuilding XML (same for NuCache)
                if (repositoryCacheMode != RepositoryCacheMode.Unspecified && parent.RepositoryCacheMode > repositoryCacheMode)
                    throw new ArgumentException($"Value '{repositoryCacheMode}' cannot be lower than parent value '{parent.RepositoryCacheMode}'.", nameof(repositoryCacheMode));

                // cannot specify a dispatcher!
                if (_eventDispatcher != null)
                    throw new ArgumentException("Value cannot be specified on nested scope.", nameof(eventDispatcher));

                // cannot specify a different fs scope!
                // can be 'true' only on outer scope (and false does not make much sense)
                if (scopeFileSystems != null && parent._scopeFileSystem != scopeFileSystems)
                    throw new ArgumentException($"Value '{scopeFileSystems.Value}' be different from parent value '{parent._scopeFileSystem}'.", nameof(scopeFileSystems));
            }
            else
            {
                // the FS scope cannot be "on demand" like the rest, because we would need to hook into
                // every scoped FS to trigger the creation of shadow FS "on demand", and that would be
                // pretty pointless since if scopeFileSystems is true, we *know* we want to shadow
                if (scopeFileSystems == true)
                    _fscope = fileSystems.Shadow();
            }
        }

        // initializes a new scope
        public Scope(ScopeProvider scopeProvider,
            ILogger logger, FileSystems fileSystems, bool detachable, ScopeContext scopeContext,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            bool? scopeFileSystems = null,
            bool callContext = false,
            bool autoComplete = false)
            : this(scopeProvider, logger, fileSystems, null, scopeContext, detachable, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems, callContext, autoComplete)
        { }

        // initializes a new scope in a nested scopes chain, with its parent
        public Scope(ScopeProvider scopeProvider,
            ILogger logger, FileSystems fileSystems, Scope parent,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            bool? scopeFileSystems = null,
            bool callContext = false,
            bool autoComplete = false)
            : this(scopeProvider, logger, fileSystems, parent, null, false, isolationLevel, repositoryCacheMode, eventDispatcher, scopeFileSystems, callContext, autoComplete)
        { }

        public Guid InstanceId { get; } = Guid.NewGuid();

        public ISqlContext SqlContext => _scopeProvider.SqlContext;

        // a value indicating whether to force call-context
        public bool CallContext
        {
            get
            {
                if (_callContext) return true;
                if (ParentScope != null) return ParentScope.CallContext;
                return false;
            }
            set => _callContext = value;
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
        public IsolatedCaches IsolatedCaches
        {
            get
            {
                if (ParentScope != null) return ParentScope.IsolatedCaches;

                return _isolatedCaches ?? (_isolatedCaches
                           = new IsolatedCaches(type => new DeepCloneAppCache(new ObjectCacheAppCache())));
            }
        }

        // a value indicating whether the scope is detachable
        // ie whether it was created by CreateDetachedScope
        public bool Detachable { get; }

        // the parent scope (in a nested scopes chain)
        public Scope ParentScope { get; set; }

        public bool Attached { get; set; }

        // the original scope (when attaching a detachable scope)
        public Scope OrigScope { get; set; }

        // the original context (when attaching a detachable scope)
        public ScopeContext OrigContext { get; set; }

        // the context (for attaching & detaching only)
        public ScopeContext Context { get; }

        public IsolationLevel IsolationLevel
        {
            get
            {
                if (_isolationLevel != IsolationLevel.Unspecified) return _isolationLevel;
                if (ParentScope != null) return ParentScope.IsolationLevel;
                return SqlContext.SqlSyntax.DefaultIsolationLevel;
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
                    EnsureDbLocks();
                    return _database;
                }   

                if (ParentScope != null)
                {
                    var database = ParentScope.Database;
                    var currentLevel = database.GetCurrentTransactionIsolationLevel();
                    if (_isolationLevel > IsolationLevel.Unspecified && currentLevel < _isolationLevel)
                        throw new Exception("Scope requires isolation level " + _isolationLevel + ", but got " + currentLevel + " from parent.");
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

        public IUmbracoDatabase DatabaseOrNull
        {
            get
            {
                EnsureNotDisposed();
                if (ParentScope == null)
                {
                    if (_database != null)
                    {
                        EnsureDbLocks();
                    }
                    return _database;
                }

                return ParentScope.DatabaseOrNull;
            }
        }

        /// <inheritdoc />
        public EventMessages Messages
        {
            get
            {
                EnsureNotDisposed();
                if (ParentScope != null) return ParentScope.Messages;
                return _messages ?? (_messages = new EventMessages());

                // TODO: event messages?
                // this may be a problem: the messages collection will be cleared at the end of the scope
                // how shall we process it in controllers etc? if we don't want the global factory from v7?
                // it'd need to be captured by the controller
                //
                // + rename // EventMessages = ServiceMessages or something
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
                return _eventDispatcher ?? (_eventDispatcher = new QueuingEventDispatcher());
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
                    _logger.Debug<Scope, string>("Uncompleted Child Scope at\r\n {StackTrace}", Environment.StackTrace);

                _completed = false;
            }
        }

        /// <summary>
        /// When we require a ReadLock or a WriteLock we don't immediately request these locks from the database,
        /// instead we only request them when necessary (lazily).
        /// To do this, we queue requests for read/write locks.
        /// This is so that if there'a request for either of these
        /// locks, but the service/repository returns an item from the cache, we don't end up making a DB call to make the
        /// read/write lock.
        /// This executes the queue of requested locks in order in an efficient way lazily whenever the database instance is
        /// resolved.
        /// </summary>
        private void EnsureDbLocks()
        {
            lock (_lockQueueLocker)
            {
                if (_queuedLocks?.Count > 0)
                {
                    var currentType = LockType.ReadLock;
                    var currentTimeout = TimeSpan.Zero;
                    var collectedIds = new HashSet<int>();
                    for (int i = 0; i < _queuedLocks.Count; i++)
                    {
                        var entry = _queuedLocks[i];
                        if (i == 0)
                        {
                            currentType = entry.lockType;
                            currentTimeout = entry.timeout;
                        }
                        else if (entry.lockType != currentType || entry.timeout != currentTimeout)
                        {
                            // the lock type or timeout switched.
                            // process the lock ids collected
                            switch (currentType)
                            {
                                case LockType.ReadLock:
                                    ReadLockInner(InstanceId, currentTimeout == TimeSpan.Zero ? null : currentTimeout, collectedIds.ToArray());
                                    break;
                                case LockType.WriteLock:
                                    WriteLockInner(InstanceId, currentTimeout == TimeSpan.Zero ? null : currentTimeout, collectedIds.ToArray());
                                    break;
                            }
                            // clear the collected and set new type
                            collectedIds.Clear();
                            currentType = entry.lockType;
                            currentTimeout = entry.timeout;
                        }
                        collectedIds.Add(entry.lockId);
                    }
                    // process the remaining
                    switch (currentType)
                    {
                        case LockType.ReadLock:
                            ReadLockInner(InstanceId, currentTimeout == TimeSpan.Zero ? null : currentTimeout, collectedIds.ToArray());
                            break;
                        case LockType.WriteLock:
                            WriteLockInner(InstanceId, currentTimeout == TimeSpan.Zero ? null : currentTimeout, collectedIds.ToArray());
                            break;
                    }

                    _queuedLocks.Clear();
                }
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            // TODO: safer?
            //if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
            //    throw new ObjectDisposedException(GetType().FullName);
        }

        public void Dispose()
        {
            EnsureNotDisposed();

            if (this != _scopeProvider.AmbientScope)
            {
                var failedMessage = $"The {nameof(Scope)} {this.InstanceId} being disposed is not the Ambient {nameof(Scope)} {(_scopeProvider.AmbientScope?.InstanceId.ToString() ?? "NULL")}. This typically indicates that a child {nameof(Scope)} was not disposed, or flowed to a child thread that was not awaited, or concurrent threads are accessing the same {nameof(Scope)} (Ambient context) which is not supported. If using Task.Run (or similar) as a fire and forget tasks or to run threads in parallel you must suppress execution context flow with ExecutionContext.SuppressFlow() and ExecutionContext.RestoreFlow().";

#if DEBUG_SCOPES
                var ambient = _scopeProvider.AmbientScope;
                _logger.Debug<Scope>("Dispose error (" + (ambient == null ? "no" : "other") + " ambient)");
                if (ambient == null)
                    throw new InvalidOperationException("Not the ambient scope (no ambient scope).");
                var ambientInfos = _scopeProvider.GetScopeInfo(ambient);
                var disposeInfos = _scopeProvider.GetScopeInfo(this);
                throw new InvalidOperationException("Not the ambient scope (see ctor stack traces).\r\n"
                    + "- ambient ctor ->\r\n" + ambientInfos.CtorStack + "\r\n"
                    + "- dispose ctor ->\r\n" + disposeInfos.CtorStack + "\r\n");
#else
                throw new InvalidOperationException(failedMessage);
#endif
            }

            // Decrement the lock counters on the parent if any.
            ClearLocks(InstanceId);
            if (ParentScope is null)
            {
                // We're the parent scope, make sure that locks of all scopes has been cleared
                // Since we're only reading we don't have to be in a lock
                if (ReadLocks?.Count > 0 || WriteLocks?.Count > 0)
                {
                    var exception = new InvalidOperationException($"All scopes has not been disposed from parent scope: {InstanceId}, see log for more details.");
                    _logger.Error<Scope>(exception, GenerateUnclearedScopesLogMessage());
                    throw exception;
                }
            }

            var parent = ParentScope;
            _scopeProvider.AmbientScope = parent; // might be null = this is how scopes are removed from context objects

#if DEBUG_SCOPES
            _scopeProvider.Disposed(this);
#endif

            if (_autoComplete && _completed == null)
                _completed = true;

            if (parent != null)
                parent.ChildCompleted(_completed);
            else
                DisposeLastScope();

            lock(_lockQueueLocker)
            {
                _queuedLocks?.Clear();
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Generates a log message with all scopes that hasn't cleared their locks, including how many, and what locks they have requested.
        /// </summary>
        /// <returns>Log message.</returns>
        private string GenerateUnclearedScopesLogMessage()
        {
            // Dump the dicts into a message for the locks.
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Lock counters aren't empty, suggesting a scope hasn't been properly disposed, parent id: {InstanceId}");
            WriteLockDictionaryToString(ReadLocks, builder, "read locks");
            WriteLockDictionaryToString(WriteLocks, builder, "write locks");
            return builder.ToString();
        }

        /// <summary>
        /// Writes a locks dictionary to a <see cref="StringBuilder"/> for logging purposes.
        /// </summary>
        /// <param name="dict">Lock dictionary to report on.</param>
        /// <param name="builder">String builder to write to.</param>
        /// <param name="dictName">The name to report the dictionary as.</param>
        private void WriteLockDictionaryToString(Dictionary<Guid, Dictionary<int, int>> dict, StringBuilder builder, string dictName)
        {
            if (dict?.Count > 0)
            {
                builder.AppendLine($"Remaining {dictName}:");
                foreach (var instance in dict)
                {
                    builder.AppendLine($"Scope {instance.Key}");
                    foreach (var lockCounter in instance.Value)
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
                if (onException == false)
                    _eventDispatcher?.ScopeExit(completed);
            }, () =>
            {
                // if *we* created it, then get rid of it
                if (_scopeProvider.AmbientContext == Context)
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
        private static bool LogUncompletedScopes => (_logUncompletedScopes
            ?? (_logUncompletedScopes = Current.Configs.CoreDebug().LogUncompletedScopes)).Value;

        /// <summary>
        /// Increment the counter of a locks dictionary, either ReadLocks or WriteLocks,
        /// for a specific scope instance and lock identifier. Must be called within a lock.
        /// </summary>
        /// <param name="lockId">Lock ID to increment.</param>
        /// <param name="instanceId">Instance ID of the scope requesting the lock.</param>
        /// <param name="locks">Reference to the dictionary to increment on</param>
        private void IncrementLock(int lockId, Guid instanceId, ref Dictionary<Guid, Dictionary<int, int>> locks)
        {
            // Since we've already checked that we're the parent in the WriteLockInner method, we don't need to check again.
            // If it's the very first time a lock has been requested the WriteLocks dict hasn't been instantiated yet.
            locks ??= new Dictionary<Guid, Dictionary<int, int>>();

            // Try and get the dict associated with the scope id.
            var locksDictFound = locks.TryGetValue(instanceId, out var locksDict);
            if (locksDictFound)
            {
                locksDict.TryGetValue(lockId, out var value);
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
        /// Clears all lock counters for a given scope instance, signalling that the scope has been disposed.
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
                    ReadLocks?.Remove(instanceId);
                    WriteLocks?.Remove(instanceId);
                }
            }
        }

        /// <inheritdoc />
        public void ReadLock(params int[] lockIds)
        {
            lock (_lockQueueLocker)
            {
                if (_queuedLocks == null)
                {
                    _queuedLocks = new List<(LockType, TimeSpan, int)>();
                }
                foreach(var lockId in lockIds)
                {
                    _queuedLocks.Add((LockType.ReadLock, TimeSpan.Zero, lockId));
                }
            }
        }

        /// <inheritdoc />
        public void ReadLock(TimeSpan timeout, int lockId)
        {
            lock (_lockQueueLocker)
            {
                if (_queuedLocks == null)
                {
                    _queuedLocks = new List<(LockType, TimeSpan, int)>();
                }
                _queuedLocks.Add((LockType.ReadLock, timeout, lockId));
            }
        }

        /// <inheritdoc />
        public void WriteLock(params int[] lockIds)
        {
            lock (_lockQueueLocker)
            {
                if (_queuedLocks == null)
                {
                    _queuedLocks = new List<(LockType, TimeSpan, int)>();
                }
                foreach (var lockId in lockIds)
                {
                    _queuedLocks.Add((LockType.WriteLock, TimeSpan.Zero, lockId));
                }
            }
        }

        /// <inheritdoc />
        public void WriteLock(TimeSpan timeout, int lockId)
        {
            lock (_lockQueueLocker)
            {
                if (_queuedLocks == null)
                {
                    _queuedLocks = new List<(LockType, TimeSpan, int)>();
                }
                _queuedLocks.Add((LockType.WriteLock, timeout, lockId));
            }
        }

        /// <summary>
        /// Handles acquiring a read lock, will delegate it to the parent if there are any.
        /// </summary>
        /// <param name="instanceId">Instance ID of the requesting scope.</param>
        /// <param name="timeout">Optional database timeout in milliseconds.</param>
        /// <param name="lockIds">Array of lock object identifiers.</param>
        private void ReadLockInner(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds)
        {
            if (ParentScope is not null)
            {
                // If we have a parent we delegate lock creation to parent.
                ParentScope.ReadLockInner(instanceId, timeout, lockIds);
            }
            else
            {
                // We are the outermost scope, handle the lock request.
                LockInner(instanceId, ref ReadLocks, ref _readLocks, ObtainReadLock, ObtainTimeoutReadLock, timeout, lockIds);
            }
        }

        /// <summary>
        /// Handles acquiring a write lock with a specified timeout, will delegate it to the parent if there are any.
        /// </summary>
        /// <param name="instanceId">Instance ID of the requesting scope.</param>
        /// <param name="timeout">Optional database timeout in milliseconds.</param>
        /// <param name="lockIds">Array of lock object identifiers.</param>
        private void WriteLockInner(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds)
        {
            if (ParentScope is not null)
            {
                // If we have a parent we delegate lock creation to parent.
                ParentScope.WriteLockInner(instanceId, timeout, lockIds);
            }
            else
            {
                // We are the outermost scope, handle the lock request.
                LockInner(instanceId, ref WriteLocks, ref _writeLocks, ObtainWriteLock, ObtainTimeoutWriteLock, timeout, lockIds);
            }
        }

        /// <summary>
        /// Handles acquiring a lock, this should only be called from the outermost scope.
        /// </summary>
        /// <param name="instanceId">Instance ID of the scope requesting the lock.</param>
        /// <param name="locks">Reference to the applicable locks dictionary (ReadLocks or WriteLocks).</param>
        /// <param name="locksSet">Reference to the applicable locks hashset (_readLocks or _writeLocks).</param>
        /// <param name="obtainLock">Delegate used to request the lock from the database without a timeout.</param>
        /// <param name="obtainLockTimeout">Delegate used to request the lock from the database with a timeout.</param>
        /// <param name="timeout">Optional timeout parameter to specify a timeout.</param>
        /// <param name="lockIds">Lock identifiers to lock on.</param>
        private void LockInner(Guid instanceId, ref Dictionary<Guid, Dictionary<int, int>> locks, ref HashSet<int> locksSet,
            Action<int> obtainLock, Action<int, TimeSpan> obtainLockTimeout, TimeSpan? timeout = null,
            params int[] lockIds)
        {
            lock (_dictionaryLocker)
            {
                locksSet ??= new HashSet<int>();
                foreach (var lockId in lockIds)
                {
                    // Only acquire the lock if we haven't done so yet.
                    if (!locksSet.Contains(lockId))
                    {
                        IncrementLock(lockId, instanceId, ref locks);
                        locksSet.Add(lockId);
                        try
                        {
                            if (timeout is null)
                            {
                                // We just want an ordinary lock.
                                obtainLock(lockId);
                            }
                            else
                            {
                                // We want a lock with a custom timeout
                                obtainLockTimeout(lockId, timeout.Value);
                            }
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
                    else
                    {
                        // We already have a lock, but need to update the dictionary for debugging purposes.
                        IncrementLock(lockId, instanceId, ref locks);
                    }
                }
            }
        }

        /// <summary>
        /// Obtains an ordinary read lock.
        /// </summary>
        /// <param name="lockId">Lock object identifier to lock.</param>
        private void ObtainReadLock(int lockId)
        {
            SqlContext.SqlSyntax.ReadLock(_database, lockId);
        }

        /// <summary>
        /// Obtains a read lock with a custom timeout.
        /// </summary>
        /// <param name="lockId">Lock object identifier to lock.</param>
        /// <param name="timeout">TimeSpan specifying the timout period.</param>
        private void ObtainTimeoutReadLock(int lockId, TimeSpan timeout)
        {
            var syntax2 = SqlContext.SqlSyntax as ISqlSyntaxProvider2;
            if (syntax2 is null)
            {
                throw new InvalidOperationException($"{SqlContext.SqlSyntax.GetType()} is not of type {typeof(ISqlSyntaxProvider2)}");
            }

            syntax2.ReadLock(_database, timeout, lockId);
        }

        /// <summary>
        /// Obtains an ordinary write lock.
        /// </summary>
        /// <param name="lockId">Lock object identifier to lock.</param>
        private void ObtainWriteLock(int lockId)
        {
            SqlContext.SqlSyntax.WriteLock(_database, lockId);
        }

        /// <summary>
        /// Obtains a write lock with a custom timeout.
        /// </summary>
        /// <param name="lockId">Lock object identifier to lock.</param>
        /// <param name="timeout">TimeSpan specifying the timout period.</param>
        private void ObtainTimeoutWriteLock(int lockId, TimeSpan timeout)
        {
            var syntax2 = SqlContext.SqlSyntax as ISqlSyntaxProvider2;
            if (syntax2 is null)
            {
                throw new InvalidOperationException($"{SqlContext.SqlSyntax.GetType()} is not of type {typeof(ISqlSyntaxProvider2)}");
            }

            syntax2.WriteLock(_database, timeout, lockId);
        }
    }
}
