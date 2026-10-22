using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.Element;

/// <summary>
/// Reacts to element and element container changes and broadcasts them via <see cref="DraftElementCacheRefresher"/>, flushing the affected index documents from the change-detection cache.
/// </summary>
internal sealed class DraftElementNotificationHandler : ContentNotificationHandlerBase<DraftElementCacheRefresher.JsonPayload>,
    IDistributedCacheNotificationHandler<ElementSavedNotification>,
    IDistributedCacheNotificationHandler<ElementMovedNotification>,
    IDistributedCacheNotificationHandler<ElementMovedToRecycleBinNotification>,
    IDistributedCacheNotificationHandler<ElementDeletedNotification>,
    IDistributedCacheNotificationHandler<EntityContainerMovedNotification>,
    IDistributedCacheNotificationHandler<EntityContainerMovedToRecycleBinNotification>
{
    private readonly IEntityService _entityService;

    /// <inheritdoc />
    protected override Guid CacheRefresherUniqueId => DraftElementCacheRefresher.UniqueId;

    /// <summary>
    /// Initializes a new instance of the <see cref="DraftElementNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache used to broadcast the paired cache refresher notification.</param>
    /// <param name="originProvider">The provider of the current server origin.</param>
    /// <param name="indexDocumentService">The service used to flush the change-detection cache for affected documents.</param>
    /// <param name="entityService">The service used to enumerate the elements beneath a moved element container.</param>
    public DraftElementNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService,
        IEntityService entityService)
        : base(distributedCache, originProvider, indexDocumentService)
        => _entityService = entityService;

    /// <summary>
    /// Flushes the change-detection cache for the given elements and broadcasts a refresh-node change for each.
    /// </summary>
    /// <param name="entities">The elements to refresh.</param>
    public void Refresh(IEnumerable<IElement> entities)
    {
        IElement[] entitiesAsArray = entities as IElement[] ?? entities.ToArray();
        if (entitiesAsArray.Length is 0)
        {
            return;
        }

        FlushDocumentIndexCache(entitiesAsArray);

        DraftElementCacheRefresher.JsonPayload[] payloads = entitiesAsArray
            .Select(entity => new DraftElementCacheRefresher.JsonPayload(entity.Key, TreeChangeTypes.RefreshNode))
            .ToArray();

        HandlePayloads(payloads);
    }

    /// <inheritdoc />
    public void Handle(ElementSavedNotification notification)
        => Refresh(notification.SavedEntities);

    /// <inheritdoc />
    public void Handle(ElementMovedNotification notification)
        => Refresh(notification.MoveInfoCollection.Select(i => i.Entity));

    /// <inheritdoc />
    public void Handle(ElementMovedToRecycleBinNotification notification)
        => Refresh(notification.MoveInfoCollection.Select(i => i.Entity));

    /// <inheritdoc />
    public void Handle(ElementDeletedNotification notification)
    {
        IElement[] deletedEntities = notification.DeletedEntities.ToArray();
        if (deletedEntities.Length is 0)
        {
            return;
        }

        FlushDocumentIndexCache(deletedEntities);

        DraftElementCacheRefresher.JsonPayload[] payloads = deletedEntities
            .Select(entity => new DraftElementCacheRefresher.JsonPayload(entity.Key, TreeChangeTypes.Remove))
            .ToArray();

        HandlePayloads(payloads);
    }

    /// <inheritdoc />
    public void Handle(EntityContainerMovedNotification notification)
        => HandleContainerMove(notification.MoveInfoCollection.Select(i => i.Entity));

    /// <inheritdoc />
    public void Handle(EntityContainerMovedToRecycleBinNotification notification)
        => HandleContainerMove(notification.MoveInfoCollection.Select(i => i.Entity));

    // moving a container changes the path of every element beneath it, without any element notifications
    private void HandleContainerMove(IEnumerable<EntityContainer> containers)
    {
        Guid[] containerKeys = containers
            .Where(container => container.ContainedObjectType == Umbraco.Cms.Core.Constants.ObjectTypes.Element)
            .Select(container => container.Key)
            .ToArray();

        if (containerKeys.Length is 0)
        {
            return;
        }

        foreach (Guid containerKey in containerKeys)
        {
            FlushDescendantElementsDocumentIndexCache(containerKey);
        }

        DraftElementCacheRefresher.JsonPayload[] payloads = containerKeys
            .Select(containerKey => new DraftElementCacheRefresher.JsonPayload(containerKey, TreeChangeTypes.RefreshBranch))
            .ToArray();

        HandlePayloads(payloads);
    }

    private void FlushDescendantElementsDocumentIndexCache(Guid containerKey)
    {
        const int pageSize = ContentChangeStrategyBase.ContentEnumerationPageSize;

        var skip = 0;
        Guid[] descendantKeys;
        do
        {
            descendantKeys = _entityService
                .GetPagedDescendants(containerKey, UmbracoObjectTypes.ElementContainer, [UmbracoObjectTypes.Element], skip, pageSize, out _, ordering: Ordering.By("Path"))
                .Select(entity => entity.Key)
                .ToArray();

            if (descendantKeys.Length > 0)
            {
                FlushDocumentIndexCache(descendantKeys, false);
            }

            skip += pageSize;
        }
        while (descendantKeys.Length == pageSize);
    }

    private void FlushDocumentIndexCache(IEnumerable<IElement> entities)
        => FlushDocumentIndexCache(entities.Select(x => x.Key).ToArray(), false);
}
