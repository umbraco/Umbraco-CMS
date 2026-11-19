using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Web.Website.Routing;

/// <summary>
///     Applies public access restrictions to a routed front-end request, re-routing it to the configured
///     login or error page when the current member may not view the requested content.
/// </summary>
public interface IPublicAccessRequestHandler
{
    /// <summary>
    ///     Ensures that access to current node is permitted.
    /// </summary>
    /// <param name="httpContext">The current HTTP context, used to authenticate the member making the request.</param>
    /// <param name="routeValues">The current route values</param>
    /// <returns>
    ///     Updated route values if public access changes the rendered content, else the original route values.
    ///     When the login or error page has no published content, the returned route values describe a not-found request.
    /// </returns>
    /// <remarks>Redirecting to a different site root and/or culture will not pick the new site root nor the new culture.</remarks>
    Task<UmbracoRouteValues?> RewriteForPublishedContentAccessAsync(
        HttpContext httpContext,
        UmbracoRouteValues routeValues);
}
