using System;
using Microsoft.AspNetCore.Builder;
using Smidge;

namespace Umbraco.Web.Common.AspNetCore
{
    public static class UmbracoRuntimeMinificationApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUmbracoRuntimeMinification(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            app.UseSmidge();

            return app;
        }
    }
}
