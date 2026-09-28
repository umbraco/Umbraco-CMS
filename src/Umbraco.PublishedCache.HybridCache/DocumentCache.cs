using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

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
    public IPublishedContent? GetById(bool preview, int contentId) => GetByIdAsync(contentId, preview).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public IPublishedContent? GetById(bool preview, Guid contentId)
    {
        // Sync fast path: when the converted-content L0 cache already holds the item we can return it
        // without spinning up an async state machine — the dominant case on a warm site. On a miss we
        // fall through to the async path which handles HybridCache (L1/L2) and database lookups.
        if (_documentCacheService.TryGetCached(contentId, preview, out IPublishedContent? cached))
        {
            return cached;
        }

        return GetByIdAsync(contentId, preview).GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public IPublishedContent? GetById(int contentId) => GetByIdAsync(contentId).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public IPublishedContent? GetById(Guid contentId) => GetByIdAsync(contentId).GetAwaiter().GetResult();

}
