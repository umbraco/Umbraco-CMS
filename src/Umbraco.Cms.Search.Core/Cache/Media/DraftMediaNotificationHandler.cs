using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.Media;

/// <summary>
/// Reacts to media changes and broadcasts them via <see cref="DraftMediaCacheRefresher"/>, flushing the affected index documents from the change-detection cache.
/// </summary>
internal sealed class DraftMediaNotificationHandler : ContentNotificationHandlerBase<DraftMediaCacheRefresher.JsonPayload>,
    IDistributedCacheNotificationHandler<MediaSavedNotification>,
    IDistributedCacheNotificationHandler<MediaMovedNotification>,
    IDistributedCacheNotificationHandler<MediaMovedToRecycleBinNotification>,
    IDistributedCacheNotificationHandler<MediaDeletedNotification>
{
    /// <inheritdoc />
    protected override Guid CacheRefresherUniqueId => DraftMediaCacheRefresher.UniqueId;

    /// <summary>
    /// Initializes a new instance of the <see cref="DraftMediaNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache used to broadcast the paired cache refresher notification.</param>
    /// <param name="originProvider">The provider of the current server origin.</param>
    /// <param name="indexDocumentService">The service used to flush the change-detection cache for affected documents.</param>
    public DraftMediaNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService)
        : base(distributedCache, originProvider, indexDocumentService)
    {
    }

    /// <summary>
    /// Flushes the change-detection cache for the given media items and broadcasts a refresh-node change for each.
    /// </summary>
    /// <param name="entities">The media items to refresh.</param>
    public void Refresh(IEnumerable<IMedia> entities)
    {
        IMedia[] entitiesAsArray = entities as IMedia[] ?? entities.ToArray();
        if (entitiesAsArray.Length is 0)
        {
            return;
        }

        FlushDocumentIndexCache(entitiesAsArray);

        DraftMediaCacheRefresher.JsonPayload[] payloads = entitiesAsArray
            .Select(entity => new DraftMediaCacheRefresher.JsonPayload(entity.Key, TreeChangeTypes.RefreshNode))
            .ToArray();

        HandlePayloads(payloads);
    }

    /// <inheritdoc />
    public void Handle(MediaSavedNotification notification)
        => Refresh(notification.SavedEntities);

    /// <inheritdoc />
    public void Handle(MediaMovedNotification notification)
        => HandleMove(notification.MoveInfoCollection);

    /// <inheritdoc />
    public void Handle(MediaMovedToRecycleBinNotification notification)
        => HandleMove(notification.MoveInfoCollection);

    /// <inheritdoc />
    public void Handle(MediaDeletedNotification notification)
    {
        IMedia[] deletedEntities = notification.DeletedEntities.ToArray();
        if (deletedEntities.Length is 0)
        {
            return;
        }

        FlushDocumentIndexCache(deletedEntities);

        DraftMediaCacheRefresher.JsonPayload[] payloads = deletedEntities
            .Select(entity => new DraftMediaCacheRefresher.JsonPayload(entity.Key, TreeChangeTypes.Remove))
            .ToArray();

        HandlePayloads(payloads);
    }

    private void HandleMove(IEnumerable<MoveEventInfoBase<IMedia>> moveEventInfo)
    {
        IMedia[] movedEntities = moveEventInfo.Select(i => i.Entity).ToArray();
        if (movedEntities.Length is 0)
        {
            return;
        }

        FlushDocumentIndexCache(movedEntities);

        IMedia[] topmostEntities = FindTopmostEntities(movedEntities);
        DraftMediaCacheRefresher.JsonPayload[] payloads = topmostEntities
            .Select(entity => new DraftMediaCacheRefresher.JsonPayload(entity.Key, TreeChangeTypes.RefreshBranch))
            .ToArray();

        HandlePayloads(payloads);
    }

    private void FlushDocumentIndexCache(IEnumerable<IMedia> entities)
        => FlushDocumentIndexCache(entities.Select(x => x.Key).ToArray(), false);
}
