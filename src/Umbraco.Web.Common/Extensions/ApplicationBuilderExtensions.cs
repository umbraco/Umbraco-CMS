using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog.Context;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Smidge;
using Smidge.Nuglify;
using StackExchange.Profiling;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Middleware;
using Umbraco.Cms.Web.Common.Plugins;
using Umbraco.Infrastructure.Logging.Serilog.Enrichers;

namespace Umbraco.Extensions
{
    /// <summary>
    /// <see cref="IApplicationBuilder"/> extensions for Umbraco
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Configures and use services required for using Umbraco
        /// </summary>
        public static IApplicationBuilder UseUmbraco(this IApplicationBuilder app)
        {
            // TODO: Should we do some checks like this to verify that the corresponding "Add" methods have been called for the
            // corresponding "Use" methods?
            // https://github.com/dotnet/aspnetcore/blob/b795ac3546eb3e2f47a01a64feb3020794ca33bb/src/Mvc/Mvc.Core/src/Builder/MvcApplicationBuilderExtensions.cs#L132
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.UseUmbracoCore();
            app.UseUmbracoRequestLogging();

            // We need to add this before UseRouting so that the UmbracoContext and other middlewares are executed
            // before endpoint routing middleware.
            app.UseUmbracoRouting();

            app.UseStatusCodePages();

            // Important we handle image manipulations before the static files, otherwise the querystring is just ignored.
            // TODO: Since we are dependent on these we need to register them but what happens when we call this multiple times since we are dependent on this for UseUmbracoBackOffice too?
            app.UseImageSharp();
            app.UseStaticFiles();
            app.UseUmbracoPlugins();

            // UseRouting adds endpoint routing middleware, this means that middlewares registered after this one
            // will execute after endpoint routing. The ordering of everything is quite important here, see
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-5.0
            // where we need to have UseAuthentication and UseAuthorization proceeding this call but before
            // endpoints are defined.
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // This must come after auth because the culture is based on the auth'd user
            app.UseRequestLocalization();

            // Must be called after UseRouting and before UseEndpoints
            app.UseSession();

            // Must come after the above!
            app.UseUmbracoInstaller();

            return app;
        }

        /// <summary>
        /// Returns true if Umbraco <see cref="IRuntimeState"/> is greater than <see cref="RuntimeLevel.BootFailed"/>
        /// </summary>
        public static bool UmbracoCanBoot(this IApplicationBuilder app)
        {
            var state = app.ApplicationServices.GetRequiredService<IRuntimeState>();

            // can't continue if boot failed
            return state.Level > RuntimeLevel.BootFailed;
        }

        /// <summary>
        /// Enables core Umbraco functionality
        /// </summary>
        public static IApplicationBuilder UseUmbracoCore(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (!app.UmbracoCanBoot())
            {
                return app;
            }

            // Register our global threadabort enricher for logging
            ThreadAbortExceptionEnricher threadAbortEnricher = app.ApplicationServices.GetRequiredService<ThreadAbortExceptionEnricher>();
            LogContext.Push(threadAbortEnricher); // NOTE: We are not in a using clause because we are not removing it, it is on the global context

            return app;
        }

        /// <summary>
        /// Enables middlewares required to run Umbraco
        /// </summary>
        /// <remarks>
        /// Must occur before UseRouting
        /// </remarks>
        public static IApplicationBuilder UseUmbracoRouting(this IApplicationBuilder app)
        {
            // TODO: This method could be internal or part of another call - this is a required system so should't be 'opt-in'
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (!app.UmbracoCanBoot())
            {
                app.UseMiddleware<BootFailedMiddleware>();
            }
            else
            {
                app.UseMiddleware<UmbracoRequestMiddleware>();
                app.UseMiddleware<MiniProfilerMiddleware>();
            }

            return app;
        }

        /// <summary>
        /// Adds request based serilog enrichers to the LogContext for each request
        /// </summary>
        public static IApplicationBuilder UseUmbracoRequestLogging(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (!app.UmbracoCanBoot()) return app;

            app.UseMiddleware<UmbracoRequestLoggingMiddleware>();

            return app;
        }

        /// <summary>
        /// Enables runtime minification for Umbraco
        /// </summary>
        public static IApplicationBuilder UseUmbracoRuntimeMinification(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (!app.UmbracoCanBoot())
            {
                return app;
            }

            app.UseSmidge();
            app.UseSmidgeNuglify();

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
    }

}
