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
        public static IUmbracoApplicationBuilder UseInstallerEndpoints(this IUmbracoApplicationBuilder app)
        {
            if (!app.AppBuilder.UmbracoCanBoot())
            {
                return app;
            }

            app.AppBuilder.UseEndpoints(endpoints =>
            {
                InstallAreaRoutes installerRoutes = app.AppBuilder.ApplicationServices.GetRequiredService<InstallAreaRoutes>();
                installerRoutes.CreateRoutes(endpoints);
            });

            return app;
        }
    }
}
