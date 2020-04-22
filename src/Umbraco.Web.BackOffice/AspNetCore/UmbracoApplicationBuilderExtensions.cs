using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Context;
using Smidge;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Infrastructure.Logging.Serilog.Enrichers;
using Umbraco.Web.Common.Middleware;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    public static class UmbracoApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUmbracoBackOffice(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            return app;
        }

        /// <summary>
        /// Start Umbraco
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseUmbracoCore(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            // Register a listener for application shutdown in order to terminate the runtime
            var hostLifetime = app.ApplicationServices.GetRequiredService<IApplicationShutdownRegistry>();
            var runtime = app.ApplicationServices.GetRequiredService<IRuntime>();
            var runtimeShutdown = new CoreRuntimeShutdown(runtime, hostLifetime);
            hostLifetime.RegisterObject(runtimeShutdown);

            // Register our global threadabort enricher for logging
            var threadAbortEnricher = app.ApplicationServices.GetRequiredService<ThreadAbortExceptionEnricher>();
            LogContext.Push(threadAbortEnricher); // NOTE: We are not in a using clause because we are not removing it, it is on the global context

            // Start the runtime!
            runtime.Start();

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

        public static IApplicationBuilder UseUmbracoRequestLogging(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            app.UseMiddleware<UmbracoRequestLoggingMiddleware>();

            return app;
        }

        public static IApplicationBuilder UseUmbracoRuntimeMinification(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            app.UseSmidge();

            return app;
        }
    }
}
