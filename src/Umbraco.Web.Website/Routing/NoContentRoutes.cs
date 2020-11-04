using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Web.Common.Routing;

namespace Umbraco.Web.Website.Routing
{
    /// <summary>
    /// Creates route for the no content page
    /// </summary>
    public class NoContentRoutes : IAreaRoutes
    {
        private readonly IRuntimeState _runtimeState;
        private readonly string _umbracoPathSegment;

        public NoContentRoutes(
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
                    endpoints.MapControllerRoute(
                        // named consistently
                        Constants.Web.NoContentRouteName,
                        _umbracoPathSegment + "/UmbNoContent",
                        new { controller = "RenderNoContent", action = "Index" }
                        );
                     break;
                case RuntimeLevel.BootFailed:
                case RuntimeLevel.Unknown:
                case RuntimeLevel.Boot:
                    break;
            }
        }
    }
}
