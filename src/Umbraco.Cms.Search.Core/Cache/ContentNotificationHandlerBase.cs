using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache;

internal abstract class ContentNotificationHandlerBase<TPayload>
{
    private readonly DistributedCache _distributedCache;
    private readonly IOriginProvider _originProvider;
    private readonly IIndexDocumentService _indexDocumentService;

    protected ContentNotificationHandlerBase(DistributedCache distributedCache, IOriginProvider originProvider, IIndexDocumentService indexDocumentService)
    {
        _distributedCache = distributedCache;
        _originProvider = originProvider;
        _indexDocumentService = indexDocumentService;
    }

    protected abstract Guid CacheRefresherUniqueId { get; }

    protected T[] FindTopmostEntities<T>(IEnumerable<T> candidates)
        where T : IContentBase
    {
        T[] candidatesAsArray = candidates as T[] ?? candidates.ToArray();
        var ids = candidatesAsArray.Select(entity => entity.Id).ToArray();
        return candidatesAsArray.Where(entity => ids.Contains(entity.ParentId) is false).ToArray();
    }

    protected void HandlePayloads(TPayload[] payloads)
    {
        var payload = new ContentCacheRefresherNotificationPayload<TPayload>(payloads, _originProvider.GetCurrent());
        _distributedCache.RefreshByPayload(CacheRefresherUniqueId, [payload]);
    }

    protected void FlushDocumentIndexCache(Guid[] ids, bool published)
        => _indexDocumentService.DeleteAsync(ids, published).GetAwaiter().GetResult();

    protected void RemoveLanguageFromDocumentIndexCache(IReadOnlyCollection<string> isoCodes)
        => _indexDocumentService.DeleteCulturesAsync(isoCodes).GetAwaiter().GetResult();
}
