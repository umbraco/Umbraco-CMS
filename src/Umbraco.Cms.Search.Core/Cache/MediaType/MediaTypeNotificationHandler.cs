using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.MediaType;

internal sealed class MediaTypeNotificationHandler
    : ContentNotificationHandlerBase<MediaTypeCacheRefresher.JsonPayload>,
        IDistributedCacheNotificationHandler<MediaTypeChangedNotification>
{
    public MediaTypeNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService)
        : base(distributedCache, originProvider, indexDocumentService)
    {
    }

    protected override Guid CacheRefresherUniqueId => MediaTypeCacheRefresher.UniqueId;

    public void Handle(MediaTypeChangedNotification notification)
    {
        MediaTypeCacheRefresher.JsonPayload[] payloads = notification
            .Changes
            .Select(change => new MediaTypeCacheRefresher.JsonPayload(change.Item.Key, change.ChangeTypes))
            .ToArray();

        HandlePayloads(payloads);
    }
}
