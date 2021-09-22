using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog.Context;
using StackExchange.Profiling;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging.Serilog.Enrichers;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Cms.Web.Common.Middleware;
using Umbraco.Cms.Web.Common.Plugins;

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
        public static IUmbracoApplicationBuilder UseUmbraco(this IApplicationBuilder app)
            => new UmbracoApplicationBuilder(app);

        /// <summary>
        /// Returns true if Umbraco <see cref="IRuntimeState"/> is greater than <see cref="RuntimeLevel.BootFailed"/>
        /// </summary>
        public static bool UmbracoCanBoot(this IApplicationBuilder app)
            => app.ApplicationServices.GetRequiredService<IRuntimeState>().UmbracoCanBoot();

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
                app.UseStaticFiles(); // We need static files to show the nice error page.
                app.UseMiddleware<BootFailedMiddleware>();
            }
            else
            {
                app.UseMiddleware<PreviewAuthenticationMiddleware>();
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
        /// Allow static file access for App_Plugins folders
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseUmbracoPluginsStaticFiles(this IApplicationBuilder app)
        {
            var hostingEnvironment = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            var umbracoPluginSettings = app.ApplicationServices.GetRequiredService<IOptionsMonitor<UmbracoPluginSettings>>();

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
