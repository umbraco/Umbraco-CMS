using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.Sync;
using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.Scoping;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class EfCoreScopeRepositoryCacheTest : UmbracoIntegrationTest
{
    private IUserRepository UserRepository => GetRequiredService<IUserRepository>();
    private IEfCoreScopeProvider<UmbracoEFContext> EfCoreScopeProvider => GetRequiredService<IEfCoreScopeProvider<UmbracoEFContext>>();

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
        // builder
        //     .AddNotificationHandler<DictionaryItemDeletedNotification, DistributedCacheBinder>()
        //     .AddNotificationHandler<DictionaryItemSavedNotification, DistributedCacheBinder>()
        //     .AddNotificationHandler<LanguageSavedNotification, DistributedCacheBinder>()
        //     .AddNotificationHandler<LanguageDeletedNotification, DistributedCacheBinder>()
        //     .AddNotificationHandler<UserSavedNotification, DistributedCacheBinder>()
        //     .AddNotificationHandler<LanguageDeletedNotification, DistributedCacheBinder>()
        //     .AddNotificationHandler<MemberGroupDeletedNotification, DistributedCacheBinder>()
        //     .AddNotificationHandler<MemberGroupSavedNotification, DistributedCacheBinder>();
        // builder.AddNotificationHandler<LanguageSavedNotification, PublishedSnapshotServiceEventHandler>();
    }

    [Test]
    public void DefaultRepositoryCachePolicy()
    {
    }


    public static string GetCacheIdKey<T>(object id) => $"{GetCacheTypeKey<T>()}{id}";
    public static string GetCacheTypeKey<T>() => $"uRepo_{typeof(T).Name}_";


    private class LocalServerMessenger : ServerMessengerBase
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
