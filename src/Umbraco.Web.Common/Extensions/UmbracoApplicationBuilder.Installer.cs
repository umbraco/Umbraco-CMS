using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Cms.Web.Common.Install;

namespace Umbraco.Extensions
{
    /// <summary>
    /// <see cref="IApplicationBuilder"/> extensions for Umbraco installer
    /// </summary>
    public static partial class UmbracoApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables the Umbraco installer
        /// </summary>
        public static IUmbracoEndpointBuilder UseInstallerEndpoints(this IUmbracoEndpointBuilder app)
        {
            if (!app.RuntimeState.UmbracoCanBoot())
            {
                return app;
            }

            InstallAreaRoutes installerRoutes = app.ApplicationServices.GetRequiredService<InstallAreaRoutes>();
            installerRoutes.CreateRoutes(app.EndpointRouteBuilder);

            return app;
        }
    }
}
