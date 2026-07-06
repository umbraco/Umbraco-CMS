using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.Content;

internal sealed class DraftContentCacheRefresher : PayloadCacheRefresherBase<DraftContentCacheRefresherNotification, ContentCacheRefresherNotificationPayload<DraftContentCacheRefresher.JsonPayload>>
{
    public static readonly Guid UniqueId = Guid.Parse("4DA581BA-07B8-4643-945E-FA9687C14D15");

    public DraftContentCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Draft Content Cache Refresher";

    public record JsonPayload(Guid ContentKey, TreeChangeTypes ChangeTypes)
    {
    }
}
