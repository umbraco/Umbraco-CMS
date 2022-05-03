using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Extensions;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    ///     Used to map Umbraco controllers consistently
    /// </summary>
    public static void MapUmbracoRoute(
        this IEndpointRouteBuilder endpoints,
        Type controllerType,
        string rootSegment,
        string? areaName,
        string? prefixPathSegment,
        string defaultAction = "Index",
        bool includeControllerNameInRoute = true,
        object? constraints = null)
    {
        var controllerName = ControllerExtensions.GetControllerName(controllerType);

        // build the route pattern
        var pattern = new StringBuilder(rootSegment);
        if (!prefixPathSegment.IsNullOrWhiteSpace())
        {
            pattern.Append('/').Append(prefixPathSegment);
        }

        if (includeControllerNameInRoute)
        {
            pattern.Append('/').Append(controllerName);
        }

        pattern.Append("/{action}/{id?}");

        var defaults = defaultAction.IsNullOrWhiteSpace()
            ? (object)new { controller = controllerName }
            : new { controller = controllerName, action = defaultAction };

        if (areaName.IsNullOrWhiteSpace())
        {
            endpoints.MapControllerRoute(
                $"umbraco-{areaName}-{controllerName}".ToLowerInvariant(),
                pattern.ToString().ToLowerInvariant(),
                defaults,
                constraints);
        }
        else
        {
            endpoints.MapAreaControllerRoute(
                $"umbraco-{areaName}-{controllerName}".ToLowerInvariant(),
                areaName!,
                pattern.ToString().ToLowerInvariant(),
                defaults,
                constraints);
        }
    }

    /// <summary>
    ///     Used to map Umbraco controllers consistently
    /// </summary>
    /// <typeparam name="T">The <see cref="ControllerBase" /> type to route</typeparam>
    public static void MapUmbracoRoute<T>(
        this IEndpointRouteBuilder endpoints,
        string rootSegment,
        string areaName,
        string? prefixPathSegment,
        string defaultAction = "Index",
        bool includeControllerNameInRoute = true,
        object? constraints = null)
        where T : ControllerBase
        => endpoints.MapUmbracoRoute(typeof(T), rootSegment, areaName, prefixPathSegment, defaultAction, includeControllerNameInRoute, constraints);

    /// <summary>
    ///     Used to map controllers as Umbraco API routes consistently
    /// </summary>
    /// <typeparam name="T">The <see cref="ControllerBase" /> type to route</typeparam>
    public static void MapUmbracoApiRoute<T>(
        this IEndpointRouteBuilder endpoints,
        string rootSegment,
        string areaName,
        bool isBackOffice,
        string defaultAction = "Index",
        object? constraints = null)
        where T : ControllerBase
        => endpoints.MapUmbracoApiRoute(typeof(T), rootSegment, areaName, isBackOffice, defaultAction, constraints);

    /// <summary>
    ///     Used to map controllers as Umbraco API routes consistently
    /// </summary>
    public static void MapUmbracoApiRoute(
        this IEndpointRouteBuilder endpoints,
        Type controllerType,
        string rootSegment,
        string? areaName,
        bool isBackOffice,
        string defaultAction = "Index",
        object? constraints = null)
    {
        var prefixPathSegment = isBackOffice
            ? areaName.IsNullOrWhiteSpace()
                ? $"{Constants.Web.Mvc.BackOfficePathSegment}/Api"
                : $"{Constants.Web.Mvc.BackOfficePathSegment}/{areaName}"
            : areaName.IsNullOrWhiteSpace()
                ? "Api"
                : areaName;

        endpoints.MapUmbracoRoute(
            controllerType,
            rootSegment,
            areaName,
            prefixPathSegment,
            defaultAction,
            true,
            constraints);
    }

    public static void MapUmbracoSurfaceRoute(
        this IEndpointRouteBuilder endpoints,
        Type controllerType,
        string rootSegment,
        string? areaName,
        string defaultAction = "Index",
        bool includeControllerNameInRoute = true,
        object? constraints = null)
    {
        // If there is an area name it's a plugin controller, and we should use the area name instead of surface
        var prefixPathSegment = areaName.IsNullOrWhiteSpace() ? "Surface" : areaName!;

        endpoints.MapUmbracoRoute(
            controllerType,
            rootSegment,
            areaName,
            prefixPathSegment,
            defaultAction,
            includeControllerNameInRoute,
            constraints);
    }
}
