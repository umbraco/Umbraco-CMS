using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.Media;

/// <summary>
/// Distributed cache refresher that broadcasts media changes to other servers, for the media index.
/// </summary>
internal sealed class DraftMediaCacheRefresher : PayloadCacheRefresherBase<DraftMediaCacheRefresherNotification, ContentCacheRefresherNotificationPayload<DraftMediaCacheRefresher.JsonPayload>>
{
    /// <summary>
    /// The unique identifier of this refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("7BDF73A8-37D4-4DD4-A530-0FFEA1C6DBA2");

    /// <summary>
    /// Initializes a new instance of the <see cref="DraftMediaCacheRefresher"/> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public DraftMediaCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Draft Media Cache Refresher";

    /// <summary>
    /// The payload broadcast for a single changed media item.
    /// </summary>
    /// <param name="MediaKey">The key of the changed media item.</param>
    /// <param name="ChangeTypes">The kind of change that occurred.</param>
    public record JsonPayload(Guid MediaKey, TreeChangeTypes ChangeTypes)
    {
    }
}
