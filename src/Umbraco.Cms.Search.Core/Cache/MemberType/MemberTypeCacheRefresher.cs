using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.MemberType;

internal sealed class MemberTypeCacheRefresher : PayloadCacheRefresherBase<MemberTypeCacheRefresherNotification, ContentCacheRefresherNotificationPayload<MemberTypeCacheRefresher.JsonPayload>>
{
    public static readonly Guid UniqueId = Guid.Parse("A8B945D2-C320-43AB-BAFA-763D4B426D0E");

    public MemberTypeCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Member Type Cache Refresher";

    public record JsonPayload(Guid MemberTypeKey, ContentTypeChangeTypes ChangeTypes)
    {
    }
}
