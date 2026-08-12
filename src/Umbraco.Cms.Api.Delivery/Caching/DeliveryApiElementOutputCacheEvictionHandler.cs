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
///     Handles <see cref="ElementCacheRefresherNotification"/> to evict Delivery API output cache entries
///     for content that references the changed element via picker properties (umbElement relations).
/// </summary>
internal sealed class DeliveryApiElementOutputCacheEvictionHandler
    : RelationOutputCacheEvictionHandlerBase, INotificationAsyncHandler<ElementCacheRefresherNotification>
{
    private readonly ILogger<DeliveryApiElementOutputCacheEvictionHandler> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeliveryApiElementOutputCacheEvictionHandler"/> class.
    /// </summary>
    /// <param name="outputCacheStore">The output cache store for evicting cached responses.</param>
    /// <param name="relationService">The relation service for querying entity references.</param>
    /// <param name="idKeyMap">The ID/key mapping service for converting between integer IDs and GUIDs.</param>
    /// <param name="logger">The logger.</param>
    public DeliveryApiElementOutputCacheEvictionHandler(
        IOutputCacheStore outputCacheStore,
        IRelationService relationService,
        IIdKeyMap idKeyMap,
        ILogger<DeliveryApiElementOutputCacheEvictionHandler> logger)
        : base(outputCacheStore, relationService, idKeyMap)
        => _logger = logger;

    /// <inheritdoc />
    public async Task HandleAsync(ElementCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        if (notification.MessageType != MessageType.RefreshByPayload
            || notification.MessageObject is not ElementCacheRefresher.JsonPayload[] payloads)
        {
            return;
        }

        foreach (ElementCacheRefresher.JsonPayload payload in payloads)
        {
            if (payload.ChangeTypes.HasFlag(TreeChangeTypes.RefreshAll))
            {
                // Evict all Delivery API responses — content responses may include referenced elements,
                // so evicting only element-related entries would leave stale element references in content responses.
                _logger.LogDebug("Element refresh all — evicting all Delivery API output cache entries.");
                await OutputCacheStore.EvictByTagAsync(Constants.DeliveryApi.OutputCache.AllTag, cancellationToken);
                return;
            }
        }

        // Evict content that references the changed elements via picker properties.
        await EvictRelatedContentAsync(
            payloads.Select(p => p.Id),
            Constants.Conventions.RelationTypes.RelatedElementAlias,
            Constants.DeliveryApi.OutputCache.ContentTagPrefix,
            _logger,
            cancellationToken);
    }
}
