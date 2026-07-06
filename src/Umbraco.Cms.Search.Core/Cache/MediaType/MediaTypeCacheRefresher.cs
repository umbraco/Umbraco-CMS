using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.MediaType;

internal sealed class MediaTypeCacheRefresher : PayloadCacheRefresherBase<MediaTypeCacheRefresherNotification, ContentCacheRefresherNotificationPayload<MediaTypeCacheRefresher.JsonPayload>>
{
    public static readonly Guid UniqueId = Guid.Parse("D9C7DFFA-444E-4928-98DF-1B61B9EC9BC9");

    public MediaTypeCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Media Type Cache Refresher";

    public record JsonPayload(Guid MediaTypeKey, ContentTypeChangeTypes ChangeTypes)
    {
    }
}
