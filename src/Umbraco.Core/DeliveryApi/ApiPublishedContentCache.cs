using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class ApiPublishedContentCache : IApiPublishedContentCache
{
    private readonly IRequestPreviewService _requestPreviewService;
    private readonly IApiDocumentUrlService _apiDocumentUrlService;
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private DeliveryApiSettings _deliveryApiSettings;

    [Obsolete("Use the non-obsolete constructor. Will be removed in V17.")]
    public ApiPublishedContentCache(
        IRequestPreviewService requestPreviewService,
        IRequestCultureService requestCultureService,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings,
        IDocumentUrlService documentUrlService,
        IPublishedContentCache publishedContentCache)
        : this(
            requestPreviewService,
            deliveryApiSettings,
            StaticServiceProvider.Instance.GetRequiredService<IApiDocumentUrlService>(),
            publishedContentCache,
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>())
    {
    }

    [Obsolete("Use the non-obsolete constructor. Will be removed in V17.")]
    public ApiPublishedContentCache(
        IRequestPreviewService requestPreviewService,
        IRequestCultureService requestCultureService,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings,
        IDocumentUrlService documentUrlService,
        IApiDocumentUrlService apiDocumentUrlService,
        IPublishedContentCache publishedContentCache,
        IVariationContextAccessor variationContextAccessor)
        : this(requestPreviewService, deliveryApiSettings, apiDocumentUrlService, publishedContentCache, variationContextAccessor)
    {
    }

    public ApiPublishedContentCache(
        IRequestPreviewService requestPreviewService,
        IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings,
        IApiDocumentUrlService apiDocumentUrlService,
        IPublishedContentCache publishedContentCache,
        IVariationContextAccessor variationContextAccessor)
    {
        _requestPreviewService = requestPreviewService;
        _apiDocumentUrlService = apiDocumentUrlService;
        _publishedContentCache = publishedContentCache;
        _variationContextAccessor = variationContextAccessor;
        _deliveryApiSettings = deliveryApiSettings.CurrentValue;
        deliveryApiSettings.OnChange(settings => _deliveryApiSettings = settings);
    }

    public async Task<IPublishedContent?> GetByRouteAsync(string route)
    {
        var isPreviewMode = _requestPreviewService.IsPreview();

        Guid? documentKey = _apiDocumentUrlService.GetDocumentKeyByRoute(
            route,
            _variationContextAccessor.VariationContext?.Culture,
            _requestPreviewService.IsPreview());

        IPublishedContent? content = documentKey.HasValue
            ? await _publishedContentCache.GetByIdAsync(documentKey.Value, isPreviewMode)
            : null;

        return ContentOrNullIfDisallowed(content);
    }

    public IPublishedContent? GetByRoute(string route)
    {
        var isPreviewMode = _requestPreviewService.IsPreview();

        Guid? documentKey = _apiDocumentUrlService.GetDocumentKeyByRoute(
            route,
            _variationContextAccessor.VariationContext?.Culture,
            _requestPreviewService.IsPreview());

        // in multi-root settings, we've historically resolved all but the first root by their ID + URL segment,
        // e.g. "1234/second-root-url-segment". in V15+, IDocumentUrlService won't resolve this anymore; it will
        // however resolve "1234/" correctly, so to remain backwards compatible, we need to perform this extra step.
        var verifyUrlSegment = false;
        if (documentKey is null && route.TrimEnd('/').CountOccurrences("/") is 1)
        {
            documentKey = _apiDocumentUrlService.GetDocumentKeyByRoute(
                route[..(route.IndexOf('/') + 1)],
                _variationContextAccessor.VariationContext?.Culture,
                _requestPreviewService.IsPreview());
            verifyUrlSegment = true;
        }

        IPublishedContent? content = documentKey.HasValue
            ? _publishedContentCache.GetById(isPreviewMode, documentKey.Value)
            : null;

        // the additional look-up above can result in false positives; if attempting to request a non-existing child to
        // the currently contextualized request root (either by start item or by domain), the root content key might
        // get resolved. to counter for this, we compare the requested URL segment with the resolved content URL segment.
        if (content is not null && verifyUrlSegment)
        {
            var expectedUrlSegment = route[(route.IndexOf('/') + 1)..];
            if (content.UrlSegment != expectedUrlSegment)
            {
                content = null;
            }
        }

        return ContentOrNullIfDisallowed(content);
    }

    public async Task<IPublishedContent?> GetByIdAsync(Guid contentId)
    {
        IPublishedContent? content = await _publishedContentCache.GetByIdAsync(contentId, _requestPreviewService.IsPreview()).ConfigureAwait(false);
        return ContentOrNullIfDisallowed(content);
    }

    public IPublishedContent? GetById(Guid contentId)
    {
        IPublishedContent? content = _publishedContentCache.GetById(_requestPreviewService.IsPreview(), contentId);
        return ContentOrNullIfDisallowed(content);
    }

    public async Task<IEnumerable<IPublishedContent>> GetByIdsAsync(IEnumerable<Guid> contentIds)
    {
        var isPreviewMode = _requestPreviewService.IsPreview();

        IEnumerable<Task<IPublishedContent?>> tasks = contentIds
            .Select(contentId => _publishedContentCache.GetByIdAsync(contentId, isPreviewMode));

        IPublishedContent?[] allContent = await Task.WhenAll(tasks);

        return allContent
            .WhereNotNull()
            .Where(IsAllowedContentType)
            .ToArray();
    }

    public IEnumerable<IPublishedContent> GetByIds(IEnumerable<Guid> contentIds)
    {
        var isPreviewMode = _requestPreviewService.IsPreview();
        return contentIds
            .Select(contentId => _publishedContentCache.GetById(isPreviewMode, contentId))
            .WhereNotNull()
            .Where(IsAllowedContentType)
            .ToArray();
    }

    private IPublishedContent? ContentOrNullIfDisallowed(IPublishedContent? content)
        => content != null && IsAllowedContentType(content)
            ? content
            : null;

    private bool IsAllowedContentType(IPublishedContent content)
        => _deliveryApiSettings.IsAllowedContentType(content);
}
