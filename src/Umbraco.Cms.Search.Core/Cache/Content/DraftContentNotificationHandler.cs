using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.Content;

/// <summary>
/// Reacts to draft content changes and broadcasts them via <see cref="DraftContentCacheRefresher"/>, flushing the affected index documents from the change-detection cache.
/// </summary>
internal sealed class DraftContentNotificationHandler : ContentNotificationHandlerBase<DraftContentCacheRefresher.JsonPayload>,
    IDistributedCacheNotificationHandler<ContentSavedNotification>,
    IDistributedCacheNotificationHandler<ContentMovedNotification>,
    IDistributedCacheNotificationHandler<ContentMovedToRecycleBinNotification>,
    IDistributedCacheNotificationHandler<ContentDeletedNotification>
{
    /// <inheritdoc />
    protected override Guid CacheRefresherUniqueId => DraftContentCacheRefresher.UniqueId;

    /// <summary>
    /// Initializes a new instance of the <see cref="DraftContentNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache used to broadcast the paired cache refresher notification.</param>
    /// <param name="originProvider">The provider of the current server origin.</param>
    /// <param name="indexDocumentService">The service used to flush the change-detection cache for affected documents.</param>
    public DraftContentNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService)
        : base(distributedCache, originProvider, indexDocumentService)
    {
    }

    /// <summary>
    /// Flushes the change-detection cache for the given content items and broadcasts a refresh-node change for each.
    /// </summary>
    /// <param name="entities">The content items to refresh.</param>
    public void Refresh(IEnumerable<IContent> entities)
    {
        IContent[] entitiesAsArray = entities as IContent[] ?? entities.ToArray();
        if (entitiesAsArray.Length is 0)
        {
            return;
        }

        FlushDocumentIndexCache(entitiesAsArray);

        DraftContentCacheRefresher.JsonPayload[] payloads = entitiesAsArray
            .Select(entity => new DraftContentCacheRefresher.JsonPayload(entity.Key, TreeChangeTypes.RefreshNode))
            .ToArray();

        HandlePayloads(payloads);
    }

    /// <inheritdoc />
    public void Handle(ContentSavedNotification notification)
        => Refresh(notification.SavedEntities);

    /// <inheritdoc />
    public void Handle(ContentMovedNotification notification)
        => HandleMove(notification.MoveInfoCollection);

    /// <inheritdoc />
    public void Handle(ContentMovedToRecycleBinNotification notification)
        => HandleMove(notification.MoveInfoCollection);

    /// <inheritdoc />
    public void Handle(ContentDeletedNotification notification)
    {
        IContent[] deletedEntities = notification.DeletedEntities.ToArray();
        if (deletedEntities.Length is 0)
        {
            return;
        }

        FlushDocumentIndexCache(deletedEntities);

        DraftContentCacheRefresher.JsonPayload[] payloads = deletedEntities
            .Select(entity => new DraftContentCacheRefresher.JsonPayload(entity.Key, TreeChangeTypes.Remove))
            .ToArray();

        HandlePayloads(payloads);
    }

    private void HandleMove(IEnumerable<MoveEventInfoBase<IContent>> moveEventInfo)
    {
        IContent[] movedEntities = moveEventInfo.Select(i => i.Entity).ToArray();
        if (movedEntities.Length is 0)
        {
            return;
        }

        FlushDocumentIndexCache(movedEntities);

        IContent[] topmostEntities = FindTopmostEntities(movedEntities);
        DraftContentCacheRefresher.JsonPayload[] payloads = topmostEntities
            .Select(entity => new DraftContentCacheRefresher.JsonPayload(entity.Key, TreeChangeTypes.RefreshBranch))
            .ToArray();

        HandlePayloads(payloads);
    }

    private void FlushDocumentIndexCache(IEnumerable<IContent> entities)
        => FlushDocumentIndexCache(entities.Select(x => x.Key).ToArray(), false);
}
