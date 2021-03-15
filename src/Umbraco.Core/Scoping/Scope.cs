using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

        // ReadLocks and WriteLocks if we're the outer most scope it's those owned by the entire chain
        // If we're a child scope it's those that we have requested.
        internal readonly Dictionary<Guid, Dictionary<int, int>> ReadLocks;
        internal readonly Dictionary<Guid, Dictionary<int, int>> WriteLocks;

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
            ReadLocks = new Dictionary<Guid, Dictionary<int, int>>();
            WriteLocks = new Dictionary<Guid, Dictionary<int, int>>();

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
                return Database.SqlContext.SqlSyntax.DefaultIsolationLevel;
            }
        }

        /// <inheritdoc />
        public IUmbracoDatabase Database
        {
            get
            {
                EnsureNotDisposed();

                if (_database != null)
                    return _database;

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
                    _logger.Debug<Scope>("Uncompleted Child Scope at\r\n {StackTrace}", Environment.StackTrace);

                _completed = false;
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
                throw new InvalidOperationException("Not the ambient scope.");
#endif
            }

            // Decrement the lock counters on the parent if any.
            ClearReadLocks(InstanceId);
            ClearWriteLocks(InstanceId);
            if (ParentScope is null)
            {
                // We're the parent scope, make sure that locks of all scopes has been cleared
                // Since we're only reading we don't have to be in a lock
                if (ReadLocks.Values.Any(x => x.Values.Any(value => value != 0))
                    || WriteLocks.Values.Any(x => x.Values.Any(value => value != 0)))
                {
                    throw new InvalidOperationException($"All scopes has not been disposed from parent scope: {InstanceId}");
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
        /// Increment the counter in WriteLocks for a specific scope instance and lock identifier.
        /// </summary>
        /// <remarks>
        /// This will not lock on the lock object, ensure that you lock on _dictionaryLocker before calling this method.
        /// </remarks>
        /// <param name="lockId">Lock ID to increment.</param>
        /// <param name="instanceId">Instance ID of the scope requesting the lock.</param>
        private void IncrementWriteLock(int lockId, Guid instanceId)
        {
            // Since we've already checked that we're the parent in the WriteLockInner method, we don't need to check again.
            // Try and get the dict associated with the scope id.
            var locksDictFound = WriteLocks.TryGetValue(instanceId, out var locksDict);
            if (locksDictFound)
            {
                locksDict.TryGetValue(lockId, out var value);
                WriteLocks[instanceId][lockId] = value + 1;
            }
            else
            {
                // The scope hasn't requested a lock yet, so we have to create a dict for it.
                WriteLocks[instanceId] = new Dictionary<int, int>();
                WriteLocks[instanceId][lockId] = 1;
            }
        }

        /// <summary>
        /// Increment the counter in ReadLocks for a specific scope instance and lock identifier.
        /// </summary>
        /// <remarks>
        /// This will not lock on the lock object, ensure that you lock on _dictionaryLocker before calling this method.
        /// </remarks>
        /// <param name="lockId">Lock ID to increment.</param>
        /// <param name="instanceId">Instance ID of the scope requesting the lock.</param>
        private void IncrementReadLock(int lockId, Guid instanceId)
        {
            // Since we've already checked that we're the parent in the WriteLockInner method, we don't need to check again.
            var locksDictFound = ReadLocks.TryGetValue(instanceId, out var locksDict);
            if (locksDictFound)
            {
                locksDict.TryGetValue(lockId, out var value);
                ReadLocks[instanceId][lockId] = value + 1;
            }
            else
            {
                // The scope hasn't requested a lock yet, so we have to create a dict for it.
                ReadLocks[instanceId] = new Dictionary<int, int>();
                ReadLocks[instanceId][lockId] = 1;
            }
        }

        /// <summary>
        /// Resets all read lock counters for a given scope instance to 0, signalling that the lock is no longer in use.
        /// </summary>
        /// <param name="instanceId">Instance ID of the scope to clear of lock counters.</param>
        private void ClearReadLocks(Guid instanceId)
        {
            if (ParentScope != null)
            {
                ParentScope.ClearReadLocks(instanceId);
            }
            else
            {
                lock (_dictionaryLocker)
                {
                    // Reset all values to 0 since the scope is being disposed
                    if (ReadLocks.ContainsKey(instanceId))
                    {
                        foreach (var key in ReadLocks[instanceId].Keys.ToList())
                        {
                            ReadLocks[instanceId][key] = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resets all write lock counters for a given scope instance to 0, signalling that the lock is no longer in use.
        /// </summary>
        /// <param name="instanceID">Instance ID of the scope to clear of lock counters.</param>
        private void ClearWriteLocks(Guid instanceID)
        {
            if (ParentScope != null)
            {
                ParentScope.ClearWriteLocks(instanceID);
            }
            else
            {
                lock (_dictionaryLocker)
                {
                    // Reset all values to 0 since the scope is being disposed
                    if (WriteLocks.ContainsKey(instanceID))
                    {
                        foreach (var key in WriteLocks[instanceID].Keys.ToList())
                        {
                            WriteLocks[instanceID][key] = 0;
                        }
                    }
                }

            }
        }

        /// <inheritdoc />
        public void ReadLock(params int[] lockIds)
        {
            ReadLockInner(InstanceId, null, lockIds);
        }

        /// <inheritdoc />
        public void ReadLock(TimeSpan timeout, int lockId)
        {
            ReadLockInner(InstanceId, timeout, lockId);
        }

        /// <inheritdoc />
        public void WriteLock(params int[] lockIds)
        {
            WriteLockInner(InstanceId, null, lockIds);
        }

        /// <inheritdoc />
        public void WriteLock(TimeSpan timeout, int lockId)
        {
            WriteLockInner(InstanceId, timeout, lockId);
        }

        /// <summary>
        /// Determine if a read lock with the specified ID has already been obtained.
        /// </summary>
        /// <param name="lockId">Id to test.</param>
        /// <returns>True if no scopes has obtained a read lock with the specific ID yet.</returns>
        private bool HasReadLock(int lockId)
        {
            // Check if there is any dictionary<int,int> with a key equal to lockId
            // And check that the value associated with that key is greater than 0, if not it could be because a lock was requested but it failed.
            return ReadLocks.Values.Any(x => x.ContainsKey(lockId));
        }

        /// <summary>
        /// Determine if a write lock with the specified ID has already been obtained.
        /// </summary>
        /// <param name="lockId">Id to test</param>
        /// <returns>>True if no scopes has obtained a write lock with the specific ID yet.</returns>
        private bool HasWriteLock(int lockId)
        {
            return WriteLocks.Values.Any(x => x.ContainsKey(lockId));
        }

        /// <summary>
        /// Handles acquiring a read lock, will delegate it to the parent if there are any.
        /// </summary>
        /// <param name="timeout">Optional database timeout in milliseconds.</param>
        /// <param name="lockIds">Array of lock object identifiers.</param>
        private void ReadLockInner(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds)
        {
            if (ParentScope != null)
            {
                // Delegate acquiring the lock to the parent if any.
                ParentScope.ReadLockInner(instanceId, timeout, lockIds);
                return;
            }

            lock (_dictionaryLocker)
            {
                // If we are the parent, then handle the lock request.
                foreach (var lockId in lockIds)
                {
                    // Only acquire the lock if we haven't done so yet.
                    if (!HasReadLock(lockId))
                    {
                        IncrementReadLock(lockId, instanceId);
                        try
                        {
                            if (timeout is null)
                            {
                                // We just want an ordinary lock.
                                ObtainReadLock(lockId);
                            }
                            else
                            {
                                // We want a lock with a custom timeout
                                ObtainTimoutReadLock(lockId, timeout.Value);
                            }
                        }
                        catch
                        {
                            // Something went wrong and we didn't get the lock
                            // Since we at this point have determined that we haven't got any key of LockID, it's safe to completely remove it instead of decrementing.
                            // It needs to be completely removed, because that's how we determine to acquire a lock.
                            ReadLocks[instanceId].Remove(lockId);
                            throw;
                        }
                    }
                    else
                    {
                        // We already have a lock, but need to update the readlock dictionary for debugging purposes.
                        IncrementReadLock(lockId, instanceId);
                    }
                }
            }
        }

        /// <summary>
        /// Handles acquiring a write lock with a specified timeout, will delegate it to the parent if there are any.
        /// </summary>
        /// <param name="timeout">Optional database timeout in milliseconds.</param>
        /// <param name="lockIds">Array of lock object identifiers.</param>
        internal void WriteLockInner(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds)
        {
            if (ParentScope != null)
            {
                // If we have a parent we delegate lock creation to parent.
                ParentScope.WriteLockInner(instanceId, timeout, lockIds);
                return;
            }

            lock (_dictionaryLocker)
            {
                foreach (var lockId in lockIds)
                {
                    // Only acquire lock if we haven't yet (WriteLocks not containing the key)
                    if (!HasWriteLock(lockId))
                    {
                        IncrementWriteLock(lockId, instanceId);
                        try
                        {
                            if (timeout is null)
                            {
                                ObtainWriteLock(lockId);
                            }
                            else
                            {
                                ObtainTimeoutWriteLock(lockId, timeout.Value);
                            }
                        }
                        catch
                        {
                            // Something went wrong and we didn't get the lock
                            // Since we at this point have determined that we haven't got any key of LockID, it's safe to completely remove it instead of decrementing.
                            // It needs to be completely removed, because that's how we determine to acquire a lock.
                            WriteLocks[instanceId].Remove(lockId);
                            throw;
                        }
                    }
                    else
                    {
                        // We already have a lock, so just increment the count for debugging purposes
                        IncrementWriteLock(lockId, instanceId);
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
            Database.SqlContext.SqlSyntax.ReadLock(Database, lockId);
        }

        /// <summary>
        /// Obtains a read lock with a custom timeout.
        /// </summary>
        /// <param name="lockId">Lock object identifier to lock.</param>
        /// <param name="timeout">TimeSpan specifying the timout period.</param>
        private void ObtainTimoutReadLock(int lockId, TimeSpan timeout)
        {
            var syntax2 = Database.SqlContext.SqlSyntax as ISqlSyntaxProvider2;
            if (syntax2 == null)
            {
                throw new InvalidOperationException($"{Database.SqlContext.SqlSyntax.GetType()} is not of type {typeof(ISqlSyntaxProvider2)}");
            }

            syntax2.ReadLock(Database, timeout, lockId);
        }

        /// <summary>
        /// Obtains an ordinary write lock.
        /// </summary>
        /// <param name="lockId">Lock object identifier to lock.</param>
        private void ObtainWriteLock(int lockId)
        {
            Database.SqlContext.SqlSyntax.WriteLock(Database, lockId);
        }

        /// <summary>
        /// Obtains a write lock with a custom timeout.
        /// </summary>
        /// <param name="lockId">Lock object identifier to lock.</param>
        /// <param name="timeout">TimeSpan specifying the timout period.</param>
        private void ObtainTimeoutWriteLock(int lockId, TimeSpan timeout)
        {
            var syntax2 = Database.SqlContext.SqlSyntax as ISqlSyntaxProvider2;
            if (syntax2 == null)
            {
                throw new InvalidOperationException($"{Database.SqlContext.SqlSyntax.GetType()} is not of type {typeof(ISqlSyntaxProvider2)}");
            }

            syntax2.WriteLock(Database, timeout, lockId);
        }
    }
}
