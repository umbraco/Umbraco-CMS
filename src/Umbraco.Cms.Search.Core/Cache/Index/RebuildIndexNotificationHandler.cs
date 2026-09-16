using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.Index;

/// <summary>
/// Broadcasts a request to rebuild the given search indexes via <see cref="RebuildIndexCacheRefresher"/>.
/// </summary>
internal sealed class RebuildIndexNotificationHandler : ContentNotificationHandlerBase<RebuildIndexCacheRefresher.JsonPayload>
{
    /// <inheritdoc />
    protected override Guid CacheRefresherUniqueId => RebuildIndexCacheRefresher.UniqueId;

    /// <summary>
    /// Initializes a new instance of the <see cref="RebuildIndexNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache used to broadcast the paired cache refresher notification.</param>
    /// <param name="originProvider">The provider of the current server origin.</param>
    /// <param name="indexDocumentService">The service used to flush the change-detection cache for affected documents.</param>
    public RebuildIndexNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService)
        : base(distributedCache, originProvider, indexDocumentService)
    {
    }

    /// <summary>
    /// Broadcasts a rebuild request for the given index aliases.
    /// </summary>
    /// <param name="indexAliases">The aliases of the indexes to rebuild.</param>
    public void Handle(IEnumerable<string> indexAliases)
    {
        RebuildIndexCacheRefresher.JsonPayload[] payloads = indexAliases
            .Select(indexAlias => new RebuildIndexCacheRefresher.JsonPayload(indexAlias))
            .ToArray();

        HandlePayloads(payloads);
    }
}
