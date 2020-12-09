using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Web.BackOffice.Middleware;
using Umbraco.Web.BackOffice.Plugins;
using Umbraco.Web.BackOffice.Routing;
using Umbraco.Web.Common.Security;

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
            app.UseUmbracoPlugins();
            app.UseUmbracoPreview();
            app.UseUmbracoInstaller();

            return app;
        }

        public static IApplicationBuilder UseUmbracoBackOffice(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            app.UseBackOfficeUserManagerAuditing();

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

        public static IApplicationBuilder UseUmbracoPlugins(this IApplicationBuilder app)
        {
            var hostingEnvironment = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var umbracoPluginSettings = app.ApplicationServices.GetRequiredService<IOptions<UmbracoPluginSettings>>();

            var pluginFolder = hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.AppPlugins);

            // Ensure the plugin folder exists
            Directory.CreateDirectory(pluginFolder);

            var fileProvider = new UmbracoPluginPhysicalFileProvider(
                pluginFolder,
                umbracoPluginSettings);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                RequestPath = Constants.SystemDirectories.AppPlugins
            });

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
        private static IApplicationBuilder UseBackOfficeUserManagerAuditing(this IApplicationBuilder app)
        {
            var auditer = app.ApplicationServices.GetRequiredService<BackOfficeUserManagerAuditer>();
            auditer.Start();
            return app;
        }
    }
}
