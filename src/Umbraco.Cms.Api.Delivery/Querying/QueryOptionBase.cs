using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Querying;

public abstract class QueryOptionBase
{
    private readonly IRequestRoutingService _requestRoutingService;
    private readonly IRequestPreviewService _requestPreviewService;
    private readonly IApiDocumentUrlService _apiDocumentUrlService;
    private readonly IVariationContextAccessor _variationContextAccessor;

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V17.")]
    public QueryOptionBase(
        IPublishedContentCache publishedContentCache,
        IRequestRoutingService requestRoutingService)
        : this(
            requestRoutingService,
            StaticServiceProvider.Instance.GetRequiredService<IRequestPreviewService>(),
            StaticServiceProvider.Instance.GetRequiredService<IApiDocumentUrlService>(),
            StaticServiceProvider.Instance.GetRequiredService<IVariationContextAccessor>())
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V17.")]
    public QueryOptionBase(
        IPublishedContentCache publishedContentCache,
        IRequestRoutingService requestRoutingService,
        IRequestPreviewService requestPreviewService,
        IRequestCultureService requestCultureService,
        IApiDocumentUrlService apiDocumentUrlService,
        IVariationContextAccessor variationContextAccessor)
        : this(requestRoutingService, requestPreviewService, apiDocumentUrlService, variationContextAccessor)
    {
    }

    public QueryOptionBase(
        IRequestRoutingService requestRoutingService,
        IRequestPreviewService requestPreviewService,
        IApiDocumentUrlService apiDocumentUrlService,
        IVariationContextAccessor variationContextAccessor)
    {
        _requestRoutingService = requestRoutingService;
        _requestPreviewService = requestPreviewService;
        _apiDocumentUrlService = apiDocumentUrlService;
        _variationContextAccessor = variationContextAccessor;
    }

    protected Guid? GetGuidFromQuery(string queryStringValue)
    {
        if (queryStringValue.IsNullOrWhiteSpace())
        {
            return null;
        }

        if (Guid.TryParse(queryStringValue, out Guid id))
        {
            return id;
        }

        // Check if the passed value is a path of a content item
        var contentRoute = _requestRoutingService.GetContentRoute(queryStringValue);
        return _apiDocumentUrlService.GetDocumentKeyByRoute(
            contentRoute,
            _variationContextAccessor.VariationContext?.Culture,
            _requestPreviewService.IsPreview());
    }
}
