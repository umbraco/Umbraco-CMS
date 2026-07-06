using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.ContentType;

internal sealed class ContentTypeCacheRefresher : PayloadCacheRefresherBase<ContentTypeCacheRefresherNotification, ContentCacheRefresherNotificationPayload<ContentTypeCacheRefresher.JsonPayload>>
{
    public static readonly Guid UniqueId = Guid.Parse("9EC8AAAB-FEBA-4F58-819B-5B1C6E80F988");

    public ContentTypeCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Content Type Cache Refresher";

    public record JsonPayload(Guid ContentTypeKey, ContentTypeChangeTypes ChangeTypes)
    {
    }
}
