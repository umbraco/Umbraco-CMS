using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.Member;

internal sealed class DraftMemberNotificationHandler : ContentNotificationHandlerBase<DraftMemberCacheRefresher.JsonPayload>,
    IDistributedCacheNotificationHandler<MemberSavedNotification>,
    IDistributedCacheNotificationHandler<MemberDeletedNotification>
{
    protected override Guid CacheRefresherUniqueId => DraftMemberCacheRefresher.UniqueId;

    public DraftMemberNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService)
        : base(distributedCache, originProvider, indexDocumentService)
    {
    }

    public void Refresh(IEnumerable<IMember> entities)
    {
        IMember[] entitiesAsArray = entities as IMember[] ?? entities.ToArray();
        if (entitiesAsArray.Length is 0)
        {
            return;
        }

        FlushDocumentIndexCache(entitiesAsArray);

        DraftMemberCacheRefresher.JsonPayload[] payloads = entitiesAsArray
            .Select(entity => new DraftMemberCacheRefresher.JsonPayload(entity.Key, TreeChangeTypes.RefreshNode))
            .ToArray();

        HandlePayloads(payloads);
    }

    public void Handle(MemberSavedNotification notification)
        => Refresh(notification.SavedEntities);

    public void Handle(MemberDeletedNotification notification)
    {
        IMember[] deletedEntities = notification.DeletedEntities.ToArray();
        if (deletedEntities.Length is 0)
        {
            return;
        }

        FlushDocumentIndexCache(deletedEntities);

        DraftMemberCacheRefresher.JsonPayload[] payloads = deletedEntities
            .Select(entity => new DraftMemberCacheRefresher.JsonPayload(entity.Key, TreeChangeTypes.Remove))
            .ToArray();

        HandlePayloads(payloads);
    }

    private void FlushDocumentIndexCache(IEnumerable<IMember> entities)
        => FlushDocumentIndexCache(entities.Select(x => x.Key).ToArray(), false);
}
