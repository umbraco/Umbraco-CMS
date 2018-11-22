using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class LockedRepositoryTests : BaseDatabaseFactoryTest
    {
        private static IServerRegistrationRepository CreateRepository(IScopeUnitOfWork uow, ILogger logger, CacheHelper cacheHelper, ISqlSyntaxProvider sqlSyntax)
        {
            return new ServerRegistrationRepository(
                uow,
                cacheHelper.StaticCache,
                logger, sqlSyntax);
        }

        protected override CacheHelper CreateCacheHelper()
        {
            // ServerRegistrationRepository wants a real cache else weird things happen
            //return CacheHelper.CreateDisabledCacheHelper();
            return new CacheHelper(
                new ObjectCacheRuntimeCacheProvider(),
                new StaticCacheProvider(),
                new NullCacheProvider(),
                new IsolatedRuntimeCache(type => new ObjectCacheRuntimeCacheProvider()));
        }

        [Test]
        public void NoOuterScopeJustWorks()
        {
            var uowProvider = new PetaPocoUnitOfWorkProvider(Logger);
            var sqlSyntax = ApplicationContext.DatabaseContext.SqlSyntax;

            var lrepo = new LockingRepository<IServerRegistrationRepository>(uowProvider,
                x => CreateRepository(x, Logger, CacheHelper, sqlSyntax),
                new[] { Constants.Locks.Servers }, new[] { Constants.Locks.Servers });

            IServerRegistration reg = null;

            lrepo.WithWriteLocked(xrepo =>
            {
                xrepo.Repository.AddOrUpdate(reg = new ServerRegistration("a1234", "i1234", DateTime.Now));

                // no need - autocommit by default
                //xrepo.UnitOfWork.Commit();
            });

            Assert.IsNull(((ScopeProvider) ApplicationContext.ScopeProvider).AmbientScope);

            Assert.AreNotEqual(0, reg.Id);

            // that's cheating somehow because it will not really hit the DB because of the cache
            var reg2 = lrepo.WithReadLocked(xrepo =>
            {
                return xrepo.Repository.Get(reg.Id);
            });

            Assert.IsNull(((ScopeProvider) ApplicationContext.ScopeProvider).AmbientScope);

            Assert.IsNotNull(reg2);
            Assert.AreEqual("a1234", reg2.ServerAddress);
            Assert.AreEqual("i1234", reg2.ServerIdentity);

            // this really makes sure there's something in database
            using (var scope = ApplicationContext.ScopeProvider.CreateScope())
            {
                var reg3 = scope.Database.Fetch<dynamic>("SELECT * FROM umbracoServer WHERE id=@id", new { id = reg2.Id }).FirstOrDefault();
                Assert.IsNotNull(reg3);
                Assert.AreEqual("a1234", reg3.address);
                Assert.AreEqual("i1234", reg3.computerName);
            }

            Assert.IsNull(((ScopeProvider) ApplicationContext.ScopeProvider).AmbientScope);
        }

        [Test]
        public void OuterScopeBadIsolationLevel()
        {
            var uowProvider = new PetaPocoUnitOfWorkProvider(Logger);
            var sqlSyntax = ApplicationContext.DatabaseContext.SqlSyntax;

            var lrepo = new LockingRepository<IServerRegistrationRepository>(uowProvider,
                x => CreateRepository(x, Logger, CacheHelper, sqlSyntax),
                new[] { Constants.Locks.Servers }, new[] { Constants.Locks.Servers });

            // this creates a IsolationLevel.Unspecified scope
            using (var scope = ApplicationContext.ScopeProvider.CreateScope())
            {
                // so outer scope creates IsolationLevel.ReadCommitted (default) transaction
                // then WithReadLocked creates a IsolationLevel.RepeatableRead scope, which
                // fails - levels conflict

                try
                {
                    lrepo.WithReadLocked(xrepo =>
                    {
                        xrepo.Repository.DeactiveStaleServers(TimeSpan.Zero);
                    });
                    Assert.Fail("Expected: Exception.");
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Scope requires isolation level RepeatableRead, but got ReadCommitted from parent.", e.Message);
                }

                scope.Complete();
            }
        }

        [Test]
        public void OuterScopeGoodIsolationLevel()
        {
            var uowProvider = new PetaPocoUnitOfWorkProvider(Logger);
            var sqlSyntax = ApplicationContext.DatabaseContext.SqlSyntax;

            var lrepo = new LockingRepository<IServerRegistrationRepository>(uowProvider,
                x => CreateRepository(x, Logger, CacheHelper, sqlSyntax),
                new[] { Constants.Locks.Servers }, new[] { Constants.Locks.Servers });

            // this creates a IsolationLevel.RepeatableRead scope
            using (var scope = ApplicationContext.ScopeProvider.CreateScope(IsolationLevel.RepeatableRead))
            {
                // so outer scope creates IsolationLevel.RepeatableRead transaction
                // then WithReadLocked creates a IsolationLevel.RepeatableRead scope, which
                // suceeds - no level conflict

                lrepo.WithReadLocked(xrepo =>
                {
                    xrepo.Repository.DeactiveStaleServers(TimeSpan.Zero);
                });

                scope.Complete();
            }
        }
    }
}
