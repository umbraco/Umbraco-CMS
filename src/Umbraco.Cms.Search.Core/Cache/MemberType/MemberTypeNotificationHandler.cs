using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.MemberType;

internal sealed class MemberTypeNotificationHandler : ContentNotificationHandlerBase<MemberTypeCacheRefresher.JsonPayload>,
        IDistributedCacheNotificationHandler<MemberTypeChangedNotification>
{
    public MemberTypeNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService)
        : base(distributedCache, originProvider, indexDocumentService)
    {
    }

    protected override Guid CacheRefresherUniqueId => MemberTypeCacheRefresher.UniqueId;

    public void Handle(MemberTypeChangedNotification notification)
    {
        MemberTypeCacheRefresher.JsonPayload[] payloads = notification
            .Changes
            .Select(change => new MemberTypeCacheRefresher.JsonPayload(change.Item.Key, change.ChangeTypes))
            .ToArray();

        HandlePayloads(payloads);
    }
}
