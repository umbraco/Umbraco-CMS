using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    public static class UmbracoBackOfficeApplicationBuilderExtensions
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
    }
}
