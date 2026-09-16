using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.Member;

/// <summary>
/// Distributed cache refresher that broadcasts member changes to other servers, for the members index.
/// </summary>
internal sealed class DraftMemberCacheRefresher : PayloadCacheRefresherBase<DraftMemberCacheRefresherNotification, ContentCacheRefresherNotificationPayload<DraftMemberCacheRefresher.JsonPayload>>
{
    /// <summary>
    /// The unique identifier of this refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("D9FA5485-624D-4BAE-BFA3-38FBFCCE4134");

    /// <summary>
    /// Initializes a new instance of the <see cref="DraftMemberCacheRefresher"/> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public DraftMemberCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Draft Member Cache Refresher";

    /// <summary>
    /// The payload broadcast for a single changed member.
    /// </summary>
    /// <param name="MemberKey">The key of the changed member.</param>
    /// <param name="ChangeTypes">The kind of change that occurred.</param>
    public record JsonPayload(Guid MemberKey, TreeChangeTypes ChangeTypes)
    {
    }
}
