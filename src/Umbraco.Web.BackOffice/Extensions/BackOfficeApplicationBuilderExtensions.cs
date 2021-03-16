using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.BackOffice.Middleware;
using Umbraco.Cms.Web.BackOffice.Routing;
using Umbraco.Cms.Web.BackOffice.Security;

namespace Umbraco.Extensions
{
    /// <summary>
    /// <see cref="IApplicationBuilder"/> extensions for Umbraco
    /// </summary>
    public static class BackOfficeApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUmbracoBackOffice(this IApplicationBuilder app)
        {
            // NOTE: This method will have been called after UseRouting, UseAuthentication, UseAuthorization
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (!app.UmbracoCanBoot())
            {
                return app;
            }

            app.UseEndpoints(endpoints =>
            {
                BackOfficeAreaRoutes backOfficeRoutes = app.ApplicationServices.GetRequiredService<BackOfficeAreaRoutes>();
                backOfficeRoutes.CreateRoutes(endpoints);
            });

            app.UseUmbracoRuntimeMinification();

            app.UseMiddleware<BackOfficeExternalLoginProviderErrorMiddleware>();

            app.UseUmbracoPreview();

            return app;
        }

        public static IApplicationBuilder UseUmbracoPreview(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                PreviewRoutes previewRoutes = app.ApplicationServices.GetRequiredService<PreviewRoutes>();
                previewRoutes.CreateRoutes(endpoints);
            });

            return app;
        }
    }
}
