using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Querying;

public abstract class QueryOptionBase
{
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IRequestRoutingService _requestRoutingService;


    public QueryOptionBase(
        IPublishedContentCache publishedContentCache,
        IRequestRoutingService requestRoutingService)
    {
        _publishedContentCache = publishedContentCache;
        _requestRoutingService = requestRoutingService;
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
        IPublishedContent? contentItem = _publishedContentCache.GetByRoute(contentRoute);

        return contentItem?.Key;
    }
}
