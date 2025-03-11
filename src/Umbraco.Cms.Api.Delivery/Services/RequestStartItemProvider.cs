using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Services;

internal sealed class RequestStartItemProvider : RequestHeaderHandler, IRequestStartItemProvider
{
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IRequestPreviewService _requestPreviewService;
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;
    private readonly IPublishedContentCache _publishedContentCache;

    // this provider lifetime is Scope, so we can cache this as a field
    private IPublishedContent? _requestedStartContent;

    public RequestStartItemProvider(
        IHttpContextAccessor httpContextAccessor,
        IVariationContextAccessor variationContextAccessor,
        IRequestPreviewService requestPreviewService,
        IDocumentNavigationQueryService documentNavigationQueryService,
        IPublishedContentCache publishedContentCache)
        : base(httpContextAccessor)
    {

        _variationContextAccessor = variationContextAccessor;
        _requestPreviewService = requestPreviewService;
        _documentNavigationQueryService = documentNavigationQueryService;
        _publishedContentCache = publishedContentCache;
    }

    /// <inheritdoc/>
    public IPublishedContent? GetStartItem()
    {
        if (_requestedStartContent != null)
        {
            return _requestedStartContent;
        }

        var headerValue = RequestedStartItem()?.Trim(Constants.CharArrays.ForwardSlash);
        if (headerValue.IsNullOrWhiteSpace())
        {
            return null;
        }

        _documentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys);
        IEnumerable<IPublishedContent> rootContent = rootKeys
            .Select(rootKey => _publishedContentCache.GetById(_requestPreviewService.IsPreview(), rootKey))
            .WhereNotNull();

        _requestedStartContent = Guid.TryParse(headerValue, out Guid key)
            ? rootContent.FirstOrDefault(c => c.Key == key)
            : rootContent.FirstOrDefault(c => c.UrlSegment(_variationContextAccessor).InvariantEquals(headerValue));

        return _requestedStartContent;
    }

    /// <inheritdoc/>
    public string? RequestedStartItem() => GetHeaderValue("Start-Item");
}
