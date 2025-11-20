// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Scoping;

[TestFixture]
internal class LockingMechanismTests
{
    private const int LockId = 1000;
    private const int LockId2 = 1001;
    private static readonly Guid _scopeInstanceId = Guid.NewGuid();

    [Test]
    public void IncrementLock_WithoutLocksDictionary_CreatesLock()
    {
        var locks = new Dictionary<Guid, Dictionary<int, int>>();
        LockingMechanism.IncrementLock(LockId, _scopeInstanceId, ref locks);
        Assert.AreEqual(1, locks.Count);
        Assert.AreEqual(1, locks[_scopeInstanceId][LockId]);
    }

    [Test]
    public void IncrementLock_WithExistingLocksDictionary_CreatesLock()
    {
        var locks = new Dictionary<Guid, Dictionary<int, int>>()
        {
            {
                _scopeInstanceId,
                new Dictionary<int, int>()
                {
                    { LockId, 100 },
                    { LockId2, 200 }
                }
            }
        };
        LockingMechanism.IncrementLock(LockId, _scopeInstanceId, ref locks);
        Assert.AreEqual(1, locks.Count);
        Assert.AreEqual(2, locks[_scopeInstanceId].Count);
        Assert.AreEqual(101, locks[_scopeInstanceId][LockId]);
        Assert.AreEqual(200, locks[_scopeInstanceId][LockId2]);
    }
}
