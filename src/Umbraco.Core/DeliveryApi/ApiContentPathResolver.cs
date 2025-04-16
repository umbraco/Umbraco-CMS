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

        // File requests will blow up with an downstream exception in GetRequiredPublishedSnapshot, which fails due to an UmbracoContext
        // not being available for what's considered a static file request.
        // Given a URL segment and hence route can't contain a period, we can safely assume that if the last segment of the path contains
        // a period, it's a file request and should return null here.
        if (IsFileRequest(path))
        {
            return null;
        }

        var contentRoute = _requestRoutingService.GetContentRoute(path);
        IPublishedContent? contentItem = _apiPublishedContentCache.GetByRoute(contentRoute);
        return contentItem;
    }

    private static bool IsFileRequest(string path) => path.Split('/', StringSplitOptions.RemoveEmptyEntries).Last().Contains('.');
}
