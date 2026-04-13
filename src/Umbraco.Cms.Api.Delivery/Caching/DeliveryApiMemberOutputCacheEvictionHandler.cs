using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Web.Common.Caching;

namespace Umbraco.Cms.Api.Delivery.Caching;

/// <summary>
///     Handles <see cref="MemberCacheRefresherNotification"/> to evict Delivery API output cache entries
///     for content that references the changed member via picker properties.
/// </summary>
internal sealed class DeliveryApiMemberOutputCacheEvictionHandler
    : RelationOutputCacheEvictionHandlerBase, INotificationAsyncHandler<MemberCacheRefresherNotification>
{
    private readonly ILogger<DeliveryApiMemberOutputCacheEvictionHandler> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeliveryApiMemberOutputCacheEvictionHandler"/> class.
    /// </summary>
    /// <param name="outputCacheStore">The output cache store for evicting cached responses.</param>
    /// <param name="relationService">The relation service for querying entity references.</param>
    /// <param name="idKeyMap">The ID/key mapping service for converting between integer IDs and GUIDs.</param>
    /// <param name="logger">The logger.</param>
    public DeliveryApiMemberOutputCacheEvictionHandler(
        IOutputCacheStore outputCacheStore,
        IRelationService relationService,
        IIdKeyMap idKeyMap,
        ILogger<DeliveryApiMemberOutputCacheEvictionHandler> logger)
        : base(outputCacheStore, relationService, idKeyMap)
        => _logger = logger;

    /// <inheritdoc />
    public async Task HandleAsync(MemberCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        if (notification.MessageType != MessageType.RefreshByPayload
            || notification.MessageObject is not MemberCacheRefresher.JsonPayload[] payloads)
        {
            return;
        }

        // Evict content that references the changed members via picker properties.
        await EvictRelatedContentAsync(
            payloads.Select(p => p.Id),
            Constants.Conventions.RelationTypes.RelatedMemberAlias,
            Constants.DeliveryApi.OutputCache.ContentTagPrefix,
            _logger,
            cancellationToken);
    }
}
