using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
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
    private readonly IDatabaseCacheRepository _databaseCacheRepository;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly Microsoft.Extensions.Caching.Hybrid.HybridCache _hybridCache;
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly ICacheNodeFactory _cacheNodeFactory;
    private readonly IEnumerable<IMediaSeedKeyProvider> _seedKeyProviders;
    private readonly CacheSettings _cacheSettings;

    private HashSet<Guid>? _seedKeys;
    private HashSet<Guid> SeedKeys
    {
        get
        {
            if (_seedKeys is not null)
            {
                return _seedKeys;
            }

            _seedKeys = [];

            foreach (IMediaSeedKeyProvider provider in _seedKeyProviders)
            {
                _seedKeys.UnionWith(provider.GetSeedKeys());
            }

            return _seedKeys;
        }
    }

    public MediaCacheService(
        IDatabaseCacheRepository databaseCacheRepository,
        IIdKeyMap idKeyMap,
        ICoreScopeProvider scopeProvider,
        Microsoft.Extensions.Caching.Hybrid.HybridCache hybridCache,
        IPublishedContentFactory publishedContentFactory,
        ICacheNodeFactory cacheNodeFactory,
        IEnumerable<IMediaSeedKeyProvider> seedKeyProviders,
        IOptions<CacheSettings> cacheSettings)
    {
        _databaseCacheRepository = databaseCacheRepository;
        _idKeyMap = idKeyMap;
        _scopeProvider = scopeProvider;
        _hybridCache = hybridCache;
        _publishedContentFactory = publishedContentFactory;
        _cacheNodeFactory = cacheNodeFactory;
        _seedKeyProviders = seedKeyProviders;
        _cacheSettings = cacheSettings.Value;
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
            async cancel => await _databaseCacheRepository.GetMediaSourceAsync(idAttempt.Result));

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
            async cancel => await _databaseCacheRepository.GetMediaSourceAsync(id));
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
        await _databaseCacheRepository.RefreshMediaAsync(cacheNode);
        scope.Complete();
    }

    public async Task DeleteItemAsync(IContentBase media)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        await _databaseCacheRepository.DeleteContentItemAsync(media.Id);
        await _hybridCache.RemoveAsync(media.Key.ToString());
        scope.Complete();
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        foreach (Guid key in SeedKeys)
        {
            if(cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var cacheKey = GetCacheKey(key, false);

            ContentCacheNode? cachedValue = await _hybridCache.GetOrCreateAsync<ContentCacheNode?>(
                cacheKey,
                async cancel => await _databaseCacheRepository.GetMediaSourceAsync(key),
                GetSeedEntryOptions());

            if (cachedValue is null)
            {
                await _hybridCache.RemoveAsync(cacheKey);
            }
        }

        scope.Complete();
    }

    private HybridCacheEntryOptions GetSeedEntryOptions() => new()
    {
        Expiration = _cacheSettings.SeedCacheDuration, LocalCacheExpiration = _cacheSettings.SeedCacheDuration,
    };

    private string GetCacheKey(Guid key, bool preview) => preview ? $"{key}+draft" : $"{key}";
}
