using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Scoping;

/// <summary>
/// Mechanism for handling read and write locks.
/// </summary>
public class LockingMechanism : ILockingMechanism
{
    private readonly IDistributedLockingMechanismFactory _distributedLockingMechanismFactory;
    private readonly ILogger<LockingMechanism> _logger;
    private readonly Lock _locker = new();
    private StackQueue<(DistributedLockType lockType, TimeSpan timeout, Guid instanceId, int lockId)>? _queuedLocks;
    private HashSet<int>? _readLocks;
    private Dictionary<Guid, Dictionary<int, int>>? _readLocksDictionary;
    private HashSet<int>? _writeLocks;
    private Dictionary<Guid, Dictionary<int, int>>? _writeLocksDictionary;
    private Queue<IDistributedLock>? _acquiredLocks;

    /// <summary>
    /// Constructs an instance of LockingMechanism
    /// </summary>
    /// <param name="distributedLockingMechanismFactory"></param>
    /// <param name="logger"></param>
    public LockingMechanism(IDistributedLockingMechanismFactory distributedLockingMechanismFactory, ILogger<LockingMechanism> logger)
    {
        _distributedLockingMechanismFactory = distributedLockingMechanismFactory;
        _logger = logger;
        _acquiredLocks = new Queue<IDistributedLock>();
    }

    /// <inheritdoc />
    public void ReadLock(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds) => EagerReadLockInner(instanceId, timeout, lockIds);

    public void ReadLock(Guid instanceId, params int[] lockIds) => ReadLock(instanceId, null, lockIds);

    /// <inheritdoc />
    public void WriteLock(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds) => EagerWriteLockInner(instanceId, timeout, lockIds);

    public void WriteLock(Guid instanceId, params int[] lockIds) => WriteLock(instanceId, null, lockIds);

    /// <inheritdoc />
    public void EagerReadLock(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds) => EagerReadLockInner(instanceId, timeout, lockIds);

    public void EagerReadLock(Guid instanceId, params int[] lockIds) =>
        EagerReadLock(instanceId, null, lockIds);

    /// <inheritdoc />
    public void EagerWriteLock(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds) => EagerWriteLockInner(instanceId, timeout, lockIds);

    public void EagerWriteLock(Guid instanceId, params int[] lockIds) =>
        EagerWriteLock(instanceId, null, lockIds);

    /// <summary>
    ///     Handles acquiring a write lock with a specified timeout, will delegate it to the parent if there are any.
    /// </summary>
    /// <param name="instanceId">Instance ID of the requesting scope.</param>
    /// <param name="timeout">Optional database timeout in milliseconds.</param>
    /// <param name="lockIds">Array of lock object identifiers.</param>
    private void EagerWriteLockInner(Guid instanceId, TimeSpan? timeout, params int[] lockIds)
    {
        lock (_locker)
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
        lock (_locker)
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
    /// <remarks>Internal for tests.</remarks>
    internal static void IncrementLock(int lockId, Guid instanceId, ref Dictionary<Guid, Dictionary<int, int>>? locks)
    {
        // Since we've already checked that we're the parent in the WriteLockInner method, we don't need to check again.
        // If it's the very first time a lock has been requested the WriteLocks dictionary hasn't been instantiated yet.
        locks ??= [];

        // Try and get the dictionary associated with the scope id.

        // The following code is a micro-optimization.
        // GetValueRefOrAddDefault does lookup or creation with only one hash key generation, internal bucket lookup and value lookup in the bucket.
        // This compares to doing it twice when initializing, one for the lookup and one for the insertion of the initial value, we had with the
        // previous code:
        //   var locksDictFound = locks.TryGetValue(instanceId, out Dictionary<int, int>? locksDict);
        //   if (locksDictFound)
        //   {
        //       locksDict!.TryGetValue(lockId, out var value);
        //       locksDict[lockId] = value + 1;
        //   }
        //   else
        //   {
        //       // The scope hasn't requested a lock yet, so we have to create a dict for it.
        //       locks.Add(instanceId, new Dictionary<int, int>());
        //       locks[instanceId][lockId] = 1;
        //   }

        ref Dictionary<int, int>? locksDict = ref CollectionsMarshal.GetValueRefOrAddDefault(locks, instanceId, out bool locksDictFound);
        if (locksDictFound)
        {
            // By getting a reference to any existing or default 0 value, we can increment it without the expensive write back into the dictionary.
            ref int value = ref CollectionsMarshal.GetValueRefOrAddDefault(locksDict!, lockId, out _);
            value++;
        }
        else
        {
            // The scope hasn't requested a lock yet, so we have to create a dictionary for it.
            locksDict = new Dictionary<int, int> { { lockId, 1 } };
        }
    }

    private void LazyWriteLockInner(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds) =>
        LazyLockInner(DistributedLockType.WriteLock, instanceId, timeout, lockIds);

    private void LazyReadLockInner(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds) =>
        LazyLockInner(DistributedLockType.ReadLock, instanceId, timeout, lockIds);

    private void LazyLockInner(DistributedLockType lockType, Guid instanceId, TimeSpan? timeout = null, params int[] lockIds)
    {
        lock (_locker)
        {
            if (_queuedLocks == null)
            {
                _queuedLocks = new StackQueue<(DistributedLockType, TimeSpan, Guid, int)>();
            }

            foreach (var lockId in lockIds)
            {
                _queuedLocks.Enqueue((lockType, timeout ?? TimeSpan.Zero, instanceId, lockId));
            }
        }
    }

    /// <summary>
    ///     Clears all lock counters for a given scope instance, signalling that the scope has been disposed.
    /// </summary>
    /// <param name="instanceId">Instance ID of the scope to clear.</param>
    public void ClearLocks(Guid instanceId)
    {
        lock (_locker)
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

    public void EnsureLocksCleared(Guid instanceId)
    {
        while (!_acquiredLocks?.IsCollectionEmpty() ?? false)
        {
            _acquiredLocks?.Dequeue().Dispose();
        }

        // We're the parent scope, make sure that locks of all scopes has been cleared
        // Since we're only reading we don't have to be in a lock
        if (!(_readLocksDictionary?.Count > 0) && !(_writeLocksDictionary?.Count > 0))
        {
            return;
        }

        var exception = new InvalidOperationException(
            $"All scopes has not been disposed from parent scope: {instanceId}, see log for more details.");
        throw exception;
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
    public void EnsureLocks(Guid scopeInstanceId)
    {
        lock (_locker)
        {
            if (!(_queuedLocks?.Count > 0))
            {
                return;
            }

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


    public Dictionary<Guid, Dictionary<int, int>>? GetReadLocks() => _readLocksDictionary;

    public Dictionary<Guid, Dictionary<int, int>>? GetWriteLocks() => _writeLocksDictionary;

    /// <inheritdoc />
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
            var exception = new InvalidOperationException(
                $"All locks have not been cleared, this usually means that all scopes have not been disposed from the parent scope");
            _logger.LogError(exception, GenerateUnclearedScopesLogMessage());
            throw exception;
        }
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
            $"Lock counters aren't empty, suggesting a scope hasn't been properly disposed");
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
}
