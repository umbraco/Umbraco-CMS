using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.Element;

/// <summary>
/// Reacts to element publish/unpublish changes and broadcasts them via <see cref="ElementChangeCacheRefresher"/>, so
/// documents referencing the changed element are only re-indexed when its published content actually changes - not
/// on every plain draft save.
/// </summary>
internal sealed class ElementPublishStatusNotificationHandler :
    IDistributedCacheNotificationHandler<ElementPublishedNotification>,
    IDistributedCacheNotificationHandler<ElementUnpublishedNotification>
{
    private readonly DistributedCache _distributedCache;
    private readonly IOriginProvider _originProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementPublishStatusNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache used to broadcast the paired cache refresher notification.</param>
    /// <param name="originProvider">The provider of the current server origin.</param>
    public ElementPublishStatusNotificationHandler(DistributedCache distributedCache, IOriginProvider originProvider)
    {
        _distributedCache = distributedCache;
        _originProvider = originProvider;
    }

    /// <summary>
    /// Broadcasts the published elements so the documents referencing them can be re-indexed.
    /// </summary>
    /// <param name="notification">The notification describing the published elements.</param>
    public void Handle(ElementPublishedNotification notification) => Broadcast(notification.PublishedEntities);

    /// <summary>
    /// Broadcasts the unpublished elements so the documents referencing them can be re-indexed.
    /// </summary>
    /// <param name="notification">The notification describing the unpublished elements.</param>
    public void Handle(ElementUnpublishedNotification notification) => Broadcast(notification.UnpublishedEntities);

    private void Broadcast(IEnumerable<IElement> entities)
    {
        ElementChangeCacheRefresher.JsonPayload[] payloads = entities
            .Select(entity => new ElementChangeCacheRefresher.JsonPayload(entity.Id, entity.Key))
            .ToArray();

        if (payloads.Length == 0)
        {
            return;
        }

        var payload = new ContentCacheRefresherNotificationPayload<ElementChangeCacheRefresher.JsonPayload>(payloads, _originProvider.GetCurrent());
        _distributedCache.RefreshByPayload(ElementChangeCacheRefresher.UniqueId, [payload]);
    }
}
