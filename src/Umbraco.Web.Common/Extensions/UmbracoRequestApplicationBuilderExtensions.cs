using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Profiling;
using Umbraco.Core;
using Umbraco.Web.Common.Middleware;

namespace Umbraco.Extensions
{
    public static class UmbracoRequestApplicationBuilderExtensions
    {
        // TODO: Could be internal or part of another call - this is a required system so should't be 'opt-in'
        public static IApplicationBuilder UseUmbracoRequestLifetime(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            var runtime = app.ApplicationServices.GetRequiredService<IRuntime>();
            // can't continue if boot failed
            if (runtime.State.Level <= RuntimeLevel.BootFailed) return app;

            app.UseMiddleware<UmbracoRequestMiddleware>();
            app.UseMiddleware<MiniProfilerMiddleware>();
            return app;
        }
    }

}
