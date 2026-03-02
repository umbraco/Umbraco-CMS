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
    private readonly IPublishStatusQueryService _publishStatusQueryService;
    private readonly IPreviewService _previewService;
    private readonly IPublishedContentCache _publishedContentCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedContentStatusFilteringService"/> class.
    /// </summary>
    /// <param name="variationContextAccessor">The variation context accessor for retrieving culture information.</param>
    /// <param name="publishStatusQueryService">The service for querying document publish status.</param>
    /// <param name="previewService">The service for determining if the current request is in preview mode.</param>
    /// <param name="publishedContentCache">The published content cache for retrieving content items.</param>
    public PublishedContentStatusFilteringService(
        IVariationContextAccessor variationContextAccessor,
        IPublishStatusQueryService publishStatusQueryService,
        IPreviewService previewService,
        IPublishedContentCache publishedContentCache)
    {
        _variationContextAccessor = variationContextAccessor;
        _publishStatusQueryService = publishStatusQueryService;
        _previewService = previewService;
        _publishedContentCache = publishedContentCache;
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
        candidateKeys = preview
            ? candidateKeysAsArray
            : candidateKeysAsArray.Where(key =>
                _publishStatusQueryService.IsDocumentPublished(key, culture)
                && _publishStatusQueryService.HasPublishedAncestorPath(key));

        return WhereIsInvariantOrHasCultureOrRequestedAllCultures(candidateKeys, culture, preview).ToArray();
    }

    /// <summary>
    /// Filters content items to include only those that are invariant, have the requested culture, or when all cultures are requested.
    /// </summary>
    /// <param name="keys">The content keys to filter.</param>
    /// <param name="culture">The requested culture.</param>
    /// <param name="preview">Whether the request is in preview mode.</param>
    /// <returns>A collection of <see cref="IPublishedContent"/> items that match the culture criteria.</returns>
    private IEnumerable<IPublishedContent> WhereIsInvariantOrHasCultureOrRequestedAllCultures(IEnumerable<Guid> keys, string culture, bool preview)
        => keys
            .Select(key => _publishedContentCache.GetById(preview, key))
            .WhereNotNull()
            .Where(content => culture == Constants.System.InvariantCulture
                              || content.ContentType.VariesByCulture() is false
                              || content.Cultures.ContainsKey(culture));
}
