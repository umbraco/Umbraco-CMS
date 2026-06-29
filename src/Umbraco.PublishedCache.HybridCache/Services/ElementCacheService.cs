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
    private readonly CacheSettings _cacheSettings;
    private readonly ILogger<ElementCacheService> _logger;
    private HashSet<Guid>? _seedKeys;

    private readonly ConvertedPublishedContentCache<string, IPublishedElement> _publishedElementCache = new();

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
        ILogger<ElementCacheService> logger)
    {
        _databaseCacheRepository = databaseCacheRepository;
        _scopeProvider = scopeProvider;
        _hybridCache = hybridCache;
        _publishedContentFactory = publishedContentFactory;
        _cacheNodeFactory = cacheNodeFactory;
        _seedKeyProviders = seedKeyProviders;
        _publishedModelFactory = publishedModelFactory;
        _previewService = previewService;
        _cacheSettings = cacheSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public string CacheName => "Published elements (converted, L0)";

    /// <inheritdoc />
    public long GetApproximateCount() => _publishedElementCache.Count;

    /// <inheritdoc />
    public long? GetApproximateBytes() => _publishedElementCache.ApproximateSizeInBytes;

    public async Task<IPublishedElement?> GetByKeyAsync(Guid key, bool? preview = null)
    {
        bool calculatedPreview = preview ?? _previewService.IsInPreview();
        return await GetNodeAsync(key, calculatedPreview);
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

    private async Task<IPublishedElement?> GetNodeAsync(Guid key, bool preview)
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

        // Only populate the L0 cache when our snapshot is still current; otherwise a concurrent
        // refresh has already written fresher content and we must not overwrite it with this one.
        if (result is not null && snapshotIsCurrent)
        {
            // The size estimate is cheap (O(properties), no IO/decompression) and only runs on the
            // cache-miss path, so keeping the running total always-current means it is accurate the
            // moment debug reporting is switched on.
            _publishedElementCache.Set(cacheKey, result, ContentCacheNodeSizeEstimator.EstimateBytes(contentCacheNode));
        }

        return result;

        async Task<ContentCacheNode?> GetContentCacheNodeFromRepo()
        {
            using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
            return await _databaseCacheRepository.GetElementSourceAsync(key, preview);
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

    public async Task RemoveFromMemoryCacheAsync(Guid key)
    {
        await _hybridCache.RemoveAsync(GetCacheKey(key, true));
        await ClearPublishedCacheAsync(key);
    }

    public async Task RefreshElementAsync(IElement element)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        if (element.Trashed)
        {
            await _databaseCacheRepository.DeleteContentItemAsync(element.Id);
            await RemoveFromMemoryCacheAsync(element.Key);
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
