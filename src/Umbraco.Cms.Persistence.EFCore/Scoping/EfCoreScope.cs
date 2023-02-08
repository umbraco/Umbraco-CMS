using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Persistence.EFCore.Entities;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal class EfCoreScope : IEfCoreScope
{
    private readonly IUmbracoEfCoreDatabaseFactory _efCoreDatabaseFactory;
    private readonly IEFCoreScopeAccessor _efCoreScopeAccessor;
    private readonly EfCoreScopeProvider _efCoreScopeProvider;
    private IUmbracoEfCoreDatabase? _umbracoEfCoreDatabase;
    private bool? _completed;
    private bool _disposed;

    // This is all used to safely track read/write locks at given Scope levels so that
    // when we dispose we can verify that everything has been cleaned up correctly
    private readonly object _dictionaryLocker;
    private StackQueue<(DistributedLockType lockType, TimeSpan timeout, Guid instanceId, int lockId)>? _queuedLocks;
    private HashSet<int>? _readLocks;
    private Dictionary<Guid, Dictionary<int, int>>? _readLocksDictionary;
    private HashSet<int>? _writeLocks;
    private Dictionary<Guid, Dictionary<int, int>>? _writeLocksDictionary;
    private Queue<IDistributedLock>? _acquiredLocks;

    public Guid InstanceId { get; }

    public EfCoreScope? ParentScope { get; }

    public IScopeContext? ScopeContext { get; set; }

    public EfCoreScope(
        IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory,
        IEFCoreScopeAccessor efCoreScopeAccessor,
        IEfCoreScopeProvider efCoreScopeProvider,
        IScopeContext? scopeContext)
    {
        _dictionaryLocker = new object();
        _efCoreDatabaseFactory = efCoreDatabaseFactory;
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _efCoreScopeProvider = (EfCoreScopeProvider)efCoreScopeProvider;
        _acquiredLocks = new Queue<IDistributedLock>();
        ScopeContext = scopeContext;

        InstanceId = Guid.NewGuid();
    }

    public EfCoreScope(
        IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory,
        IEFCoreScopeAccessor efCoreScopeAccessor,
        IEfCoreScopeProvider efCoreScopeProvider,
        EfCoreScope parentScope,
        IScopeContext? scopeContext)
        : this(
            efCoreDatabaseFactory,
            efCoreScopeAccessor,
            efCoreScopeProvider,
            scopeContext) =>
        ParentScope = parentScope;

    public async Task<T> ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task<T>> method)
    {
        if (_disposed)
        {
            throw new InvalidOperationException("The scope has been disposed, therefore the database is not available.");
        }

        if (_umbracoEfCoreDatabase is null)
        {
            InitializeDatabase();
        }

        return await method(_umbracoEfCoreDatabase!.UmbracoEFContext);
    }

    public async Task ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task> method) =>
        await ExecuteWithContextAsync(async db =>
        {
            await method(db);
            return true; // Do nothing
        });

    public void Complete()
    {
        if (_completed.HasValue == false)
        {
            _completed = true;
        }
    }

    public void Reset() => _completed = null;

    public void Dispose()
    {
        if (this != _efCoreScopeAccessor.AmbientScope)
        {
            var failedMessage =
                $"The {nameof(EfCoreScope)} {InstanceId} being disposed is not the Ambient {nameof(EfCoreScope)} {_efCoreScopeAccessor.AmbientScope?.InstanceId.ToString() ?? "NULL"}. This typically indicates that a child {nameof(EfCoreScope)} was not disposed, or flowed to a child thread that was not awaited, or concurrent threads are accessing the same {nameof(EfCoreScope)} (Ambient context) which is not supported. If using Task.Run (or similar) as a fire and forget tasks or to run threads in parallel you must suppress execution context flow with ExecutionContext.SuppressFlow() and ExecutionContext.RestoreFlow().";
            throw new InvalidOperationException(failedMessage);
        }

        // Decrement the lock counters on the parent if any.
        ClearLocks(InstanceId);

        if (ParentScope is null)
        {
            DisposeEfCoreDatabase();
        }
        else
        {
            ParentScope.ChildCompleted(_completed);
        }

        _efCoreScopeProvider.PopAmbientScope();

        // if *we* created it, then get rid of it
        if (_efCoreScopeProvider.AmbientScopeContext == ScopeContext)
        {
            try
            {
                _efCoreScopeProvider.AmbientScopeContext?.ScopeExit(_completed.HasValue && _completed.Value);
            }
            finally
            {
                // removes the ambient context (ambient scope already gone)
                _efCoreScopeProvider.PopAmbientScopeContext();
            }
        }

        _disposed = true;
    }

    public void ChildCompleted(bool? completed)
    {
        // if child did not complete we cannot complete
        if (completed.HasValue == false || completed.Value == false)
        {
            _completed = false;
        }
    }

    public void EagerReadLock(params int[] lockIds) => EagerReadLockInner(InstanceId, null, lockIds);

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

        _acquiredLocks.Enqueue(_efCoreScopeProvider.DistributedLockingMechanismFactory.DistributedLockingMechanism.ReadLock(lockId, timeout));
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
        ref HashSet<int>? locksSet,
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


    private void InitializeDatabase()
    {
        _umbracoEfCoreDatabase = _efCoreDatabaseFactory.Create();

        // Check if we are already in a transaction before starting one
        if (_umbracoEfCoreDatabase.UmbracoEFContext.Database.CurrentTransaction is null)
        {
            _umbracoEfCoreDatabase.UmbracoEFContext.Database.BeginTransaction();
        }
    }

    private void DisposeEfCoreDatabase()
    {
        var completed = _completed.HasValue && _completed.Value;
        if (_umbracoEfCoreDatabase is not null)
        {
            if (completed)
            {
                _umbracoEfCoreDatabase.UmbracoEFContext.Database.CommitTransaction();
            }
            else
            {
                _umbracoEfCoreDatabase.UmbracoEFContext.Database.RollbackTransaction();
            }

            // _umbracoEfCoreDatabase.Dispose();
        }

        // _efCoreDatabaseFactory.Dispose();

    }
}
