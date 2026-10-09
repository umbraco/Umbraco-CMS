using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.Element;

/// <summary>
/// Distributed cache refresher that broadcasts element changes to other servers, for the elements index.
/// </summary>
internal sealed class DraftElementCacheRefresher : PayloadCacheRefresherBase<DraftElementCacheRefresherNotification, ContentCacheRefresherNotificationPayload<DraftElementCacheRefresher.JsonPayload>>
{
    /// <summary>
    /// The unique identifier of this refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("DC2F8EF2-29EA-4752-B722-3A648C8B78FD");

    /// <summary>
    /// Initializes a new instance of the <see cref="DraftElementCacheRefresher"/> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public DraftElementCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Draft Element Cache Refresher";

    /// <summary>
    /// The payload broadcast for a single changed element or element container.
    /// </summary>
    /// <param name="ElementKey">The key of the changed element, or of the element container whose descendants changed.</param>
    /// <param name="ChangeTypes">The kind of change that occurred.</param>
    public record JsonPayload(Guid ElementKey, TreeChangeTypes ChangeTypes)
    {
    }
}
