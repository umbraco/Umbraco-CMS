using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Profiling;
using Umbraco.Core;
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
        /// Enables middlewares required to run Umbraco
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        // TODO: Could be internal or part of another call - this is a required system so should't be 'opt-in'
        public static IApplicationBuilder UseUmbracoRouting(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            if (!app.UmbracoCanBoot()) return app;

            app.UseMiddleware<UmbracoRequestMiddleware>();
            app.UseMiddleware<MiniProfilerMiddleware>();
            return app;
        }
    }

}
