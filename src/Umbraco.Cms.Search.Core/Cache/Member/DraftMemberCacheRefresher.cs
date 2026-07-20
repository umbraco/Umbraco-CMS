using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.Member;

internal sealed class DraftMemberCacheRefresher : PayloadCacheRefresherBase<DraftMemberCacheRefresherNotification, ContentCacheRefresherNotificationPayload<DraftMemberCacheRefresher.JsonPayload>>
{
    public static readonly Guid UniqueId = Guid.Parse("D9FA5485-624D-4BAE-BFA3-38FBFCCE4134");

    public DraftMemberCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Draft Member Cache Refresher";

    public record JsonPayload(Guid MemberKey, TreeChangeTypes ChangeTypes)
    {
    }
}
