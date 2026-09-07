using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.MediaType;

/// <summary>
/// Reacts to media type changes and broadcasts them via <see cref="MediaTypeCacheRefresher"/>.
/// </summary>
internal sealed class MediaTypeNotificationHandler
    : ContentNotificationHandlerBase<MediaTypeCacheRefresher.JsonPayload>,
        IDistributedCacheNotificationHandler<MediaTypeChangedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache used to broadcast the paired cache refresher notification.</param>
    /// <param name="originProvider">The provider of the current server origin.</param>
    /// <param name="indexDocumentService">The service used to flush the change-detection cache for affected documents.</param>
    public MediaTypeNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService)
        : base(distributedCache, originProvider, indexDocumentService)
    {
    }

    /// <inheritdoc />
    protected override Guid CacheRefresherUniqueId => MediaTypeCacheRefresher.UniqueId;

    /// <inheritdoc />
    public void Handle(MediaTypeChangedNotification notification)
    {
        MediaTypeCacheRefresher.JsonPayload[] payloads = notification
            .Changes
            .Select(change => new MediaTypeCacheRefresher.JsonPayload(change.Item.Key, change.ChangeTypes))
            .ToArray();

        HandlePayloads(payloads);
    }
}
