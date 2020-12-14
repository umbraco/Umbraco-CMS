using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Smidge;
using Smidge.Nuglify;
using StackExchange.Profiling;
using Umbraco.Core;
using Umbraco.Core.Hosting;
using Umbraco.Infrastructure.Logging.Serilog.Enrichers;
using Umbraco.Web.Common.Middleware;
using Umbraco.Web.PublishedCache.NuCache;

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

            // UseRouting adds endpoint routing middleware, this means that middlewares registered after this one
            // will execute after endpoint routing. The ordering of everything is quite important here, see
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-5.0
            // where we need to have UseAuthentication and UseAuthorization proceeding this call but before
            // endpoints are defined.
            app.UseRouting();
            app.UseRequestLocalization();
            app.UseAuthentication();
            app.UseAuthorization();

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
            IRuntime runtime = app.ApplicationServices.GetRequiredService<IRuntime>();

            // can't continue if boot failed
            return runtime.State.Level > RuntimeLevel.BootFailed;
        }

        /// <summary>
        /// Start Umbraco
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

            IHostingEnvironment hostingEnvironment = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            AppDomain.CurrentDomain.SetData("DataDirectory", hostingEnvironment?.MapPathContentRoot(Constants.SystemDirectories.Data));

            IRuntime runtime = app.ApplicationServices.GetRequiredService<IRuntime>();

            // Register a listener for application shutdown in order to terminate the runtime
            IApplicationShutdownRegistry hostLifetime = app.ApplicationServices.GetRequiredService<IApplicationShutdownRegistry>();
            var runtimeShutdown = new CoreRuntimeShutdown(runtime, hostLifetime);
            hostLifetime.RegisterObject(runtimeShutdown);

            // Register our global threadabort enricher for logging
            ThreadAbortExceptionEnricher threadAbortEnricher = app.ApplicationServices.GetRequiredService<ThreadAbortExceptionEnricher>();
            LogContext.Push(threadAbortEnricher); // NOTE: We are not in a using clause because we are not removing it, it is on the global context

            StaticApplicationLogging.Initialize(app.ApplicationServices.GetRequiredService<ILoggerFactory>());

            // Start the runtime!
            runtime.Start();

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
