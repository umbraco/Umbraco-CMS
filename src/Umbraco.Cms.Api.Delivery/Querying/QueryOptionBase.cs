using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Querying;

public abstract class QueryOptionBase
{
    private readonly IRequestRoutingService _requestRoutingService;
    private readonly IRequestPreviewService _requestPreviewService;
    private readonly IRequestCultureService _requestCultureService;
    private readonly IApiDocumentUrlService _apiDocumentUrlService;

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V17.")]
    public QueryOptionBase(
        IPublishedContentCache publishedContentCache,
        IRequestRoutingService requestRoutingService)
        : this(
            requestRoutingService,
            StaticServiceProvider.Instance.GetRequiredService<IRequestPreviewService>(),
            StaticServiceProvider.Instance.GetRequiredService<IRequestCultureService>(),
            StaticServiceProvider.Instance.GetRequiredService<IApiDocumentUrlService>())
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Will be removed in V17.")]
    public QueryOptionBase(
        IPublishedContentCache publishedContentCache,
        IRequestRoutingService requestRoutingService,
        IRequestPreviewService requestPreviewService,
        IRequestCultureService requestCultureService,
        IApiDocumentUrlService apiDocumentUrlService)
        : this(requestRoutingService, requestPreviewService, requestCultureService, apiDocumentUrlService)
    {
    }

    public QueryOptionBase(
        IRequestRoutingService requestRoutingService,
        IRequestPreviewService requestPreviewService,
        IRequestCultureService requestCultureService,
        IApiDocumentUrlService apiDocumentUrlService)
    {
        _requestRoutingService = requestRoutingService;
        _requestPreviewService = requestPreviewService;
        _requestCultureService = requestCultureService;
        _apiDocumentUrlService = apiDocumentUrlService;
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
            _requestCultureService.GetRequestedCulture(),
            _requestPreviewService.IsPreview());
    }
}
