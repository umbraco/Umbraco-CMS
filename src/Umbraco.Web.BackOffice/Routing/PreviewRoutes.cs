using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.BackOffice.SignalR;
using Umbraco.Web.Common.Extensions;
using Umbraco.Web.Common.Routing;

namespace Umbraco.Web.BackOffice.Routing
{
    /// <summary>
    /// Creates routes for the preview hub
    /// </summary>
    public class PreviewRoutes : IAreaRoutes
    {
        private readonly IRuntimeState _runtimeState;
        private readonly string _umbracoPathSegment;

        public PreviewRoutes(
            IOptions<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment,
            IRuntimeState runtimeState)
        {
            _runtimeState = runtimeState;
            _umbracoPathSegment = globalSettings.Value.GetUmbracoMvcArea(hostingEnvironment);
        }

        public void CreateRoutes(IEndpointRouteBuilder endpoints)
        {
            switch (_runtimeState.Level)
            {
                case RuntimeLevel.Install:
                    break;
                case RuntimeLevel.Upgrade:
                    break;
                case RuntimeLevel.Run:
                    endpoints.MapHub<PreviewHub>(GetPreviewHubRoute());
                    endpoints.MapUmbracoRoute<PreviewController>(_umbracoPathSegment, Constants.Web.Mvc.BackOfficeArea, null);
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
    }
}
