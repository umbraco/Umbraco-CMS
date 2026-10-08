using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.ContentType;

/// <summary>
/// Distributed cache refresher that broadcasts content type changes to other servers, for reindexing affected content.
/// </summary>
internal sealed class ContentTypeCacheRefresher : PayloadCacheRefresherBase<ContentTypeCacheRefresherNotification, ContentCacheRefresherNotificationPayload<ContentTypeCacheRefresher.JsonPayload>>
{
    /// <summary>
    /// The unique identifier of this refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("9EC8AAAB-FEBA-4F58-819B-5B1C6E80F988");

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeCacheRefresher"/> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public ContentTypeCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Content Type Cache Refresher";

    /// <summary>
    /// The payload broadcast for a single changed content type.
    /// </summary>
    /// <param name="ContentTypeKey">The key of the changed content type.</param>
    /// <param name="ChangeTypes">The kind of change that occurred.</param>
    public record JsonPayload(Guid ContentTypeKey, ContentTypeChangeTypes ChangeTypes)
    {
    }
}
