#if DEBUG
    using System.Diagnostics;
#endif
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Extensions;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal sealed class MediaCacheService : IMediaCacheService
{
    private readonly IDatabaseCacheRepository _databaseCacheRepository;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly Microsoft.Extensions.Caching.Hybrid.HybridCache _hybridCache;
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly ICacheNodeFactory _cacheNodeFactory;
    private readonly IEnumerable<IMediaSeedKeyProvider> _seedKeyProviders;
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly ILogger<MediaCacheService> _logger;
    private readonly CacheSettings _cacheSettings;

    private readonly ConcurrentDictionary<string, IPublishedContent> _publishedContentCache = [];

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
        IOptions<CacheSettings> cacheSettings,
        ILogger<MediaCacheService> logger)
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
        _logger = logger;
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

        if (_publishedContentCache.TryGetValue(cacheKey, out IPublishedContent? cached))
        {
            return cached;
        }

        ContentCacheNode? contentCacheNode = await _hybridCache.GetOrCreateAsync(
            cacheKey, // Unique key to the cache entry
            async cancel =>
            {
                using ICoreScope scope = _scopeProvider.CreateCoreScope();
                ContentCacheNode? mediaCacheNode = await _databaseCacheRepository.GetMediaSourceAsync(key);
                scope.Complete();
                return mediaCacheNode;
            },
            GetEntryOptions(key),
            GenerateTags(key));

        // We don't want to cache removed items, this may cause issues if the L2 serializer changes.
        if (contentCacheNode is null)
        {
            await _hybridCache.RemoveAsync(cacheKey);
            return null;
        }

        IPublishedContent? result = _publishedContentFactory.ToIPublishedMedia(contentCacheNode).CreateModel(_publishedModelFactory);
        if (result is not null)
        {
            _publishedContentCache[cacheKey] = result;
        }

        return result;
    }

    public async Task<bool> HasContentByIdAsync(int id)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Media);
        if (keyAttempt.Success is false)
        {
            return false;
        }

        return await _hybridCache.ExistsAsync<ContentCacheNode?>($"{keyAttempt.Result}", CancellationToken.None);
    }

    public async Task RefreshMediaAsync(IMedia media)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        var cacheNode = _cacheNodeFactory.ToContentCacheNode(media);
        await _databaseCacheRepository.RefreshMediaAsync(cacheNode);
        _publishedContentCache.Remove(GetCacheKey(media.Key, false), out _);
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
#if DEBUG
        var sw = new Stopwatch();
        sw.Start();
#endif

        foreach (IEnumerable<Guid> group in SeedKeys.InGroupsOf(_cacheSettings.MediaSeedBatchSize))
        {
            var uncachedKeys = new HashSet<Guid>();
            foreach (Guid key in group)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var cacheKey = GetCacheKey(key, false);

                var existsInCache = await _hybridCache.ExistsAsync<ContentCacheNode?>(cacheKey, CancellationToken.None);
                if (existsInCache is false)
                {
                    uncachedKeys.Add(key);
                }
            }

            _logger.LogDebug("Uncached key count {KeyCount}", uncachedKeys.Count);

            if (uncachedKeys.Count == 0)
            {
                continue;
            }

            using ICoreScope scope = _scopeProvider.CreateCoreScope();

            IEnumerable<ContentCacheNode> cacheNodes = await _databaseCacheRepository.GetMediaSourcesAsync(uncachedKeys);

            scope.Complete();

            _logger.LogDebug("Media nodes to cache {NodeCount}", cacheNodes.Count());

            foreach (ContentCacheNode cacheNode in cacheNodes)
            {
                var cacheKey = GetCacheKey(cacheNode.Key, false);
                await _hybridCache.SetAsync(
                    cacheKey,
                    cacheNode,
                    GetSeedEntryOptions(),
                    GenerateTags(cacheNode.Key),
                    cancellationToken: cancellationToken);
            }
        }

