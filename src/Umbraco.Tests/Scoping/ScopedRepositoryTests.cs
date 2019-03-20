﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Web.Cache;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;
using Umbraco.Web;

namespace Umbraco.Tests.Scoping
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
    public class ScopedRepositoryTests : TestWithDatabaseBase
    {
        private DistributedCacheBinder _distributedCacheBinder;

        protected override void Compose()
        {
            base.Compose();

            // the cache refresher component needs to trigger to refresh caches
            // but then, it requires a lot of plumbing ;(
            // FIXME: and we cannot inject a DistributedCache yet
            // so doing all this mess
            Composition.RegisterUnique<IServerMessenger, LocalServerMessenger>();
            Composition.RegisterUnique(f => Mock.Of<IServerRegistrar>());
            Composition.WithCollectionBuilder<CacheRefresherCollectionBuilder>()
                .Add(() => Composition.TypeLoader.GetCacheRefreshers());
        }

        protected override AppCaches GetAppCaches()
        {
            // this is what's created core web runtime
            return new AppCaches(
                new DeepCloneAppCache(new ObjectCacheAppCache()),
                NoAppCache.Instance,
                new IsolatedCaches(type => new DeepCloneAppCache(new ObjectCacheAppCache())));
        }

        [TearDown]
        public void Teardown()
        {
            _distributedCacheBinder?.UnbindEvents();
            _distributedCacheBinder = null;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DefaultRepositoryCachePolicy(bool complete)
        {
            var scopeProvider = ScopeProvider;
            var service = Current.Services.UserService;
            var globalCache = Current.AppCaches.IsolatedCaches.GetOrCreate(typeof(IUser));

            var user = (IUser)new User("name", "email", "username", "rawPassword");
            service.Save(user);

            // global cache contains the entity
            var globalCached = (IUser) globalCache.Get(GetCacheIdKey<IUser>(user.Id), () => null);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(user.Id, globalCached.Id);
            Assert.AreEqual("name", globalCached.Name);

            // get user again - else we'd modify the one that's in the cache
            user = service.GetUserById(user.Id);

            _distributedCacheBinder = new DistributedCacheBinder(new DistributedCache(), Mock.Of<IUmbracoContextFactory>(), Mock.Of<ILogger>());
            _distributedCacheBinder.BindEvents(true);

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
            {
                Assert.IsInstanceOf<Core.Scoping.Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);

                // scope has its own isolated cache
                var scopedCache = scope.IsolatedCaches.GetOrCreate(typeof (IUser));
                Assert.AreNotSame(globalCache, scopedCache);

                user.Name = "changed";
                service.Save(user);

                // scoped cache contains the "new" entity
                var scopeCached = (IUser) scopedCache.Get(GetCacheIdKey<IUser>(user.Id), () => null);
                Assert.IsNotNull(scopeCached);
                Assert.AreEqual(user.Id, scopeCached.Id);
                Assert.AreEqual("changed", scopeCached.Name);

                // global cache is unchanged
                globalCached = (IUser) globalCache.Get(GetCacheIdKey<IUser>(user.Id), () => null);
                Assert.IsNotNull(globalCached);
                Assert.AreEqual(user.Id, globalCached.Id);
                Assert.AreEqual("name", globalCached.Name);

                if (complete)
                    scope.Complete();
            }
            Assert.IsNull(scopeProvider.AmbientScope);

            globalCached = (IUser) globalCache.Get(GetCacheIdKey<IUser>(user.Id), () => null);
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
            globalCached = (IUser) globalCache.Get(GetCacheIdKey<IUser>(user.Id), () => null);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(user.Id, globalCached.Id);
            Assert.AreEqual(complete ? "changed" : "name", globalCached.Name);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FullDataSetRepositoryCachePolicy(bool complete)
        {
            var scopeProvider = ScopeProvider;
            var service = Current.Services.LocalizationService;
            var globalCache = Current.AppCaches.IsolatedCaches.GetOrCreate(typeof (ILanguage));

            var lang = (ILanguage) new Language("fr-FR");
            service.Save(lang);

            // global cache has been flushed, reload
            var globalFullCached = (IEnumerable<ILanguage>) globalCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
            Assert.IsNull(globalFullCached);
            var reload = service.GetLanguageById(lang.Id);

            // global cache contains the entity
            globalFullCached = (IEnumerable<ILanguage>) globalCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
            Assert.IsNotNull(globalFullCached);
            var globalCached = globalFullCached.First(x => x.Id == lang.Id);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(lang.Id, globalCached.Id);
            Assert.AreEqual("fr-FR", globalCached.IsoCode);

            _distributedCacheBinder = new DistributedCacheBinder(new DistributedCache(), Mock.Of<IUmbracoContextFactory>(), Mock.Of<ILogger>());
            _distributedCacheBinder.BindEvents(true);

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
            {
                Assert.IsInstanceOf<Core.Scoping.Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);

                // scope has its own isolated cache
                var scopedCache = scope.IsolatedCaches.GetOrCreate(typeof (ILanguage));
                Assert.AreNotSame(globalCache, scopedCache);

                lang.IsoCode = "de-DE";
                service.Save(lang);

                // scoped cache has been flushed, reload
                var scopeFullCached = (IEnumerable<ILanguage>) scopedCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
                Assert.IsNull(scopeFullCached);
                reload = service.GetLanguageById(lang.Id);

                // scoped cache contains the "new" entity
                scopeFullCached = (IEnumerable<ILanguage>) scopedCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
                Assert.IsNotNull(scopeFullCached);
                var scopeCached = scopeFullCached.First(x => x.Id == lang.Id);
                Assert.IsNotNull(scopeCached);
                Assert.AreEqual(lang.Id, scopeCached.Id);
                Assert.AreEqual("de-DE", scopeCached.IsoCode);

                // global cache is unchanged
                globalFullCached = (IEnumerable<ILanguage>) globalCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
                Assert.IsNotNull(globalFullCached);
                globalCached = globalFullCached.First(x => x.Id == lang.Id);
                Assert.IsNotNull(globalCached);
                Assert.AreEqual(lang.Id, globalCached.Id);
                Assert.AreEqual("fr-FR", globalCached.IsoCode);

                if (complete)
                    scope.Complete();
            }
            Assert.IsNull(scopeProvider.AmbientScope);

            globalFullCached = (IEnumerable<ILanguage>) globalCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
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
            globalFullCached = (IEnumerable<ILanguage>) globalCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
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
            var scopeProvider = ScopeProvider;
            var service = Current.Services.LocalizationService;
            var globalCache = Current.AppCaches.IsolatedCaches.GetOrCreate(typeof (IDictionaryItem));

            var lang = (ILanguage)new Language("fr-FR");
            service.Save(lang);

            var item = (IDictionaryItem) new DictionaryItem("item-key");
            item.Translations = new IDictionaryTranslation[]
            {
                new DictionaryTranslation(lang.Id, "item-value"),
            };
            service.Save(item);

            // global cache contains the entity
            var globalCached = (IDictionaryItem) globalCache.Get(GetCacheIdKey<IDictionaryItem>(item.Id), () => null);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(item.Id, globalCached.Id);
            Assert.AreEqual("item-key", globalCached.ItemKey);

            _distributedCacheBinder = new DistributedCacheBinder(new DistributedCache(), Mock.Of<IUmbracoContextFactory>(), Mock.Of<ILogger>());
            _distributedCacheBinder.BindEvents(true);

            Assert.IsNull(scopeProvider.AmbientScope);
            using (var scope = scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
            {
                Assert.IsInstanceOf<Core.Scoping.Scope>(scope);
                Assert.IsNotNull(scopeProvider.AmbientScope);
                Assert.AreSame(scope, scopeProvider.AmbientScope);

                // scope has its own isolated cache
                var scopedCache = scope.IsolatedCaches.GetOrCreate(typeof (IDictionaryItem));
                Assert.AreNotSame(globalCache, scopedCache);

                item.ItemKey = "item-changed";
                service.Save(item);

                // scoped cache contains the "new" entity
                var scopeCached = (IDictionaryItem) scopedCache.Get(GetCacheIdKey<IDictionaryItem>(item.Id), () => null);
                Assert.IsNotNull(scopeCached);
                Assert.AreEqual(item.Id, scopeCached.Id);
                Assert.AreEqual("item-changed", scopeCached.ItemKey);

                // global cache is unchanged
                globalCached = (IDictionaryItem) globalCache.Get(GetCacheIdKey<IDictionaryItem>(item.Id), () => null);
                Assert.IsNotNull(globalCached);
                Assert.AreEqual(item.Id, globalCached.Id);
                Assert.AreEqual("item-key", globalCached.ItemKey);

                if (complete)
                    scope.Complete();
            }
            Assert.IsNull(scopeProvider.AmbientScope);

            globalCached = (IDictionaryItem) globalCache.Get(GetCacheIdKey<IDictionaryItem>(item.Id), () => null);
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
            globalCached = (IDictionaryItem) globalCache.Get(GetCacheIdKey<IDictionaryItem>(item.Id), () => null);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(item.Id, globalCached.Id);
            Assert.AreEqual(complete ? "item-changed" : "item-key", globalCached.ItemKey);
        }

        public static string GetCacheIdKey<T>(object id)
        {
            return $"{GetCacheTypeKey<T>()}{id}";
        }

        public static string GetCacheTypeKey<T>()
        {
            return $"uRepo_{typeof (T).Name}_";
        }

        public class PassiveEventDispatcher : QueuingEventDispatcherBase
        {
            public PassiveEventDispatcher()
                : base(false)
            { }

            protected override void ScopeExitCompleted()
            {
                // do nothing
            }
        }

        public class LocalServerMessenger : ServerMessengerBase
        {
            public LocalServerMessenger()
                : base(false)
            { }

            protected override void DeliverRemote(ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null)
            {
                throw new NotImplementedException();
            }
        }
    }
}
