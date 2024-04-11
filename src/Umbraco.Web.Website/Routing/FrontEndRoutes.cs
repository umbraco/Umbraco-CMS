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
    private readonly IRuntimeState _runtimeState;
    private readonly SurfaceControllerTypeCollection _surfaceControllerTypeCollection;

    /// <summary>
    /// Initializes a new instance of the <see cref="FrontEndRoutes" /> class.
    /// </summary>
    public FrontEndRoutes(IRuntimeState runtimeState, SurfaceControllerTypeCollection surfaceControllerTypeCollection)
    {
        _runtimeState = runtimeState;
        _surfaceControllerTypeCollection = surfaceControllerTypeCollection;
    }

    [Obsolete("The globalSettings and hostingEnvironment parameters are not required anymore, use the other constructor instead. This constructor will be removed in a future version.")]
    public FrontEndRoutes(IOptions<GlobalSettings> globalSettings, IHostingEnvironment hostingEnvironment, IRuntimeState runtimeState, SurfaceControllerTypeCollection surfaceControllerTypeCollection)
        : this(runtimeState, surfaceControllerTypeCollection)
    { }

    /// <inheritdoc />
    public void CreateRoutes(IEndpointRouteBuilder endpoints)
    {
        if (_runtimeState.Level is RuntimeLevel.Install or RuntimeLevel.Upgrade or RuntimeLevel.Run)
        {
            AutoRouteSurfaceControllers(endpoints);
        }
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
                Constants.System.UmbracoPathSegment,
                meta.AreaName);
        }
    }
}
