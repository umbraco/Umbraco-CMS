using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Querying;

public abstract class QueryOptionBase
{
    private readonly IRequestRoutingService _requestRoutingService;
    private readonly IRequestPreviewService _requestPreviewService;
    private readonly IApiDocumentUrlService _apiDocumentUrlService;
    private readonly IVariationContextAccessor _variationContextAccessor;

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
