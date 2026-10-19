using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.Content;

/// <summary>
/// Distributed cache refresher that broadcasts draft content changes to other servers, for the draft content index.
/// </summary>
internal sealed class DraftContentCacheRefresher : PayloadCacheRefresherBase<DraftContentCacheRefresherNotification, ContentCacheRefresherNotificationPayload<DraftContentCacheRefresher.JsonPayload>>
{
    /// <summary>
    /// The unique identifier of this refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("4DA581BA-07B8-4643-945E-FA9687C14D15");

    /// <summary>
    /// Initializes a new instance of the <see cref="DraftContentCacheRefresher"/> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public DraftContentCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Draft Content Cache Refresher";

    /// <summary>
    /// The payload broadcast for a single changed draft content item.
    /// </summary>
    /// <param name="ContentKey">The key of the changed content item.</param>
    /// <param name="ChangeTypes">The kind of change that occurred.</param>
    public record JsonPayload(Guid ContentKey, TreeChangeTypes ChangeTypes)
    {
    }
}
