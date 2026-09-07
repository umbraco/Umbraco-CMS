using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache;

/// <summary>
/// Provides shared helpers for notification handlers that broadcast a custom, paired cache refresher notification and keep the index document change-detection cache in sync.
/// </summary>
/// <typeparam name="TPayload">The type of the cache refresher payload broadcast by the derived handler.</typeparam>
internal abstract class ContentNotificationHandlerBase<TPayload>
{
    private readonly DistributedCache _distributedCache;
    private readonly IOriginProvider _originProvider;
    private readonly IIndexDocumentService _indexDocumentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentNotificationHandlerBase{TPayload}"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache used to broadcast the paired cache refresher notification.</param>
    /// <param name="originProvider">The provider of the current server origin.</param>
    /// <param name="indexDocumentService">The service used to flush the change-detection cache for affected documents.</param>
    protected ContentNotificationHandlerBase(DistributedCache distributedCache, IOriginProvider originProvider, IIndexDocumentService indexDocumentService)
    {
        _distributedCache = distributedCache;
        _originProvider = originProvider;
        _indexDocumentService = indexDocumentService;
    }

    /// <summary>
    /// Gets the unique identifier of the paired cache refresher to broadcast payloads to.
    /// </summary>
    protected abstract Guid CacheRefresherUniqueId { get; }

    /// <summary>
    /// Filters a set of entities down to those whose parent is not also in the set, i.e. the topmost affected entities.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    /// <param name="candidates">The candidate entities.</param>
    /// <returns>The topmost entities among <paramref name="candidates"/>.</returns>
    protected T[] FindTopmostEntities<T>(IEnumerable<T> candidates)
        where T : IContentBase
    {
        T[] candidatesAsArray = candidates as T[] ?? candidates.ToArray();
        var ids = candidatesAsArray.Select(entity => entity.Id).ToArray();
        return candidatesAsArray.Where(entity => ids.Contains(entity.ParentId) is false).ToArray();
    }

    /// <summary>
    /// Broadcasts the given payloads to the paired cache refresher, tagged with the current server as origin.
    /// </summary>
    /// <param name="payloads">The payloads to broadcast.</param>
    protected void HandlePayloads(TPayload[] payloads)
    {
        var payload = new ContentCacheRefresherNotificationPayload<TPayload>(payloads, _originProvider.GetCurrent());
        _distributedCache.RefreshByPayload(CacheRefresherUniqueId, [payload]);
    }

    /// <summary>
    /// Removes the given entities from the index document change-detection cache, so they are re-indexed on next change.
    /// </summary>
    /// <param name="ids">The keys of the entities to flush.</param>
    /// <param name="published">Whether to flush the published or draft index document cache.</param>
    protected void FlushDocumentIndexCache(Guid[] ids, bool published)
        => _indexDocumentService.DeleteAsync(ids, published).GetAwaiter().GetResult();

    /// <summary>
    /// Removes the given cultures from the index document change-detection cache.
    /// </summary>
    /// <param name="isoCodes">The ISO codes of the deleted languages.</param>
    protected void RemoveLanguageFromDocumentIndexCache(IReadOnlyCollection<string> isoCodes)
        => _indexDocumentService.DeleteCulturesAsync(isoCodes).GetAwaiter().GetResult();
}
