using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.MediaType;

/// <summary>
/// Distributed cache refresher that broadcasts media type changes to other servers, for reindexing affected media.
/// </summary>
internal sealed class MediaTypeCacheRefresher : PayloadCacheRefresherBase<MediaTypeCacheRefresherNotification, ContentCacheRefresherNotificationPayload<MediaTypeCacheRefresher.JsonPayload>>
{
    /// <summary>
    /// The unique identifier of this refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("D9C7DFFA-444E-4928-98DF-1B61B9EC9BC9");

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeCacheRefresher"/> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public MediaTypeCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Media Type Cache Refresher";

    /// <summary>
    /// The payload broadcast for a single changed media type.
    /// </summary>
    /// <param name="MediaTypeKey">The key of the changed media type.</param>
    /// <param name="ChangeTypes">The kind of change that occurred.</param>
    public record JsonPayload(Guid MediaTypeKey, ContentTypeChangeTypes ChangeTypes)
    {
    }
}
