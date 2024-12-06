using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Extensions;

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
    private readonly IPublishedModelFactory _publishedModelFactory;
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
        IPublishedModelFactory publishedModelFactory,
        IOptions<CacheSettings> cacheSettings)
    {
        _databaseCacheRepository = databaseCacheRepository;
        _idKeyMap = idKeyMap;
        _scopeProvider = scopeProvider;
        _hybridCache = hybridCache;
        _publishedContentFactory = publishedContentFactory;
        _cacheNodeFactory = cacheNodeFactory;
        _seedKeyProviders = seedKeyProviders;
        _publishedModelFactory = publishedModelFactory;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<IPublishedContent?> GetByKeyAsync(Guid key)
    {
        Attempt<int> idAttempt = _idKeyMap.GetIdForKey(key, UmbracoObjectTypes.Media);
        if (idAttempt.Success is false)
        {
            return null;
        }

        return await GetNodeAsync(key);
    }

    public async Task<IPublishedContent?> GetByIdAsync(int id)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Media);
        if (keyAttempt.Success is false)
        {
            return null;
        }

        Guid key = keyAttempt.Result;

        return await GetNodeAsync(key);
    }

    private async Task<IPublishedContent?> GetNodeAsync(Guid key)
    {
        var cacheKey = $"{key}";
        ContentCacheNode? contentCacheNode = await _hybridCache.GetOrCreateAsync(
            cacheKey, // Unique key to the cache entry
            async cancel =>
            {
                using ICoreScope scope = _scopeProvider.CreateCoreScope();
                ContentCacheNode? mediaCacheNode = await _databaseCacheRepository.GetMediaSourceAsync(key);
                scope.Complete();
                return mediaCacheNode;
            }, GetEntryOptions(key));

        // We don't want to cache removed items, this may cause issues if the L2 serializer changes.
        if (contentCacheNode is null)
        {
            await _hybridCache.RemoveAsync(cacheKey);
            return null;
        }

        return _publishedContentFactory.ToIPublishedMedia(contentCacheNode).CreateModel(_publishedModelFactory);
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
        await _databaseCacheRepository.RefreshMediaAsync(cacheNode);
        scope.Complete();
    }

    public async Task DeleteItemAsync(IContentBase media)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        await _databaseCacheRepository.DeleteContentItemAsync(media.Id);
        scope.Complete();
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {

        foreach (Guid key in SeedKeys)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var cacheKey = GetCacheKey(key, false);

            ContentCacheNode? cachedValue = await _hybridCache.GetOrCreateAsync<ContentCacheNode?>(
                cacheKey,
                async cancel =>
                {
                    using ICoreScope scope = _scopeProvider.CreateCoreScope();
                    ContentCacheNode? mediaCacheNode = await _databaseCacheRepository.GetMediaSourceAsync(key);
                    scope.Complete();
                    return mediaCacheNode;
                },
                GetSeedEntryOptions());

            if (cachedValue is null)
            {
                await _hybridCache.RemoveAsync(cacheKey);
            }
        }
    }

    public async Task RefreshMemoryCacheAsync(Guid key)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        ContentCacheNode? publishedNode = await _databaseCacheRepository.GetMediaSourceAsync(key);
        if (publishedNode is not null)
        {
            await _hybridCache.SetAsync(GetCacheKey(publishedNode.Key, false), publishedNode, GetEntryOptions(publishedNode.Key));
        }

        scope.Complete();
    }

    public async Task ClearMemoryCacheAsync(CancellationToken cancellationToken)
    {
        // TODO: This should be done with tags, however this is not implemented yet, so for now we have to naively get all content keys and clear them all.
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        // We have to get ALL document keys in order to be able to remove them from the cache,
        IEnumerable<Guid> documentKeys = await _databaseCacheRepository.GetContentKeysAsync(Constants.ObjectTypes.Media);

        foreach (Guid documentKey in documentKeys)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            // We'll remove both the draft and published cache
            await _hybridCache.RemoveAsync(GetCacheKey(documentKey, false), cancellationToken);
        }

        // We have to run seeding again after the cache is cleared
        await SeedAsync(cancellationToken);

        scope.Complete();
    }

    public async Task RemoveFromMemoryCacheAsync(Guid key)
        => await _hybridCache.RemoveAsync(GetCacheKey(key, false));

    public async Task RebuildMemoryCacheByContentTypeAsync(IEnumerable<int> mediaTypeIds)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        IEnumerable<ContentCacheNode> contentByContentTypeKey = _databaseCacheRepository.GetContentByContentTypeKey(mediaTypeIds.Select(x => _idKeyMap.GetKeyForId(x, UmbracoObjectTypes.MediaType).Result), ContentCacheDataSerializerEntityType.Media);

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

    public void Rebuild(IReadOnlyCollection<int> contentTypeIds)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        _databaseCacheRepository.Rebuild(contentTypeIds.ToList());

        IEnumerable<Guid> mediaTypeKeys = contentTypeIds.Select(x => _idKeyMap.GetKeyForId(x, UmbracoObjectTypes.MediaType))
            .Where(x => x.Success)
            .Select(x => x.Result);

        IEnumerable<ContentCacheNode> mediaCacheNodesByContentTypeKey =
            _databaseCacheRepository.GetContentByContentTypeKey(mediaTypeKeys, ContentCacheDataSerializerEntityType.Media);

        foreach (ContentCacheNode media in mediaCacheNodesByContentTypeKey)
        {
            _hybridCache.RemoveAsync(GetCacheKey(media.Key, false));
        }

        scope.Complete();
    }

    public IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        IEnumerable<ContentCacheNode> nodes = _databaseCacheRepository.GetContentByContentTypeKey([contentType.Key], ContentCacheDataSerializerEntityType.Media);
        scope.Complete();

        return nodes
            .Select(x => _publishedContentFactory.ToIPublishedContent(x, x.IsDraft).CreateModel(_publishedModelFactory))
            .WhereNotNull();
    }

    private HybridCacheEntryOptions GetEntryOptions(Guid key)
    {
        if (SeedKeys.Contains(key))
        {
            return GetSeedEntryOptions();
        }

        return new HybridCacheEntryOptions
        {
            Expiration = _cacheSettings.Entry.Media.RemoteCacheDuration,
            LocalCacheExpiration = _cacheSettings.Entry.Media.LocalCacheDuration,
        };
    }


    private HybridCacheEntryOptions GetSeedEntryOptions() => new()
    {
        Expiration = _cacheSettings.SeedCacheDuration,
        LocalCacheExpiration = _cacheSettings.SeedCacheDuration,
    };

    private string GetCacheKey(Guid key, bool preview) => preview ? $"{key}+draft" : $"{key}";
}
