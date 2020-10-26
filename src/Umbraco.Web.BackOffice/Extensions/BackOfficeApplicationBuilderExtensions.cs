using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Umbraco.Composing;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.IO;
using Umbraco.Web.BackOffice.Middleware;
using Umbraco.Web.BackOffice.Routing;

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


            // TODO: remove current class, it's on its last legs.
            Current.Initialize(
                app.ApplicationServices.GetService<ILogger<object>>(),
                app.ApplicationServices.GetService<IOptions<SecuritySettings>>().Value,
                app.ApplicationServices.GetService<IOptions<GlobalSettings>>().Value,
                app.ApplicationServices.GetService<IIOHelper>(),
                app.ApplicationServices.GetService<Umbraco.Core.Hosting.IHostingEnvironment>(),
                app.ApplicationServices.GetService<IBackOfficeInfo>(),
                app.ApplicationServices.GetService<Umbraco.Core.Logging.IProfiler>()
            );

            return app;
        }

        public static IApplicationBuilder UseUmbracoBackOffice(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            // Important we handle image manipulations before the static files, otherwise the querystring is just ignored.
            // TODO: Since we are dependent on these we need to register them but what happens when we call this multiple times since we are dependent on this for UseUmbracoBackOffice too?
            app.UseImageSharp();
            app.UseStaticFiles();

            // Must be called after UseRouting and before UseEndpoints
            app.UseSession();

            if (!app.UmbracoCanBoot()) return app;

            app.UseEndpoints(endpoints =>
            {
                var backOfficeRoutes = app.ApplicationServices.GetRequiredService<BackOfficeAreaRoutes>();
                backOfficeRoutes.CreateRoutes(endpoints);
            });

            app.UseUmbracoRuntimeMinification();

            app.UseMiddleware<PreviewAuthenticationMiddleware>();
            app.UseMiddleware<BackOfficeExternalLoginProviderErrorMiddleware>();

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
