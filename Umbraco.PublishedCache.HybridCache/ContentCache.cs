using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal sealed class ContentCache : IPublishedHybridCache
{
    private readonly Microsoft.Extensions.Caching.Hybrid.HybridCache _cache;
    private readonly ICacheService _cacheService;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IPublishedModelFactory _publishedModelFactory;
    private readonly PublishedContentTypeCache _contentTypeCache;

    public ContentCache(
        Microsoft.Extensions.Caching.Hybrid.HybridCache cache,
        ICacheService cacheService,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IVariationContextAccessor variationContextAccessor,
        IPublishedModelFactory publishedModelFactory,
        IContentTypeService contentTypeService,
        IPublishedContentTypeFactory publishedContentTypeFactory,
        ILoggerFactory loggerFactory)
    {
        _cache = cache;
        _cacheService = cacheService;
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _variationContextAccessor = variationContextAccessor;
        _publishedModelFactory = publishedModelFactory;
        _contentTypeCache = new PublishedContentTypeCache(
            contentTypeService,
            null,
            null,
            publishedContentTypeFactory,
            loggerFactory.CreateLogger<PublishedContentTypeCache>());
    }

    public async Task<IPublishedContent?> GetById(int contentId, bool preview = false)
    {
        ContentCacheNode? contentCacheNode = await _cache.GetOrCreateAsync(
            $"{contentId}", // Unique key to the cache entry
            async cancel => await _cacheService.GetById(contentId, preview));

        return contentCacheNode is null ? null : ToIPublishedContent(contentCacheNode, preview);
    }

    public async Task<IPublishedContent?> GetById(Guid contentId, bool preview = false)
    {
        ContentCacheNode? contentCacheNode = await _cache.GetOrCreateAsync(
            $"{contentId}", // Unique key to the cache entry
            async cancel => await _cacheService.GetByKey(contentId, preview));

        return contentCacheNode is null ? null : ToIPublishedContent(contentCacheNode, preview);
    }

    public Task<bool> HasById(bool preview, int contentId) => throw new NotImplementedException();

    public Task<bool> HasById(int contentId) => throw new NotImplementedException();

    public Task<bool> HasContent(bool preview) => throw new NotImplementedException();

    public Task<bool> HasContent() => throw new NotImplementedException();

    // TODO: Refactor to use mapping service
    private IPublishedContent? ToIPublishedContent(ContentCacheNode contentCacheNode, bool preview)
    {
        var n = new ContentNode(contentCacheNode.Id, contentCacheNode.Key, contentCacheNode.Path, contentCacheNode.SortOrder, contentCacheNode.CreateDate, contentCacheNode.CreatorId);
        IPublishedContentType contentType = _contentTypeCache.Get(PublishedItemType.Content, contentCacheNode.ContentTypeId);
        n.SetContentTypeAndData(contentType, contentCacheNode.Draft, contentCacheNode.Published, _publishedSnapshotAccessor, _variationContextAccessor, _publishedModelFactory);
        return preview ? n.DraftModel : n.PublishedModel;
    }
}
