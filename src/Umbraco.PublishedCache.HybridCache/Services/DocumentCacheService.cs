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
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache.Extensions;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
/// Caches published documents (content) across the converted-content (L0) cache, the HybridCache
/// (L1/L2) tier and the database, and handles seeding, refreshing and rebuilding those caches.
/// </summary>
internal sealed class DocumentCacheService : IDocumentCacheService, IMemoryCacheSizeReporter
{
    private readonly IDatabaseCacheRepository _databaseCacheRepository;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly Microsoft.Extensions.Caching.Hybrid.HybridCache _hybridCache;
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly ICacheNodeFactory _cacheNodeFactory;
    private readonly IEnumerable<IDocumentSeedKeyProvider> _seedKeyProviders;
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly IPreviewService _previewService;
    private readonly IDocumentPublishStatusQueryService _publishStatusQueryService;
    private readonly CacheSettings _cacheSettings;
    private readonly ILogger<DocumentCacheService> _logger;

    private readonly IConvertedPublishedContentCache<string, IPublishedContent> _publishedContentCache;

    // Monotonic counter bumped whenever the in-memory cache (L0/L1) is invalidated or refreshed.
    // GetNodeAsync captures it before reading the backing store and re-checks it before writing
    // back, so a snapshot read before a concurrent publish/refresh is never written over the
    // refreshed entry — preventing the stale-set clobber that otherwise persists until a full clear.
    //
    // Deliberately a single global counter, not per-key: any invalidation invalidates every in-flight
    // read-through. The only cost is an occasional skipped cache population when a read-through for one
    // key overlaps an unrelated publish — a re-miss on the next request, never stale data. A per-key
    // scheme would avoid that but needs a global epoch for bulk clears plus an exact per-key bump on
    // every mutated cache key, which is easy to get wrong and would silently reintroduce the clobber.
    // Global is correctness-robust; only revisit if read-through churn under heavy concurrent
    // publishing ever shows up in profiling.
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

            foreach (IDocumentSeedKeyProvider provider in _seedKeyProviders)
            {
                _seedKeys.UnionWith(provider.GetSeedKeys());
            }

            return _seedKeys;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentCacheService"/> class.
    /// </summary>
    /// <param name="databaseCacheRepository">The repository providing cached content from the database.</param>
    /// <param name="idKeyMap">The id/key map used to resolve integer identifiers to keys.</param>
    /// <param name="scopeProvider">The scope provider for database access.</param>
    /// <param name="hybridCache">The HybridCache (L1/L2) backing store for cache nodes.</param>
    /// <param name="publishedContentFactory">The factory that converts cache nodes to <see cref="IPublishedContent"/>.</param>
    /// <param name="cacheNodeFactory">The factory that builds cache nodes from content.</param>
    /// <param name="seedKeyProviders">The providers that supply the keys to seed on startup.</param>
    /// <param name="cacheSettings">The cache configuration options.</param>
    /// <param name="publishedModelFactory">The factory that creates strongly-typed published models.</param>
    /// <param name="previewService">The service that determines whether the current request is in preview.</param>
    /// <param name="publishStatusQueryService">The service used to check published state and ancestor paths.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cacheFactory">The factory that creates the in-memory converted-content (L0) cache.</param>
    public DocumentCacheService(
        IDatabaseCacheRepository databaseCacheRepository,
        IIdKeyMap idKeyMap,
        ICoreScopeProvider scopeProvider,
        Microsoft.Extensions.Caching.Hybrid.HybridCache hybridCache,
        IPublishedContentFactory publishedContentFactory,
        ICacheNodeFactory cacheNodeFactory,
        IEnumerable<IDocumentSeedKeyProvider> seedKeyProviders,
        IOptions<CacheSettings> cacheSettings,
        IPublishedModelFactory publishedModelFactory,
        IPreviewService previewService,
        IDocumentPublishStatusQueryService publishStatusQueryService,
        ILogger<DocumentCacheService> logger,
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
        _previewService = previewService;
        _publishStatusQueryService = publishStatusQueryService;
        _cacheSettings = cacheSettings.Value;
        _logger = logger;
        _publishedContentCache = cacheFactory.Create<string, IPublishedContent>(_cacheSettings.Entry.Document.MaximumLocalCacheItems, CacheName);
    }

    /// <inheritdoc />
    public string CacheName => "Published content (converted, L0)";

