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

internal sealed class ElementCacheService : IElementCacheService
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

    private readonly ConcurrentDictionary<string, IPublishedElement> _publishedElementCache = [];

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

    public async Task<IPublishedElement?> GetByKeyAsync(Guid key, bool? preview = null)
    {
        bool calculatedPreview = preview ?? _previewService.IsInPreview();
        return await GetNodeAsync(key, calculatedPreview);
    }

    private async Task<IPublishedElement?> GetNodeAsync(Guid key, bool preview)
    {
        var cacheKey = GetCacheKey(key, preview);

        if (preview is false && _publishedElementCache.TryGetValue(cacheKey, out IPublishedElement? cached))
        {
            return cached;
        }

        (bool exists, ContentCacheNode? contentCacheNode) = await _hybridCache.TryGetValueAsync<ContentCacheNode?>(cacheKey, CancellationToken.None);
        if (exists is false)
        {
            contentCacheNode = await GetContentCacheNodeFromRepo();
            await _hybridCache.SetAsync(
                cacheKey,
                contentCacheNode,
                GetEntryOptions(key, preview),
                GenerateTags(contentCacheNode));
        }

        if (contentCacheNode is null)
        {
            return null;
        }

        IPublishedElement? result = _publishedContentFactory.ToIPublishedElement(contentCacheNode, preview)?.CreateModel(_publishedModelFactory);
        if (result is not null)
        {
            _publishedElementCache[cacheKey] = result;
        }

        return result;

        async Task<ContentCacheNode?> GetContentCacheNodeFromRepo()
        {
            using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
            return await _databaseCacheRepository.GetElementSourceAsync(key, preview);
        }
    }

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

        if (publishedNode is not null)
        {
            var cacheKey = GetCacheKey(publishedNode.Key, false);
            await _hybridCache.SetAsync(cacheKey, publishedNode, GetEntryOptions(publishedNode.Key, false), GenerateTags(publishedNode));
            _publishedElementCache.Remove(cacheKey, out _);
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

        // Always set draft node
        // We have nodes separate in the cache, cause 99% of the time, you are only using one
        // and thus we won't get too much data when retrieving from the cache.
        var draftCacheNode = _cacheNodeFactory.ToContentCacheNode(element, true);

        await _databaseCacheRepository.RefreshElementAsync(draftCacheNode, element.PublishedState);

        if (element.PublishedState == PublishedState.Publishing || element.PublishedState == PublishedState.Unpublishing)
        {
            var publishedCacheNode = _cacheNodeFactory.ToContentCacheNode(element, false);

            await _databaseCacheRepository.RefreshElementAsync(publishedCacheNode, element.PublishedState);

            if (element.PublishedState == PublishedState.Unpublishing)
            {
                await ClearPublishedCacheAsync(publishedCacheNode.Key);
            }
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
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        _databaseCacheRepository.Rebuild(elementTypeIds: elementTypeIds.ToList());
        scope.Complete();
    }

    public async Task RebuildMemoryCacheByContentTypeAsync(IEnumerable<int> elementTypeIds)
    {
        // Clear the hybrid cache by element type tag for the affected element types.
        var elementTypeIdsAsArray = elementTypeIds as int[] ?? elementTypeIds.ToArray();
        var elementTypeIdTags = elementTypeIdsAsArray.Select(ElementTypeIdTag).ToArray();
        await _hybridCache.RemoveByTagAsync(elementTypeIdTags);

        // Clear converted elements for the affected types so entries are re-converted when next requested.
        ClearConvertedContentCache(elementTypeIdsAsArray);
    }

    public void ClearConvertedContentCache() => _publishedElementCache.Clear();

    public void ClearConvertedContentCache(IReadOnlyCollection<int> elementTypeIds)
    {
        var ids = elementTypeIds as int[] ?? elementTypeIds.ToArray();
        _publishedElementCache.RemoveAll(element => ids.Contains(element.Value.ContentType.Id));
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
        _publishedElementCache.Remove(cacheKey, out _);
    }

    private static string ElementTypeIdTag(int elementTypeId)
        => $"et:{elementTypeId}";
}
