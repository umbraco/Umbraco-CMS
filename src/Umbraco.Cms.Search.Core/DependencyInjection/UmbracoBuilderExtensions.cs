using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Search.Indexing.Collection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Search.PropertyValueHandlers;
using Umbraco.Cms.Search.Core.Cache;
using Umbraco.Cms.Search.Core.Cache.Content;
using Umbraco.Cms.Search.Core.Cache.ContentType;
using Umbraco.Cms.Search.Core.Cache.Index;
using Umbraco.Cms.Search.Core.Cache.Language;
using Umbraco.Cms.Search.Core.Cache.Media;
using Umbraco.Cms.Search.Core.Cache.MediaType;
using Umbraco.Cms.Search.Core.Cache.Member;
using Umbraco.Cms.Search.Core.Cache.MemberType;
using Umbraco.Cms.Search.Core.Cache.PublicAccess;
using Umbraco.Cms.Search.Core.NotificationHandlers;
using Umbraco.Cms.Search.Core.Persistence;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Search.Core.Services.ContentIndexing.Indexers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.DependencyInjection;

/// <summary>
/// Provides extension methods for registering the core Umbraco Search services on an <see cref="IUmbracoBuilder"/>.
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    /// Adds all core services required to run Umbraco Search.
    /// </summary>
    /// <remarks>
    /// This method is idempotent - calling it multiple times has no effect after the first call.
    /// </remarks>
    /// <param name="builder">The Umbraco builder.</param>
    /// <returns>The Umbraco builder.</returns>
    public static IUmbracoBuilder AddSearchCore(this IUmbracoBuilder builder)
    {
        // Idempotency check - safe to call multiple times.
        if (builder.Services.Any(s => s.ServiceType == typeof(AddSearchCoreMarker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<AddSearchCoreMarker>();

        builder.Services.AddSingleton<IContentIndexingService, ContentIndexingService>();
        builder.Services.AddSingleton<IContentTypeIndexingService, ContentTypeIndexingService>();
        builder.Services.AddSingleton<IOriginProvider, OriginProvider>();
        builder.Services.AddSingleton<ISearcherResolver, SearcherResolver>();
        builder.Services.AddSingleton<IIndexerResolver, IndexerResolver>();
        builder.Services.AddTransient<IHtmlIndexValueParser, HtmlIndexValueParser>();

        builder.Services.AddTransient<IContentIndexingDataCollectionService, ContentIndexingDataCollectionService>();

        builder.Services.AddTransient<IContentIndexer, SystemFieldsContentIndexer>();
        builder.Services.AddTransient<IContentIndexer, PropertyValueFieldsContentIndexer>();
        builder.Services.AddTransient<ISystemFieldsContentIndexer, SystemFieldsContentIndexer>();

        builder.Services.AddTransient<IDateTimeOffsetConverter, DateTimeOffsetConverter>();
        builder.Services.AddTransient<IContentProtectionProvider, ContentProtectionProvider>();

        builder.Services.AddTransient<PublishedContentChangeStrategy>();
        builder.Services.AddTransient<DraftContentChangeStrategy>();

        builder.Services.AddTransient<IPublishedContentChangeStrategy, PublishedContentChangeStrategy>();
        builder.Services.AddTransient<IDraftContentChangeStrategy, DraftContentChangeStrategy>();

        builder.Services.AddSingleton<IIndexDocumentRepository, IndexDocumentRepository>();
        builder.Services.AddSingleton<IIndexDocumentService, IndexDocumentService>();

        // replace the core IPublishedContentQuery with the search enabled implementation (same scoped lifetime as the core registration)
        builder.Services.AddUnique<Umbraco.Cms.Core.IPublishedContentQuery, SearchEnabledPublishedContentQuery>(ServiceLifetime.Scoped);

        builder.Services.AddUnique<IIndexedEntitySearchService, IndexedEntitySearchService>();
        builder.Services.AddUnique<IContentSearchService, ContentSearchService>();
        builder.Services.AddUnique<IMediaSearchService, MediaSearchService>();

        // we need these notification handlers explicitly registered for the distributed content index refresher
        builder.Services.AddTransient<DraftContentNotificationHandler>();
        builder.Services.AddTransient<PublishedContentNotificationHandler>();
        builder.Services.AddTransient<DraftMediaNotificationHandler>();
        builder.Services.AddTransient<DraftMemberNotificationHandler>();

        builder.Services.AddTransient<RebuildIndexNotificationHandler>();
        builder.Services.AddTransient<IDistributedContentIndexRefresher, DistributedContentIndexRefresher>();
        builder.Services.AddTransient<IDistributedContentIndexRebuilder, DistributedContentIndexRebuilder>();

        builder
            .AddNotificationHandler<LanguageCacheRefresherNotification, RebuildIndexesNotificationHandler>()
            .AddNotificationHandler<ContentTypeCacheRefresherNotification, RebuildIndexesNotificationHandler>()
            .AddNotificationHandler<MemberTypeCacheRefresherNotification, RebuildIndexesNotificationHandler>()
            .AddNotificationHandler<MediaTypeCacheRefresherNotification, RebuildIndexesNotificationHandler>()
            .AddNotificationHandler<RebuildIndexCacheRefresherNotification, RebuildIndexesNotificationHandler>();

        builder
            .AddNotificationHandler<DraftContentCacheRefresherNotification, ContentIndexingNotificationHandler>()
            .AddNotificationHandler<DraftMediaCacheRefresherNotification, ContentIndexingNotificationHandler>()
            .AddNotificationHandler<DraftMemberCacheRefresherNotification, ContentIndexingNotificationHandler>()
            .AddNotificationHandler<PublishedContentCacheRefresherNotification, ContentIndexingNotificationHandler>()
            .AddNotificationAsyncHandler<PublicAccessDetailedCacheRefresherNotification, PublicAccessIndexingNotificationHandler>();

        builder
            .WithCollectionBuilder<PropertyValueHandlerCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<IPropertyValueHandler>());

        builder.AddCustomCacheRefresherNotificationHandlers();

        return builder;
    }

    /// <summary>
    /// Marker class to ensure AddSearchCore is only executed once.
    /// </summary>
    private sealed class AddSearchCoreMarker
    {
    }
}