    /// <inheritdoc />
    public long GetApproximateCount() => _publishedContentCache.Count;

    /// <inheritdoc />
    public long? GetApproximateBytes() => _publishedContentCache.ApproximateSizeInBytes;

    /// <inheritdoc />
    public async Task<IPublishedContent?> GetByKeyAsync(Guid key, bool? preview = null)
    {
        bool calculatedPreview = preview ?? GetPreview();

        return await GetNodeAsync(key, calculatedPreview);
    }

    /// <inheritdoc />
    public async Task<IPublishedContent?> GetByIdAsync(int id, bool? preview = null)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document);
        if (keyAttempt.Success is false)
        {
            return null;
        }

        bool calculatedPreview = preview ?? GetPreview();
        Guid key = keyAttempt.Result;

        return await GetNodeAsync(key, calculatedPreview);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IPublishedContent>> GetByKeysAsync(IReadOnlyCollection<Guid> keys, bool? preview = null)
    {
        bool calculatedPreview = preview ?? GetPreview();

        // Capture the generation once before any backing-store read so a concurrent publish/refresh
        // landing mid-fetch makes us skip the write-back rather than clobber fresher entries — applied
        // once for the whole set, and more conservatively than GetNodeAsync (which exempts HybridCache
        // hits from the guard, applying it only to the database read).
        var generation = Interlocked.Read(ref _cacheGeneration);

        var resolved = new Dictionary<Guid, IPublishedContent>(keys.Count);

        // Three tiers, each handed only the keys the previous one could not account for: the synchronous
        // L0 probe, the L1/L2 probe, then a single batched database read.
        List<Guid> pending = ProbeConvertedCache(keys, calculatedPreview, resolved);
        if (pending.Count > 0)
        {
            pending = await ProbeHybridCacheAsync(pending, calculatedPreview, generation, resolved);
        }

        if (pending.Count > 0)
        {
            await ReadFromDatabaseAsync(pending, calculatedPreview, generation, resolved);
        }

        // Return in input order; keys that resolved to nothing (missing/unpublished) are omitted. A key
        // repeated in the input resolves to the same item at every occurrence, having only been looked
        // up once — so the input count, not the resolved count, is the upper bound on the result.
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

    // L0 (converted) fast path — published only, mirroring GetNodeAsync (via the shared TryGetCached).
    private List<Guid> ProbeConvertedCache(IReadOnlyCollection<Guid> keys, bool preview, Dictionary<Guid, IPublishedContent> resolved)
    {
        var pending = new List<Guid>(keys.Count);
        var seen = new HashSet<Guid>(keys.Count);

        foreach (Guid key in keys)
        {
            if (seen.Add(key) is false)
            {
                continue;
            }

            if (TryGetCached(key, preview, out IPublishedContent? cached) && cached is not null)
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
    // node is a cached "this key resolves to nothing", which GetNodeAsync writes deliberately, so it
    // accounts for its key and is not passed on to the database read — re-reading such a key on every
    // request is the regression reported in #18869. Keys are probed one at a time, and the probe is not
    // free even on a hit: TryGetValueAsync takes a per-key lock and goes through GetOrCreateAsync, which
    // on a miss creates and then removes an entry. With a distributed L2 (e.g. Redis) configured that is
    // a serial round-trip per key, plus a write and a delete for each miss.
    private async Task<List<Guid>> ProbeHybridCacheAsync(List<Guid> keys, bool preview, long generation, Dictionary<Guid, IPublishedContent> resolved)
    {
        var pending = new List<Guid>(keys.Count);
        var idKeyPairs = new List<(int Id, Guid Key)>();

        foreach (Guid key in keys)
        {
            var cacheKey = GetCacheKey(key, preview);
            (bool exists, ContentCacheNode? node) = await _hybridCache.TryGetValueAsync<ContentCacheNode?>(cacheKey, CancellationToken.None);
            if (exists is false)
            {
                pending.Add(key);
                continue;
            }

            if (node is not null)
            {
                idKeyPairs.Add((node.Id, node.Key));
                ResolveNode(key, node, preview, generation, resolved);
            }
        }

        // Mirrors GetNodeAsync, which warms the id/key map for every resolved node. Batched into one
        // call since PopulateCache takes a write lock per call.
        if (idKeyPairs.Count > 0)
        {
            _idKeyMap.PopulateCache(idKeyPairs, UmbracoObjectTypes.Document);
        }

        return pending;
    }

    // The single batched database read for whatever L0 and L1/L2 missed. A database-read node
    // additionally gets the published-ancestor guard applied (mirroring GetNodeAsync's read-through
    // result) and, once resolved, is promoted into L1 — an L1/L2 hit is already there.
    private async Task ReadFromDatabaseAsync(List<Guid> keys, bool preview, long generation, Dictionary<Guid, IPublishedContent> resolved)
    {
        IReadOnlyCollection<ContentCacheNode> coldNodes;
        using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
        {
            coldNodes = (await _databaseCacheRepository.GetDocumentSourcesAsync(keys, preview)).ToArray();
        }

        // Mirrors GetNodeAsync, which warms the id/key map for every node the database returns,
        // regardless of the ancestor guard below.
        if (coldNodes.Count > 0)
        {
            _idKeyMap.PopulateCache(coldNodes.Select(node => (node.Id, node.Key)).ToArray(), UmbracoObjectTypes.Document);
        }

        foreach (ContentCacheNode node in coldNodes)
        {
            if (preview is false && _publishStatusQueryService.HasPublishedAncestorPath(node.Key) is false)
            {
                continue;
            }

            if (ResolveNode(node.Key, node, preview, generation, resolved)
                && preview is false && IsCacheGenerationCurrent(generation))
            {
                await _hybridCache.SetAsync(GetCacheKey(node.Key, preview), node, GetEntryOptions(node.Key, preview), GenerateTags(node));
            }
        }
    }

    // Converts a resolved cache node to IPublishedContent, writes it into the resolved set and — when
    // our snapshot is still current — populates L0. Returns whether conversion succeeded, so a caller
    // that also needs L1 (only the database read does) knows whether there's anything worth promoting.
    private bool ResolveNode(Guid key, ContentCacheNode node, bool preview, long generation, Dictionary<Guid, IPublishedContent> resolved)
    {
        IPublishedContent? content = _publishedContentFactory.ToIPublishedContent(node, preview).CreateModel(_publishedModelFactory);
        if (content is null)
        {
            return false;
        }

        resolved[key] = content;

        if (preview is false && IsCacheGenerationCurrent(generation))
        {
            _publishedContentCache.Set(GetCacheKey(key, preview), content, ContentCacheNodeSizeEstimator.EstimateBytes(node));
        }

        return true;
    }

    /// <inheritdoc />
    public bool TryGetCached(Guid key, bool preview, out IPublishedContent? content)
    {
        // Mirror the L0 (published content cache) fast path in GetNodeAsync.
        if (preview is false && _publishedContentCache.TryGet(GetCacheKey(key, preview), out content))
        {
            return true;
        }

        content = null;
        return false;
    }

    private async Task<IPublishedContent?> GetNodeAsync(Guid key, bool preview)
    {
        var cacheKey = GetCacheKey(key, preview);

        if (preview is false && _publishedContentCache.TryGet(cacheKey, out IPublishedContent? cached))
        {
            return cached;
        }

        (bool exists, ContentCacheNode? contentCacheNode) = await _hybridCache.TryGetValueAsync<ContentCacheNode?>(cacheKey, CancellationToken.None);

        // A value found in the backing store is already current, so it can always populate the caches
        // below; only a value built from the read-through DB fetch needs the generation guard.
        bool snapshotIsCurrent = true;
        if (exists is false)
        {
            // Capture the cache generation before reading the backing store. If a concurrent publish or
            // invalidation bumps the generation while we read and build below, the snapshot we hold is
            // stale and must not be written back over the refreshed entries (the clobber that leaves
            // memory permanently stale until a full clear).
            long generation = Interlocked.Read(ref _cacheGeneration);

            bool ancestorCheckFailed;
            (contentCacheNode, ancestorCheckFailed) = await GetContentCacheNodeFromRepo();

            snapshotIsCurrent = IsCacheGenerationCurrent(generation);

            // Only cache the result if the ancestor check didn't fail.
            // When content exists in DB but the ancestor check fails, this could be a transient
            // race condition during cache rebuild. Caching null would poison the distributed cache.
            // Skip the write when the generation moved — a refresh has superseded this snapshot.
            if (ancestorCheckFailed is false && snapshotIsCurrent)
            {
                await _hybridCache.SetAsync(
                    cacheKey,
                    contentCacheNode,
                    GetEntryOptions(key, preview),
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
        _idKeyMap.PopulateCache(contentCacheNode.Id, contentCacheNode.Key, UmbracoObjectTypes.Document);


        IPublishedContent? result = _publishedContentFactory.ToIPublishedContent(contentCacheNode, preview).CreateModel(_publishedModelFactory);

        // Only published content is stored in L0: the read fast path above is guarded by preview is false, so a
        // draft entry would never be served back, and draft keys have no per-key invalidation (RemoveFromMemoryCacheAsync
        // only removes the published key) so they would linger until a full clear. In bounded mode they would also
        // waste eviction slots and dilute the W-TinyLFU frequency signal.
        //
        // Only populate when our snapshot is still current; otherwise a concurrent refresh has already written
        // fresher content and we must not overwrite it with this stale one (the clobber that leaves L0 stale
        // until a full clear).
        if (result is not null && preview is false && snapshotIsCurrent)
        {
            // The size estimate runs unconditionally (not only when reporting is enabled): it is cheap
            // (O(properties), no IO/decompression) and only on the cache-miss path, and keeping the running
            // total always-current means it is accurate the moment debug reporting is switched on.
            _publishedContentCache.Set(cacheKey, result, ContentCacheNodeSizeEstimator.EstimateBytes(contentCacheNode));
        }

        return result;

        async Task<(ContentCacheNode? Node, bool AncestorCheckFailed)> GetContentCacheNodeFromRepo()
        {
            using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
            ContentCacheNode? contentCacheNode = await _databaseCacheRepository.GetDocumentSourceAsync(key, preview);

            // If we can resolve the content cache node, we still need to check if the ancestor path is published.
            // This does cost some performance, but it's necessary to ensure that the content is actually published.
            // When unpublishing a node, a payload with RefreshBranch is published, so we don't have to worry about this.
            // Similarly, when a branch is published, next time the content is requested, the parent will be published.
            // Null values are cached here are tagged and cleared by ClearMemoryCacheAsync, so the next request after a
            // cache clear will re-query the database.
            if (preview is false && contentCacheNode is not null && _publishStatusQueryService.HasPublishedAncestorPath(contentCacheNode.Key) is false)
            {
                // Content exists in the DB but the ancestor path is not published. Return null but
                // signal to the caller that this should NOT be cached — the ancestor check may be
                // transiently wrong during a cache rebuild.
                return (null, true);
            }

            return (contentCacheNode, false);
        }
    }

    private bool GetPreview() => _previewService.IsInPreview();

    // Bumped after every in-memory cache invalidation/refresh so in-flight read-through snapshots
    // (see GetNodeAsync) can detect they have been superseded and skip writing back stale content.
    private void InvalidateMemoryCacheGeneration() => Interlocked.Increment(ref _cacheGeneration);

    private bool IsCacheGenerationCurrent(long capturedGeneration)
        => Interlocked.Read(ref _cacheGeneration) == capturedGeneration;

    /// <inheritdoc />
    public IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        IEnumerable<ContentCacheNode> nodes = _databaseCacheRepository.GetContentByContentTypeKey([contentType.Key], ContentCacheDataSerializerEntityType.Document);
        scope.Complete();

        return nodes
            .Select(x => _publishedContentFactory.ToIPublishedContent(x, x.IsDraft).CreateModel(_publishedModelFactory))
            .WhereNotNull();
    }

    /// <inheritdoc />
    public async Task ClearMemoryCacheAsync(CancellationToken cancellationToken)
    {
        // Bump first so any read-through that read the backing store before this clear is rejected
        // when it tries to write back, even while the reseed below is still running.
        InvalidateMemoryCacheGeneration();

        _publishedContentCache.Clear();
        await _hybridCache.RemoveByTagAsync(Constants.Cache.Tags.Content, cancellationToken);

        // We have to run seeding again after the cache is cleared
        await SeedAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RefreshMemoryCacheAsync(Guid key)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.ContentTree);

        (ContentCacheNode? draftNode, ContentCacheNode? publishedNode) = await _databaseCacheRepository.GetDocumentSourceForPublishStatesAsync(key);

        if (draftNode is not null)
        {
            await _hybridCache.SetAsync(GetCacheKey(draftNode.Key, true), draftNode, GetEntryOptions(draftNode.Key, true), GenerateTags(draftNode));
        }
        else
        {
            // No draft in the database cache — remove any stale draft entry from the local memory cache.
            await _hybridCache.RemoveAsync(GetCacheKey(key, true));
        }

        if (publishedNode is not null && _publishStatusQueryService.HasPublishedAncestorPath(publishedNode.Key))
        {
            var cacheKey = GetCacheKey(publishedNode.Key, false);
            await _hybridCache.SetAsync(cacheKey, publishedNode, GetEntryOptions(publishedNode.Key, false), GenerateTags(publishedNode));
            _publishedContentCache.Remove(cacheKey);
            InvalidateMemoryCacheGeneration();
        }
        else
        {
            // Either no published node in the database cache, or the ancestor path is no longer published —
            // remove any stale published entry from the local memory cache. ClearPublishedCacheAsync
            // bumps the generation itself, so this path is already covered.
            await ClearPublishedCacheAsync(key);
        }

        scope.Complete();
    }

    /// <inheritdoc />
    public async Task RemoveFromMemoryCacheAsync(Guid key)
    {
        await _hybridCache.RemoveAsync(GetCacheKey(key, true));
        await ClearPublishedCacheAsync(key);
    }

    /// <inheritdoc />
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
#if DEBUG
        var sw = new Stopwatch();
        sw.Start();
#endif

        foreach (IEnumerable<Guid> group in SeedKeys.InGroupsOf(_cacheSettings.DocumentSeedBatchSize))
        {
            var uncachedKeys = new HashSet<Guid>();
            foreach (Guid key in group)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var cacheKey = GetCacheKey(key, false);

                var existsInCache = await _hybridCache.ExistsAsync<ContentCacheNode?>(cacheKey, cancellationToken).ConfigureAwait(false);
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
            var cacheNodes = (await _databaseCacheRepository.GetDocumentSourcesAsync(uncachedKeys)).ToList();

            scope.Complete();

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Document nodes to cache {NodeCount}", cacheNodes.Count);
            }

            // The seeded nodes carry both identifiers, so the id/key map is warmed without any lookups of its own.
            var idKeyPairs = new List<(int Id, Guid Key)>(cacheNodes.Count);

            foreach (ContentCacheNode cacheNode in cacheNodes)
            {
                var cacheKey = GetCacheKey(cacheNode.Key, false);
                await _hybridCache.SetAsync(
                    cacheKey,
                    cacheNode,
                    GetSeedEntryOptions(),
                    GenerateTags(cacheNode),
                    cancellationToken: cancellationToken);

                idKeyPairs.Add((cacheNode.Id, cacheNode.Key));
            }

            _idKeyMap.PopulateCache(idKeyPairs, UmbracoObjectTypes.Document);
        }

#if DEBUG
        sw.Stop();
        _logger.LogInformation("Document cache seeding completed in {ElapsedMilliseconds} ms with {SeedCount} seed keys.", sw.ElapsedMilliseconds, SeedKeys.Count);
#else
        _logger.LogInformation("Document cache seeding completed with {SeedCount} seed keys.", SeedKeys.Count);
#endif
    }

    /// <summary>
    /// Resets the cached seed keys so they are recomputed on next access. Internal for test purposes.
    /// </summary>
    internal void ResetSeedKeys() => _seedKeys = null;

    private HybridCacheEntryOptions GetSeedEntryOptions() => new()
    {
        Expiration = _cacheSettings.Entry.Document.SeedCacheDuration,
        LocalCacheExpiration = _cacheSettings.Entry.Document.SeedCacheDuration
    };

    private HybridCacheEntryOptions GetEntryOptions(Guid key, bool preview)
    {
        if (SeedKeys.Contains(key) && preview is false)
        {
            return GetSeedEntryOptions();
        }

        return new HybridCacheEntryOptions
        {
            Expiration = _cacheSettings.Entry.Document.RemoteCacheDuration,
            LocalCacheExpiration = _cacheSettings.Entry.Document.LocalCacheDuration,
        };
    }

    /// <inheritdoc />
    public async Task<bool> HasContentByIdAsync(int id, bool preview = false)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document);
        if (keyAttempt.Success is false)
        {
            return false;
        }

        return await _hybridCache.ExistsAsync<ContentCacheNode?>(GetCacheKey(keyAttempt.Result, preview), CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task RefreshContentAsync(IContent content)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        if (content.Trashed)
        {
            await _databaseCacheRepository.DeleteContentItemAsync(content.Id);
            await RemoveFromMemoryCacheAsync(content.Key);
            scope.Complete();
            return;
        }

        // Always set draft node
        // We have nodes seperate in the cache, cause 99% of the time, you are only using one
        // and thus we won't get too much data when retrieving from the cache.
        var draftCacheNode = _cacheNodeFactory.ToContentCacheNode(content, true);
        await _databaseCacheRepository.RefreshDocumentAsync(draftCacheNode);


        if (content.PublishedState is PublishedState.Publishing)
        {
            var publishedCacheNode = _cacheNodeFactory.ToContentCacheNode(content, false);
            await _databaseCacheRepository.RefreshDocumentAsync(publishedCacheNode);
        }
        else if (content.PublishedState is PublishedState.Unpublishing)
        {
            await _databaseCacheRepository.RemovePublishedDocumentAsync(content.Id);
            await ClearPublishedCacheAsync(content.Key);
        }

        scope.Complete();
    }

    private static string GetCacheKey(Guid key, bool preview) => preview ? $"{key}+draft" : $"{key}";

    /// <summary>
    /// Generates the cache tags for a given <see cref="ContentCacheNode"/>.
    /// </summary>
    /// <param name="cacheNode">The cache node to generate tags for, or <c>null</c> for a negative-cache entry.</param>
    /// <returns>
    /// A set of tags that always includes <see cref="Constants.Cache.Tags.Content"/>.
    /// When <paramref name="cacheNode"/> is non-null, the content type ID tag is also included.
    /// </returns>
    /// <remarks>
    /// Tags are used to clear all cache entries related to a given content item or type.
    /// The <see cref="Constants.Cache.Tags.Content"/> tag is always included — even for null entries — so
    /// that <see cref="ClearMemoryCacheAsync"/> (which clears by this tag) can evict negative-cache entries.
    /// Without this, null entries survive tag-based cache clears and become permanently stale.
    /// Tags currently cover content/media distinctions but can be expanded with draft/published later.
    /// </remarks>
    private static HashSet<string> GenerateTags(ContentCacheNode? cacheNode) =>
        cacheNode is null
            ? [Constants.Cache.Tags.Content]
            : [Constants.Cache.Tags.Content, ContentTypeIdTag(cacheNode.ContentTypeId)];

    /// <inheritdoc />
    public async Task DeleteItemAsync(IContentBase content)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        await _databaseCacheRepository.DeleteContentItemAsync(content.Id);
        scope.Complete();
    }

    /// <inheritdoc />
    public void Rebuild(IReadOnlyCollection<int> contentTypeIds)
        => _databaseCacheRepository.Rebuild(
            contentTypeIds.ToList(),
            null,
            null,
            action =>
            {
                using ICoreScope scope = _scopeProvider.CreateCoreScope();
                action();
                scope.Complete();
            });

    /// <inheritdoc />
    public async Task RebuildMemoryCacheByContentTypeAsync(IEnumerable<int> contentTypeIds)
    {
        // Clear the hybrid cache by content type tag for the affected content types.
        var contentTypeIdsAsArray = contentTypeIds as int[] ?? contentTypeIds.ToArray();
        var contentTypeIdTags = contentTypeIdsAsArray.Select(ContentTypeIdTag).ToArray();
        await _hybridCache.RemoveByTagAsync(contentTypeIdTags);

        // Clear converted content for the affected types so entries are re-converted when next requested.
        ClearConvertedContentCache(contentTypeIdsAsArray);
    }

    /// <inheritdoc />
    public void ClearConvertedContentCache()
    {
        _publishedContentCache.Clear();
        InvalidateMemoryCacheGeneration();
    }

    /// <inheritdoc />
    public void ClearConvertedContentCache(IReadOnlyCollection<int> contentTypeIds)
    {
        var ids = contentTypeIds as int[] ?? contentTypeIds.ToArray();
        _publishedContentCache.RemoveWhere(content => ids.Contains(content.ContentType.Id));
        InvalidateMemoryCacheGeneration();
    }

    private async Task ClearPublishedCacheAsync(Guid key)
    {
        var cacheKey = GetCacheKey(key, false);
        await _hybridCache.RemoveAsync(cacheKey);
        _publishedContentCache.Remove(cacheKey);
        InvalidateMemoryCacheGeneration();
    }

    private static string ContentTypeIdTag(int contentTypeId)
        => $"ct:{contentTypeId}";
}
