using System;

namespace Umbraco.Cms.Infrastructure.DistributedLocking.Exceptions;

public abstract class DistributedLockingTimeoutException : ApplicationException
{
    protected DistributedLockingTimeoutException(int lockId, bool isWrite)
        : base($"Failed to acquire {(isWrite ? "write" : "read")} lock for id: {lockId}.")
    {
    }
}
