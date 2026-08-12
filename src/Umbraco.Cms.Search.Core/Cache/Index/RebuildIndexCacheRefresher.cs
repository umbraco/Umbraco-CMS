using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Search.Core.Cache.Index;

/// <summary>
/// Distributed cache refresher that broadcasts a request to rebuild a search index to other servers.
/// </summary>
internal sealed class RebuildIndexCacheRefresher : PayloadCacheRefresherBase<RebuildIndexCacheRefresherNotification, ContentCacheRefresherNotificationPayload<RebuildIndexCacheRefresher.JsonPayload>>
{
    /// <summary>
    /// The unique identifier of this refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("5268743B-7D6B-47A1-A9C8-1C03F2FFE242");

    /// <summary>
    /// Initializes a new instance of the <see cref="RebuildIndexCacheRefresher"/> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public RebuildIndexCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Reindex Cache Refresher";

    /// <summary>
    /// The payload broadcast to request a rebuild of a specific index.
    /// </summary>
    /// <param name="IndexAlias">The alias of the index to rebuild.</param>
    public record JsonPayload(string IndexAlias)
    {
    }
}
