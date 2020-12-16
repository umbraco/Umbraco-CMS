using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Web.BackOffice.Middleware;
using Umbraco.Web.BackOffice.Routing;
using Umbraco.Web.Common.Security;

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

            app.UseBackOfficeUserManagerAuditing();

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
            // TODO: I'm unsure this middleware will execute before the endpoint, we'll have to see
            app.UseMiddleware<PreviewAuthenticationMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                PreviewRoutes previewRoutes = app.ApplicationServices.GetRequiredService<PreviewRoutes>();
                previewRoutes.CreateRoutes(endpoints);
            });

            return app;
        }

        private static IApplicationBuilder UseBackOfficeUserManagerAuditing(this IApplicationBuilder app)
        {
            var auditer = app.ApplicationServices.GetRequiredService<BackOfficeUserManagerAuditer>();
            auditer.Start();
            return app;
        }
    }
}
