using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.MemberType;

/// <summary>
/// Distributed cache refresher that broadcasts member type changes to other servers, for reindexing affected members.
/// </summary>
internal sealed class MemberTypeCacheRefresher : PayloadCacheRefresherBase<MemberTypeCacheRefresherNotification, ContentCacheRefresherNotificationPayload<MemberTypeCacheRefresher.JsonPayload>>
{
    /// <summary>
    /// The unique identifier of this refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("A8B945D2-C320-43AB-BAFA-763D4B426D0E");

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberTypeCacheRefresher"/> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public MemberTypeCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Member Type Cache Refresher";

    /// <summary>
    /// The payload broadcast for a single changed member type.
    /// </summary>
    /// <param name="MemberTypeKey">The key of the changed member type.</param>
    /// <param name="ChangeTypes">The kind of change that occurred.</param>
    public record JsonPayload(Guid MemberTypeKey, ContentTypeChangeTypes ChangeTypes)
    {
    }
}
