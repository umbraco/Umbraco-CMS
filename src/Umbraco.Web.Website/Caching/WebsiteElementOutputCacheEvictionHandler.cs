using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Web.Website.Caching;

/// <summary>
///     Handles <see cref="ElementCacheRefresherNotification"/> to evict output cache entries
///     for pages that reference the changed element via picker properties.
/// </summary>
internal sealed class WebsiteElementOutputCacheEvictionHandler
    : RelationOutputCacheEvictionHandlerBase, INotificationAsyncHandler<ElementCacheRefresherNotification>
{
    private readonly ILogger<WebsiteElementOutputCacheEvictionHandler> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebsiteElementOutputCacheEvictionHandler"/> class.
    /// </summary>
    public WebsiteElementOutputCacheEvictionHandler(
        IOutputCacheStore outputCacheStore,
        IRelationService relationService,
        IIdKeyMap idKeyMap,
        ILogger<WebsiteElementOutputCacheEvictionHandler> logger)
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
                _logger.LogDebug("Element refresh all — evicting all website output cache entries.");
                await OutputCacheStore.EvictByTagAsync(Constants.Website.OutputCache.AllContentTag, cancellationToken);
                return;
            }
        }

        // Evict documents that reference the changed elements via picker properties.
        await EvictRelatedPagesAsync(
            payloads.Select(p => p.Id),
            Constants.Conventions.RelationTypes.RelatedElementAlias,
            _logger,
            cancellationToken);
    }
}
