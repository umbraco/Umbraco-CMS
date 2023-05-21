using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Routing;

/// <summary>
///     Creates routes for the back office area
/// </summary>
public sealed class BackOfficeAreaRoutes : IAreaRoutes
{
    private readonly UmbracoApiControllerTypeCollection _apiControllers;
    private readonly GlobalSettings _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IRuntimeState _runtimeState;
    private readonly string _umbracoPathSegment;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeAreaRoutes" /> class.
    /// </summary>
    public BackOfficeAreaRoutes(
        IOptions<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment,
        IRuntimeState runtimeState,
        UmbracoApiControllerTypeCollection apiControllers)
    {
        _globalSettings = globalSettings.Value;
        _hostingEnvironment = hostingEnvironment;
        _runtimeState = runtimeState;
        _apiControllers = apiControllers;
        _umbracoPathSegment = _globalSettings.GetUmbracoMvcArea(_hostingEnvironment);
    }

    /// <inheritdoc />
    public void CreateRoutes(IEndpointRouteBuilder endpoints)
    {
        switch (_runtimeState.Level)
        {
            case RuntimeLevel.Install:
            case RuntimeLevel.Upgrade:
            case RuntimeLevel.Run:

                MapMinimalBackOffice(endpoints);
                AutoRouteBackOfficeApiControllers(endpoints);
                break;
            case RuntimeLevel.BootFailed:
            case RuntimeLevel.Unknown:
            case RuntimeLevel.Boot:
                break;
        }
    }

    /// <summary>
    ///     Map the minimal routes required to load the back office login and auth
    /// </summary>
    private void MapMinimalBackOffice(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapUmbracoRoute<BackOfficeController>(
            _umbracoPathSegment,
            Constants.Web.Mvc.BackOfficeArea,
            string.Empty,
            "Default",
            false,
            // Limit the action/id to only allow characters - this is so this route doesn't hog all other
            // routes like: /umbraco/channels/word.aspx, etc...
            // (Not that we have to worry about too many of those these days, there still might be a need for these constraints).
            new { action = @"[a-zA-Z]*", id = @"[a-zA-Z]*" });

        endpoints.MapUmbracoApiRoute<AuthenticationController>(_umbracoPathSegment, Constants.Web.Mvc.BackOfficeApiArea, true, string.Empty);
    }

    /// <summary>
    ///     Auto-routes all back office api controllers
    /// </summary>
    private void AutoRouteBackOfficeApiControllers(IEndpointRouteBuilder endpoints)
    {
        // TODO: We could investigate dynamically routing plugin controllers so we don't have to eagerly type scan for them,
        // it would probably work well, see https://www.strathweb.com/2019/08/dynamic-controller-routing-in-asp-net-core-3-0/
        // will probably be what we use for front-end routing too. BTW the orig article about migrating from IRouter to endpoint
        // routing for things like a CMS is here https://github.com/dotnet/aspnetcore/issues/4221

        foreach (Type controller in _apiControllers)
        {
            PluginControllerMetadata meta = PluginController.GetMetadata(controller);

            // exclude front-end api controllers
            if (!meta.IsBackOffice)
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
