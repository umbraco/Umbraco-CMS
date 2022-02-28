using System;

namespace Umbraco.Cms.Infrastructure.DistributedLocking;

public interface IDistributedLock : IDisposable
{
    int LockId { get; }

    DistributedLockType LockType { get; }

    void Release();
}
