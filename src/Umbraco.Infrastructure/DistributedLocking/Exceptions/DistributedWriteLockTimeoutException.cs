namespace Umbraco.Cms.Infrastructure.DistributedLocking.Exceptions;

public class DistributedWriteLockTimeoutException : DistributedLockingTimeoutException
{
    public DistributedWriteLockTimeoutException(int lockId)
        : base(lockId, true)
    {
    }
}
