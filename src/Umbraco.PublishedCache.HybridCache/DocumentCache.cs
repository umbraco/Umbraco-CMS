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
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;

    // TODO (V19): Remove the unused parameters from the constructor.

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentCache"/> class.
    /// </summary>
    /// <param name="documentCacheService">The service that retrieves and caches published document nodes.</param>
    /// <param name="publishedContentTypeCache">The cache of published content types.</param>
    /// <param name="documentNavigationQueryService">The service used to query the document navigation structure.</param>
    /// <param name="documentUrlService">The service that resolves document URLs.</param>
    /// <param name="publishedUrlProvider">A lazily resolved provider of published URLs.</param>
    public DocumentCache(
        IDocumentCacheService documentCacheService,
#pragma warning disable IDE0060 // Remove unused parameter
        IPublishedContentTypeCache publishedContentTypeCache,
        IDocumentNavigationQueryService documentNavigationQueryService,
        IDocumentUrlService documentUrlService,
        Lazy<IPublishedUrlProvider> publishedUrlProvider)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        _documentCacheService = documentCacheService;
        _documentNavigationQueryService = documentNavigationQueryService;
    }

    /// <inheritdoc/>
    public async Task<IPublishedContent?> GetByIdAsync(int id, bool? preview = null) => await _documentCacheService.GetByIdAsync(id, preview);

    /// <inheritdoc/>
    public async Task<IPublishedContent?> GetByIdAsync(Guid key, bool? preview = null) => await _documentCacheService.GetByKeyAsync(key, preview);

    /// <inheritdoc/>
    public IPublishedContent? GetById(bool preview, int contentId) => GetByIdAsync(contentId, preview).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public IPublishedContent? GetById(bool preview, Guid contentId)
    {
        // Sync fast path: when the converted-content L0 cache already holds the item we can
        // return it without spinning up an async state machine. This is the dominant case on
        // a warm site and is hit per-key by the FilterAvailable lazy chain. On a miss we fall
        // through to the async path which handles HybridCache (L1/L2) and database lookups.
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

    /// <summary>
    /// Gets the published documents at the root of the content tree.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <param name="culture">
    /// The culture to filter root documents by. When <c>null</c>, all root documents are returned; otherwise only those
    /// that are invariant or vary for the specified culture are returned.
    /// </param>
    /// <returns>The published documents at root level available for the specified culture.</returns>
    [Obsolete("This method is no longer used in Umbraco and is not defined on the interface. " +
        "Any usage can be replaced with a call to IDocumentNavigationQueryService.TryGetRootKeys to retrieve the document keys, " +
        "with each key passed to IPublishedContentCache.GetById to retrieve the IPublishedContent instances. Scheduled for removal in Umbraco 19.")]
    public IEnumerable<IPublishedContent> GetAtRoot(bool preview, string? culture = null)
    {
        if (_documentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys) is false)
        {
            return [];
        }

        IEnumerable<IPublishedContent> rootContent = rootKeys.Select(key => GetById(preview, key)).WhereNotNull();
        return culture is null ? rootContent : rootContent.Where(x => x.IsInvariantOrHasCulture(culture));
    }
}
