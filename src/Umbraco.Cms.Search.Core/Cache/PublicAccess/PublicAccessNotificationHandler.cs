using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.Cache.PublicAccess;

internal sealed class PublicAccessNotificationHandler : ContentNotificationHandlerBase<PublicAccessDetailedCacheRefresher.JsonPayload>,
    IDistributedCacheNotificationHandler<PublicAccessEntrySavedNotification>,
    IDistributedCacheNotificationHandler<PublicAccessEntryDeletedNotification>
{
    private readonly IIdKeyMap _idKeyMap;

    protected override Guid CacheRefresherUniqueId => PublicAccessDetailedCacheRefresher.UniqueId;

    public PublicAccessNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService,
        IIdKeyMap idKeyMap)
        : base(distributedCache, originProvider, indexDocumentService)
        => _idKeyMap = idKeyMap;

    public void Handle(PublicAccessEntrySavedNotification notification)
        => Handle(notification.SavedEntities);

    public void Handle(PublicAccessEntryDeletedNotification notification)
        => Handle(notification.DeletedEntities);

    private void Handle(IEnumerable<PublicAccessEntry> entities)
    {
        PublicAccessDetailedCacheRefresher.JsonPayload[] payloads = entities.Select(entity =>
            {
                Attempt<Guid> attempt = _idKeyMap.GetKeyForId(entity.ProtectedNodeId, UmbracoObjectTypes.Document);
                return attempt.Success
                    ? new PublicAccessDetailedCacheRefresher.JsonPayload(attempt.Result)
                    : null;
            })
            .WhereNotNull()
            .ToArray();

        HandlePayloads(payloads);
    }
}
