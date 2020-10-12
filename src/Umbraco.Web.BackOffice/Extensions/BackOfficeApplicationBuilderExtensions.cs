using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Umbraco.Web.BackOffice;
using Umbraco.Web.BackOffice.Routing;
using Umbraco.Web.BackOffice.Security;

namespace Umbraco.Extensions
{
    public static class BackOfficeApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUmbraco(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            app.UseStatusCodePages();
            app.UseRouting();

            app.UseUmbracoCore();
            app.UseUmbracoRouting();
            app.UseRequestLocalization();
            app.UseUmbracoRequestLogging();
            app.UseUmbracoBackOffice();
            app.UseUmbracoPreview();
            app.UseUmbracoInstaller();

            return app;
        }

        public static IApplicationBuilder UseUmbracoBackOffice(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            // Important we handle image manipulations before the static files, otherwise the querystring is just ignored.
            // TODO: Since we are dependent on these we need to register them but what happens when we call this multiple times since we are dependent on this for UseUmbracoBackOffice too?
            app.UseImageSharp();
            app.UseStaticFiles();

            if (!app.UmbracoCanBoot()) return app;

            app.UseEndpoints(endpoints =>
            {
                var backOfficeRoutes = app.ApplicationServices.GetRequiredService<BackOfficeAreaRoutes>();
                backOfficeRoutes.CreateRoutes(endpoints);
            });

            app.UseUmbracoRuntimeMinification();

            app.UseMiddleware<PreviewAuthenticationMiddleware>();
            app.UseMiddleware<UnhandledExceptionLoggerMiddleware>();

            return app;
        }

        public static IApplicationBuilder UseUmbracoPreview(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                var previewRoutes = app.ApplicationServices.GetRequiredService<PreviewRoutes>();
                previewRoutes.CreateRoutes(endpoints);
            });

            return app;
        }
    }
}
