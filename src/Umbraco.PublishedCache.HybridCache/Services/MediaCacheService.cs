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

/// <summary>
/// Caches published media across the converted-content (L0) cache, the HybridCache (L1/L2) tier and
/// the database, and handles seeding, refreshing and rebuilding those caches.
/// </summary>
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

    private readonly IConvertedPublishedContentCache<Guid, IPublishedContent> _publishedContentCache;

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

#pragma warning disable IDE0032 // Use auto property - auto-property can't express the lazy initialization of the seed keys and reset, so we use a backing field instead.
    private HashSet<Guid>? _seedKeys;
#pragma warning restore IDE0032 // Use auto property

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

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaCacheService"/> class.
    /// </summary>
    /// <param name="databaseCacheRepository">The repository providing cached media from the database.</param>
    /// <param name="idKeyMap">The id/key map used to resolve integer identifiers to keys.</param>
    /// <param name="scopeProvider">The scope provider for database access.</param>
    /// <param name="hybridCache">The HybridCache (L1/L2) backing store for cache nodes.</param>
    /// <param name="publishedContentFactory">The factory that converts cache nodes to <see cref="IPublishedContent"/>.</param>
    /// <param name="cacheNodeFactory">The factory that builds cache nodes from media.</param>
    /// <param name="seedKeyProviders">The providers that supply the keys to seed on startup.</param>
    /// <param name="publishedModelFactory">The factory that creates strongly-typed published models.</param>
    /// <param name="cacheSettings">The cache configuration options.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cacheFactory">The factory that creates the in-memory converted-content (L0) cache.</param>
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
        _publishedContentCache = cacheFactory.Create<Guid, IPublishedContent>(_cacheSettings.Entry.Media.MaximumLocalCacheItems, CacheName);
    }

    /// <inheritdoc />
    public string CacheName => "Published media (converted, L0)";

    /// <inheritdoc />
    public long GetApproximateCount() => _publishedContentCache.Count;

    /// <inheritdoc />
    public long? GetApproximateBytes() => _publishedContentCache.ApproximateSizeInBytes;

    /// <inheritdoc />
    public Task<IPublishedContent?> GetByKeyAsync(Guid key) => GetNodeAsync(key);

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<IReadOnlyList<IPublishedContent>> GetByKeysAsync(IReadOnlyCollection<Guid> keys)
    {
        // Capture the generation once before any backing-store read so a concurrent refresh landing
        // mid-fetch makes us skip the write-back rather than clobber fresher entries — applied once
        // for the whole set, and more conservatively than GetNodeAsync (which exempts HybridCache hits
        // from the guard, applying it only to the database read).
        var generation = Interlocked.Read(ref _cacheGeneration);

        var resolved = new Dictionary<Guid, IPublishedContent>(keys.Count);

        // Three tiers, each handed only the keys the previous one could not account for: the synchronous
        // L0 probe, the L1/L2 probe, then a single batched database read.
        List<Guid> pending = ProbeConvertedCache(keys, resolved);
        if (pending.Count > 0)
        {
            pending = await ProbeHybridCacheAsync(pending, generation, resolved);
        }

        if (pending.Count > 0)
        {
            await ReadFromDatabaseAsync(pending, generation, resolved);
        }

        // Return in input order; keys that resolved to nothing (missing) are omitted. A key repeated in
        // the input resolves to the same item at every occurrence, having only been looked up once — so
        // the input count, not the resolved count, is the upper bound on the result.
        var ordered = new List<IPublishedContent>(keys.Count);
        foreach (Guid key in keys)
        {
            if (resolved.TryGetValue(key, out IPublishedContent? content))
            {
                ordered.Add(content);
            }
        }

        return ordered;
    }

    // L0 (converted) fast path (via the shared TryGetCached).
    private List<Guid> ProbeConvertedCache(IReadOnlyCollection<Guid> keys, Dictionary<Guid, IPublishedContent> resolved)
    {
        var pending = new List<Guid>(keys.Count);
        var seen = new HashSet<Guid>(keys.Count);

        foreach (Guid key in keys)
        {
            if (seen.Add(key) is false)
            {
                continue;
            }

            if (TryGetCached(key, out IPublishedContent? cached) && cached is not null)
            {
                resolved[key] = cached;
            }
            else
            {
                pending.Add(key);
            }
        }

        return pending;
    }

    // L1/L2 probe without a database hit (same primitive GetNodeAsync uses). An entry holding a null
    // node accounts for its key and is not passed on to the database read; GetNodeAsync does not write
    // those for media, but honouring one costs nothing and keeps the two paths in step. Keys are probed
    // one at a time, and the probe is not free even on a hit: TryGetValueAsync takes a per-key lock and
    // goes through GetOrCreateAsync, which on a miss creates and then removes an entry. With a
    // distributed L2 (e.g. Redis) configured that is a serial round-trip per key, plus a write and a
    // delete for each miss.
    private async Task<List<Guid>> ProbeHybridCacheAsync(List<Guid> keys, long generation, Dictionary<Guid, IPublishedContent> resolved)
    {
        var pending = new List<Guid>(keys.Count);
        var idKeyPairs = new List<(int Id, Guid Key)>();

        foreach (Guid key in keys)
        {
            (bool exists, ContentCacheNode? node) = await _hybridCache.TryGetValueAsync<ContentCacheNode?>(GetCacheKey(key), CancellationToken.None);
            if (exists is false)
            {
                pending.Add(key);
                continue;
            }

            if (node is not null)
            {
                idKeyPairs.Add((node.Id, node.Key));
                ResolveNode(key, node, generation, resolved);
            }
        }

        // Mirrors GetNodeAsync, which warms the id/key map for every resolved node. Batched into one
        // call since PopulateCache takes a write lock per call.
        if (idKeyPairs.Count > 0)
        {
            _idKeyMap.PopulateCache(idKeyPairs, UmbracoObjectTypes.Media);
        }

        return pending;
    }

    // The single batched database read for whatever L0 and L1/L2 missed. Once resolved, a
    // database-read node is promoted into L1 — an L1/L2 hit is already there.
    private async Task ReadFromDatabaseAsync(List<Guid> keys, long generation, Dictionary<Guid, IPublishedContent> resolved)
    {
        IReadOnlyCollection<ContentCacheNode> coldNodes;
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            coldNodes = (await _databaseCacheRepository.GetMediaSourcesAsync(keys)).ToArray();
            scope.Complete();
        }

        // Mirrors GetNodeAsync, which warms the id/key map for every node the database returns.
        if (coldNodes.Count > 0)
        {
            _idKeyMap.PopulateCache(coldNodes.Select(node => (node.Id, node.Key)).ToArray(), UmbracoObjectTypes.Media);
        }

        foreach (ContentCacheNode node in coldNodes)
        {
            if (ResolveNode(node.Key, node, generation, resolved) && IsCacheGenerationCurrent(generation))
            {
                await _hybridCache.SetAsync(GetCacheKey(node.Key), node, GetEntryOptions(node.Key), GenerateTags(node));
            }
        }
    }

    // Converts a resolved cache node to IPublishedContent, writes it into the resolved set and — when
    // our snapshot is still current — populates L0. Returns whether conversion succeeded, so a caller
    // that also needs L1 (only the database read does) knows whether there's anything worth promoting.
    private bool ResolveNode(Guid key, ContentCacheNode node, long generation, Dictionary<Guid, IPublishedContent> resolved)
    {
        IPublishedContent? content = _publishedContentFactory.ToIPublishedMedia(node).CreateModel(_publishedModelFactory);
        if (content is null)
        {
            return false;
        }

        resolved[key] = content;

        if (IsCacheGenerationCurrent(generation))
        {
            _publishedContentCache.Set(key, content, ContentCacheNodeSizeEstimator.EstimateBytes(node));
        }

        return true;
    }

    /// <inheritdoc />
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

        // The node carries both identifiers, so the id/key map can be warmed without a lookup of its own.
        // Unlike the content itself the mapping is permanent, so this is done regardless of snapshotIsCurrent.
        // Deliberately outside the read-through branch above, so that backing store hits populate the map too.
        _idKeyMap.PopulateCache(contentCacheNode.Id, contentCacheNode.Key, UmbracoObjectTypes.Media);


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

    /// <inheritdoc />
    public async Task<bool> HasContentByIdAsync(int id)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Media);
        if (keyAttempt.Success is false)
        {
            return false;
        }

        return await _hybridCache.ExistsAsync<ContentCacheNode?>(GetCacheKey(keyAttempt.Result), CancellationToken.None);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task DeleteItemAsync(IContentBase media)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        await _databaseCacheRepository.DeleteContentItemAsync(media.Id);
        scope.Complete();
    }

    /// <inheritdoc />
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

            // Materialized because the repository defers deserialization of each node until it is enumerated,
            // and the sequence is walked more than once below.
            var cacheNodes = (await _databaseCacheRepository.GetMediaSourcesAsync(uncachedKeys)).ToList();

            scope.Complete();

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Media nodes to cache {NodeCount}", cacheNodes.Count);
            }

            // The seeded nodes carry both identifiers, so the id/key map is warmed without any lookups of its own.
            var idKeyPairs = new List<(int Id, Guid Key)>(cacheNodes.Count);

            foreach (ContentCacheNode cacheNode in cacheNodes)
            {
                await _hybridCache.SetAsync(
                    GetCacheKey(cacheNode.Key),
                    cacheNode,
                    GetSeedEntryOptions(),
                    GenerateTags(cacheNode),
                    cancellationToken: cancellationToken);

                idKeyPairs.Add((cacheNode.Id, cacheNode.Key));
            }

            _idKeyMap.PopulateCache(idKeyPairs, UmbracoObjectTypes.Media);
        }

