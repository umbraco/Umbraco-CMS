using System;
using Microsoft.AspNetCore.Builder;
using SixLabors.ImageSharp.Web.DependencyInjection;

namespace Umbraco.Web.Website.AspNetCore
{
    public static class UmbracoBackOfficeApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUmbracoWebsite(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }


            // Important we handle image manipulations before the static files, otherwise the querystring is just ignored.
            app.UseImageSharp();
            app.UseStaticFiles();

            return app;
        }
    }
}
