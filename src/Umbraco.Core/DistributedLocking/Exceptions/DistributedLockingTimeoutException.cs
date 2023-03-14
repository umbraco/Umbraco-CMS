namespace Umbraco.Cms.Core.DistributedLocking.Exceptions;

/// <summary>
///     Base class for all DistributedLocking timeout related exceptions.
/// </summary>
public abstract class DistributedLockingTimeoutException : DistributedLockingException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DistributedLockingTimeoutException" /> class.
    /// </summary>
    protected DistributedLockingTimeoutException(int lockId, bool isWrite)
        : base($"Failed to acquire {(isWrite ? "write" : "read")} lock for id: {lockId}.")
    {
    }
}
