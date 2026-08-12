using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.ContentType;

/// <summary>
/// Reacts to content type changes and broadcasts them via <see cref="ContentTypeCacheRefresher"/>.
/// </summary>
internal sealed class ContentTypeNotificationHandler
    : ContentNotificationHandlerBase<ContentTypeCacheRefresher.JsonPayload>,
        IDistributedCacheNotificationHandler<ContentTypeChangedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeNotificationHandler"/> class.
    /// </summary>
    public ContentTypeNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService)
        : base(distributedCache, originProvider, indexDocumentService)
    {
    }

    /// <inheritdoc />
    protected override Guid CacheRefresherUniqueId => ContentTypeCacheRefresher.UniqueId;

    /// <inheritdoc />
    public void Handle(ContentTypeChangedNotification notification)
    {
        ContentTypeCacheRefresher.JsonPayload[] payloads = notification
            .Changes
            .Select(change => new ContentTypeCacheRefresher.JsonPayload(change.Item.Key, change.ChangeTypes))
            .ToArray();

        HandlePayloads(payloads);
    }
}
