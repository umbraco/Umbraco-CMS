using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Scoping
{
    [TestFixture]
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    public class ScopedRepositoryTests : BaseDatabaseFactoryTest
    {
        // setup
        public override void Initialize()
        {
            base.Initialize();

            Assert.IsNull(DatabaseContext.ScopeProvider.AmbientScope); // gone
        }

        protected override CacheHelper CreateCacheHelper()
        {
            //return CacheHelper.CreateDisabledCacheHelper();
            return new CacheHelper(
                new ObjectCacheRuntimeCacheProvider(),
                new StaticCacheProvider(),
                new NullCacheProvider(),
                new IsolatedRuntimeCache(type => new ObjectCacheRuntimeCacheProvider()));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Test(bool complete)
        {
            var scopeProvider = DatabaseContext.ScopeProvider;
            var userService = ApplicationContext.Services.UserService;
            var globalCache = ApplicationContext.ApplicationCache.IsolatedRuntimeCache.GetOrCreateCache(typeof(IUser));

            var userType = userService.GetUserTypeByAlias("admin");
            var user = (IUser) new User("name", "email", "username", "rawPassword", userType);
            userService.Save(user);

            // global cache contains the user entity
            var globalCached = (IUser) globalCache.GetCacheItem(GetCacheIdKey<IUser>(user.Id), () => null);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(user.Id, globalCached.Id);
            Assert.AreEqual("name", globalCached.Name);

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);

                // scope has its own isolated cache
                var scopedCache = scope.IsolatedRuntimeCache.GetOrCreateCache(typeof (IUser));
                Assert.AreNotSame(globalCache, scopedCache);

                user.Name = "changed";
                ApplicationContext.Services.UserService.Save(user);

                // scoped cache contains the "new" user entity
                var scopeCached = (IUser) scopedCache.GetCacheItem(GetCacheIdKey<IUser>(user.Id), () => null);
                Assert.IsNotNull(scopeCached);
                Assert.AreEqual(user.Id, scopeCached.Id);
                Assert.AreEqual("changed", scopeCached.Name);

                // global cache is unchanged
                globalCached = (IUser) globalCache.GetCacheItem(GetCacheIdKey<IUser>(user.Id), () => null);
                Assert.IsNotNull(globalCached);
                Assert.AreEqual(user.Id, globalCached.Id);
                Assert.AreEqual("name", globalCached.Name);

                if (complete)
                    scope.Complete();
            }
            Assert.IsNull(scopeProvider.AmbientScope);

            // global cache has been cleared
            globalCached = (IUser) globalCache.GetCacheItem(GetCacheIdKey<IUser>(user.Id), () => null);
            Assert.IsNull(globalCached);

            // get again, updated if completed
            user = userService.GetUserById(user.Id);
            Assert.AreEqual(complete ? "changed" : "name", user.Name);

            // global cache contains the entity again
            globalCached = (IUser) globalCache.GetCacheItem(GetCacheIdKey<IUser>(user.Id), () => null);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(user.Id, globalCached.Id);
            Assert.AreEqual(complete ? "changed" : "name", globalCached.Name);
        }

        public static string GetCacheIdKey<T>(object id)
        {
            return string.Format("{0}{1}", GetCacheTypeKey<T>(), id);
        }

        public static string GetCacheTypeKey<T>()
        {
            return string.Format("uRepo_{0}_", typeof(T).Name);
        }
    }
}
