using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.Content;

internal sealed class DraftContentNotificationHandler : ContentNotificationHandlerBase<DraftContentCacheRefresher.JsonPayload>,
    IDistributedCacheNotificationHandler<ContentSavedNotification>,
    IDistributedCacheNotificationHandler<ContentMovedNotification>,
    IDistributedCacheNotificationHandler<ContentMovedToRecycleBinNotification>,
    IDistributedCacheNotificationHandler<ContentDeletedNotification>
{
    protected override Guid CacheRefresherUniqueId => DraftContentCacheRefresher.UniqueId;

    public DraftContentNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService)
        : base(distributedCache, originProvider, indexDocumentService)
    {
    }

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

    public void Handle(ContentSavedNotification notification)
        => Refresh(notification.SavedEntities);

    public void Handle(ContentMovedNotification notification)
        => HandleMove(notification.MoveInfoCollection);

    public void Handle(ContentMovedToRecycleBinNotification notification)
        => HandleMove(notification.MoveInfoCollection);

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
