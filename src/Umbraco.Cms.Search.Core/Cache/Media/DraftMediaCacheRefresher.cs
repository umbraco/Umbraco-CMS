using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.Media;

internal sealed class DraftMediaCacheRefresher : PayloadCacheRefresherBase<DraftMediaCacheRefresherNotification, ContentCacheRefresherNotificationPayload<DraftMediaCacheRefresher.JsonPayload>>
{
    public static readonly Guid UniqueId = Guid.Parse("7BDF73A8-37D4-4DD4-A530-0FFEA1C6DBA2");

    public DraftMediaCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Draft Media Cache Refresher";

    public record JsonPayload(Guid MediaKey, TreeChangeTypes ChangeTypes)
    {
    }
}
