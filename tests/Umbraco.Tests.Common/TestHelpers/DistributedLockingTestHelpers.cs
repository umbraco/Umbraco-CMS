using System;
using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.DistributedLocking.Exceptions;

namespace Umbraco.Cms.Tests.Common.TestHelpers;

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Seems reasonable")]
[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "YOLO")]
public static class DistributedLockingMechanismExtensions
{
    public static LockTester ReadTester(this IDistributedLockingMechanism lockingMechanism, int lockId, TimeSpan timeout)
        => new(lockingMechanism, lockId, false, timeout);

    public static LockTester WriteTester(this IDistributedLockingMechanism lockingMechanism, int lockId, TimeSpan timeout)
        => new(lockingMechanism, lockId, true, timeout);
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Seems reasonable")]
public class LockTester : IDisposable
{
    private readonly IDistributedLockingMechanism _lockingMechanism;
    private readonly int _lockId;
    private readonly bool _write;
    private readonly TimeSpan _timeout;
    private IDistributedLock _lock;

    public bool Success => Exception == null;

    // ReSharper disable once MemberCanBePrivate.Global
    public DistributedLockingTimeoutException Exception { get; private set; }

    public LockTester(IDistributedLockingMechanism lockingMechanism, int lockId, bool write, TimeSpan timeout)
    {
        _lockingMechanism = lockingMechanism;
        _lockId = lockId;
        _write = write;
        _timeout = timeout;
    }

    public LockTester Run()
    {
        try
        {
            _lock = _write
                ? _lockingMechanism.WriteLock(_lockId, _timeout)
                : _lockingMechanism.ReadLock(_lockId, _timeout);
        }
        catch (DistributedLockingTimeoutException ex)
        {
            Exception = ex;
        }

        return this;
    }

    public void Dispose() => _lock?.Dispose();

    public void ThreadStart() => _ = Run();
}
