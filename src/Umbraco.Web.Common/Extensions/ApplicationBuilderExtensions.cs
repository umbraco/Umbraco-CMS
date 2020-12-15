using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Context;
using Smidge;
using Smidge.Nuglify;
using StackExchange.Profiling;
using Umbraco.Core;
using Umbraco.Infrastructure.Logging.Serilog.Enrichers;
using Umbraco.Web.Common.Middleware;

namespace Umbraco.Extensions
{

    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Returns true if Umbraco <see cref="IRuntimeState"/> is greater than <see cref="RuntimeLevel.BootFailed"/>
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static bool UmbracoCanBoot(this IApplicationBuilder app)
        {
            var state = app.ApplicationServices.GetRequiredService<IRuntimeState>();
            // can't continue if boot failed
            return state.Level > RuntimeLevel.BootFailed;
        }

        /// <summary>
        /// Start Umbraco
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseUmbracoCore(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            if (!app.UmbracoCanBoot()) return app;

            // Register our global threadabort enricher for logging
            var threadAbortEnricher = app.ApplicationServices.GetRequiredService<ThreadAbortExceptionEnricher>();
            LogContext.Push(threadAbortEnricher); // NOTE: We are not in a using clause because we are not removing it, it is on the global context

            return app;
        }

        /// <summary>
        /// Enables middlewares required to run Umbraco
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        // TODO: Could be internal or part of another call - this is a required system so should't be 'opt-in'
        public static IApplicationBuilder UseUmbracoRouting(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            if (!app.UmbracoCanBoot())
            {
                app.UseMiddleware<BootFailedMiddleware>();
            }
            else
            {
                app.UseMiddleware<UmbracoRequestMiddleware>();
                app.UseMiddleware<MiniProfilerMiddleware>();

                // TODO: Both of these need to be done before any endpoints but after UmbracoRequestMiddleware
                // because they rely on an UmbracoContext. But should they be here?
                app.UseAuthentication();
                app.UseAuthorization();
            }

            return app;
        }

        /// <summary>
        /// Adds request based serilog enrichers to the LogContext for each request
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseUmbracoRequestLogging(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            if (!app.UmbracoCanBoot()) return app;

            app.UseMiddleware<UmbracoRequestLoggingMiddleware>();

            return app;
        }

        /// <summary>
        /// Enables runtime minification for Umbraco
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseUmbracoRuntimeMinification(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            if (!app.UmbracoCanBoot()) return app;

            app.UseSmidge();
            app.UseSmidgeNuglify();

            return app;
        }
    }

}
