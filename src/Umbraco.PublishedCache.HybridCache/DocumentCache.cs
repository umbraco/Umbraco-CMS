using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.HybridCache;

/// <summary>
/// Provides access to published documents (content) held in the hybrid cache.
/// </summary>
public sealed class DocumentCache : IPublishedContentCache
{
    private readonly IDocumentCacheService _documentCacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentCache"/> class.
    /// </summary>
    /// <param name="documentCacheService">The service that retrieves and caches published document nodes.</param>
    public DocumentCache(IDocumentCacheService documentCacheService)
        => _documentCacheService = documentCacheService;

    /// <inheritdoc/>
    public async Task<IPublishedContent?> GetByIdAsync(int id, bool? preview = null) => await _documentCacheService.GetByIdAsync(id, preview);

    /// <inheritdoc/>
    public async Task<IPublishedContent?> GetByIdAsync(Guid key, bool? preview = null) => await _documentCacheService.GetByKeyAsync(key, preview);

    /// <inheritdoc/>
    public IPublishedContent? GetById(bool preview, int contentId) => _documentCacheService.GetById(contentId, preview);

    /// <inheritdoc/>
    public IPublishedContent? GetById(bool preview, Guid contentId) => _documentCacheService.GetByKey(contentId, preview);

    /// <inheritdoc/>
    public IPublishedContent? GetById(int contentId) => _documentCacheService.GetById(contentId);

    /// <inheritdoc/>
    public IPublishedContent? GetById(Guid contentId) => _documentCacheService.GetByKey(contentId);
}
