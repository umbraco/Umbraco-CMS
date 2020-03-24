using System;
using Microsoft.AspNetCore.Builder;
using Umbraco.Web.Common.Middleware;

namespace Umbraco.Web.Common.Extensions
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

            return app;
        }
    }
}
