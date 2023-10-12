namespace Umbraco.Cms.Core.Scoping;

public interface ILockingMechanism : IDisposable
{
    /// <summary>
    ///     Read-locks some lock objects lazily.
    /// </summary>
    /// <param name="instanceId">Instance id of the scope who is requesting the lock</param>
    /// <param name="timeout">Timeout for the lock</param>
    /// <param name="lockIds">Array of lock object identifiers.</param>
    void ReadLock(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds);

    void ReadLock(Guid instanceId, params int[] lockIds);

    /// <summary>
    ///     Write-locks some lock objects lazily.
    /// </summary>
    /// <param name="instanceId">Instance id of the scope who is requesting the lock</param>
    /// <param name="timeout">Timeout for the lock</param>
    /// <param name="lockIds">Array of object identifiers.</param>
    void WriteLock(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds);

    void WriteLock(Guid instanceId, params int[] lockIds);

    /// <summary>
    /// Eagerly acquires a read-lock
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="timeout">Timeout for the lock</param>
    /// <param name="lockIds"></param>
    void EagerReadLock(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds);

    void EagerReadLock(Guid instanceId, params int[] lockIds);

    /// <summary>
    /// Eagerly acquires a write-lock
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="timeout">Timeout for the lock</param>
    /// <param name="lockIds"></param>
    void EagerWriteLock(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds);

    void EagerWriteLock(Guid instanceId, params int[] lockIds);

    /// <summary>
    /// Clears all the locks held
    /// </summary>
    /// <param name="instanceId"></param>
    void ClearLocks(Guid instanceId);

    /// <summary>
    /// Acquires all the non-eagerly requested locks.
    /// </summary>
    /// <param name="scopeInstanceId"></param>
    void EnsureLocks(Guid scopeInstanceId);

    void EnsureLocksCleared(Guid instanceId);

    Dictionary<Guid, Dictionary<int, int>>? GetReadLocks();

    Dictionary<Guid, Dictionary<int, int>>? GetWriteLocks();
}
