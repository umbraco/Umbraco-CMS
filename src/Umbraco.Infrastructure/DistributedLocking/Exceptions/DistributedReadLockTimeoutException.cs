namespace Umbraco.Cms.Infrastructure.DistributedLocking.Exceptions;

public class DistributedReadLockTimeoutException : DistributedLockingTimeoutException
{
    public DistributedReadLockTimeoutException(int lockId)
        : base(lockId, false)
    {
    }
}
