using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.Media;

internal sealed class DraftMediaNotificationHandler : ContentNotificationHandlerBase<DraftMediaCacheRefresher.JsonPayload>,
    IDistributedCacheNotificationHandler<MediaSavedNotification>,
    IDistributedCacheNotificationHandler<MediaMovedNotification>,
    IDistributedCacheNotificationHandler<MediaMovedToRecycleBinNotification>,
    IDistributedCacheNotificationHandler<MediaDeletedNotification>
{
    protected override Guid CacheRefresherUniqueId => DraftMediaCacheRefresher.UniqueId;

    public DraftMediaNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService)
        : base(distributedCache, originProvider, indexDocumentService)
    {
    }

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

    public void Handle(MediaSavedNotification notification)
        => Refresh(notification.SavedEntities);

    public void Handle(MediaMovedNotification notification)
        => HandleMove(notification.MoveInfoCollection);

    public void Handle(MediaMovedToRecycleBinNotification notification)
        => HandleMove(notification.MoveInfoCollection);

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