#if DEBUG
        sw.Stop();
        _logger.LogInformation("Media cache seeding completed in {ElapsedMilliseconds} ms with {SeedCount} seed keys.", sw.ElapsedMilliseconds, SeedKeys.Count);
#else
        _logger.LogInformation("Media cache seeding completed with {SeedCount} seed keys.", SeedKeys.Count);
#endif
    }

    public async Task RefreshMemoryCacheAsync(Guid key)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        ContentCacheNode? publishedNode = await _databaseCacheRepository.GetMediaSourceAsync(key);
        if (publishedNode is not null)
        {
            var cacheKey = GetCacheKey(publishedNode.Key, false);
            await _hybridCache.SetAsync(cacheKey, publishedNode, GetEntryOptions(publishedNode.Key));
            _publishedContentCache.Remove(cacheKey, out _);
        }

        scope.Complete();
    }

    public async Task ClearMemoryCacheAsync(CancellationToken cancellationToken)
    {
        await _hybridCache.RemoveByTagAsync(Constants.Cache.Tags.Media, cancellationToken);

        // We have to run seeding again after the cache is cleared
        await SeedAsync(cancellationToken);
    }

    public async Task RemoveFromMemoryCacheAsync(Guid key)
        => await ClearPublishedCacheAsync(key);

    public async Task RebuildMemoryCacheByContentTypeAsync(IEnumerable<int> mediaTypeIds)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        IEnumerable<ContentCacheNode> contentByContentTypeKey = _databaseCacheRepository.GetContentByContentTypeKey(mediaTypeIds.Select(x => _idKeyMap.GetKeyForId(x, UmbracoObjectTypes.MediaType).Result), ContentCacheDataSerializerEntityType.Media);

        foreach (ContentCacheNode content in contentByContentTypeKey)
        {
            await _hybridCache.RemoveAsync(GetCacheKey(content.Key, true));

            if (content.IsDraft is false)
            {
                await ClearPublishedCacheAsync(content.Key);
            }
        }

        scope.Complete();
    }

    public void Rebuild(IReadOnlyCollection<int> contentTypeIds)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        _databaseCacheRepository.Rebuild(mediaTypeIds: contentTypeIds.ToList());

        IEnumerable<Guid> mediaTypeKeys = contentTypeIds.Select(x => _idKeyMap.GetKeyForId(x, UmbracoObjectTypes.MediaType))
            .Where(x => x.Success)
            .Select(x => x.Result);

        IEnumerable<ContentCacheNode> mediaCacheNodesByContentTypeKey =
            _databaseCacheRepository.GetContentByContentTypeKey(mediaTypeKeys, ContentCacheDataSerializerEntityType.Media);

        foreach (ContentCacheNode media in mediaCacheNodesByContentTypeKey)
        {
            ClearPublishedCacheAsync(media.Key).GetAwaiter().GetResult();
        }

        scope.Complete();

        // Clear the entire published content cache.
        // It doesn't seem feasible to be smarter about this, as a changed content type could be used for a media item,
        // elements within the media item, an ancestor, or a composition.
        _publishedContentCache.Clear();
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
        Expiration = _cacheSettings.Entry.Media.SeedCacheDuration,
        LocalCacheExpiration = _cacheSettings.Entry.Media.SeedCacheDuration,
    };

    private static string GetCacheKey(Guid key, bool preview) => preview ? $"{key}+draft" : $"{key}";

    // Generates the cache tags for a given CacheNode
    // We use the tags to be able to clear all cache entries that are related to a given content item.
    // Tags for now are only content/media, but can be expanded with draft/published later.
    private static HashSet<string> GenerateTags(Guid? key) => key is null ? [] : [Constants.Cache.Tags.Media];

    private async Task ClearPublishedCacheAsync(Guid key)
    {
        var cacheKey = GetCacheKey(key, false);
        await _hybridCache.RemoveAsync(cacheKey);
        _publishedContentCache.Remove(cacheKey, out _);
    }
}
