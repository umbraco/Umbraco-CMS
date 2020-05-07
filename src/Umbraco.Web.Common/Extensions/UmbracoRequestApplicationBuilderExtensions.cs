using System;
using Microsoft.AspNetCore.Builder;
using StackExchange.Profiling;
using Umbraco.Web.Common.Middleware;

namespace Umbraco.Extensions
{
    public static class UmbracoRequestApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUmbracoRequest(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }


            app.UseMiddleware<UmbracoRequestMiddleware>();
            app.UseMiddleware<MiniProfilerMiddleware>();
            return app;
        }
    }

}
