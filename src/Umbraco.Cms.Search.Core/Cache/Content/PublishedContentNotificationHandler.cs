using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.Cache.Content;

internal sealed class PublishedContentNotificationHandler : ContentNotificationHandlerBase<PublishedContentCacheRefresher.JsonPayload>,
    IDistributedCacheNotificationHandler<ContentPublishedNotification>,
    IDistributedCacheNotificationHandler<ContentUnpublishedNotification>,
    IDistributedCacheNotificationHandler<ContentMovedNotification>,
    INotificationHandler<ContentMovedToRecycleBinNotification>
{
    protected override Guid CacheRefresherUniqueId => PublishedContentCacheRefresher.UniqueId;

    public PublishedContentNotificationHandler(
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

        PublishedContentCacheRefresher.JsonPayload[] payloads = entitiesAsArray
            .Select(entity =>
                new PublishedContentCacheRefresher.JsonPayload(
                    entity.Key,
                    TreeChangeTypes.RefreshNode,
                    entity.PublishedCultures.ToArray()))
            .ToArray();

        HandlePayloads(payloads);
    }

    public void Handle(ContentPublishedNotification notification)
    {
        // we sometimes get unpublished entities here... filter those out, we don't need them
        IContent[] publishedEntities = notification.PublishedEntities.Where(entity => entity.Published).ToArray();
        if (publishedEntities.Length is 0)
        {
            return;
        }

        FlushDocumentIndexCache(publishedEntities);

        IContent[] topmostEntities = FindTopmostEntities(publishedEntities);
        PublishedContentCacheRefresher.JsonPayload[] payloads = topmostEntities
            .Select(entity =>
            {
                IEnumerable<string> publishedCultures = entity.CultureInfos?.Values
                    .Where(x => entity.WasPropertyDirty($"{ContentBase.ChangeTrackingPrefix.PublishedCulture}{x.Culture}"))
                    .Select(x => x.Culture) ?? [];
                IEnumerable<string> unpublishedCultures = entity.CultureInfos?.Values
                    .Where(x => entity.WasPropertyDirty($"{ContentBase.ChangeTrackingPrefix.UnpublishedCulture}{x.Culture}"))
                    .Select(x => x.Culture) ?? [];
                var wasUnpublished = entity.WasPropertyDirty(nameof(IContent.Published));

                var affectedCultures = publishedCultures.Union(unpublishedCultures).Distinct().ToArray();
                return new PublishedContentCacheRefresher.JsonPayload(entity.Key, wasUnpublished || affectedCultures.Length > 0 ? TreeChangeTypes.RefreshBranch : TreeChangeTypes.RefreshNode, affectedCultures);
            })
            .WhereNotNull()
            .ToArray();

        HandlePayloads(payloads);
    }

    public void Handle(ContentUnpublishedNotification notification)
    {
        IContent[] unpublishedEntities = notification.UnpublishedEntities.ToArray();
        if (unpublishedEntities.Length is 0)
        {
            return;
        }

        FlushDocumentIndexCache(unpublishedEntities);

        PublishedContentCacheRefresher.JsonPayload[] payloads = unpublishedEntities
            .Select(entity => new PublishedContentCacheRefresher.JsonPayload(entity.Key, TreeChangeTypes.Remove, []))
            .ToArray();

        HandlePayloads(payloads);
    }

    public void Handle(ContentMovedNotification notification)
        => HandleMove(notification.MoveInfoCollection, TreeChangeTypes.RefreshBranch);

    public void Handle(ContentMovedToRecycleBinNotification notification)
        => HandleMove(notification.MoveInfoCollection, TreeChangeTypes.Remove);

    private void HandleMove(IEnumerable<MoveEventInfoBase<IContent>> moveEventInfo, TreeChangeTypes changeType)
    {
        IContent[] movedEntities = moveEventInfo.Select(i => i.Entity).ToArray();
        if (movedEntities.Length is 0)
        {
            return;
        }

        FlushDocumentIndexCache(movedEntities);

        IContent[] topmostEntities = FindTopmostEntities(movedEntities);
        PublishedContentCacheRefresher.JsonPayload[] payloads = topmostEntities
            .Select(entity => new PublishedContentCacheRefresher.JsonPayload(entity.Key, changeType, []))
            .ToArray();

        HandlePayloads(payloads);
    }

    private void FlushDocumentIndexCache(IEnumerable<IContent> entities)
        => FlushDocumentIndexCache(entities.Select(x => x.Key).ToArray(), true);
}
