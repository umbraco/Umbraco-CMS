using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Search.Core.Cache.PublicAccess;

/// <summary>
/// Distributed cache refresher that broadcasts, per protected content node, which public access entry changed — finer-grained than the core public access cache refresher, so search only reindexes what's actually affected.
/// </summary>
internal sealed class PublicAccessDetailedCacheRefresher : PayloadCacheRefresherBase<PublicAccessDetailedCacheRefresherNotification, ContentCacheRefresherNotificationPayload<PublicAccessDetailedCacheRefresher.JsonPayload>>
{
    /// <summary>
    /// The unique identifier of this refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("81CF9AC4-B257-4997-BDCA-2826A90FBA0D");

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicAccessDetailedCacheRefresher"/> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public PublicAccessDetailedCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Public Access Cache Refresher (Search.Core)";

    /// <summary>
    /// The payload broadcast for a single content node whose public access entry changed.
    /// </summary>
    /// <param name="ProtectedContentKey">The key of the protected content node.</param>
    public record JsonPayload(Guid ProtectedContentKey)
    {
    }
}
