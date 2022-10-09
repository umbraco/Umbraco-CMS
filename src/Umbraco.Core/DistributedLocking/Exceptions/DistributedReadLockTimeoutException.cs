namespace Umbraco.Cms.Core.DistributedLocking.Exceptions;

/// <summary>
///     Exception thrown when a read lock could not be obtained in a timely manner.
/// </summary>
public class DistributedReadLockTimeoutException : DistributedLockingTimeoutException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DistributedReadLockTimeoutException" /> class.
    /// </summary>
    public DistributedReadLockTimeoutException(int lockId)
        : base(lockId, false)
    {
    }
}
