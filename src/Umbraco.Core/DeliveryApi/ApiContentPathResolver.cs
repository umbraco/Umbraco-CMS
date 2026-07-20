using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Default implementation of <see cref="IApiContentPathResolver"/> that resolves content by URL path for the Delivery API.
/// </summary>
/// <remarks>
///     This class is left unsealed on purpose so it is extendable.
/// </remarks>
public class ApiContentPathResolver : IApiContentPathResolver
{
    private readonly IRequestRoutingService _requestRoutingService;
    private readonly IApiPublishedContentCache _apiPublishedContentCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiContentPathResolver"/> class.
    /// </summary>
    /// <param name="requestRoutingService">The request routing service.</param>
    /// <param name="apiPublishedContentCache">The API published content cache.</param>
    public ApiContentPathResolver(IRequestRoutingService requestRoutingService, IApiPublishedContentCache apiPublishedContentCache)
    {
        _requestRoutingService = requestRoutingService;
        _apiPublishedContentCache = apiPublishedContentCache;
    }

    /// <inheritdoc />
    public virtual IPublishedContent? ResolveContentPath(string path)
    {
        path = path.EnsureStartsWith("/");

        var contentRoute = _requestRoutingService.GetContentRoute(path);
        IPublishedContent? contentItem = _apiPublishedContentCache.GetByRoute(contentRoute);
        return contentItem;
    }
}
