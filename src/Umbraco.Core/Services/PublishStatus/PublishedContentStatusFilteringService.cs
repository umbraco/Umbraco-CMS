using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Filters published content based on availability, considering publish status, culture, and preview mode.
/// </summary>
/// <remarks>
/// This service determines which content items from a set of candidates are available for display,
/// taking into account whether the content is published, whether it has a published ancestor path,
/// and whether the request is in preview mode.
/// </remarks>
internal sealed class PublishedContentStatusFilteringService : IPublishedContentStatusFilteringService
{
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IDocumentPublishStatusQueryService _publishStatusQueryService;
    private readonly IPreviewService _previewService;
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IDocumentCacheService _documentCacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedContentStatusFilteringService"/> class.
    /// </summary>
    /// <param name="variationContextAccessor">The variation context accessor for retrieving culture information.</param>
    /// <param name="publishStatusQueryService">The service for querying document publish status.</param>
    /// <param name="previewService">The service for determining if the current request is in preview mode.</param>
    /// <param name="publishedContentCache">The published content cache for retrieving content items.</param>
    /// <param name="documentCacheService">The document cache service used to materialise candidate keys in batches.</param>
    public PublishedContentStatusFilteringService(
        IVariationContextAccessor variationContextAccessor,
        IDocumentPublishStatusQueryService publishStatusQueryService,
        IPreviewService previewService,
        IPublishedContentCache publishedContentCache,
        IDocumentCacheService documentCacheService)
    {
        _variationContextAccessor = variationContextAccessor;
        _publishStatusQueryService = publishStatusQueryService;
        _previewService = previewService;
        _publishedContentCache = publishedContentCache;
        _documentCacheService = documentCacheService;
    }

    /// <inheritdoc />
    public IEnumerable<IPublishedContent> FilterAvailable(IEnumerable<Guid> candidateKeys, string? culture)
    {
        culture ??= _variationContextAccessor.VariationContext?.Culture ?? string.Empty;

        Guid[] candidateKeysAsArray = candidateKeys as Guid[] ?? candidateKeys.ToArray();
        if (candidateKeysAsArray.Length == 0)
        {
            return [];
        }

        var preview = _previewService.IsInPreview();

        // Kept lazy so the publish-status filter is only evaluated for keys actually drawn — preserving
        // the short-circuit for .FirstOrDefault() / .Take(n).
        IEnumerable<Guid> keys = preview
            ? candidateKeysAsArray
            : candidateKeysAsArray.Where(key =>
                _publishStatusQueryService.IsPublished(key, culture)
                && _publishStatusQueryService.HasPublishedAncestorPath(key, culture));

        // Materialise in growing chunks: an all-L0-hit chunk stays fully synchronous (no async, no
        // batch), while a cold set collapses its database access into batched reads. Returned lazily
        // so short-circuiting consumers still exit early; callers that enumerate more than once should
        // buffer the result themselves (.ToList() / .ToArray()).
        return ChunkedTieredResolver.Resolve<Guid, IPublishedContent>(
            keys,
            (batchKeys, results) => ResolveCachedItems(batchKeys, preview, results),
            (missedKeys, results) => ResolvePersistedItems(missedKeys, preview, results))
            .Where(content => culture == Constants.System.InvariantCulture
                               || content.ContentType.VariesByCulture() is false
                               || content.Cultures.ContainsKey(culture));
    }

    /// <inheritdoc />
    public IEnumerable<IPublishedContent> Unfiltered(IEnumerable<Guid> candidateKeys)
    {
        var preview = _previewService.IsInPreview();
        return candidateKeys.Select(key => _publishedContentCache.GetById(preview, key)).WhereNotNull();
    }

    private void ResolveCachedItems(IReadOnlyCollection<Guid> batchKeys, bool preview, IDictionary<Guid, IPublishedContent> results)
    {
        foreach (Guid key in batchKeys)
        {
            if (_documentCacheService.TryGetCached(key, preview, out IPublishedContent? content) && content is not null)
            {
                results[key] = content;
            }
        }
    }

    // Sync-over-async is intentional: FilterAvailable backs the sync IPublishedContent.Children()/
    // Descendants() surface, and GetByKeysAsync has no sync counterpart.
    private void ResolvePersistedItems(IReadOnlyCollection<Guid> missedKeys, bool preview, IDictionary<Guid, IPublishedContent> results)
    {
        foreach (IPublishedContent content in _documentCacheService.GetByKeysAsync(missedKeys, preview).GetAwaiter().GetResult())
        {
            results[content.Key] = content;
        }
    }
}
