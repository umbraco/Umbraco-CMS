using Microsoft.Extensions.Caching.Hybrid;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal sealed class DocumentCacheService : IDocumentCacheService
{
    private readonly IDatabaseCacheRepository _databaseCacheRepository;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly Microsoft.Extensions.Caching.Hybrid.HybridCache _hybridCache;
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly ICacheNodeFactory _cacheNodeFactory;
    private readonly IEnumerable<IDocumentSeedKeyProvider> _seedKeyProviders;


    public DocumentCacheService(
        IDatabaseCacheRepository databaseCacheRepository,
        IIdKeyMap idKeyMap,
        ICoreScopeProvider scopeProvider,
        Microsoft.Extensions.Caching.Hybrid.HybridCache hybridCache,
        IPublishedContentFactory publishedContentFactory,
        ICacheNodeFactory cacheNodeFactory,
        IEnumerable<IDocumentSeedKeyProvider> seedKeyProviders)
    {
        _databaseCacheRepository = databaseCacheRepository;
        _idKeyMap = idKeyMap;
        _scopeProvider = scopeProvider;
        _hybridCache = hybridCache;
        _publishedContentFactory = publishedContentFactory;
        _cacheNodeFactory = cacheNodeFactory;
        _seedKeyProviders = seedKeyProviders;
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
            async cancel => await _databaseCacheRepository.GetContentSourceAsync(idAttempt.Result, preview));

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
            async cancel => await _databaseCacheRepository.GetContentSourceAsync(id, preview));
        scope.Complete();
        return contentCacheNode is null ? null : _publishedContentFactory.ToIPublishedContent(contentCacheNode, preview);
    }

    public async Task SeedAsync(IReadOnlyCollection<Guid> contentTypeKeys)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        var keys = new HashSet<Guid>();

        foreach (IDocumentSeedKeyProvider provider in _seedKeyProviders)
        {
            keys.UnionWith(provider.GetSeedKeys());
        }

        foreach (Guid key in keys)
        {
            // TODO: Make these expiration dates configurable.
            // Never expire seeded values, we cannot do TimeSpan.MaxValue sadly, so best we can do is a year.
            var entryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromDays(365),
                LocalCacheExpiration = TimeSpan.FromDays(365),
            };

            // We'll use GetOrCreateAsync because it may be in the second level cache, in which case we don't have to re-seed.
            await _hybridCache.GetOrCreateAsync<ContentCacheNode?>(
                GetCacheKey(key, false),
                cancel => new ValueTask<ContentCacheNode?>(
                    _databaseCacheRepository.GetContentSourceAsync(key, false)),
                entryOptions);
        }

        scope.Complete();
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
        await _hybridCache.RemoveAsync(GetCacheKey(content.Key, true));
        await _databaseCacheRepository.RefreshContentAsync(draftCacheNode, content.PublishedState);

        if (content.PublishedState == PublishedState.Publishing)
        {
            var publishedCacheNode = _cacheNodeFactory.ToContentCacheNode(content, false);
            await _hybridCache.RemoveAsync(GetCacheKey(content.Key, false));
            await _databaseCacheRepository.RefreshContentAsync(publishedCacheNode, content.PublishedState);
        }

        scope.Complete();
    }

    private string GetCacheKey(Guid key, bool preview) => preview ? $"{key}+draft" : $"{key}";

    public async Task DeleteItemAsync(int id)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        await _databaseCacheRepository.DeleteContentItemAsync(id);
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document);
        await _hybridCache.RemoveAsync(GetCacheKey(keyAttempt.Result, true));
        await _hybridCache.RemoveAsync(GetCacheKey(keyAttempt.Result, false));
        _idKeyMap.ClearCache(keyAttempt.Result);
        _idKeyMap.ClearCache(id);
        scope.Complete();
    }

    public void Rebuild(IReadOnlyCollection<int> contentTypeKeys)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        _databaseCacheRepository.Rebuild(contentTypeKeys.ToList());
        IEnumerable<ContentCacheNode> contentByContentTypeKey = _databaseCacheRepository.GetContentByContentTypeKey(contentTypeKeys.Select(x => _idKeyMap.GetKeyForId(x, UmbracoObjectTypes.DocumentType).Result));

        foreach (ContentCacheNode content in contentByContentTypeKey)
        {
            _hybridCache.RemoveAsync(GetCacheKey(content.Key, true)).GetAwaiter().GetResult();

            if (content.IsDraft is false)
            {
                _hybridCache.RemoveAsync(GetCacheKey(content.Key, false)).GetAwaiter().GetResult();
            }
        }

        scope.Complete();
    }
}
