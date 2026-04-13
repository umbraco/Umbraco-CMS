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
///     Handles <see cref="MediaCacheRefresherNotification"/> to evict Delivery API output cache entries
///     when media is created, updated, or deleted. Also evicts content responses that reference
///     the changed media via picker properties.
/// </summary>
internal sealed class DeliveryApiMediaOutputCacheEvictionHandler
    : RelationOutputCacheEvictionHandlerBase, INotificationAsyncHandler<MediaCacheRefresherNotification>
{
    private readonly ILogger<DeliveryApiMediaOutputCacheEvictionHandler> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeliveryApiMediaOutputCacheEvictionHandler"/> class.
    /// </summary>
    /// <param name="outputCacheStore">The output cache store for evicting cached responses.</param>
    /// <param name="relationService">The relation service for querying entity references.</param>
    /// <param name="idKeyMap">The ID/key mapping service for converting between integer IDs and GUIDs.</param>
    /// <param name="logger">The logger.</param>
    public DeliveryApiMediaOutputCacheEvictionHandler(
        IOutputCacheStore outputCacheStore,
        IRelationService relationService,
        IIdKeyMap idKeyMap,
        ILogger<DeliveryApiMediaOutputCacheEvictionHandler> logger)
        : base(outputCacheStore, relationService, idKeyMap)
        => _logger = logger;

    /// <inheritdoc />
    public async Task HandleAsync(MediaCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        if (notification.MessageType != MessageType.RefreshByPayload
            || notification.MessageObject is not MediaCacheRefresher.JsonPayload[] payloads)
        {
            return;
        }

        foreach (MediaCacheRefresher.JsonPayload payload in payloads)
        {
            if (payload.ChangeTypes.HasFlag(TreeChangeTypes.RefreshAll))
            {
                // Evict all Delivery API responses — content responses may include referenced media,
                // so evicting only media entries would leave stale media references in content responses.
                _logger.LogDebug("Media refresh all — evicting all Delivery API output cache entries.");
                await OutputCacheStore.EvictByTagAsync(Constants.DeliveryApi.OutputCache.AllTag, cancellationToken);
                return;
            }

            if (payload.Key.HasValue is false)
            {
                continue;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Evicting Delivery API output cache for media {MediaKey}.", payload.Key.Value);
            }

            await OutputCacheStore.EvictByTagAsync(Constants.DeliveryApi.OutputCache.MediaTagPrefix + payload.Key.Value, cancellationToken);
        }

        // Evict content that references the changed media via picker properties.
        await EvictRelatedContentAsync(
            payloads.Select(p => p.Id),
            Constants.Conventions.RelationTypes.RelatedMediaAlias,
            Constants.DeliveryApi.OutputCache.ContentTagPrefix,
            _logger,
            cancellationToken);
    }
}
