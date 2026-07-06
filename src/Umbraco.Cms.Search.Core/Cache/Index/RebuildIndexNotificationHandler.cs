using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.Index;

internal sealed class RebuildIndexNotificationHandler : ContentNotificationHandlerBase<RebuildIndexCacheRefresher.JsonPayload>
{
    protected override Guid CacheRefresherUniqueId => RebuildIndexCacheRefresher.UniqueId;

    public RebuildIndexNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService)
        : base(distributedCache, originProvider, indexDocumentService)
    {
    }

    public void Handle(IEnumerable<string> indexAliases)
    {
        RebuildIndexCacheRefresher.JsonPayload[] payloads = indexAliases
            .Select(indexAlias => new RebuildIndexCacheRefresher.JsonPayload(indexAlias))
            .ToArray();

        HandlePayloads(payloads);
    }
}
