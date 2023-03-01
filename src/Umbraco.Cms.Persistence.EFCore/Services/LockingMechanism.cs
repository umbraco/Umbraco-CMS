using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.Services;

/// <summary>
/// Mechanism for handling read and write locks
/// </summary>
public class LockingMechanism : ILockingMechanism
{
    private readonly IDistributedLockingMechanismFactory _distributedLockingMechanismFactory;
    private readonly object _lockQueueLocker = new();
    private readonly object _dictionaryLocker = new();
    private StackQueue<(DistributedLockType lockType, TimeSpan timeout, Guid instanceId, int lockId)>? _queuedLocks;
    private HashSet<int>? _readLocks;
    private Dictionary<Guid, Dictionary<int, int>>? _readLocksDictionary;
    private HashSet<int>? _writeLocks;
    private Dictionary<Guid, Dictionary<int, int>>? _writeLocksDictionary;
    private Queue<IDistributedLock>? _acquiredLocks;

    public LockingMechanism(IDistributedLockingMechanismFactory distributedLockingMechanismFactory)
    {
        _distributedLockingMechanismFactory = distributedLockingMechanismFactory;
        _acquiredLocks = new Queue<IDistributedLock>();
    }

    public void ReadLock(Guid instanceId, params int[] lockIds) => LazyReadLockInner(instanceId, lockIds);

    public void WriteLock(Guid instanceId, params int[] lockIds) => LazyWriteLockInner(instanceId, lockIds);

    public void EagerReadLock(Guid instanceId, params int[] lockIds) => EagerReadLockInner(instanceId, null, lockIds);

    public void EagerWriteLock(Guid instanceId, params int[] lockIds) => EagerWriteLockInner(instanceId, null, lockIds);

    /// <summary>
    ///     Handles acquiring a write lock with a specified timeout, will delegate it to the parent if there are any.
    /// </summary>
    /// <param name="instanceId">Instance ID of the requesting scope.</param>
    /// <param name="timeout">Optional database timeout in milliseconds.</param>
    /// <param name="lockIds">Array of lock object identifiers.</param>
    private void EagerWriteLockInner(Guid instanceId, TimeSpan? timeout, params int[] lockIds)
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

    /// <summary>
    ///     Obtains a write lock with a custom timeout.
    /// </summary>
    /// <param name="lockId">Lock object identifier to lock.</param>
    /// <param name="timeout">TimeSpan specifying the timout period.</param>
    private void ObtainWriteLock(int lockId, TimeSpan? timeout)
    {
        if (_acquiredLocks == null)
        {
            throw new InvalidOperationException(
                $"Cannot obtain a write lock as the {nameof(_acquiredLocks)} queue is null.");
        }

        _acquiredLocks.Enqueue(_distributedLockingMechanismFactory.DistributedLockingMechanism.WriteLock(lockId, timeout));
    }

    /// <summary>
    ///     Handles acquiring a read lock, will delegate it to the parent if there are any.
    /// </summary>
    /// <param name="instanceId">The id of the scope requesting the lock.</param>
    /// <param name="timeout">Optional database timeout in milliseconds.</param>
    /// <param name="lockIds">Array of lock object identifiers.</param>
    private void EagerReadLockInner(Guid instanceId, TimeSpan? timeout, params int[] lockIds)
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

    /// <summary>
    ///     Obtains a read lock with a custom timeout.
    /// </summary>
    /// <param name="lockId">Lock object identifier to lock.</param>
    /// <param name="timeout">TimeSpan specifying the timout period.</param>
    private void ObtainReadLock(int lockId, TimeSpan? timeout)
    {
        if (_acquiredLocks == null)
        {
            throw new InvalidOperationException(
                $"Cannot obtain a read lock as the {nameof(_acquiredLocks)} queue is null.");
        }

        _acquiredLocks.Enqueue(
            _distributedLockingMechanismFactory.DistributedLockingMechanism.ReadLock(lockId, timeout));
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

    private void LazyWriteLockInner(Guid instanceId, params int[] lockIds) =>
        LazyLockInner(DistributedLockType.WriteLock, instanceId, lockIds);

    private void LazyReadLockInner(Guid instanceId, params int[] lockIds) =>
        LazyLockInner(DistributedLockType.ReadLock, instanceId, lockIds);

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

    /// <summary>
    ///     Clears all lock counters for a given scope instance, signalling that the scope has been disposed.
    /// </summary>
    /// <param name="instanceId">Instance ID of the scope to clear.</param>
    public void ClearLocks(Guid instanceId)
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
    public void EnsureDbLocks(Guid scopeInstanceId)
    {
        lock (_lockQueueLocker)
        {
            if (_queuedLocks?.Count > 0)
            {
                DistributedLockType currentType = DistributedLockType.ReadLock;
                TimeSpan currentTimeout = TimeSpan.Zero;
                Guid currentInstanceId = scopeInstanceId;
                var collectedIds = new HashSet<int>();

                var i = 0;
                while (_queuedLocks.Count > 0)
                {
                    (DistributedLockType lockType, TimeSpan timeout, Guid instanceId, var lockId) =
                        _queuedLocks.Dequeue();

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
                                EagerReadLockInner(
                                    currentInstanceId,
                                    currentTimeout == TimeSpan.Zero ? null : currentTimeout,
                                    collectedIds.ToArray());
                                break;
                            case DistributedLockType.WriteLock:
                                EagerWriteLockInner(
                                    currentInstanceId,
                                    currentTimeout == TimeSpan.Zero ? null : currentTimeout,
                                    collectedIds.ToArray());
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
                        EagerReadLockInner(
                            currentInstanceId,
                            currentTimeout == TimeSpan.Zero ? null : currentTimeout,
                            collectedIds.ToArray());
                        break;
                    case DistributedLockType.WriteLock:
                        EagerWriteLockInner(
                            currentInstanceId,
                            currentTimeout == TimeSpan.Zero ? null : currentTimeout,
                            collectedIds.ToArray());
                        break;
                }
            }
        }
    }

    public void Dispose()
    {
        while (!_acquiredLocks?.IsCollectionEmpty() ?? false)
        {
            _acquiredLocks?.Dequeue().Dispose();
        }

        // We're the parent scope, make sure that locks of all scopes has been cleared
        // Since we're only reading we don't have to be in a lock
        if (_readLocksDictionary?.Count > 0 || _writeLocksDictionary?.Count > 0)
        {
            throw new InvalidOperationException($"All scopes has not been disposed from parent scope.");
        }
    }
}
