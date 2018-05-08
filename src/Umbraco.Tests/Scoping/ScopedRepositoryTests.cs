using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Scoping;
using Umbraco.Core.Sync;
using Umbraco.Tests.Cache.DistributedCache;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Cache;

namespace Umbraco.Tests.Scoping
{
    [TestFixture]
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    public class ScopedRepositoryTests : BaseDatabaseFactoryTest
    {
        private CacheRefresherEventHandler _cacheHandler;

        // setup
        public override void Initialize()
        {
            base.Initialize();

            Assert.IsNull(DatabaseContext.ScopeProvider.AmbientScope); // gone
        }

        protected override void FreezeResolution()
        {
            ServerRegistrarResolver.Current = new ServerRegistrarResolver(
                new DistributedCacheTests.TestServerRegistrar());
            ServerMessengerResolver.Current = new ServerMessengerResolver(
                new DatabaseServerMessenger(ApplicationContext, false, new DatabaseServerMessengerOptions()));
            CacheRefreshersResolver.Current = new CacheRefreshersResolver(
                new ActivatorServiceProvider(), Mock.Of<ILogger>(), () => new[]
                {
                    typeof(PageCacheRefresher),
                    typeof(UnpublishedPageCacheRefresher),
                    typeof(DomainCacheRefresher),
                    typeof(MacroCacheRefresher),
                    typeof(UserCacheRefresher),
                    typeof(LanguageCacheRefresher),
                    typeof(DictionaryCacheRefresher)
                });

            base.FreezeResolution();
        }

        [TearDown]
        public void Teardown()
        {
            if (_cacheHandler != null)
                _cacheHandler.Unbind();
            _cacheHandler = null;

            ServerRegistrarResolver.Reset();
            ServerMessengerResolver.Reset();
            CacheRefreshersResolver.Reset();
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
        public void DefaultRepositoryCachePolicy(bool complete)
        {
            var scopeProvider = DatabaseContext.ScopeProvider;
            var service = ApplicationContext.Services.UserService;
            var globalCache = ApplicationContext.ApplicationCache.IsolatedRuntimeCache.GetOrCreateCache(typeof(IUser));

            var user = (IUser) new User("name", "email", "username", "rawPassword");
            service.Save(user);

            // global cache contains the entity
            var globalCached = (IUser) globalCache.GetCacheItem(GetCacheIdKey<IUser>(user.Id), () => null);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(user.Id, globalCached.Id);
            Assert.AreEqual("name", globalCached.Name);

            _cacheHandler = new CacheRefresherEventHandler(true);
            _cacheHandler.OnApplicationStarted(null, ApplicationContext);

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
                service.Save(user);

                // scoped cache contains the "new" entity
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

            globalCached = (IUser)globalCache.GetCacheItem(GetCacheIdKey<IUser>(user.Id), () => null);
            if (complete)
            {
                // global cache has been cleared
                Assert.IsNull(globalCached);
            }
            else
            {
                // global cache has *not* been cleared
                Assert.IsNotNull(globalCached);
            }

            // get again, updated if completed
            user = service.GetUserById(user.Id);
            Assert.AreEqual(complete ? "changed" : "name", user.Name);

            // global cache contains the entity again
            globalCached = (IUser) globalCache.GetCacheItem(GetCacheIdKey<IUser>(user.Id), () => null);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(user.Id, globalCached.Id);
            Assert.AreEqual(complete ? "changed" : "name", globalCached.Name);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FullDataSetRepositoryCachePolicy(bool complete)
        {
            var scopeProvider = DatabaseContext.ScopeProvider;
            var service = ApplicationContext.Services.LocalizationService;
            var globalCache = ApplicationContext.ApplicationCache.IsolatedRuntimeCache.GetOrCreateCache(typeof (ILanguage));

            var lang = (ILanguage) new Language("fr-FR");
            service.Save(lang);

            // global cache has been flushed, reload
            var globalFullCached = (IEnumerable<ILanguage>) globalCache.GetCacheItem(GetCacheTypeKey<ILanguage>(), () => null);
            Assert.IsNull(globalFullCached);
            var reload = service.GetLanguageById(lang.Id);

            // global cache contains the entity
            globalFullCached = (IEnumerable<ILanguage>) globalCache.GetCacheItem(GetCacheTypeKey<ILanguage>(), () => null);
            Assert.IsNotNull(globalFullCached);
            var globalCached = globalFullCached.First(x => x.Id == lang.Id);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(lang.Id, globalCached.Id);
            Assert.AreEqual("fr-FR", globalCached.IsoCode);

            _cacheHandler = new CacheRefresherEventHandler(true);
            _cacheHandler.OnApplicationStarted(null, ApplicationContext);

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);

                // scope has its own isolated cache
                var scopedCache = scope.IsolatedRuntimeCache.GetOrCreateCache(typeof (ILanguage));
                Assert.AreNotSame(globalCache, scopedCache);

                lang.IsoCode = "de-DE";
                service.Save(lang);

                // scoped cache has been flushed, reload
                var scopeFullCached = (IEnumerable<ILanguage>) scopedCache.GetCacheItem(GetCacheTypeKey<ILanguage>(), () => null);
                Assert.IsNull(scopeFullCached);
                reload = service.GetLanguageById(lang.Id);

                // scoped cache contains the "new" entity
                scopeFullCached = (IEnumerable<ILanguage>) scopedCache.GetCacheItem(GetCacheTypeKey<ILanguage>(), () => null);
                Assert.IsNotNull(scopeFullCached);
                var scopeCached = scopeFullCached.First(x => x.Id == lang.Id);
                Assert.IsNotNull(scopeCached);
                Assert.AreEqual(lang.Id, scopeCached.Id);
                Assert.AreEqual("de-DE", scopeCached.IsoCode);

                // global cache is unchanged
                globalFullCached = (IEnumerable<ILanguage>) globalCache.GetCacheItem(GetCacheTypeKey<ILanguage>(), () => null);
                Assert.IsNotNull(globalFullCached);
                globalCached = globalFullCached.First(x => x.Id == lang.Id);
                Assert.IsNotNull(globalCached);
                Assert.AreEqual(lang.Id, globalCached.Id);
                Assert.AreEqual("fr-FR", globalCached.IsoCode);

                if (complete)
                    scope.Complete();
            }
            Assert.IsNull(scopeProvider.AmbientScope);

            globalFullCached = (IEnumerable<ILanguage>) globalCache.GetCacheItem(GetCacheTypeKey<ILanguage>(), () => null);
            if (complete)
            {
                // global cache has been cleared
                Assert.IsNull(globalFullCached);
            }
            else
            {
                // global cache has *not* been cleared
                Assert.IsNotNull(globalFullCached);
            }

            // get again, updated if completed
            lang = service.GetLanguageById(lang.Id);
            Assert.AreEqual(complete ? "de-DE" : "fr-FR", lang.IsoCode);

            // global cache contains the entity again
            globalFullCached = (IEnumerable<ILanguage>) globalCache.GetCacheItem(GetCacheTypeKey<ILanguage>(), () => null);
            Assert.IsNotNull(globalFullCached);
            globalCached = globalFullCached.First(x => x.Id == lang.Id);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(lang.Id, globalCached.Id);
            Assert.AreEqual(complete ? "de-DE" : "fr-FR", lang.IsoCode);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SingleItemsOnlyRepositoryCachePolicy(bool complete)
        {
            var scopeProvider = DatabaseContext.ScopeProvider;
            var service = ApplicationContext.Services.LocalizationService;
            var globalCache = ApplicationContext.ApplicationCache.IsolatedRuntimeCache.GetOrCreateCache(typeof (IDictionaryItem));

            var lang = (ILanguage)new Language("fr-FR");
            service.Save(lang);

            var item = (IDictionaryItem) new DictionaryItem("item-key");
            item.Translations = new IDictionaryTranslation[]
            {
                new DictionaryTranslation(lang.Id, "item-value"),
            };
            service.Save(item);

            // global cache contains the entity
            var globalCached = (IDictionaryItem) globalCache.GetCacheItem(GetCacheIdKey<IDictionaryItem>(item.Id), () => null);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(item.Id, globalCached.Id);
            Assert.AreEqual("item-key", globalCached.ItemKey);

            _cacheHandler = new CacheRefresherEventHandler(true);
            _cacheHandler.OnApplicationStarted(null, ApplicationContext);

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
            {
                Assert.IsInstanceOf<Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);

                // scope has its own isolated cache
                var scopedCache = scope.IsolatedRuntimeCache.GetOrCreateCache(typeof (IDictionaryItem));
                Assert.AreNotSame(globalCache, scopedCache);

                item.ItemKey = "item-changed";
                service.Save(item);

                // scoped cache contains the "new" entity
                var scopeCached = (IDictionaryItem) scopedCache.GetCacheItem(GetCacheIdKey<IDictionaryItem>(item.Id), () => null);
                Assert.IsNotNull(scopeCached);
                Assert.AreEqual(item.Id, scopeCached.Id);
                Assert.AreEqual("item-changed", scopeCached.ItemKey);

                // global cache is unchanged
                globalCached = (IDictionaryItem) globalCache.GetCacheItem(GetCacheIdKey<IDictionaryItem>(item.Id), () => null);
                Assert.IsNotNull(globalCached);
                Assert.AreEqual(item.Id, globalCached.Id);
                Assert.AreEqual("item-key", globalCached.ItemKey);

                if (complete)
                    scope.Complete();
            }
            Assert.IsNull(scopeProvider.AmbientScope);

            globalCached = (IDictionaryItem) globalCache.GetCacheItem(GetCacheIdKey<IDictionaryItem>(item.Id), () => null);
            if (complete)
            {
                // global cache has been cleared
                Assert.IsNull(globalCached);
            }
            else
            {
                // global cache has *not* been cleared
                Assert.IsNotNull(globalCached);
            }

            // get again, updated if completed
            item = service.GetDictionaryItemById(item.Id);
            Assert.AreEqual(complete ? "item-changed" : "item-key", item.ItemKey);

            // global cache contains the entity again
            globalCached = (IDictionaryItem) globalCache.GetCacheItem(GetCacheIdKey<IDictionaryItem>(item.Id), () => null);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(item.Id, globalCached.Id);
            Assert.AreEqual(complete ? "item-changed" : "item-key", globalCached.ItemKey);
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
