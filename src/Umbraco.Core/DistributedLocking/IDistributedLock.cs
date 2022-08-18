namespace Umbraco.Cms.Core.DistributedLocking;

/// <summary>
///     Interface representing a DistributedLock.
/// </summary>
public interface IDistributedLock : IDisposable
{
    /// <summary>
    ///     Gets the LockId.
    /// </summary>
    int LockId { get; }

    /// <summary>
    ///     Gets the DistributedLockType.
    /// </summary>
    DistributedLockType LockType { get; }
}
