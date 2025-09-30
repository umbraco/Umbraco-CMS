using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.Navigation;

internal sealed class PublishedContentStatusFilteringService : IPublishedContentStatusFilteringService
{
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IPublishStatusQueryService _publishStatusQueryService;
    private readonly IPreviewService _previewService;
    private readonly IPublishedContentCache _publishedContentCache;

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

        return WhereIsInvariantOrHasCulture(candidateKeys, culture, preview).ToArray();
    }

    private IEnumerable<IPublishedContent> WhereIsInvariantOrHasCulture(IEnumerable<Guid> keys, string culture, bool preview)
        => keys
            .Select(key => _publishedContentCache.GetById(preview, key))
            .WhereNotNull()
            .Where(content => content.ContentType.VariesByCulture() is false
                              || content.Cultures.ContainsKey(culture));
}
