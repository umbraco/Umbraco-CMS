using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace Umbraco.Cms.Web.Common.Extensions;

/// <summary>
/// Extensions methods for Endpoint.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Gets the controller action descriptor from the endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint.</param>
    /// <returns>The controller action descriptor or null if not found.</returns>
    public static ControllerActionDescriptor? GetControllerActionDescriptor(this Endpoint endpoint)
    {
        return endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
    }

    /// <summary>
    /// Gets the route name metadata from the endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint.</param>
    /// <returns>The route name metadata or null if not found.</returns>
    public static RouteNameMetadata? GetRouteNameMetadata(this Endpoint endpoint)
    {
        return endpoint.Metadata.GetMetadata<RouteNameMetadata>();
    }

    /// <summary>
    /// Gets the route name from the endpoint metadata.
    /// </summary>
    /// <param name="endpoint">The endpoint.</param>
    /// <returns>The route name or null if not found.</returns>
    public static string? GetRouteName(this Endpoint endpoint)
    {
        return endpoint.GetRouteNameMetadata()?.RouteName;
    }
}
