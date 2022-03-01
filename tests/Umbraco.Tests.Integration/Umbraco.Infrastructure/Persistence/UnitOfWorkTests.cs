// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Persistence.SqlServer.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class UnitOfWorkTests : UmbracoIntegrationTest
    {
        [Test]
        public void ReadLockNonExisting()
        {
            if (Configuration[Constants.Configuration.ConfigGlobalDistributedLockingMechanism] !=
                nameof(SqlServerDistributedLockingMechanism))
            {
                Assert.Ignore("This test only applies to SqlServerDistributedLockingMechanism");
            }

            IScopeProvider provider = ScopeProvider;
            Assert.Throws<ArgumentException>(() =>
            {
                using (IScope scope = provider.CreateScope())
                {
                    scope.EagerReadLock(-666);
                    scope.Complete();
                }
            });
        }

        [Test]
        public void ReadLockExisting()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                scope.EagerReadLock(Constants.Locks.Servers);
                scope.Complete();
            }
        }

        [Test]
        public void WriteLockNonExisting()
        {
            if (Configuration[Constants.Configuration.ConfigGlobalDistributedLockingMechanism] !=
                nameof(SqlServerDistributedLockingMechanism))
            {
                Assert.Ignore("This test only applies to SqlServerDistributedLockingMechanism");
            }

            IScopeProvider provider = ScopeProvider;
            Assert.Throws<ArgumentException>(() =>
            {
                using (IScope scope = provider.CreateScope())
                {
                    scope.EagerWriteLock(-666);
                    scope.Complete();
                }
            });
        }

        [Test]
        public void WriteLockExisting()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                scope.EagerWriteLock(Constants.Locks.Servers);
                scope.Complete();
            }
        }
    }
}
