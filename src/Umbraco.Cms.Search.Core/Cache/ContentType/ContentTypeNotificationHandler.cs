using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.ContentType;

internal sealed class ContentTypeNotificationHandler
    : ContentNotificationHandlerBase<ContentTypeCacheRefresher.JsonPayload>,
        IDistributedCacheNotificationHandler<ContentTypeChangedNotification>
{
    public ContentTypeNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService)
        : base(distributedCache, originProvider, indexDocumentService)
    {
    }

    protected override Guid CacheRefresherUniqueId => ContentTypeCacheRefresher.UniqueId;

    public void Handle(ContentTypeChangedNotification notification)
    {
        ContentTypeCacheRefresher.JsonPayload[] payloads = notification
            .Changes
            .Select(change => new ContentTypeCacheRefresher.JsonPayload(change.Item.Key, change.ChangeTypes))
            .ToArray();

        HandlePayloads(payloads);
    }
}
