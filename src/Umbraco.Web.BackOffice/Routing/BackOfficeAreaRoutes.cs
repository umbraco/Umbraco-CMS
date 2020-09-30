using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.BackOffice.SignalR;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Routing;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.BackOffice.Routing
{
    /// <summary>
    /// Creates routes for the back office area
    /// </summary>
    public class BackOfficeAreaRoutes : IAreaRoutes
    {
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRuntimeState _runtimeState;
        private readonly UmbracoApiControllerTypeCollection _apiControllers;
        private readonly string _umbracoPathSegment;

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

        public void CreateRoutes(IEndpointRouteBuilder endpoints)
        {
            switch (_runtimeState.Level)
            {
                case RuntimeLevel.Install:
                    // a new install so we don't route the back office
                    break;
                case RuntimeLevel.Upgrade:
                    // for upgrades we only need to route the back office and auth controllers
                    MapMinimalBackOffice(endpoints);
                    break;
                case RuntimeLevel.Run:

                    MapMinimalBackOffice(endpoints);
                    endpoints.MapUmbracoRoute<PreviewController>(_umbracoPathSegment, Constants.Web.Mvc.BackOfficeArea, null);
                    endpoints.MapHub<PreviewHub>(GetPreviewHubRoute());
                    AutoRouteBackOfficeControllers(endpoints);
                    break;
                case RuntimeLevel.BootFailed:
                case RuntimeLevel.Unknown:
                case RuntimeLevel.Boot:
                    break;
            }
        }

        /// <summary>
        /// Returns the path to the signalR hub used for preview
        /// </summary>
        /// <returns>Path to signalR hub</returns>
        public string GetPreviewHubRoute()
        {
            return $"/{_umbracoPathSegment}/{nameof(PreviewHub)}";
        }

        /// <summary>
        /// Map the minimal routes required to load the back office login and auth
        /// </summary>
        /// <param name="endpoints"></param>
        private void MapMinimalBackOffice(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapUmbracoRoute<BackOfficeController>(_umbracoPathSegment, Constants.Web.Mvc.BackOfficeArea,
                string.Empty,
                "Default",
                includeControllerNameInRoute: false,
                constraints:
                    // Limit the action/id to only allow characters - this is so this route doesn't hog all other
                    // routes like: /umbraco/channels/word.aspx, etc...
                    // (Not that we have to worry about too many of those these days, there still might be a need for these constraints).
                    new
                    {
                        action = @"[a-zA-Z]*",
                        id = @"[a-zA-Z]*"
                    });

            endpoints.MapUmbracoApiRoute<AuthenticationController>(_umbracoPathSegment, Constants.Web.Mvc.BackOfficeApiArea, true, defaultAction: string.Empty);
        }

        /// <summary>
        /// Auto-routes all back office controllers
        /// </summary>
        private void AutoRouteBackOfficeControllers(IEndpointRouteBuilder endpoints)
        {
            // TODO: We could investigate dynamically routing plugin controllers so we don't have to eagerly type scan for them,
            // it would probably work well, see https://www.strathweb.com/2019/08/dynamic-controller-routing-in-asp-net-core-3-0/
            // will probably be what we use for front-end routing too. BTW the orig article about migrating from IRouter to endpoint
            // routing for things like a CMS is here https://github.com/dotnet/aspnetcore/issues/4221

            foreach (var controller in _apiControllers)
            {
                // exclude front-end api controllers
                var meta = PluginController.GetMetadata(controller);
                if (!meta.IsBackOffice) continue;

                endpoints.MapUmbracoApiRoute(
                    meta.ControllerType,
                    _umbracoPathSegment,
                    meta.AreaName,
                    true,
                    defaultAction: string.Empty); // no default action (this is what we had before)
            }
        }
    }
}
