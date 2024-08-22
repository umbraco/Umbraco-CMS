using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal class MediaCacheService : IMediaCacheService
{
    private readonly INuCacheContentRepository _nuCacheContentRepository;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly Microsoft.Extensions.Caching.Hybrid.HybridCache _hybridCache;
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly ICacheNodeFactory _cacheNodeFactory;

    public MediaCacheService(
        INuCacheContentRepository nuCacheContentRepository,
        IIdKeyMap idKeyMap,
        ICoreScopeProvider scopeProvider,
        Microsoft.Extensions.Caching.Hybrid.HybridCache hybridCache,
        IPublishedContentFactory publishedContentFactory,
        ICacheNodeFactory cacheNodeFactory)
    {
        _nuCacheContentRepository = nuCacheContentRepository;
        _idKeyMap = idKeyMap;
        _scopeProvider = scopeProvider;
        _hybridCache = hybridCache;
        _publishedContentFactory = publishedContentFactory;
        _cacheNodeFactory = cacheNodeFactory;
    }

    public async Task<IPublishedContent?> GetByKeyAsync(Guid key)
    {
        Attempt<int> idAttempt = _idKeyMap.GetIdForKey(key, UmbracoObjectTypes.Media);
        if (idAttempt.Success is false)
        {
            return null;
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        ContentCacheNode? contentCacheNode = await _hybridCache.GetOrCreateAsync(
            $"{key}", // Unique key to the cache entry
            cancel => ValueTask.FromResult(_nuCacheContentRepository.GetMediaSource(idAttempt.Result)));

        scope.Complete();
        return contentCacheNode is null ? null : _publishedContentFactory.ToIPublishedMedia(contentCacheNode);
    }

    public async Task<IPublishedContent?> GetByIdAsync(int id)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Media);
        if (keyAttempt.Success is false)
        {
            return null;
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        ContentCacheNode? contentCacheNode = await _hybridCache.GetOrCreateAsync(
            $"{keyAttempt.Result}", // Unique key to the cache entry
            cancel => ValueTask.FromResult(_nuCacheContentRepository.GetMediaSource(id)));
        scope.Complete();
        return contentCacheNode is null ? null : _publishedContentFactory.ToIPublishedMedia(contentCacheNode);
    }

    public async Task<bool> HasContentByIdAsync(int id)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Media);
        if (keyAttempt.Success is false)
        {
            return false;
        }

        ContentCacheNode? contentCacheNode = await _hybridCache.GetOrCreateAsync<ContentCacheNode?>(
            $"{keyAttempt.Result}", // Unique key to the cache entry
            cancel => ValueTask.FromResult<ContentCacheNode?>(null));

        if (contentCacheNode is null)
        {
            await _hybridCache.RemoveAsync($"{keyAttempt.Result}");
        }

        return contentCacheNode is not null;
    }


    public async Task RefreshMediaAsync(IMedia media)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        // Always set draft node
        // We have nodes seperate in the cache, cause 99% of the time, you are only using one
        // and thus we won't get too much data when retrieving from the cache.
        var cacheNode = _cacheNodeFactory.ToContentCacheNode(media);
        await _hybridCache.SetAsync(GetCacheKey(media.Key, false), cacheNode);
        _nuCacheContentRepository.RefreshMedia(cacheNode);
        scope.Complete();
    }

    public async Task DeleteItemAsync(int id)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        _nuCacheContentRepository.DeleteContentItem(id);
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Media);
        if (keyAttempt.Success)
        {
            await _hybridCache.RemoveAsync(keyAttempt.Result.ToString());
        }

        _idKeyMap.ClearCache(keyAttempt.Result);
        _idKeyMap.ClearCache(id);

        scope.Complete();
    }

    private string GetCacheKey(Guid key, bool preview) => preview ? $"{key}+draft" : $"{key}";
}
