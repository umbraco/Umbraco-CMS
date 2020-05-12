using System;
using Microsoft.AspNetCore.Builder;
using SixLabors.ImageSharp.Web.DependencyInjection;

namespace Umbraco.Extensions
{
    public static class UmbracoWebsiteApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUmbracoWebsite(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            if (!app.UmbracoCanBoot()) return app;

            // Important we handle image manipulations before the static files, otherwise the querystring is just ignored.
            // TODO: Since we are dependent on these we need to register them but what happens when we call this multiple times since we are dependent on this for UseUmbracoBackOffice too?
            app.UseImageSharp();
            app.UseStaticFiles();

            return app;
        }
    }
}
