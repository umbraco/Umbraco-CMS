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

        /// <summary>
        /// Initializes a new instance of the <see cref="NoContentRoutes"/> class.
        /// </summary>
        public NoContentRoutes(
            IOptions<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment,
            IRuntimeState runtimeState)
        {
            _runtimeState = runtimeState;
            _umbracoPathSegment = globalSettings.Value.GetUmbracoMvcArea(hostingEnvironment);
        }

        /// <inheritdoc/>
        public void CreateRoutes(IEndpointRouteBuilder endpoints)
        {
            switch (_runtimeState.Level)
            {
                case RuntimeLevel.Install:
                    break;
                case RuntimeLevel.Upgrade:
                    break;
                case RuntimeLevel.Run:

                    // TODO: I don't really think this is working AFAIK the code has just been migrated but it's not really enabled
                    // yet. Our route handler needs to be aware that there is no content and redirect there. Though, this could all be
                    // managed directly in UmbracoRouteValueTransformer. Else it could actually do a 'redirect' but that would need to be
                    // an internal rewrite.
                    endpoints.MapControllerRoute(
                        Constants.Web.NoContentRouteName, // named consistently
                        _umbracoPathSegment + "/UmbNoContent",
                        new { controller = "RenderNoContent", action = "Index" });
                    break;
                case RuntimeLevel.BootFailed:
                case RuntimeLevel.Unknown:
                case RuntimeLevel.Boot:
                    break;
            }
        }
    }
}
