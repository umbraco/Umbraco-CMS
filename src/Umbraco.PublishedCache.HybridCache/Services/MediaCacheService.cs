#if DEBUG
    using System.Diagnostics;
#endif
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
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

internal sealed class MediaCacheService : IMediaCacheService, IMemoryCacheSizeReporter
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

    private readonly IConvertedPublishedContentCache<Guid> _publishedContentCache;

    // Monotonic counter bumped whenever the in-memory cache (L0/L1) is invalidated or refreshed.
    // GetNodeAsync captures it before reading the backing store and re-checks it before writing
    // back, so a snapshot read before a concurrent refresh is never written over the refreshed
    // entry — preventing the stale-set clobber that otherwise persists until a full clear.
    //
    // Deliberately a single global counter, not per-key: any invalidation invalidates every in-flight
    // read-through. The only cost is an occasional skipped cache population when a read-through for one
    // key overlaps an unrelated refresh — a re-miss on the next request, never stale data. A per-key
    // scheme would avoid that but needs a global epoch for bulk clears plus an exact per-key bump on
    // every mutated cache key, which is easy to get wrong and would silently reintroduce the clobber.
    // Global is correctness-robust; only revisit if read-through churn under heavy concurrent
    // refreshing ever shows up in profiling.
    private long _cacheGeneration;

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
        ILogger<MediaCacheService> logger,
        IConvertedPublishedContentCacheFactory cacheFactory)
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
        _publishedContentCache = cacheFactory.Create<Guid>(_cacheSettings.Entry.Media.MaximumLocalCacheItems, CacheName);
    }

    /// <inheritdoc />
    public string CacheName => "Published media (converted, L0)";

    /// <inheritdoc />
    public long GetApproximateCount() => _publishedContentCache.Count;

    /// <inheritdoc />
    public long? GetApproximateBytes() => _publishedContentCache.ApproximateSizeInBytes;

    public Task<IPublishedContent?> GetByKeyAsync(Guid key) => GetNodeAsync(key);

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

    public bool TryGetCached(Guid key, out IPublishedContent? content)
    {
        // Mirror the L0 (published content cache) fast path in GetNodeAsync.
        if (_publishedContentCache.TryGet(key, out content))
        {
            return true;
        }

        content = null;
        return false;
    }

    private async Task<IPublishedContent?> GetNodeAsync(Guid key)
    {
        if (_publishedContentCache.TryGet(key, out IPublishedContent? cached))
        {
            return cached;
        }

        string cacheKey = GetCacheKey(key);
        (bool exists, ContentCacheNode? contentCacheNode) = await _hybridCache.TryGetValueAsync<ContentCacheNode?>(cacheKey, CancellationToken.None);

        // A value found in the backing store is already current, so it can always populate the caches
        // below; only a value built from the read-through DB fetch needs the generation guard.
        bool snapshotIsCurrent = true;
        if (exists is false)
        {
            // Capture the cache generation before reading the backing store. If a concurrent refresh or
            // invalidation bumps the generation while we read and build below, the snapshot we hold is
            // stale and must not be written back over the refreshed entries (the clobber that leaves
            // memory permanently stale until a full clear).
            long generation = Interlocked.Read(ref _cacheGeneration);

            contentCacheNode = await GetContentCacheNodeFromRepo();
            snapshotIsCurrent = IsCacheGenerationCurrent(generation);

            // We don't want to cache removed items, this may cause issues if the L2 serializer changes.
            // Skip the write when the generation moved — a refresh has superseded this snapshot.
            if (contentCacheNode is not null && snapshotIsCurrent)
            {
                await _hybridCache.SetAsync(
                    cacheKey,
                    contentCacheNode,
                    GetEntryOptions(key),
                    GenerateTags(contentCacheNode));
            }
        }

        if (contentCacheNode is null)
        {
            return null;
        }

        IPublishedContent? result = _publishedContentFactory.ToIPublishedMedia(contentCacheNode).CreateModel(_publishedModelFactory);

        // Only populate the L0 cache when our snapshot is still current; otherwise a concurrent
        // refresh has already written fresher content and we must not overwrite it with this one.
        if (result is not null && snapshotIsCurrent)
        {
            // The size estimate runs unconditionally (not only when reporting is enabled): it is cheap
            // (O(properties), no IO/decompression) and only on the cache-miss path, and keeping the running
            // total always-current means it is accurate the moment debug reporting is switched on.
            _publishedContentCache.Set(key, result, ContentCacheNodeSizeEstimator.EstimateBytes(contentCacheNode));
        }

        return result;

        async Task<ContentCacheNode?> GetContentCacheNodeFromRepo()
        {
            using ICoreScope scope = _scopeProvider.CreateCoreScope();
			scope.ReadLock(Constants.Locks.MediaTree);
            ContentCacheNode? mediaCacheNode = await _databaseCacheRepository.GetMediaSourceAsync(key);
            scope.Complete();
            return mediaCacheNode;
        }
    }

    // Bumped after every in-memory cache invalidation/refresh so in-flight read-through snapshots
    // (see GetNodeAsync) can detect they have been superseded and skip writing back stale content.
    private void InvalidateMemoryCacheGeneration() => Interlocked.Increment(ref _cacheGeneration);

    private bool IsCacheGenerationCurrent(long capturedGeneration)
        => Interlocked.Read(ref _cacheGeneration) == capturedGeneration;

    public async Task<bool> HasContentByIdAsync(int id)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Media);
        if (keyAttempt.Success is false)
        {
            return false;
        }

        return await _hybridCache.ExistsAsync<ContentCacheNode?>(GetCacheKey(keyAttempt.Result), CancellationToken.None);
    }

    public async Task RefreshMediaAsync(IMedia media)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        if (media.Trashed)
        {
            await _databaseCacheRepository.DeleteContentItemAsync(media.Id);
            await RemoveFromMemoryCacheAsync(media.Key);
            scope.Complete();
            return;
        }

        var cacheNode = _cacheNodeFactory.ToContentCacheNode(media);
        await _databaseCacheRepository.RefreshMediaAsync(cacheNode);
        _publishedContentCache.Remove(media.Key);
        InvalidateMemoryCacheGeneration();
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

                var existsInCache = await _hybridCache.ExistsAsync<ContentCacheNode?>(GetCacheKey(key), CancellationToken.None);
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
            scope.ReadLock(Constants.Locks.MediaTree);

            IEnumerable<ContentCacheNode> cacheNodes = await _databaseCacheRepository.GetMediaSourcesAsync(uncachedKeys);

            scope.Complete();

            _logger.LogDebug("Media nodes to cache {NodeCount}", cacheNodes.Count());

            foreach (ContentCacheNode cacheNode in cacheNodes)
            {
                await _hybridCache.SetAsync(
                    GetCacheKey(cacheNode.Key),
                    cacheNode,
                    GetSeedEntryOptions(),
                    GenerateTags(cacheNode),
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
        scope.ReadLock(Constants.Locks.MediaTree);

        ContentCacheNode? publishedNode = await _databaseCacheRepository.GetMediaSourceAsync(key);
        if (publishedNode is not null)
        {
            await _hybridCache.SetAsync(GetCacheKey(publishedNode.Key), publishedNode, GetEntryOptions(publishedNode.Key));
            _publishedContentCache.Remove(key);
            InvalidateMemoryCacheGeneration();
        }
        else
        {
            // RemoveFromMemoryCacheAsync → ClearPublishedCacheAsync bumps the generation itself,
            // so this path is already covered.
            await RemoveFromMemoryCacheAsync(key);
        }

        scope.Complete();
    }

    public async Task ClearMemoryCacheAsync(CancellationToken cancellationToken)
    {
        // Bump first so any read-through that read the backing store before this clear is rejected
        // when it tries to write back, even while the reseed below is still running.
        InvalidateMemoryCacheGeneration();

        _publishedContentCache.Clear();
        await _hybridCache.RemoveByTagAsync(Constants.Cache.Tags.Media, cancellationToken);

        // We have to run seeding again after the cache is cleared
        await SeedAsync(cancellationToken);
    }

    public async Task RemoveFromMemoryCacheAsync(Guid key)
        => await ClearPublishedCacheAsync(key);

    public async Task RebuildMemoryCacheByContentTypeAsync(IEnumerable<int> mediaTypeIds)
    {
        // Clear the hybrid cache by media type tag for the affected media types.
        var mediaTypeIdsAsArray = mediaTypeIds as int[] ?? mediaTypeIds.ToArray();
        var mediaTypeIdTags = mediaTypeIdsAsArray.Select(MediaTypeIdTag).ToArray();
        await _hybridCache.RemoveByTagAsync(mediaTypeIdTags);

        // Clear converted media for the affected types so entries are re-converted when next requested.
        ClearConvertedContentCache(mediaTypeIdsAsArray);
    }

    public void ClearConvertedContentCache()
    {
        _publishedContentCache.Clear();
        InvalidateMemoryCacheGeneration();
    }

    public void ClearConvertedContentCache(IReadOnlyCollection<int> mediaTypeIds)
    {
        var ids = mediaTypeIds as int[] ?? mediaTypeIds.ToArray();
        _publishedContentCache.RemoveWhere(content => ids.Contains(content.ContentType.Id));
        InvalidateMemoryCacheGeneration();
    }

    public void Rebuild(IReadOnlyCollection<int> contentTypeIds)
        => _databaseCacheRepository.Rebuild(
            null,
            contentTypeIds.ToList(),
            null,
            action =>
            {
                using ICoreScope scope = _scopeProvider.CreateCoreScope();
                action();
                scope.Complete();
            });

    public IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.MediaTree);
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

    private static string GetCacheKey(Guid key) => $"{key}";

    // Generates the cache tags for a given CacheNode.
    // We use the tags to be able to clear all cache entries that are related to a given content item.
    private static HashSet<string> GenerateTags(ContentCacheNode? cacheNode) => cacheNode is null ? [] : [Constants.Cache.Tags.Media, MediaTypeIdTag(cacheNode.ContentTypeId)];

    private async Task ClearPublishedCacheAsync(Guid key)
    {
        await _hybridCache.RemoveAsync(GetCacheKey(key));
        _publishedContentCache.Remove(key);
        InvalidateMemoryCacheGeneration();
    }

    private static string MediaTypeIdTag(int mediaTypeId)
        => $"mt:{mediaTypeId}";
}
