using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Umbraco.Web.Website.Routing;

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
            app.UseUmbracoNoContentPage();

            return app;
        }

        public static IApplicationBuilder UseUmbracoNoContentPage(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                var noContentRoutes = app.ApplicationServices.GetRequiredService<NoContentRoutes>();
                noContentRoutes.CreateRoutes(endpoints);
            });

            return app;
        }
    }
}
