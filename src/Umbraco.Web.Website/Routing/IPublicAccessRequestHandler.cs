using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Web.Website.Routing;

public interface IPublicAccessRequestHandler
{
    /// <summary>
    ///     Ensures that access to current node is permitted.
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="routeValues">The current route values</param>
    /// <returns>Updated route values if public access changes the rendered content, else the original route values.</returns>
    /// <remarks>Redirecting to a different site root and/or culture will not pick the new site root nor the new culture.</remarks>
    Task<UmbracoRouteValues?> RewriteForPublishedContentAccessAsync(
        HttpContext httpContext,
        UmbracoRouteValues routeValues);
}
