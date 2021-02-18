using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.Common.Install;

namespace Umbraco.Extensions
{
    /// <summary>
    /// <see cref="IApplicationBuilder"/> extensions for Umbraco installer
    /// </summary>
    public static class UmbracoInstallApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables the Umbraco installer
        /// </summary>
        public static IApplicationBuilder UseUmbracoInstaller(this IApplicationBuilder app)
        {
            if (!app.UmbracoCanBoot())
            {
                return app;
            }

            app.UseEndpoints(endpoints =>
            {
                InstallAreaRoutes installerRoutes = app.ApplicationServices.GetRequiredService<InstallAreaRoutes>();
                installerRoutes.CreateRoutes(endpoints);
            });

            return app;
        }
    }
}
