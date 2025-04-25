using System.Web;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

// NOTE: left unsealed on purpose so it is extendable.
public class ApiContentPathResolver : IApiContentPathResolver
{
    private readonly IRequestRoutingService _requestRoutingService;
    private readonly IApiPublishedContentCache _apiPublishedContentCache;

    public ApiContentPathResolver(IRequestRoutingService requestRoutingService, IApiPublishedContentCache apiPublishedContentCache)
    {
        _requestRoutingService = requestRoutingService;
        _apiPublishedContentCache = apiPublishedContentCache;
    }

    public virtual IPublishedContent? ResolveContentPath(string path)
    {
        path = path.EnsureStartsWith("/");
        var contentRoute = _requestRoutingService.GetContentRoute(path);
        contentRoute = HttpUtility.UrlDecode(contentRoute);
        IPublishedContent? contentItem = _apiPublishedContentCache.GetByRoute(contentRoute);
        return contentItem;
    }
}
