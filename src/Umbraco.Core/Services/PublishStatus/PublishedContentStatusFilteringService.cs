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

    public PublishedContentStatusFilteringService(IVariationContextAccessor variationContextAccessor, IPublishStatusQueryService publishStatusQueryService, IPreviewService previewService, IPublishedContentCache publishedContentCache)
    {
        _variationContextAccessor = variationContextAccessor;
        _publishStatusQueryService = publishStatusQueryService;
        _previewService = previewService;
        _publishedContentCache = publishedContentCache;
    }

    public IEnumerable<IPublishedContent> FilterAncestors(IEnumerable<Guid> ancestorsKeys, string? culture)
    {
        culture = CultureOrEmpty(culture);

        return FilterKeys(
            ancestorsKeys,
            keys => keys.TakeWhile(ancestorKey => _publishStatusQueryService.IsDocumentPublished(ancestorKey, culture)),
            culture);
    }

    public IEnumerable<IPublishedContent> FilterSiblings(IEnumerable<Guid> siblingKeys, string? culture)
        => DefaultFilter(siblingKeys, culture);

    public IEnumerable<IPublishedContent> FilterChildren(IEnumerable<Guid> childrenKeys, string? culture)
        => DefaultFilter(childrenKeys, culture);

    public IEnumerable<IPublishedContent> FilterDescendants(IEnumerable<Guid> descendantKeys, string? culture)
        => DefaultFilter(descendantKeys, culture);

    private IEnumerable<IPublishedContent> DefaultFilter(IEnumerable<Guid> candidateKeys, string? culture)
    {
        culture = CultureOrEmpty(culture);
        return FilterKeys(
            candidateKeys,
            keys => keys.Where(key => _publishStatusQueryService.IsDocumentPublished(key, culture)),
            culture);
    }

    private string CultureOrEmpty(string? culture)
        => culture ?? _variationContextAccessor.VariationContext?.Culture ?? string.Empty;

    private IEnumerable<IPublishedContent> WhereIsInvariantOrHasCulture(IEnumerable<Guid> keys, string culture, bool preview)
        => keys
            .Select(key => _publishedContentCache.GetById(preview, key))
            .WhereNotNull()
            .Where(content => content.ContentType.VariesByCulture() is false
                              || content.Cultures.ContainsKey(culture));

    private IPublishedContent[] FilterKeys(IEnumerable<Guid> candidateKeys, Func<IEnumerable<Guid>, IEnumerable<Guid>> filterCandidateKeysForNonPreview, string culture)
    {
        Guid[] candidateKeysAsArray = candidateKeys as Guid[] ?? candidateKeys.ToArray();
        if (candidateKeysAsArray.Length == 0)
        {
            return [];
        }

        var preview = _previewService.IsInPreview();
        candidateKeys = preview
            ? candidateKeysAsArray
            : filterCandidateKeysForNonPreview(candidateKeysAsArray);

        return WhereIsInvariantOrHasCulture(candidateKeys, culture, preview).ToArray();
    }
}
