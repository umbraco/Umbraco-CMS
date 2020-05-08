using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Profiling;
using Umbraco.Core;
using Umbraco.Web.Common.Middleware;

namespace Umbraco.Extensions
{
    public static class UmbracoCommonApplicationBuilderExtensions
    {
        public static bool UmbracoCanBoot(this IApplicationBuilder app)
        {
            var runtime = app.ApplicationServices.GetRequiredService<IRuntime>();
            // can't continue if boot failed
            return runtime.State.Level > RuntimeLevel.BootFailed;
        }

        // TODO: Could be internal or part of another call - this is a required system so should't be 'opt-in'
        public static IApplicationBuilder UseUmbracoRequestLifetime(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            if (!app.UmbracoCanBoot()) return app;

            app.UseMiddleware<UmbracoRequestMiddleware>();
            app.UseMiddleware<MiniProfilerMiddleware>();
            return app;
        }
    }

}
