// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Persistence.Sqlite.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class UnitOfWorkTests : UmbracoIntegrationTest
{
    [Test]
    public void ReadLockNonExisting()
    {
        var lockingMechanism = GetRequiredService<IDistributedLockingMechanismFactory>().DistributedLockingMechanism;
        if (lockingMechanism is SqliteDistributedLockingMechanism)
        {
            Assert.Ignore("SqliteDistributedLockingMechanism doesn't query the umbracoLock table for read locks.");
        }

        var provider = ScopeProvider;
        Assert.Throws<ArgumentException>(() =>
        {
            using (var scope = provider.CreateScope())
            {
                scope.EagerReadLock(-666);
                scope.Complete();
            }
        });
    }

    [Test]
    public void ReadLockExisting()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            scope.EagerReadLock(Constants.Locks.Servers);
            scope.Complete();
        }
    }

    [Test]
    public void WriteLockNonExisting()
    {
        var provider = ScopeProvider;
        Assert.Throws<ArgumentException>(() =>
        {
            using (var scope = provider.CreateScope())
            {
                scope.EagerWriteLock(-666);
                scope.Complete();
            }
        });
    }

    [Test]
    public void WriteLockExisting()
    {
        var provider = ScopeProvider;
        using (var scope = provider.CreateScope())
        {
            scope.EagerWriteLock(Constants.Locks.Servers);
            scope.Complete();
        }
    }
}
