using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Web.Common.Caching;

namespace Umbraco.Cms.Api.Delivery.Caching;

/// <summary>
///     Handles <see cref="ContentCacheRefresherNotification"/> to evict Delivery API output cache entries
///     when content is published, unpublished, moved, or deleted. Also evicts responses for content
///     that references the changed content via picker properties (umbDocument relations).
/// </summary>
internal sealed class DeliveryApiDocumentOutputCacheEvictionHandler
    : RelationOutputCacheEvictionHandlerBase, INotificationAsyncHandler<ContentCacheRefresherNotification>
{
    private readonly IEnumerable<IDeliveryApiOutputCacheEvictionProvider> _evictionProviders;
    private readonly ILogger<DeliveryApiDocumentOutputCacheEvictionHandler> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeliveryApiDocumentOutputCacheEvictionHandler"/> class.
    /// </summary>
    /// <param name="outputCacheStore">The output cache store for evicting cached responses.</param>
    /// <param name="relationService">The relation service for querying entity references.</param>
    /// <param name="idKeyMap">The ID/key mapping service for converting between integer IDs and GUIDs.</param>
    /// <param name="evictionProviders">Custom eviction providers for additional tag-based eviction.</param>
    /// <param name="logger">The logger.</param>
    public DeliveryApiDocumentOutputCacheEvictionHandler(
        IOutputCacheStore outputCacheStore,
        IRelationService relationService,
        IIdKeyMap idKeyMap,
        IEnumerable<IDeliveryApiOutputCacheEvictionProvider> evictionProviders,
        ILogger<DeliveryApiDocumentOutputCacheEvictionHandler> logger)
        : base(outputCacheStore, relationService, idKeyMap)
    {
        _evictionProviders = evictionProviders;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(ContentCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        if (notification.MessageType != MessageType.RefreshByPayload
            || notification.MessageObject is not ContentCacheRefresher.JsonPayload[] payloads)
        {
            return;
        }

        var changedEntityIds = new List<int>();

        foreach (ContentCacheRefresher.JsonPayload payload in payloads)
        {
            if (payload.Blueprint)
            {
                continue;
            }

            await EvictForPayloadAsync(payload, cancellationToken);
            changedEntityIds.Add(payload.Id);
        }

        // Evict content that references the changed content via picker properties.
        await EvictRelatedContentAsync(
            changedEntityIds,
            Constants.Conventions.RelationTypes.RelatedDocumentAlias,
            Constants.DeliveryApi.OutputCache.ContentTagPrefix,
            _logger,
            cancellationToken);
    }

    private async Task EvictForPayloadAsync(ContentCacheRefresher.JsonPayload payload, CancellationToken cancellationToken)
    {
        if (payload.ChangeTypes.HasFlag(TreeChangeTypes.RefreshAll))
        {
            _logger.LogDebug("Evicting all Delivery API content output cache entries.");
            await OutputCacheStore.EvictByTagAsync(Constants.DeliveryApi.OutputCache.AllContentTag, cancellationToken);
            return;
        }

        if (payload.Key.HasValue is false)
        {
            return;
        }

        Guid contentKey = payload.Key.Value;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Evicting Delivery API output cache for content {ContentKey}.", contentKey);
        }

        await OutputCacheStore.EvictByTagAsync(Constants.DeliveryApi.OutputCache.ContentTagPrefix + contentKey, cancellationToken);

        if (payload.ChangeTypes.HasFlag(TreeChangeTypes.RefreshBranch))
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Evicting Delivery API output cache for descendants of {ContentKey}.", contentKey);
            }

            await OutputCacheStore.EvictByTagAsync(Constants.DeliveryApi.OutputCache.AncestorTagPrefix + contentKey, cancellationToken);
        }

        await InvokeCustomEvictionProvidersAsync(payload, contentKey, cancellationToken);
    }

    private async Task InvokeCustomEvictionProvidersAsync(ContentCacheRefresher.JsonPayload payload, Guid contentKey, CancellationToken cancellationToken)
    {
        var context = new OutputCacheContentChangedContext(
            payload.Id,
            contentKey,
            payload.PublishedCultures ?? [],
            payload.UnpublishedCultures ?? []);

        foreach (IDeliveryApiOutputCacheEvictionProvider provider in _evictionProviders)
        {
            IEnumerable<string> additionalTags = await provider.GetAdditionalEvictionTagsAsync(context, cancellationToken);
            foreach (var tag in additionalTags)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Evicting Delivery API output cache tag {Tag} via custom provider.", tag);
                }

                await OutputCacheStore.EvictByTagAsync(tag, cancellationToken);
            }
        }
    }
}
