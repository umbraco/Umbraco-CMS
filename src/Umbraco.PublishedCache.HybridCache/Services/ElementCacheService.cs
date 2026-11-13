using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Extensions;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal sealed class ElementCacheService : IElementCacheService, IMemoryCacheSizeReporter
{
    private readonly IDatabaseCacheRepository _databaseCacheRepository;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly Microsoft.Extensions.Caching.Hybrid.HybridCache _hybridCache;
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly ICacheNodeFactory _cacheNodeFactory;
    private readonly IEnumerable<IElementSeedKeyProvider> _seedKeyProviders;
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly IPreviewService _previewService;
    private readonly IPreviewAccessEvaluator _previewAccessEvaluator;
    private readonly CacheSettings _cacheSettings;
    private readonly ILogger<ElementCacheService> _logger;
    private readonly ElementSourceReader _readElementSourceAsync;
    private readonly ElementSourceReader _readElementSource;
    private readonly ElementSourcesReader _readElementSourcesAsync;
    private readonly ElementSourcesReader _readElementSources;
    private readonly KeyedLock<Guid> _refreshLocks = new();
    private HashSet<Guid>? _seedKeys;

    private readonly IConvertedPublishedContentCache<string, IPublishedElement> _publishedElementCache;

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

    private HashSet<Guid> SeedKeys
    {
        get
        {
            if (_seedKeys is not null)
            {
                return _seedKeys;
            }

            _seedKeys = [];

            foreach (IElementSeedKeyProvider provider in _seedKeyProviders)
            {
                _seedKeys.UnionWith(provider.GetSeedKeys());
            }

            return _seedKeys;
        }
    }

    public ElementCacheService(
        IDatabaseCacheRepository databaseCacheRepository,
        ICoreScopeProvider scopeProvider,
        Microsoft.Extensions.Caching.Hybrid.HybridCache hybridCache,
        IPublishedContentFactory publishedContentFactory,
        ICacheNodeFactory cacheNodeFactory,
        IEnumerable<IElementSeedKeyProvider> seedKeyProviders,
        IPublishedModelFactory publishedModelFactory,
        IPreviewService previewService,
        IOptions<CacheSettings> cacheSettings,
        ILogger<ElementCacheService> logger,
        IConvertedPublishedContentCacheFactory cacheFactory,
        IPreviewAccessEvaluator previewAccessEvaluator)
    {
        _databaseCacheRepository = databaseCacheRepository;
        _scopeProvider = scopeProvider;
        _hybridCache = hybridCache;
        _publishedContentFactory = publishedContentFactory;
        _cacheNodeFactory = cacheNodeFactory;
        _seedKeyProviders = seedKeyProviders;
        _publishedModelFactory = publishedModelFactory;
        _previewService = previewService;
        _previewAccessEvaluator = previewAccessEvaluator;
        _cacheSettings = cacheSettings.Value;
        _logger = logger;
        _publishedElementCache = cacheFactory.Create<string, IPublishedElement>(_cacheSettings.Entry.Element.MaximumLocalCacheItems, CacheName);

        _readElementSourceAsync = _databaseCacheRepository.GetElementSourceAsync;
        _readElementSource = (key, preview) => Task.FromResult(_databaseCacheRepository.GetElementSource(key, preview));
        _readElementSourcesAsync = _databaseCacheRepository.GetElementSourcesAsync;
        _readElementSources = (keys, preview) => Task.FromResult(_databaseCacheRepository.GetElementSources(keys, preview));
    }

    // Reads element sources from the database. The synchronous members pass a reader that reads synchronously and
    // returns a completed task, so the shared read logic only awaits what HybridCache itself has to wait for.
    private delegate Task<ContentCacheNode?> ElementSourceReader(Guid key, bool preview);

    private delegate Task<IEnumerable<ContentCacheNode>> ElementSourcesReader(IEnumerable<Guid> keys, bool preview);

    /// <inheritdoc />
    public string CacheName => "Published elements (converted, L0)";

    /// <inheritdoc />
    public long GetApproximateCount() => _publishedElementCache.Count;

    /// <inheritdoc />
    public long? GetApproximateBytes() => _publishedElementCache.ApproximateSizeInBytes;

    // The synchronous member returns an L0 hit directly, and otherwise runs the same read logic as the asynchronous
    // one with a synchronous database reader. The returned task has then already completed, unless HybridCache
    // itself had to wait (an L2 round trip, or a concurrent read of the same key), so synchronous callers do not
    // block on asynchronous I/O.

    /// <inheritdoc />
    public Task<IPublishedElement?> GetByKeyAsync(Guid key, bool? preview = null)
        => GetAccessibleNodeAsync(key, preview ?? _previewService.IsInPreview(), _readElementSourceAsync);

    /// <inheritdoc />
    public IPublishedElement? GetByKey(Guid key, bool? preview = null)
    {
        bool calculatedPreview = preview ?? _previewService.IsInPreview();
        return TryGetCached(key, calculatedPreview, out IPublishedElement? cached)
            ? cached
            : GetByKeyWithSynchronousReadsAsync(key, calculatedPreview).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Runs the read logic behind <see cref="GetByKey"/>, reading the database synchronously on a cache miss.
    /// </summary>
    /// <remarks>
    /// <see cref="GetByKey"/> blocks on the returned task, which only avoids sync-over-async if the task has already
    /// completed when it is returned. That holds only while every await in the shared read logic completes
    /// synchronously, so it is kept as a separate method for tests to assert on directly; the public member can't
    /// show whether it blocked. It is internal because it exists only to verify that guarantee, not as an API.
    /// </remarks>
    internal Task<IPublishedElement?> GetByKeyWithSynchronousReadsAsync(Guid key, bool preview)
        => GetAccessibleNodeAsync(key, preview, _readElementSource);

    private async Task<IPublishedElement?> GetAccessibleNodeAsync(Guid key, bool preview, ElementSourceReader readElementSource)
    {
        IPublishedElement? element = await GetNodeAsync(key, preview, readElementSource);

        // As for documents (see DocumentCacheService), a draft is only handed out when the preview user may browse
        // the element; otherwise it resolves as it would outside preview: its published version, or nothing.
        return preview && element is not null && _previewAccessEvaluator.CanPreviewElement(element) is false
            ? await GetNodeAsync(key, false, readElementSource)
            : element;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<IPublishedElement>> GetByKeysAsync(IReadOnlyCollection<Guid> keys, bool? preview = null)
        => GetByKeysCoreAsync(keys, preview ?? _previewService.IsInPreview(), _readElementSourcesAsync);

    /// <inheritdoc />
    public IReadOnlyList<IPublishedElement> GetByKeys(IReadOnlyCollection<Guid> keys, bool? preview = null)
        => GetByKeysWithSynchronousReadsAsync(keys, preview ?? _previewService.IsInPreview()).GetAwaiter().GetResult();

    /// <summary>
    /// Runs the read logic behind <see cref="GetByKeys"/>, reading the database synchronously on a cache miss.
    /// </summary>
    /// <remarks>See <see cref="GetByKeyWithSynchronousReadsAsync"/>.</remarks>
    internal Task<IReadOnlyList<IPublishedElement>> GetByKeysWithSynchronousReadsAsync(IReadOnlyCollection<Guid> keys, bool preview)
        => GetByKeysCoreAsync(keys, preview, _readElementSources);

    // The batch equivalent of GetAccessibleNodeAsync: it resolves to what the same keys would one at a time, but the
    // keys that miss both caches share one database read.
    private async Task<IReadOnlyList<IPublishedElement>> GetByKeysCoreAsync(IReadOnlyCollection<Guid> keys, bool preview, ElementSourcesReader readElementSources)
    {
        // Captured once before any backing-store read, so a refresh landing mid-fetch makes the whole set skip its
        // write-back rather than overwrite fresher entries (see GetNodeAsync).
        long generation = Interlocked.Read(ref _cacheGeneration);

        var resolved = new Dictionary<Guid, IPublishedElement>(keys.Count);

        // Each tier is handed only the keys the previous one could not account for: the L0 probe, the L1/L2 probe,
        // then a single batched database read.
        List<Guid> pending = ProbeConvertedCache(keys, preview, resolved);
        if (pending.Count > 0)
        {
            pending = await ProbeHybridCacheAsync(pending, preview, generation, resolved);
        }

        if (pending.Count > 0)
        {
            await ReadFromDatabaseAsync(pending, preview, generation, resolved, readElementSources);
        }

        if (preview)
        {
            await ReplaceInaccessibleDraftsAsync(resolved, readElementSources);
        }

        var ordered = new List<IPublishedElement>(keys.Count);
        foreach (Guid key in keys)
        {
            if (resolved.TryGetValue(key, out IPublishedElement? element))
            {
                ordered.Add(element);
            }
        }

        return ordered;
    }

    // Replaces the drafts the preview user may not browse with their published versions; see GetAccessibleNodeAsync.
    private async Task ReplaceInaccessibleDraftsAsync(Dictionary<Guid, IPublishedElement> resolved, ElementSourcesReader readElementSources)
    {
        Guid[] inaccessibleKeys = resolved
            .Where(entry => _previewAccessEvaluator.CanPreviewElement(entry.Value) is false)
            .Select(entry => entry.Key)
            .ToArray();
        if (inaccessibleKeys.Length == 0)
        {
            return;
        }

        foreach (Guid key in inaccessibleKeys)
        {
            resolved.Remove(key);
        }

        foreach (IPublishedElement element in await GetByKeysCoreAsync(inaccessibleKeys, false, readElementSources))
        {
            resolved[element.Key] = element;
        }
    }

    private List<Guid> ProbeConvertedCache(IReadOnlyCollection<Guid> keys, bool preview, Dictionary<Guid, IPublishedElement> resolved)
    {
        var pending = new List<Guid>(keys.Count);
        var seen = new HashSet<Guid>(keys.Count);

        foreach (Guid key in keys)
        {
            if (seen.Add(key) is false)
            {
                continue;
            }

            if (TryGetCached(key, preview, out IPublishedElement? cached) && cached is not null)
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

    // An entry holding a null node is a cached "this key resolves to nothing", written deliberately by GetNodeAsync,
    // so it accounts for its key and is not passed on to the database read.
    private async Task<List<Guid>> ProbeHybridCacheAsync(List<Guid> keys, bool preview, long generation, Dictionary<Guid, IPublishedElement> resolved)
    {
        var pending = new List<Guid>(keys.Count);

        foreach (Guid key in keys)
        {
            (bool exists, ContentCacheNode? node) = await _hybridCache.TryGetValueAsync<ContentCacheNode?>(GetCacheKey(key, preview), CancellationToken.None);
            if (exists is false)
            {
                pending.Add(key);
            }
            else if (node is not null)
            {
                ResolveNode(key, node, preview, generation, resolved);
            }
        }

        return pending;
    }

    private async Task ReadFromDatabaseAsync(List<Guid> keys, bool preview, long generation, Dictionary<Guid, IPublishedElement> resolved, ElementSourcesReader readElementSources)
    {
        IReadOnlyCollection<ContentCacheNode> nodes = (await readElementSources(keys, preview)).ToArray();

        foreach (ContentCacheNode node in nodes)
        {
            ResolveNode(node.Key, node, preview, generation, resolved);
        }

        if (IsCacheGenerationCurrent(generation) is false)
        {
            return;
        }

        // As GetNodeAsync does for a single key, promote what the database returned into L1, and cache a null for the
        // keys it had nothing for so they are not re-read on every request.
        foreach (ContentCacheNode node in nodes)
        {
            await _hybridCache.SetAsync(GetCacheKey(node.Key, preview), node, GetEntryOptions(node.Key, preview), GenerateTags(node));
        }

        var returnedKeys = nodes.Select(node => node.Key).ToHashSet();
        foreach (Guid key in keys.Where(key => returnedKeys.Contains(key) is false))
        {
            await _hybridCache.SetAsync(GetCacheKey(key, preview), (ContentCacheNode?)null, GetEntryOptions(key, preview), GenerateTags(null));
        }
    }

    // Converts a cache node, adds it to the resolved set and, when the snapshot is still current, populates L0
    // (published only, as in GetNodeAsync).
    private void ResolveNode(Guid key, ContentCacheNode node, bool preview, long generation, Dictionary<Guid, IPublishedElement> resolved)
    {
        IPublishedElement? element = _publishedContentFactory.ToIPublishedElement(node, preview)?.CreateModel(_publishedModelFactory);
        if (element is null)
        {
            return;
        }

        resolved[key] = element;

        if (preview is false && IsCacheGenerationCurrent(generation))
        {
            _publishedElementCache.Set(GetCacheKey(key, preview), element, ContentCacheNodeSizeEstimator.EstimateBytes(node));
        }
    }

    public bool TryGetCached(Guid key, bool preview, out IPublishedElement? element)
    {
        // Mirror the L0 (published element cache) fast path in GetNodeAsync.
        if (preview is false && _publishedElementCache.TryGet(GetCacheKey(key, preview), out element))
        {
            return true;
        }

        element = null;
        return false;
    }

    private async Task<IPublishedElement?> GetNodeAsync(Guid key, bool preview, ElementSourceReader readElementSource)
    {
        var cacheKey = GetCacheKey(key, preview);

        if (preview is false && _publishedElementCache.TryGet(cacheKey, out IPublishedElement? cached))
        {
            return cached;
        }

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

            // Skip the write when the generation moved — a refresh has superseded this snapshot. A null
            // node is still written when current (negative caching, tagged so it can be cleared).
            if (snapshotIsCurrent)
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

        IPublishedElement? result = _publishedContentFactory.ToIPublishedElement(contentCacheNode, preview)?.CreateModel(_publishedModelFactory);

        // Only published elements are stored in L0: the read fast path above is guarded by preview is false, so a
        // draft entry would never be served back, and draft keys have no per-key invalidation (RemoveFromMemoryCacheAsync
        // only removes the published key) so they would linger until a full clear. In bounded mode they would also
        // waste eviction slots and dilute the W-TinyLFU frequency signal.
        //
        // Only populate when our snapshot is still current; otherwise a concurrent refresh has already written
        // fresher content and we must not overwrite it with this stale one (the clobber that leaves L0 stale
        // until a full clear).
        if (result is not null && preview is false && snapshotIsCurrent)
        {
            // The size estimate is cheap (O(properties), no IO/decompression) and only runs on the
            // cache-miss path, so keeping the running total always-current means it is accurate the
            // moment debug reporting is switched on.
            _publishedElementCache.Set(cacheKey, result, ContentCacheNodeSizeEstimator.EstimateBytes(contentCacheNode));
        }

        return result;

        async Task<ContentCacheNode?> GetContentCacheNodeFromRepo()
        {
            return await readElementSource(key, preview);
        }
    }

    // Bumped after every in-memory cache invalidation/refresh so in-flight read-through snapshots
    // (see GetNodeAsync) can detect they have been superseded and skip writing back stale content.
    private void InvalidateMemoryCacheGeneration() => Interlocked.Increment(ref _cacheGeneration);

    private bool IsCacheGenerationCurrent(long capturedGeneration)
        => Interlocked.Read(ref _cacheGeneration) == capturedGeneration;

    public IEnumerable<IPublishedElement> GetByContentType(IPublishedContentType contentType)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        IEnumerable<ContentCacheNode> nodes = _databaseCacheRepository.GetContentByContentTypeKey([contentType.Key], ContentCacheDataSerializerEntityType.Element);
        scope.Complete();

        return nodes
            .Select(x => _publishedContentFactory.ToIPublishedElement(x, x.IsDraft)?.CreateModel(_publishedModelFactory))
            .WhereNotNull();
    }

    public async Task ClearMemoryCacheAsync(CancellationToken cancellationToken)
    {
        // Bump first so any read-through that read the backing store before this clear is rejected
        // when it tries to write back, even while the reseed below is still running.
        InvalidateMemoryCacheGeneration();

        _publishedElementCache.Clear();
        await _hybridCache.RemoveByTagAsync(Constants.Cache.Tags.Element, cancellationToken);

        // We have to run seeding again after the cache is cleared
        await SeedAsync(cancellationToken);
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
#if DEBUG
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
#endif

        foreach (IEnumerable<Guid> group in SeedKeys.InGroupsOf(_cacheSettings.ElementSeedBatchSize))
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

            List<ContentCacheNode> cacheNodes = (await _databaseCacheRepository.GetElementSourcesAsync(uncachedKeys)).ToList();

            scope.Complete();

            _logger.LogDebug("Element nodes to cache {NodeCount}", cacheNodes.Count);

            foreach (ContentCacheNode cacheNode in cacheNodes)
            {
                var cacheKey = GetCacheKey(cacheNode.Key, false);
                await _hybridCache.SetAsync(
                    cacheKey,
                    cacheNode,
                    GetSeedEntryOptions(),
                    GenerateTags(cacheNode),
                    cancellationToken: cancellationToken);
            }
        }


#if DEBUG
        sw.Stop();
        _logger.LogInformation("Element cache seeding completed in {ElapsedMilliseconds} ms with {SeedCount} seed keys.", sw.ElapsedMilliseconds, SeedKeys.Count);
#else
        _logger.LogInformation("Element cache seeding completed with {SeedCount} seed keys.", SeedKeys.Count);
#endif
    }

    // Internal for test purposes.
    internal void ResetSeedKeys() => _seedKeys = null;

    public async Task RefreshMemoryCacheAsync(Guid key)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.ElementTree);

        (ContentCacheNode? draftNode, ContentCacheNode? publishedNode) = await _databaseCacheRepository.GetElementSourceForPublishStatesAsync(key);

        if (draftNode is not null)
        {
            await _hybridCache.SetAsync(GetCacheKey(draftNode.Key, true), draftNode, GetEntryOptions(draftNode.Key, true), GenerateTags(draftNode));
        }
        else
        {
            // No draft in the database cache — remove any stale draft entry from the local memory cache.
            await _hybridCache.RemoveAsync(GetCacheKey(key, true));
        }

        if (publishedNode is not null)
        {
            var cacheKey = GetCacheKey(publishedNode.Key, false);
            await _hybridCache.SetAsync(cacheKey, publishedNode, GetEntryOptions(publishedNode.Key, false), GenerateTags(publishedNode));
            _publishedElementCache.Remove(cacheKey);
            InvalidateMemoryCacheGeneration();
        }
        else
        {
            // No published node in the database cache — remove any stale published entry from the local
            // memory cache. ClearPublishedCacheAsync bumps the generation itself, so this path is covered.
            await ClearPublishedCacheAsync(key);
        }

        scope.Complete();
    }

    /// <inheritdoc />
    public void RefreshMemoryCache(Guid key)
    {
        // Waiting for the distributed cache write needs a thread-pool thread. Under the element tree lock that wait
        // can starve on a busy pool and hold the lock for seconds, so the writes are issued under the lock and only
        // waited for once the scope is disposed. HybridCache updates the in-process cache and sends the distributed
        // write before its first await, so both still happen under the lock; only the wait for the acknowledgement
        // moves out. The per-element lock makes the next refresh or removal of the same element on this node wait
        // for these writes, so a slow write cannot land on top of a newer one from this node.
        // TODO (V20): await the cache writes directly once the refresh pipeline is asynchronous.
        using (_refreshLocks.Enter(key))
        {
            ValueTask draftWrite;
            ValueTask publishedWrite;
            using (ICoreScope scope = _scopeProvider.CreateCoreScope())
            {
                scope.ReadLock(Constants.Locks.ElementTree);

                (ContentCacheNode? draftNode, ContentCacheNode? publishedNode) = _databaseCacheRepository.GetElementSourceForPublishStates(key);

                draftWrite = draftNode is not null
                    ? _hybridCache.SetAsync(GetCacheKey(draftNode.Key, true), draftNode, GetEntryOptions(draftNode.Key, true), GenerateTags(draftNode))
                    : _hybridCache.RemoveAsync(GetCacheKey(key, true));

                var cacheKey = GetCacheKey(key, false);
                publishedWrite = publishedNode is not null
                    ? _hybridCache.SetAsync(cacheKey, publishedNode, GetEntryOptions(publishedNode.Key, false), GenerateTags(publishedNode))
                    : _hybridCache.RemoveAsync(cacheKey);
                _publishedElementCache.Remove(cacheKey);
                InvalidateMemoryCacheGeneration();

                scope.Complete();
            }

            // Both writes settle before the lock is released, even when one of them fails.
            Task.WhenAll(draftWrite.AsTask(), publishedWrite.AsTask()).GetAwaiter().GetResult();
        }
    }

    /// <inheritdoc />
    public async Task RemoveFromMemoryCacheAsync(Guid key)
    {
        // Ordered after a pending refresh of the same element, so a slow write from that refresh cannot bring the
        // entries back. Not for callers holding the element tree lock: a refresh waiting for that lock holds this one.
        using (await _refreshLocks.EnterAsync(key))
        {
            await RemoveFromMemoryCacheUnorderedAsync(key);
        }
    }

    // Removes both entries of an element without waiting for a pending refresh of it, for callers that hold the
    // element tree lock. The refresh that follows their commit removes the entries again, in order.
    private async Task RemoveFromMemoryCacheUnorderedAsync(Guid key)
    {
        await _hybridCache.RemoveAsync(GetCacheKey(key, true));
        await ClearPublishedCacheAsync(key);
    }

    /// <inheritdoc />
    public void RefreshElement(IElement element)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        if (element.Trashed)
        {
            _databaseCacheRepository.DeleteContentItem(element.Id);

            // TODO (V20): await the memory-cache removals once the notification pipeline is asynchronous.
            RemoveFromMemoryCacheUnorderedAsync(element.Key).GetAwaiter().GetResult();
            scope.Complete();
            return;
        }

        var draftCacheNode = _cacheNodeFactory.ToContentCacheNode(element, true);
        _databaseCacheRepository.RefreshElement(draftCacheNode);

        if (element.PublishedState is PublishedState.Publishing)
        {
            var publishedCacheNode = _cacheNodeFactory.ToContentCacheNode(element, false);
            _databaseCacheRepository.RefreshElement(publishedCacheNode);
        }
        else if (element.PublishedState is PublishedState.Unpublishing)
        {
            _databaseCacheRepository.RemovePublishedElement(element.Id);
            ClearPublishedCacheAsync(element.Key).GetAwaiter().GetResult();
        }

        scope.Complete();
    }

    /// <inheritdoc />
    public async Task RefreshElementAsync(IElement element)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        if (element.Trashed)
        {
            await _databaseCacheRepository.DeleteContentItemAsync(element.Id);
            await RemoveFromMemoryCacheUnorderedAsync(element.Key);
            scope.Complete();
            return;
        }

        // Always set draft node
        // We have nodes separate in the cache, cause 99% of the time, you are only using one
        // and thus we won't get too much data when retrieving from the cache.
        var draftCacheNode = _cacheNodeFactory.ToContentCacheNode(element, true);
        await _databaseCacheRepository.RefreshElementAsync(draftCacheNode);

        if (element.PublishedState is PublishedState.Publishing)
        {
            var publishedCacheNode = _cacheNodeFactory.ToContentCacheNode(element, false);
            await _databaseCacheRepository.RefreshElementAsync(publishedCacheNode);
        }
        else if (element.PublishedState is PublishedState.Unpublishing)
        {
            await _databaseCacheRepository.RemovePublishedElementAsync(element.Id);
            await ClearPublishedCacheAsync(element.Key);
        }

        scope.Complete();
    }

    public async Task DeleteItemAsync(IContentBase content)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        await _databaseCacheRepository.DeleteContentItemAsync(content.Id);
        scope.Complete();
    }

    public void Rebuild(IReadOnlyCollection<int> elementTypeIds)
        => _databaseCacheRepository.Rebuild(
            null,
            null,
            elementTypeIds.ToList(),
            action =>
            {
                using ICoreScope scope = _scopeProvider.CreateCoreScope();
                action();
                scope.Complete();
            });

    public async Task RebuildMemoryCacheByContentTypeAsync(IEnumerable<int> elementTypeIds)
    {
        // Clear the hybrid cache by element type tag for the affected element types.
        var elementTypeIdsAsArray = elementTypeIds as int[] ?? elementTypeIds.ToArray();
        var elementTypeIdTags = elementTypeIdsAsArray.Select(ElementTypeIdTag).ToArray();
        await _hybridCache.RemoveByTagAsync(elementTypeIdTags);

        // Clear converted elements for the affected types so entries are re-converted when next requested.
        ClearConvertedContentCache(elementTypeIdsAsArray);
    }

    public void ClearConvertedContentCache()
    {
        _publishedElementCache.Clear();
        InvalidateMemoryCacheGeneration();
    }

    public void ClearConvertedContentCache(IReadOnlyCollection<int> elementTypeIds)
    {
        var ids = elementTypeIds as int[] ?? elementTypeIds.ToArray();
        _publishedElementCache.RemoveWhere(element => ids.Contains(element.ContentType.Id));
        InvalidateMemoryCacheGeneration();
    }

    private static string GetCacheKey(Guid key, bool preview) => preview ? $"{key}+draft" : $"{key}";

    private HybridCacheEntryOptions GetSeedEntryOptions() => new()
    {
        Expiration = _cacheSettings.Entry.Element.SeedCacheDuration,
        LocalCacheExpiration = _cacheSettings.Entry.Element.SeedCacheDuration,
    };

    private HybridCacheEntryOptions GetEntryOptions(Guid key, bool preview)
    {
        if (SeedKeys.Contains(key) && preview is false)
        {
            return GetSeedEntryOptions();
        }

        return new HybridCacheEntryOptions
        {
            Expiration = _cacheSettings.Entry.Element.RemoteCacheDuration,
            LocalCacheExpiration = _cacheSettings.Entry.Element.LocalCacheDuration,
        };
    }

    /// <summary>
    /// Generates the cache tags for a given <see cref="ContentCacheNode"/>.
    /// </summary>
    /// <param name="cacheNode">The cache node to generate tags for, or <c>null</c> for a negative-cache entry.</param>
    /// <returns>
    /// A set of tags that always includes <see cref="Constants.Cache.Tags.Element"/>.
    /// When <paramref name="cacheNode"/> is non-null, the element type ID tag is also included.
    /// </returns>
    /// <remarks>
    /// Tags are used to clear all cache entries related to a given element or type.
    /// The <see cref="Constants.Cache.Tags.Element"/> tag is always included — even for null entries — so
    /// that <see cref="ClearMemoryCacheAsync"/> (which clears by this tag) can evict negative-cache entries.
    /// Without this, null entries survive tag-based cache clears and become permanently stale.
    /// </remarks>
    private static HashSet<string> GenerateTags(ContentCacheNode? cacheNode) =>
        cacheNode is null
            ? [Constants.Cache.Tags.Element]
            : [Constants.Cache.Tags.Element, ElementTypeIdTag(cacheNode.ContentTypeId)];

    private async Task ClearPublishedCacheAsync(Guid key)
    {
        var cacheKey = GetCacheKey(key, false);
        await _hybridCache.RemoveAsync(cacheKey);
        _publishedElementCache.Remove(cacheKey);
        InvalidateMemoryCacheGeneration();
    }

    private static string ElementTypeIdTag(int elementTypeId)
        => $"et:{elementTypeId}";
}
