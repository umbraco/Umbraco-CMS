using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Search.Core.Cache.Index;

internal sealed class RebuildIndexCacheRefresher : PayloadCacheRefresherBase<RebuildIndexCacheRefresherNotification, ContentCacheRefresherNotificationPayload<RebuildIndexCacheRefresher.JsonPayload>>
{
    public static readonly Guid UniqueId = Guid.Parse("5268743B-7D6B-47A1-A9C8-1C03F2FFE242");

    public RebuildIndexCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Reindex Cache Refresher";

    public record JsonPayload(string IndexAlias)
    {
    }
}
