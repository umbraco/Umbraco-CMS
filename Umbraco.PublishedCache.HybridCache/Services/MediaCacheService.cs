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

    public MediaCacheService(INuCacheContentRepository nuCacheContentRepository, IIdKeyMap idKeyMap, ICoreScopeProvider scopeProvider, Microsoft.Extensions.Caching.Hybrid.HybridCache hybridCache, IPublishedContentFactory publishedContentFactory)
    {
        _nuCacheContentRepository = nuCacheContentRepository;
        _idKeyMap = idKeyMap;
        _scopeProvider = scopeProvider;
        _hybridCache = hybridCache;
        _publishedContentFactory = publishedContentFactory;
    }

    public async Task<IPublishedContent?> GetByKeyAsync(Guid key, bool preview = false)
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

    public async Task<IPublishedContent?> GetByIdAsync(int id, bool preview = false)
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

    public async Task<bool> HasContentByIdAsync(int id, bool preview = false)
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
        await _hybridCache.RemoveAsync(media.Key.ToString());
        _nuCacheContentRepository.RefreshMedia(media);
        scope.Complete();
    }

    public Task DeleteItemAsync(int id)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        _nuCacheContentRepository.DeleteContentItem(id);
        scope.Complete();
        return Task.CompletedTask;
    }
}
