using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Search.Core.Cache.PublicAccess;

internal sealed class PublicAccessDetailedCacheRefresher : PayloadCacheRefresherBase<PublicAccessDetailedCacheRefresherNotification, ContentCacheRefresherNotificationPayload<PublicAccessDetailedCacheRefresher.JsonPayload>>
{
    public static readonly Guid UniqueId = Guid.Parse("81CF9AC4-B257-4997-BDCA-2826A90FBA0D");

    public PublicAccessDetailedCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Public Access Cache Refresher (Search.Core)";

    public record JsonPayload(Guid ProtectedContentKey)
    {
    }
}
