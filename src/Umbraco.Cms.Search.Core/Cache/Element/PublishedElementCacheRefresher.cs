using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Search.Core.Cache.Element;

/// <summary>
/// Distributed cache refresher that broadcasts published/unpublished external (reusable) element changes to other
/// servers, so documents referencing the changed element can be re-indexed.
/// </summary>
/// <remarks>
/// The core distributed caching for element changes cannot tell the difference between "an element was published"
/// and "an element was simply saved" - both broadcast the same tree change type. Re-indexing every document
/// referencing an element on every draft save would be wasteful, since only a published change is ever reflected in
/// the published index. This custom cache refresher is only ever broadcast from an actual publish, unpublish or move
/// to the recycle bin (see <see cref="PublishedElementNotificationHandler"/>), giving that missing level of
/// granularity.
/// </remarks>
internal sealed class PublishedElementCacheRefresher : PayloadCacheRefresherBase<PublishedElementCacheRefresherNotification, ContentCacheRefresherNotificationPayload<PublishedElementCacheRefresher.JsonPayload>>
{
    /// <summary>
    /// The unique identifier of this refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("71D9516E-DC65-4E19-95E3-6720CC0693CE");

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedElementCacheRefresher"/> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public PublishedElementCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Published Element Cache Refresher";

    /// <summary>
    /// The payload broadcast for a single published, unpublished or trashed element.
    /// </summary>
    /// <param name="Id">The identifier of the changed element.</param>
    /// <param name="Key">The key of the changed element.</param>
    public record JsonPayload(int Id, Guid Key)
    {
    }
}
