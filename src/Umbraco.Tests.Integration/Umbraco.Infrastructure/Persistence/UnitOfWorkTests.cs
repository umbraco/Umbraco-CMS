// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core.Scoping;
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
            IScopeProvider provider = ScopeProvider;
            Assert.Throws<ArgumentException>(() =>
            {
                using (IScope scope = provider.CreateScope())
                {
                    scope.ReadLock(-666);
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
                scope.ReadLock(Constants.Locks.Servers);
                scope.Complete();
            }
        }

        [Test]
        public void WriteLockNonExisting()
        {
            IScopeProvider provider = ScopeProvider;
            Assert.Throws<ArgumentException>(() =>
            {
                using (IScope scope = provider.CreateScope())
                {
                    scope.WriteLock(-666);
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
                scope.WriteLock(Constants.Locks.Servers);
                scope.Complete();
            }
        }
    }
}
