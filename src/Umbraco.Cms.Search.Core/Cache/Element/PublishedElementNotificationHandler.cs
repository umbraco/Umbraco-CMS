using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Search.Indexing;

namespace Umbraco.Cms.Search.Core.Cache.Element;

/// <summary>
/// Reacts to element publish/unpublish/trash changes and broadcasts them via <see cref="PublishedElementCacheRefresher"/>,
/// so documents referencing the changed element are only re-indexed when its published content actually changes -
/// not on every plain draft save.
/// </summary>
internal sealed class PublishedElementNotificationHandler :
    IDistributedCacheNotificationHandler<ElementPublishedNotification>,
    IDistributedCacheNotificationHandler<ElementUnpublishedNotification>,
    IDistributedCacheNotificationHandler<ElementMovedToRecycleBinNotification>
{
    private readonly DistributedCache _distributedCache;
    private readonly IOriginProvider _originProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedElementNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache used to broadcast the paired cache refresher notification.</param>
    /// <param name="originProvider">The provider of the current server origin.</param>
    public PublishedElementNotificationHandler(DistributedCache distributedCache, IOriginProvider originProvider)
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

    /// <summary>
    /// Broadcasts the trashed elements so the documents referencing them can be re-indexed.
    /// </summary>
    /// <param name="notification">The notification describing the elements moved to the recycle bin.</param>
    /// <remarks>
    /// Moving an element to the recycle bin raises neither <see cref="ElementPublishedNotification"/> nor
    /// <see cref="ElementUnpublishedNotification"/>, even though a trashed element's content must no longer appear
    /// in any referencing document's published index (see the <c>Trashed</c> check in
    /// <c>BlockEditorPropertyValueHandler</c>) - without this handler, referencing documents would keep serving the
    /// trashed element's stale content until something else happened to trigger a reindex.
    /// </remarks>
    public void Handle(ElementMovedToRecycleBinNotification notification)
        => Broadcast(notification.MoveInfoCollection.Select(info => info.Entity));

    private void Broadcast(IEnumerable<IElement> entities)
    {
        PublishedElementCacheRefresher.JsonPayload[] payloads = entities
            .Select(entity => new PublishedElementCacheRefresher.JsonPayload(entity.Id, entity.Key))
            .ToArray();

        if (payloads.Length == 0)
        {
            return;
        }

        var payload = new ContentCacheRefresherNotificationPayload<PublishedElementCacheRefresher.JsonPayload>(payloads, _originProvider.GetCurrent());
        _distributedCache.RefreshByPayload(PublishedElementCacheRefresher.UniqueId, [payload]);
    }
}
