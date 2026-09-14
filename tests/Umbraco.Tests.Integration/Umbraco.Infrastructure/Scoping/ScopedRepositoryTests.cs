// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Infrastructure.Sync;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ScopedRepositoryTests : UmbracoIntegrationTest
{
    private IUserService UserService => GetRequiredService<IUserService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

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
        builder.AddUmbracoHybridCache();
        builder.Services.AddUnique<IServerMessenger, LocalServerMessenger>();
        builder
            .AddNotificationHandler<DictionaryItemDeletedNotification, DictionaryItemDeletedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<DictionaryItemSavedNotification, DictionaryItemSavedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<LanguageSavedNotification, LanguageSavedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<LanguageDeletedNotification, LanguageDeletedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<UserSavedNotification, UserSavedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<LanguageDeletedNotification, LanguageDeletedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<MemberGroupDeletedNotification, MemberGroupDeletedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<MemberGroupSavedNotification, MemberGroupSavedDistributedCacheNotificationHandler>();
        // builder.AddNotificationHandler<LanguageSavedNotification, PublishedSnapshotServiceEventHandler>();
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task DefaultRepositoryCachePolicy(bool complete)
    {
        var scopeProvider = (ScopeProvider)ScopeProvider;
        var service = (UserService)UserService;
        var globalCache = AppCaches.IsolatedCaches.GetOrCreate(typeof(IUser));
        var user = (IUser)new User(GlobalSettings, "name", "email", "username", "rawPassword");
        service.Save(user);

        // User has been saved so the cache has been cleared of it
        var globalCached = (IUser)globalCache.Get(GetCacheIdKey<IUser>(user.Key), () => null);
        Assert.That(globalCached, Is.Null);
        // Get user again to load it into the cache again, this also ensure we don't modify the one that's in the cache.
        user = await service.GetAsync(user.Key);

        // global cache contains the entity
        globalCached = (IUser)globalCache.Get(GetCacheIdKey<IUser>(user.Key), () => null);
        Assert.That(globalCached, Is.Not.Null);
        Assert.That(globalCached.Id, Is.EqualTo(user.Id));
        Assert.That(globalCached.Name, Is.EqualTo("name"));

        Assert.That(scopeProvider.AmbientScope, Is.Null);
        using (var scope = scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
        {
            Assert.That(scope, Is.InstanceOf<Scope>());
            Assert.That(scopeProvider.AmbientScope, Is.Not.Null);
            Assert.That(scopeProvider.AmbientScope, Is.SameAs(scope));

            // scope has its own isolated cache
            var scopedCache = scope.IsolatedCaches.GetOrCreate(typeof(IUser));
            Assert.That(scopedCache, Is.Not.SameAs(globalCache));

            user.Name = "changed";
            service.Save(user);

            // scoped cache contains the "new" entity
            var scopeCached = (IUser)scopedCache.Get(GetCacheIdKey<IUser>(user.Id), () => null);
            Assert.That(scopeCached, Is.Not.Null);
            Assert.That(scopeCached.Id, Is.EqualTo(user.Id));
            Assert.That(scopeCached.Name, Is.EqualTo("changed"));

            // global cache is unchanged
            globalCached = (IUser)globalCache.Get(GetCacheIdKey<IUser>(user.Key), () => null);
            Assert.That(globalCached, Is.Not.Null);
            Assert.That(globalCached.Id, Is.EqualTo(user.Id));
            Assert.That(globalCached.Name, Is.EqualTo("name"));

            if (complete)
            {
                scope.Complete();
            }
        }

        Assert.That(scopeProvider.AmbientScope, Is.Null);

        globalCached = (IUser)globalCache.Get(GetCacheIdKey<IUser>(user.Key), () => null);
        if (complete)
        {
            // global cache has been cleared
            Assert.That(globalCached, Is.Null);
        }
        else
        {
            // global cache has *not* been cleared
            Assert.That(globalCached, Is.Not.Null);
        }

        // get again, updated if completed
        user = await service.GetAsync(user.Key);
        Assert.That(user.Name, Is.EqualTo(complete ? "changed" : "name"));

        // global cache contains the entity again
        globalCached = (IUser)globalCache.Get(GetCacheIdKey<IUser>(user.Key), () => null);
        Assert.That(globalCached, Is.Not.Null);
        Assert.That(globalCached.Id, Is.EqualTo(user.Id));
        Assert.That(globalCached.Name, Is.EqualTo(complete ? "changed" : "name"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task FullDataSetRepositoryCachePolicy(bool complete)
    {
        var scopeProvider = (ScopeProvider)ScopeProvider;
        var service = LanguageService;
        var globalCache = AppCaches.IsolatedCaches.GetOrCreate(typeof(ILanguage));

        ILanguage lang = new Language("fr-FR", "French (France)");
        await service.CreateAsync(lang, Constants.Security.SuperUserKey);

        // global cache has been flushed, reload
        var globalFullCached = (IEnumerable<ILanguage>)globalCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
        Assert.That(globalFullCached, Is.Null);
        var reload = await service.GetAsync(lang.IsoCode);

        // global cache contains the entity
        globalFullCached = (IEnumerable<ILanguage>)globalCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
        Assert.That(globalFullCached, Is.Not.Null);
        var globalCached = globalFullCached.First(x => x.Id == lang.Id);
        Assert.That(globalCached, Is.Not.Null);
        Assert.That(globalCached.Id, Is.EqualTo(lang.Id));
        Assert.That(globalCached.IsoCode, Is.EqualTo("fr-FR"));

        Assert.That(scopeProvider.AmbientScope, Is.Null);
        using (var scope = scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
        {
            Assert.That(scope, Is.InstanceOf<Scope>());
            Assert.That(scopeProvider.AmbientScope, Is.Not.Null);
            Assert.That(scopeProvider.AmbientScope, Is.SameAs(scope));

            // scope has its own isolated cache
            var scopedCache = scope.IsolatedCaches.GetOrCreate(typeof(ILanguage));
            Assert.That(scopedCache, Is.Not.SameAs(globalCache));

            // Use IsMandatory of isocode to ensure publishedContent cache is not also rebuild
            lang.IsMandatory = true;
            await service.UpdateAsync(lang, Constants.Security.SuperUserKey);

            // scoped cache has been flushed, reload
            var scopeFullCached = (IEnumerable<ILanguage>)scopedCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
            Assert.That(scopeFullCached, Is.Null);
            reload = await service.GetAsync(lang.IsoCode);

            // scoped cache contains the "new" entity
            scopeFullCached = (IEnumerable<ILanguage>)scopedCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
            Assert.That(scopeFullCached, Is.Not.Null);
            var scopeCached = scopeFullCached.First(x => x.Id == lang.Id);
            Assert.That(scopeCached, Is.Not.Null);
            Assert.That(scopeCached.Id, Is.EqualTo(lang.Id));
            Assert.That(scopeCached.IsMandatory, Is.EqualTo(true));

            // global cache is unchanged
            globalFullCached = (IEnumerable<ILanguage>)globalCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
            Assert.That(globalFullCached, Is.Not.Null);
            globalCached = globalFullCached.First(x => x.Id == lang.Id);
            Assert.That(globalCached, Is.Not.Null);
            Assert.That(globalCached.Id, Is.EqualTo(lang.Id));
            Assert.That(globalCached.IsMandatory, Is.EqualTo(false));

            if (complete)
            {
                scope.Complete();
            }
        }

        Assert.That(scopeProvider.AmbientScope, Is.Null);

        globalFullCached = (IEnumerable<ILanguage>)globalCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
        if (complete)
        {
            // global cache has been cleared
            Assert.That(globalFullCached, Is.Null);
        }
        else
        {
            // global cache has *not* been cleared
            Assert.That(globalFullCached, Is.Not.Null);
        }

        // get again, updated if completed
        lang = await service.GetAsync(lang.IsoCode);
        Assert.That(lang.IsMandatory, Is.EqualTo(complete));

        // global cache contains the entity again
        globalFullCached = (IEnumerable<ILanguage>)globalCache.Get(GetCacheTypeKey<ILanguage>(), () => null);
        Assert.That(globalFullCached, Is.Not.Null);
        globalCached = globalFullCached.First(x => x.Id == lang.Id);
        Assert.That(globalCached, Is.Not.Null);
        Assert.That(globalCached.Id, Is.EqualTo(lang.Id));
        Assert.That(lang.IsMandatory, Is.EqualTo(complete));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task SingleItemsOnlyRepositoryCachePolicy(bool complete)
    {
        var scopeProvider = (ScopeProvider)ScopeProvider;
        var languageService = LanguageService;
        var dictionaryItemService = DictionaryItemService;
        var globalCache = AppCaches.IsolatedCaches.GetOrCreate(typeof(IDictionaryItem));

        var lang = new Language("fr-FR", "French (France)");
        await languageService.CreateAsync(lang, Constants.Security.SuperUserKey);

        var item = (await dictionaryItemService.CreateAsync(
            new DictionaryItem("item-key")
            {
                Translations = new IDictionaryTranslation[] { new DictionaryTranslation(lang, "item-value") }
            },
            Constants.Security.SuperUserKey)).Result;

        // Refresh the keyed cache manually
        await dictionaryItemService.GetAsync(item.Key);
        await languageService.GetAsync(lang.IsoCode);

        // global cache contains the entity
        var globalCached = (IDictionaryItem)globalCache.Get(GetCacheIdKey<IDictionaryItem>(item.Key), () => null);
        Assert.That(globalCached, Is.Not.Null);
        Assert.That(globalCached.Id, Is.EqualTo(item.Id));
        Assert.That(globalCached.Key, Is.EqualTo(item.Key));
        Assert.That(globalCached.ItemKey, Is.EqualTo("item-key"));

        Assert.That(scopeProvider.AmbientScope, Is.Null);
        using (var scope = scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
        {
            Assert.That(scope, Is.InstanceOf<Scope>());
            Assert.That(scopeProvider.AmbientScope, Is.Not.Null);
            Assert.That(scopeProvider.AmbientScope, Is.SameAs(scope));

            // scope has its own isolated cache
            var scopedCache = scope.IsolatedCaches.GetOrCreate(typeof(IDictionaryItem));
            Assert.That(scopedCache, Is.Not.SameAs(globalCache));

            item.ItemKey = "item-changed";
            await dictionaryItemService.UpdateAsync(item, Constants.Security.SuperUserKey);

            // scoped cache contains the "new" entity
            // Refresh the keyed cache manually
            await dictionaryItemService.GetAsync(item.Key);
            var scopeCached = (IDictionaryItem)scopedCache.Get(GetCacheIdKey<IDictionaryItem>(item.Key), () => null);
            Assert.That(scopeCached, Is.Not.Null);
            Assert.That(scopeCached.Id, Is.EqualTo(item.Id));
            Assert.That(scopeCached.ItemKey, Is.EqualTo("item-changed"));

            // global cache is unchanged
            globalCached = (IDictionaryItem)globalCache.Get(GetCacheIdKey<IDictionaryItem>(item.Key), () => null);
            Assert.That(globalCached, Is.Not.Null);
            Assert.That(globalCached.Id, Is.EqualTo(item.Id));
            Assert.That(globalCached.ItemKey, Is.EqualTo("item-key"));

            if (complete)
            {
                scope.Complete();
            }
        }

        Assert.That(scopeProvider.AmbientScope, Is.Null);

        globalCached = (IDictionaryItem)globalCache.Get(GetCacheIdKey<IDictionaryItem>(item.Key), () => null);
        if (complete)
        {
            // global cache has been cleared
            Assert.That(globalCached, Is.Null);
        }
        else
        {
            // global cache has *not* been cleared
            Assert.That(globalCached, Is.Not.Null);
        }

        // get again, updated if completed
        item = await dictionaryItemService.GetAsync(item.Key);
        Assert.That(item.ItemKey, Is.EqualTo(complete ? "item-changed" : "item-key"));

        // global cache contains the entity again
        globalCached = (IDictionaryItem)globalCache.Get(GetCacheIdKey<IDictionaryItem>(item.Key), () => null);
        Assert.That(globalCached, Is.Not.Null);
        Assert.That(globalCached.Id, Is.EqualTo(item.Id));
        Assert.That(globalCached.ItemKey, Is.EqualTo(complete ? "item-changed" : "item-key"));
    }

    public static string GetCacheIdKey<T>(object id) => $"{GetCacheTypeKey<T>()}{id}";

    public static string GetCacheTypeKey<T>() => $"uRepo_{typeof(T).Name}_";

    public class LocalServerMessenger : ServerMessengerBase
    {
        public LocalServerMessenger()
            : base(false, new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()))
        {
        }

        public override void SendMessages() { }

        public override void Sync() { }

        protected override void DeliverRemote(ICacheRefresher refresher, MessageType messageType, IEnumerable<object> ids = null, string json = null)
        {
        }
    }
}
