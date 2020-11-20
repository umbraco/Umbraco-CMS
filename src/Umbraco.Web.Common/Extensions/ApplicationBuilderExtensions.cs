using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Smidge;
using Smidge.Nuglify;
using StackExchange.Profiling;
using Umbraco.Core;
using Umbraco.Core.Hosting;
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
            var runtime = app.ApplicationServices.GetRequiredService<IRuntime>();
            // can't continue if boot failed
            return runtime.State.Level > RuntimeLevel.BootFailed;
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

            var runtime = app.ApplicationServices.GetRequiredService<IRuntime>();

            // Register a listener for application shutdown in order to terminate the runtime
            var hostLifetime = app.ApplicationServices.GetRequiredService<IApplicationShutdownRegistry>();
            var runtimeShutdown = new CoreRuntimeShutdown(runtime, hostLifetime);
            hostLifetime.RegisterObject(runtimeShutdown);

            // Register our global threadabort enricher for logging
            var threadAbortEnricher = app.ApplicationServices.GetRequiredService<ThreadAbortExceptionEnricher>();
            LogContext.Push(threadAbortEnricher); // NOTE: We are not in a using clause because we are not removing it, it is on the global context

            StaticApplicationLogging.Initialize(app.ApplicationServices.GetRequiredService<ILoggerFactory>());

            // Start the runtime!
            runtime.Start();

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

        /// <summary>
        /// Ensures the runtime is shutdown when the application is shutting down
        /// </summary>
        private class CoreRuntimeShutdown : IRegisteredObject
        {
            public CoreRuntimeShutdown(IRuntime runtime, IApplicationShutdownRegistry hostLifetime)
            {
                _runtime = runtime;
                _hostLifetime = hostLifetime;
            }

            private bool _completed = false;
            private readonly IRuntime _runtime;
            private readonly IApplicationShutdownRegistry _hostLifetime;

            public void Stop(bool immediate)
            {
                if (!_completed)
                {
                    _completed = true;
                    _runtime.Terminate();
                    _hostLifetime.UnregisterObject(this);
                }

            }
        }
    }

}
