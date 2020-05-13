using Microsoft.AspNetCore.Routing;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Common.Routing;

namespace Umbraco.Web.BackOffice.Routing
{
    public class BackOfficeAreaRoutes : IAreaRoutes
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRuntimeState _runtimeState;
        private readonly string _umbracoAreaPathSegment;

        public BackOfficeAreaRoutes(IGlobalSettings globalSettings, IHostingEnvironment hostingEnvironment, IRuntimeState runtimeState)
        {
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
            _runtimeState = runtimeState;
            _umbracoAreaPathSegment = _globalSettings.GetUmbracoMvcArea(_hostingEnvironment);
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
                    endpoints.MapUmbracoRoute<PreviewController>(_umbracoAreaPathSegment, Constants.Web.Mvc.BackOfficeArea, "preview");
                    AutoRouteBackOfficeControllers(endpoints);

                    break;
                case RuntimeLevel.BootFailed:
                case RuntimeLevel.Unknown:
                case RuntimeLevel.Boot:
                    break;
            }
        }

        /// <summary>
        /// Map the minimal routes required to load the back office login and auth
        /// </summary>
        /// <param name="endpoints"></param>
        private void MapMinimalBackOffice(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapUmbracoRoute<BackOfficeController>(_umbracoAreaPathSegment, Constants.Web.Mvc.BackOfficeArea,
                string.Empty,
                "Default",
                constraints:
                    // Limit the action/id to only allow characters - this is so this route doesn't hog all other
                    // routes like: /umbraco/channels/word.aspx, etc...
                    // (Not that we have to worry about too many of those these days, there still might be a need for these constraints).
                    new
                    {
                        action = @"[a-zA-Z]*",
                        id = @"[a-zA-Z]*"
                    });

            endpoints.MapUmbracoRoute<AuthenticationController>(_umbracoAreaPathSegment, Constants.Web.Mvc.BackOfficeArea, true);
        }

        /// <summary>
        /// Auto-routes all back office controllers
        /// </summary>
        private void AutoRouteBackOfficeControllers(IEndpointRouteBuilder endpoints)
        {

        }
    }
}
