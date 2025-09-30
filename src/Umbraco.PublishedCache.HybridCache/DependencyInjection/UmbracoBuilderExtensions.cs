
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders.Document;
using Umbraco.Cms.Infrastructure.HybridCache.SeedKeyProviders.Media;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.HybridCache.Services;


namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder" /> for the Umbraco's NuCache
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds Umbraco NuCache dependencies
    /// </summary>
    public static IUmbracoBuilder AddUmbracoHybridCache(this IUmbracoBuilder builder)
    {
#pragma warning disable EXTEXP0018
        builder.Services.AddHybridCache(options =>
        {
            // We'll be a bit friendlier and default this to a higher value, you quickly hit the 1MB limit with a few languages and especially blocks.
            // This can be overwritten later if needed.
            options.MaximumPayloadBytes = 1024 * 1024 * 100; // 100MB
        }).AddSerializer<ContentCacheNode, HybridCacheSerializer>();
#pragma warning restore EXTEXP0018
        builder.Services.AddSingleton<IDatabaseCacheRepository, DatabaseCacheRepository>();
        builder.Services.AddSingleton<IPublishedContentCache, DocumentCache>();
        builder.Services.AddSingleton<IPublishedMediaCache, MediaCache>();
        builder.Services.AddSingleton<IPublishedMemberCache, MemberCache>();
        builder.Services.AddSingleton<IDomainCache, DomainCache>();
        builder.Services.AddSingleton<IElementsCache, ElementsDictionaryAppCache>();
        builder.Services.AddSingleton<IPublishedContentTypeCache, PublishedContentTypeCache>();
        builder.Services.AddSingleton<IDocumentCacheService, DocumentCacheService>();
        builder.Services.AddSingleton<IMediaCacheService, MediaCacheService>();
        builder.Services.AddSingleton<IMemberCacheService, MemberCacheService>();
        builder.Services.AddSingleton<IDomainCacheService, DomainCacheService>();
        builder.Services.AddSingleton<IPublishedContentFactory, PublishedContentFactory>();
        builder.Services.AddSingleton<ICacheNodeFactory, CacheNodeFactory>();
        builder.Services.AddSingleton<ICacheManager, CacheManager>();
        builder.Services.AddSingleton<IDatabaseCacheRebuilder, DatabaseCacheRebuilder>();
        builder.Services.AddSingleton<IContentCacheDataSerializerFactory>(s =>
        {
            IOptions<NuCacheSettings> options = s.GetRequiredService<IOptions<NuCacheSettings>>();
            switch (options.Value.NuCacheSerializerType)
            {
                case NuCacheSerializerType.JSON:
                    return new JsonContentNestedDataSerializerFactory();
                case NuCacheSerializerType.MessagePack:
                    return ActivatorUtilities.CreateInstance<MsgPackContentNestedDataSerializerFactory>(s);
                default:
                    throw new IndexOutOfRangeException();
            }
        });
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, HybridCacheStartupNotificationHandler>(); // Need to happen before notification handlers use the cache. Eg. seeding
        builder.Services.AddSingleton<IPropertyCacheCompressionOptions, NoopPropertyCacheCompressionOptions>();
        builder.AddNotificationAsyncHandler<ContentRefreshNotification, CacheRefreshingNotificationHandler>();
        builder.AddNotificationAsyncHandler<ContentDeletedNotification, CacheRefreshingNotificationHandler>();
        builder.AddNotificationAsyncHandler<MediaRefreshNotification, CacheRefreshingNotificationHandler>();
        builder.AddNotificationAsyncHandler<MediaDeletedNotification, CacheRefreshingNotificationHandler>();
        builder.AddNotificationAsyncHandler<ContentTypeRefreshedNotification, CacheRefreshingNotificationHandler>();
        builder.AddNotificationAsyncHandler<ContentTypeDeletedNotification, CacheRefreshingNotificationHandler>();
        builder.AddNotificationAsyncHandler<MediaTypeRefreshedNotification, CacheRefreshingNotificationHandler>();
        builder.AddNotificationAsyncHandler<MediaTypeDeletedNotification, CacheRefreshingNotificationHandler>();
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, SeedingNotificationHandler>();
        builder.AddCacheSeeding();
        return builder;
    }

    private static IUmbracoBuilder AddCacheSeeding(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IDocumentSeedKeyProvider, ContentTypeSeedKeyProvider>();
        builder.Services.AddSingleton<IDocumentSeedKeyProvider, DocumentBreadthFirstKeyProvider>();


        builder.Services.AddSingleton<IMediaSeedKeyProvider, MediaBreadthFirstKeyProvider>();
        return builder;
    }
}
