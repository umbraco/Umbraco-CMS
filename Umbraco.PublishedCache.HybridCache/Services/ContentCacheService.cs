using Microsoft.Extensions.Caching.Hybrid;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal sealed class ContentCacheService : IContentCacheService
{
    private readonly INuCacheContentRepository _nuCacheContentRepository;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly Microsoft.Extensions.Caching.Hybrid.HybridCache _hybridCache;
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly ICacheNodeFactory _cacheNodeFactory;


    public ContentCacheService(
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

    // TODO: Stop using IdKeyMap for these, but right now we both need key and id for caching..
    public async Task<IPublishedContent?> GetByKeyAsync(Guid key, bool preview = false)
    {
        Attempt<int> idAttempt = _idKeyMap.GetIdForKey(key, UmbracoObjectTypes.Document);
        if (idAttempt.Success is false)
        {
            return null;
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        ContentCacheNode? contentCacheNode = await _hybridCache.GetOrCreateAsync(
            GetCacheKey(key, preview), // Unique key to the cache entry
            cancel => ValueTask.FromResult(_nuCacheContentRepository.GetContentSource(idAttempt.Result)));

        scope.Complete();
        return contentCacheNode is null ? null : _publishedContentFactory.ToIPublishedContent(contentCacheNode, preview);
    }

    public async Task<IPublishedContent?> GetByIdAsync(int id, bool preview = false)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document);
        if (keyAttempt.Success is false)
        {
            return null;
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        ContentCacheNode? contentCacheNode = await _hybridCache.GetOrCreateAsync(
            GetCacheKey(keyAttempt.Result, preview), // Unique key to the cache entry
            cancel => ValueTask.FromResult(_nuCacheContentRepository.GetContentSource(id, preview)));
        scope.Complete();
        return contentCacheNode is null ? null : _publishedContentFactory.ToIPublishedContent(contentCacheNode, preview);
    }

    public async Task SeedAsync(IReadOnlyCollection<int>? contentTypeIds)
    {
        IEnumerable<ContentCacheNode> contentCacheNodes = _nuCacheContentRepository.GetContentByContentTypeId(contentTypeIds);
        foreach (ContentCacheNode contentCacheNode in contentCacheNodes)
        {
            // Never expire seeded values
            var entryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.MaxValue,
            };

            await _hybridCache.SetAsync(
                GetCacheKey(contentCacheNode.Key, false),
                contentCacheNode,
                entryOptions);
        }
    }

    public async Task<bool> HasContentByIdAsync(int id, bool preview = false)
    {
        Attempt<Guid>  keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document);
        if (keyAttempt.Success is false)
        {
            return false;
        }

        ContentCacheNode? contentCacheNode = await _hybridCache.GetOrCreateAsync<ContentCacheNode?>(
            GetCacheKey(keyAttempt.Result, preview), // Unique key to the cache entry
            cancel => ValueTask.FromResult<ContentCacheNode?>(null));

        if (contentCacheNode is null)
        {
            await _hybridCache.RemoveAsync(GetCacheKey(keyAttempt.Result, preview));
        }

        return contentCacheNode is not null;
    }

    public async Task RefreshContentAsync(IContent content)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        // Always set draft node
        // We have nodes seperate in the cache, cause 99% of the time, you are only using one
        // and thus we won't get too much data when retrieving from the cache.
        var draftCacheNode = _cacheNodeFactory.ToContentCacheNode(content, true);
        await _hybridCache.SetAsync(GetCacheKey(content.Key, true), draftCacheNode);

        if (content.PublishedState == PublishedState.Publishing)
        {
            var publishedCacheNode = _cacheNodeFactory.ToContentCacheNode(content, false);
            await _hybridCache.SetAsync(GetCacheKey(content.Key, false), publishedCacheNode);
        }

        _nuCacheContentRepository.RefreshContent(content);
        scope.Complete();
    }

    private string GetCacheKey(Guid key, bool preview) => preview ? $"{key}+draft" : $"{key}";

    public Task DeleteItemAsync(int id)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        _nuCacheContentRepository.DeleteContentItem(id);
        scope.Complete();
        return Task.CompletedTask;
    }
}
