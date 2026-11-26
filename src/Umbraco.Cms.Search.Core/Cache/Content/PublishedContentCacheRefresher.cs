using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.Content;

/// <summary>
/// Distributed cache refresher that broadcasts published content changes to other servers, for the published content index.
/// </summary>
internal sealed class PublishedContentCacheRefresher : PayloadCacheRefresherBase<PublishedContentCacheRefresherNotification, ContentCacheRefresherNotificationPayload<PublishedContentCacheRefresher.JsonPayload>>
{
    /// <summary>
    /// The unique identifier of this refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("6BDC4BA1-5454-436B-80AC-FD13442CD216");

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedContentCacheRefresher"/> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public PublishedContentCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Published Content Cache Refresher";

    /// <summary>
    /// The payload broadcast for a single changed published content item.
    /// </summary>
    /// <param name="ContentKey">The key of the changed content item.</param>
    /// <param name="ChangeTypes">The kind of change that occurred.</param>
    /// <param name="AffectedCultures">The cultures affected by the change, or an empty array for invariant changes.</param>
    public record JsonPayload(Guid ContentKey, TreeChangeTypes ChangeTypes, string[] AffectedCultures)
    {
    }
}
