using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Search.Core.Cache.Element;

/// <summary>
/// Reacts to element publish/unpublish/trash changes and broadcasts them via <see cref="PublishedElementCacheRefresher"/>,
/// so documents referencing the changed element are only re-indexed when its published content actually changes -
/// not on every plain draft save.
/// </summary>
internal sealed class PublishedElementNotificationHandler :
    IDistributedCacheNotificationHandler<ElementPublishedNotification>,
    IDistributedCacheNotificationHandler<ElementUnpublishedNotification>,
    IDistributedCacheNotificationHandler<ElementMovedToRecycleBinNotification>,
    IDistributedCacheNotificationHandler<EntityContainerMovedToRecycleBinNotification>
{
    private const int DescendantsPageSize = 500;

    private readonly DistributedCache _distributedCache;
    private readonly IOriginProvider _originProvider;
    private readonly IEntityService _entityService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedElementNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache used to broadcast the paired cache refresher notification.</param>
    /// <param name="originProvider">The provider of the current server origin.</param>
    /// <param name="entityService">The service used to find the elements below a trashed element container.</param>
    public PublishedElementNotificationHandler(DistributedCache distributedCache, IOriginProvider originProvider, IEntityService entityService)
    {
        _distributedCache = distributedCache;
        _originProvider = originProvider;
        _entityService = entityService;
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

    /// <summary>
    /// Broadcasts the published elements below the trashed element containers so the documents referencing them can be re-indexed.
    /// </summary>
    /// <param name="notification">The notification describing the containers moved to the recycle bin.</param>
    /// <remarks>
    /// Moving a container to the recycle bin trashes all of its descendant elements, but raises no
    /// <see cref="ElementMovedToRecycleBinNotification"/> for them - only this notification for the container itself.
    /// </remarks>
    public void Handle(EntityContainerMovedToRecycleBinNotification notification)
        => Broadcast(notification.MoveInfoCollection
            .Where(info => info.Entity.ContainedObjectType == Umbraco.Cms.Core.Constants.ObjectTypes.Element)
            .SelectMany(info => GetPublishedDescendantElements(info.Entity.Key)));

    private IEnumerable<IEntitySlim> GetPublishedDescendantElements(Guid containerKey)
    {
        var skip = 0;
        long total;
        do
        {
            IEntitySlim[] descendants = _entityService.GetPagedDescendants(
                containerKey,
                UmbracoObjectTypes.ElementContainer,
                [UmbracoObjectTypes.Element],
                skip,
                DescendantsPageSize,
                out total).ToArray();
            skip += DescendantsPageSize;

            // an unpublished element contributes nothing to any published index entry, so there is nothing to refresh
            foreach (IEntitySlim descendant in descendants.Where(descendant => descendant is IPublishableContentEntitySlim { Published: true }))
            {
                yield return descendant;
            }
        }
        while (skip < total);
    }

    private void Broadcast(IEnumerable<IEntity> entities)
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
