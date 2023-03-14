namespace Umbraco.Cms.Core.DistributedLocking.Exceptions;

/// <summary>
///     Exception thrown when a write lock could not be obtained in a timely manner.
/// </summary>
public class DistributedWriteLockTimeoutException : DistributedLockingTimeoutException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DistributedWriteLockTimeoutException" /> class.
    /// </summary>
    public DistributedWriteLockTimeoutException(int lockId)
        : base(lockId, true)
    {
    }
}
