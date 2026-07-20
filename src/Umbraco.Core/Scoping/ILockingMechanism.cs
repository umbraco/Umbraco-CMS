namespace Umbraco.Cms.Core.Scoping;

/// <summary>
///     Defines a mechanism for acquiring and managing read and write locks on resources.
/// </summary>
/// <remarks>
///     <para>Locks can be acquired lazily (deferred until actually needed) or eagerly (immediately).</para>
///     <para>The mechanism tracks locks by scope instance ID to support nested scopes.</para>
/// </remarks>
public interface ILockingMechanism : IDisposable
{
    /// <summary>
    ///     Read-locks some lock objects lazily.
    /// </summary>
    /// <param name="instanceId">Instance id of the scope who is requesting the lock</param>
    /// <param name="timeout">Timeout for the lock</param>
    /// <param name="lockIds">Array of lock object identifiers.</param>
    void ReadLock(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds);

    /// <summary>
    ///     Read-locks some lock objects lazily using the default timeout.
    /// </summary>
    /// <param name="instanceId">Instance id of the scope who is requesting the lock.</param>
    /// <param name="lockIds">Array of lock object identifiers.</param>
    void ReadLock(Guid instanceId, params int[] lockIds);

    /// <summary>
    ///     Write-locks some lock objects lazily.
    /// </summary>
    /// <param name="instanceId">Instance id of the scope who is requesting the lock.</param>
    /// <param name="timeout">Timeout for the lock.</param>
    /// <param name="lockIds">Array of object identifiers.</param>
    void WriteLock(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds);

    /// <summary>
    ///     Write-locks some lock objects lazily using the default timeout.
    /// </summary>
    /// <param name="instanceId">Instance id of the scope who is requesting the lock.</param>
    /// <param name="lockIds">Array of object identifiers.</param>
    void WriteLock(Guid instanceId, params int[] lockIds);

    /// <summary>
    /// Eagerly acquires a read-lock
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="timeout">Timeout for the lock</param>
    /// <param name="lockIds"></param>
    void EagerReadLock(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds);

    /// <summary>
    ///     Eagerly acquires a read-lock using the default timeout.
    /// </summary>
    /// <param name="instanceId">Instance id of the scope who is requesting the lock.</param>
    /// <param name="lockIds">Array of lock object identifiers.</param>
    void EagerReadLock(Guid instanceId, params int[] lockIds);

    /// <summary>
    ///     Eagerly acquires a write-lock.
    /// </summary>
    /// <param name="instanceId">Instance id of the scope who is requesting the lock.</param>
    /// <param name="timeout">Timeout for the lock.</param>
    /// <param name="lockIds">Array of lock object identifiers.</param>
    void EagerWriteLock(Guid instanceId, TimeSpan? timeout = null, params int[] lockIds);

    /// <summary>
    ///     Eagerly acquires a write-lock using the default timeout.
    /// </summary>
    /// <param name="instanceId">Instance id of the scope who is requesting the lock.</param>
    /// <param name="lockIds">Array of lock object identifiers.</param>
    void EagerWriteLock(Guid instanceId, params int[] lockIds);

    /// <summary>
    ///     Clears all the locks held by a specific scope instance.
    /// </summary>
    /// <param name="instanceId">Instance id of the scope whose locks should be cleared.</param>
    void ClearLocks(Guid instanceId);

    /// <summary>
    ///     Acquires all the non-eagerly (lazily) requested locks.
    /// </summary>
    /// <param name="scopeInstanceId">Instance id of the scope whose pending locks should be acquired.</param>
    void EnsureLocks(Guid scopeInstanceId);

    /// <summary>
    ///     Ensures all locks have been cleared for a specific scope instance, throwing if any remain.
    /// </summary>
    /// <param name="instanceId">Instance id of the scope to verify.</param>
    /// <exception cref="InvalidOperationException">Thrown when locks have not been properly cleared.</exception>
    void EnsureLocksCleared(Guid instanceId);

    /// <summary>
    ///     Gets the dictionary of read locks held by scope instances.
    /// </summary>
    /// <returns>A dictionary mapping scope instance IDs to their read lock counts by lock ID, or <c>null</c> if no read locks exist.</returns>
    Dictionary<Guid, Dictionary<int, int>>? GetReadLocks();

    /// <summary>
    ///     Gets the dictionary of write locks held by scope instances.
    /// </summary>
    /// <returns>A dictionary mapping scope instance IDs to their write lock counts by lock ID, or <c>null</c> if no write locks exist.</returns>
    Dictionary<Guid, Dictionary<int, int>>? GetWriteLocks();
}