#if DEBUG
        sw.Stop();
        _logger.LogInformation("Media cache seeding completed in {ElapsedMilliseconds} ms with {SeedCount} seed keys.", sw.ElapsedMilliseconds, SeedKeys.Count);
#else
        _logger.LogInformation("Media cache seeding completed with {SeedCount} seed keys.", SeedKeys.Count);
#endif
    }

    /// <inheritdoc />
    public async Task RefreshMemoryCacheAsync(Guid key)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task RemoveFromMemoryCacheAsync(Guid key)
        => await ClearPublishedCacheAsync(key);

    /// <inheritdoc />
    public async Task RebuildMemoryCacheByContentTypeAsync(IEnumerable<int> mediaTypeIds)
    {
        // Clear the hybrid cache by media type tag for the affected media types.
        var mediaTypeIdsAsArray = mediaTypeIds as int[] ?? mediaTypeIds.ToArray();
        var mediaTypeIdTags = mediaTypeIdsAsArray.Select(MediaTypeIdTag).ToArray();
        await _hybridCache.RemoveByTagAsync(mediaTypeIdTags);

        // Clear converted media for the affected types so entries are re-converted when next requested.
        ClearConvertedContentCache(mediaTypeIdsAsArray);
    }

    /// <inheritdoc />
    public void ClearConvertedContentCache()
    {
        _publishedContentCache.Clear();
        InvalidateMemoryCacheGeneration();
    }

    /// <inheritdoc />
    public void ClearConvertedContentCache(IReadOnlyCollection<int> mediaTypeIds)
    {
        var ids = mediaTypeIds as int[] ?? mediaTypeIds.ToArray();
        _publishedContentCache.RemoveWhere(content => ids.Contains(content.ContentType.Id));
        InvalidateMemoryCacheGeneration();
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        IEnumerable<ContentCacheNode> nodes = _databaseCacheRepository.GetContentByContentTypeKey([contentType.Key], ContentCacheDataSerializerEntityType.Media);
        scope.Complete();

        return nodes
            .Select(x => _publishedContentFactory.ToIPublishedContent(x, x.IsDraft).CreateModel(_publishedModelFactory))
            .WhereNotNull();
    }

    /// <summary>
    ///     Discards the memoized seed keys so that they are recalculated on the next seeding run.
    /// </summary>
    /// <remarks>
    ///     Internal for test purposes, so that media created after the keys were first resolved is seeded.
    /// </remarks>
    internal void ResetSeedKeys() => _seedKeys = null;

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
