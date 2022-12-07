using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Web.Common.Routing;

/// <summary>
/// This is used to setup the virtual page route so the route values and content are set for virtual pages.
/// </summary>
public interface IUmbracoVirtualPageRoute
{
    /// <summary>
    /// This sets up the virtual page route for the current request if a mtahcing endpoint is found.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>Nothing</returns>
    Task SetupVirtualPageRoute(HttpContext httpContext);

    /// <summary>
    /// Finds the content from the custom route finder delegate or the virtual page controller.
    /// Note - This creates a dummay action executing context so the FindContent method of the
    /// IVirtualPageController can be called (without changing the interface contract).
    /// </summary>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="routeValues">The route values.</param>
    /// <param name="controllerActionDescriptor">The action descriptor.</param>
    /// <param name="controller">The controller.</param>
    /// <returns></returns>
    IPublishedContent? FindContent(
        Endpoint endpoint,
        HttpContext httpContext,
        RouteValueDictionary routeValues,
        ControllerActionDescriptor controllerActionDescriptor,
        object controller);

    /// <summary>
    /// Finds the content from the custom route finder delegate or the virtual page controller.
    /// </summary>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="actionExecutingContext">The action executing context.</param>
    /// <returns>The published content if found or null.</returns>
    IPublishedContent? FindContent(Endpoint endpoint, ActionExecutingContext actionExecutingContext);

    /// <summary>
    /// Creates the published request for the published content.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="publishedContent">The published content.</param>
    /// <returns>The published request.</returns>
    Task<IPublishedRequest> CreatePublishedRequest(HttpContext httpContext, IPublishedContent publishedContent);

    /// <summary>
    /// Sets the route values for the published content and the controller action descriptor.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="publishedContent">The published content.</param>
    /// <param name="controllerActionDescriptor">The controller action descriptor.</param>
    /// <returns>Nothing.</returns>
    Task SetRouteValues(HttpContext httpContext, IPublishedContent publishedContent, ControllerActionDescriptor controllerActionDescriptor);
}
