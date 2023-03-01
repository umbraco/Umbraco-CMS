using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace Umbraco.Cms.Persistence.EFCore.Services;

public interface ILockingMechanism
{
    /// <summary>
    ///     Read-locks some lock objects.
    /// </summary>
    /// <param name="lockIds">Array of lock object identifiers.</param>
    void ReadLock(Guid instanceId, params int[] lockIds);

    /// <summary>
    ///     Write-locks some lock objects.
    /// </summary>
    /// <param name="lockIds">Array of object identifiers.</param>
    void WriteLock(Guid instanceId, params int[] lockIds);

    void EagerReadLock(Guid instanceId, params int[] lockIds);

    void EagerWriteLock(Guid instanceId, params int[] lockIds);

    void ClearLocks(Guid instanceId);
    void EnsureDbLocks(Guid scopeInstanceId);
}
