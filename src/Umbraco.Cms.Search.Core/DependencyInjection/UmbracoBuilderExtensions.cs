using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.Cache;
using Umbraco.Cms.Search.Core.Notifications;
using Umbraco.Cms.Search.Core.Cache.Content;
using Umbraco.Cms.Search.Core.Cache.ContentType;
using Umbraco.Cms.Search.Core.Cache.Index;
using Umbraco.Cms.Search.Core.Cache.Language;
using Umbraco.Cms.Search.Core.Cache.Media;
using Umbraco.Cms.Search.Core.Cache.MediaType;
using Umbraco.Cms.Search.Core.Cache.Member;
using Umbraco.Cms.Search.Core.Cache.MemberType;
using Umbraco.Cms.Search.Core.Cache.PublicAccess;
using Umbraco.Cms.Search.Core.Helpers;
using Umbraco.Cms.Search.Core.NotificationHandlers;
using Umbraco.Cms.Search.Core.Persistence;
using Umbraco.Cms.Search.Core.PropertyValueHandlers;
using Umbraco.Cms.Search.Core.PropertyValueHandlers.Collection;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Search.Core.Services.ContentIndexing.Indexers;

namespace Umbraco.Cms.Search.Core.DependencyInjection;

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
            .AddNotificationHandler<RebuildIndexCacheRefresherNotification, RebuildIndexesNotificationHandler>()
            .AddNotificationAsyncHandler<IndexRebuildStartingNotification, IndexRebuildServerEventNotificationHandler>()
            .AddNotificationAsyncHandler<IndexRebuildCompletedNotification, IndexRebuildServerEventNotificationHandler>();

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

        // Add a dedicated OpenAPI document for our own package that can be browsed via the Swagger UI,
        // along with a generated swagger JSON file used to auto-generate the TypeScript client.
        builder.AddBackOfficeOpenApiDocument(
            Constants.Api.Name,
            document => document
                .WithTitle("Umbraco Search Management API")
                .WithBackOfficeAuthentication()
                .WithJsonOptions(Umbraco.Cms.Core.Constants.JsonOptionsNames.BackOffice)
                .ConfigureOpenApiOptions(options =>
                {
                    options.AddDocumentTransformer((openApiDocument, _, _) =>
                    {
                        openApiDocument.Info.Version = "1.0";
                        return Task.CompletedTask;
                    });

                    // Emit short operation IDs (the controller action name) so the generated TypeScript
                    // client has concise method names instead of the verbose path-based defaults.
                    options.AddOperationTransformer<ActionNameOperationIdTransformer>();
                }));

        return builder;
    }

    // Sets each operation's ID to the controller action name, matching the operation IDs the generated
    // TypeScript client was built against.
    // https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/umbraco-schema-and-operation-ids#operation-ids
    private sealed class ActionNameOperationIdTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            if (context.Description.ActionDescriptor.RouteValues.TryGetValue("action", out var action)
                && string.IsNullOrWhiteSpace(action) is false)
            {
                operation.OperationId = action;
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Marker class to ensure AddSearchCore is only executed once.
    /// </summary>
    private sealed class AddSearchCoreMarker
    {
    }
}
