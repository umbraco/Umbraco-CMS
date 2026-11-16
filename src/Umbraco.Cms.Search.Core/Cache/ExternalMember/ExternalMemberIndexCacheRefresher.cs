using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.ExternalMember;

/// <summary>
/// Broadcasts external member changes to all servers, so each server can update the member indexes it maintains.
/// </summary>
internal sealed class ExternalMemberIndexCacheRefresher : PayloadCacheRefresherBase<ExternalMemberIndexCacheRefresherNotification, ContentCacheRefresherNotificationPayload<ExternalMemberIndexCacheRefresher.JsonPayload>>
{
    public static readonly Guid UniqueId = Guid.Parse("AA8969E4-6EB7-46C6-A355-8782A3C17831");

    public ExternalMemberIndexCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "External Member Index Cache Refresher";

    public record JsonPayload(Guid MemberKey, TreeChangeTypes ChangeTypes)
    {
    }
}
