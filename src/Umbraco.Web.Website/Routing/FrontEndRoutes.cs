using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web.Mvc;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Collections;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Routing;

/// <summary>
///     Creates routes for surface controllers
/// </summary>
public sealed class FrontEndRoutes : IAreaRoutes
{
    private readonly UmbracoApiControllerTypeCollection _apiControllers;
    private readonly IRuntimeState _runtimeState;
    private readonly SurfaceControllerTypeCollection _surfaceControllerTypeCollection;
    private readonly string _umbracoPathSegment;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FrontEndRoutes" /> class.
    /// </summary>
    public FrontEndRoutes(
        IOptions<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment,
        IRuntimeState runtimeState,
        SurfaceControllerTypeCollection surfaceControllerTypeCollection,
        UmbracoApiControllerTypeCollection apiControllers)
    {
        _runtimeState = runtimeState;
        _surfaceControllerTypeCollection = surfaceControllerTypeCollection;
        _apiControllers = apiControllers;
        _umbracoPathSegment = globalSettings.Value.GetUmbracoMvcArea(hostingEnvironment);
    }

    /// <inheritdoc />
    public void CreateRoutes(IEndpointRouteBuilder endpoints)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        AutoRouteSurfaceControllers(endpoints);
        AutoRouteFrontEndApiControllers(endpoints);
    }

    /// <summary>
    ///     Auto-routes all front-end surface controllers
    /// </summary>
    private void AutoRouteSurfaceControllers(IEndpointRouteBuilder endpoints)
    {
        foreach (Type controller in _surfaceControllerTypeCollection)
        {
            // exclude front-end api controllers
            PluginControllerMetadata meta = PluginController.GetMetadata(controller);

            endpoints.MapUmbracoSurfaceRoute(
                meta.ControllerType,
                _umbracoPathSegment,
                meta.AreaName);
        }
    }

    /// <summary>
    ///     Auto-routes all front-end api controllers
    /// </summary>
    private void AutoRouteFrontEndApiControllers(IEndpointRouteBuilder endpoints)
    {
        foreach (Type controller in _apiControllers)
        {
            PluginControllerMetadata meta = PluginController.GetMetadata(controller);

            // exclude back-end api controllers
            if (meta.IsBackOffice)
            {
                continue;
            }

            endpoints.MapUmbracoApiRoute(
                meta.ControllerType,
                _umbracoPathSegment,
                meta.AreaName,
                meta.IsBackOffice,
                string.Empty); // no default action (this is what we had before)
        }
    }
}
