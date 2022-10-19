// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Sync;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class ScopedRepositoryTests : UmbracoIntegrationTest
{
    private IUserService UserService => GetRequiredService<IUserService>();

    private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        // this is what's created core web runtime
        var appCaches = new AppCaches(
            new DeepCloneAppCache(new ObjectCacheAppCache()),
            NoAppCache.Instance,
            new IsolatedCaches(type => new DeepCloneAppCache(new ObjectCacheAppCache())));

        services.AddUnique(appCaches);
    }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNuCache();
        builder.Services.AddUnique<IServerMessenger, LocalServerMessenger>();
        builder
            .AddNotificationHandler<DictionaryItemDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<DictionaryItemSavedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<LanguageSavedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<LanguageDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<UserSavedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<LanguageDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<MemberGroupDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<MemberGroupSavedNotification, DistributedCacheBinder>();
        builder.AddNotificationHandler<LanguageSavedNotification, PublishedSnapshotServiceEventHandler>();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void DefaultRepositoryCachePolicy(bool complete)
    {
        var scopeProvider = (ScopeProvider)ScopeProvider;
        var service = (UserService)UserService;
        var globalCache = AppCaches.IsolatedCaches.GetOrCreate(typeof(IUser));
        var user = (IUser)new User(GlobalSettings, "name", "email", "username", "rawPassword");
        service.Save(user);

        // User has been saved so the cache has been cleared of it
        var globalCached = (IUser)globalCache.Get(GetCacheIdKey<IUser>(user.Id), () => null);
        Assert.IsNull(globalCached);
        // Get user again to load it into the cache again, this also ensure we don't modify the one that's in the cache.
        user = service.GetUserById(user.Id);

        // global cache contains the entity
        globalCached = (IUser)globalCache.Get(GetCacheIdKey<IUser>(user.Id), () => null);
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
            var scopedCache = scope.IsolatedCaches.GetOrCreate(typeof(IUser));
            Assert.AreNotSame(globalCache, scopedCache);

            user.Name = "changed";
            service.Save(user);

            // scoped cache contains the "new" entity
            var scopeCached = (IUser)scopedCache.Get(GetCacheIdKey<IUser>(user.Id), () => null);
            Assert.IsNotNull(scopeCached);
            Assert.AreEqual(user.Id, scopeCached.Id);
            Assert.AreEqual("changed", scopeCached.Name);

            // global cache is unchanged
            globalCached = (IUser)globalCache.Get(GetCacheIdKey<IUser>(user.Id), () => null);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(user.Id, globalCached.Id);
            Assert.AreEqual("name", globalCached.Name);

            if (complete)
            {
                scope.Complete();
            }
        }

        Assert.IsNull(scopeProvider.AmbientScope);

        globalCached = (IUser)globalCache.Get(GetCacheIdKey<IUser>(user.Id), () => null);
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
        globalCached = (IUser)globalCache.Get(GetCacheIdKey<IUser>(user.Id), () => null);
        Assert.IsNotNull(globalCached);
        Assert.AreEqual(user.Id, globalCached.Id);
        Assert.AreEqual(complete ? "changed" : "name", globalCached.Name);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void FullDataSetRepositoryCachePolicy(bool complete)
    {
        var scopeProvider = (ScopeProvider)ScopeProvider;
        var service = LocalizationService;
        var globalCache = AppCaches.IsolatedCaches.GetOrCreate(typeof(ILanguage));

        ILanguage lang = new Language("fr-FR", "French (France)");
        service.Save(lang);

        // global cache has been flushed, reload
        var globalFullCached = (IEnumerable<ILanguage>)globalCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
        Assert.IsNull(globalFullCached);
        var reload = service.GetLanguageById(lang.Id);

        // global cache contains the entity
        globalFullCached = (IEnumerable<ILanguage>)globalCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
        Assert.IsNotNull(globalFullCached);
        var globalCached = globalFullCached.First(x => x.Id == lang.Id);
        Assert.IsNotNull(globalCached);
        Assert.AreEqual(lang.Id, globalCached.Id);
        Assert.AreEqual("fr-FR", globalCached.IsoCode);

        Assert.IsNull(scopeProvider.AmbientScope);
        using (var scope = scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
        {
            Assert.IsInstanceOf<Scope>(scope);
            Assert.IsNotNull(scopeProvider.AmbientScope);
            Assert.AreSame(scope, scopeProvider.AmbientScope);

            // scope has its own isolated cache
            var scopedCache = scope.IsolatedCaches.GetOrCreate(typeof(ILanguage));
            Assert.AreNotSame(globalCache, scopedCache);

            // Use IsMandatory of isocode to ensure publishedContent cache is not also rebuild
            lang.IsMandatory = true;
            service.Save(lang);

            // scoped cache has been flushed, reload
            var scopeFullCached = (IEnumerable<ILanguage>)scopedCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
            Assert.IsNull(scopeFullCached);
            reload = service.GetLanguageById(lang.Id);

            // scoped cache contains the "new" entity
            scopeFullCached = (IEnumerable<ILanguage>)scopedCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
            Assert.IsNotNull(scopeFullCached);
            var scopeCached = scopeFullCached.First(x => x.Id == lang.Id);
            Assert.IsNotNull(scopeCached);
            Assert.AreEqual(lang.Id, scopeCached.Id);
            Assert.AreEqual(true, scopeCached.IsMandatory);

            // global cache is unchanged
            globalFullCached = (IEnumerable<ILanguage>)globalCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
            Assert.IsNotNull(globalFullCached);
            globalCached = globalFullCached.First(x => x.Id == lang.Id);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(lang.Id, globalCached.Id);
            Assert.AreEqual(false, globalCached.IsMandatory);

            if (complete)
            {
                scope.Complete();
            }
        }

        Assert.IsNull(scopeProvider.AmbientScope);

        globalFullCached = (IEnumerable<ILanguage>)globalCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
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
        Assert.AreEqual(complete, lang.IsMandatory);

        // global cache contains the entity again
        globalFullCached = (IEnumerable<ILanguage>)globalCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
        Assert.IsNotNull(globalFullCached);
        globalCached = globalFullCached.First(x => x.Id == lang.Id);
        Assert.IsNotNull(globalCached);
        Assert.AreEqual(lang.Id, globalCached.Id);
        Assert.AreEqual(complete, lang.IsMandatory);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SingleItemsOnlyRepositoryCachePolicy(bool complete)
    {
        var scopeProvider = (ScopeProvider)ScopeProvider;
        var service = LocalizationService;
        var globalCache = AppCaches.IsolatedCaches.GetOrCreate(typeof(IDictionaryItem));

        var lang = new Language("fr-FR", "French (France)");
        service.Save(lang);

        var item = (IDictionaryItem)new DictionaryItem("item-key");
        item.Translations = new IDictionaryTranslation[] { new DictionaryTranslation(lang.Id, "item-value") };
        service.Save(item);

        // Refresh the cache manually because we can't unbind
        service.GetDictionaryItemById(item.Id);
        service.GetLanguageById(lang.Id);

        // global cache contains the entity
        var globalCached = (IDictionaryItem)globalCache.Get(GetCacheIdKey<IDictionaryItem>(item.Id), () => null);
        Assert.IsNotNull(globalCached);
        Assert.AreEqual(item.Id, globalCached.Id);
        Assert.AreEqual("item-key", globalCached.ItemKey);

        Assert.IsNull(scopeProvider.AmbientScope);
        using (var scope = scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
        {
            Assert.IsInstanceOf<Scope>(scope);
            Assert.IsNotNull(scopeProvider.AmbientScope);
            Assert.AreSame(scope, scopeProvider.AmbientScope);

            // scope has its own isolated cache
            var scopedCache = scope.IsolatedCaches.GetOrCreate(typeof(IDictionaryItem));
            Assert.AreNotSame(globalCache, scopedCache);

            item.ItemKey = "item-changed";
            service.Save(item);

            // scoped cache contains the "new" entity
            var scopeCached = (IDictionaryItem)scopedCache.Get(GetCacheIdKey<IDictionaryItem>(item.Id), () => null);
            Assert.IsNotNull(scopeCached);
            Assert.AreEqual(item.Id, scopeCached.Id);
            Assert.AreEqual("item-changed", scopeCached.ItemKey);

            // global cache is unchanged
            globalCached = (IDictionaryItem)globalCache.Get(GetCacheIdKey<IDictionaryItem>(item.Id), () => null);
            Assert.IsNotNull(globalCached);
            Assert.AreEqual(item.Id, globalCached.Id);
            Assert.AreEqual("item-key", globalCached.ItemKey);

            if (complete)
            {
                scope.Complete();
            }
        }

        Assert.IsNull(scopeProvider.AmbientScope);

        globalCached = (IDictionaryItem)globalCache.Get(GetCacheIdKey<IDictionaryItem>(item.Id), () => null);
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
        globalCached = (IDictionaryItem)globalCache.Get(GetCacheIdKey<IDictionaryItem>(item.Id), () => null);
        Assert.IsNotNull(globalCached);
        Assert.AreEqual(item.Id, globalCached.Id);
        Assert.AreEqual(complete ? "item-changed" : "item-key", globalCached.ItemKey);
    }

    public static string GetCacheIdKey<T>(object id) => $"{GetCacheTypeKey<T>()}{id}";

    public static string GetCacheTypeKey<T>() => $"uRepo_{typeof(T).Name}_";

    public class LocalServerMessenger : ServerMessengerBase
    {
        public LocalServerMessenger()
            : base(false)
        {
        }

        public override void SendMessages() { }

        public override void Sync() { }

        protected override void DeliverRemote(ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null)
        {
        }
    }
}
