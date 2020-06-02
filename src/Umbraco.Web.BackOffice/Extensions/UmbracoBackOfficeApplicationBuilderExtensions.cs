using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Umbraco.Web.BackOffice.Routing;

namespace Umbraco.Extensions
{
    public static class UmbracoBackOfficeApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUmbracoBackOffice(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            if (!app.UmbracoCanBoot()) return app;

            app.UseEndpoints(endpoints =>
            {
                var backOfficeRoutes = app.ApplicationServices.GetRequiredService<BackOfficeAreaRoutes>();
                backOfficeRoutes.CreateRoutes(endpoints);
            });

            app.UseUmbracoRuntimeMinification();

            // Important we handle image manipulations before the static files, otherwise the querystring is just ignored.
            // TODO: Since we are dependent on these we need to register them but what happens when we call this multiple times since we are dependent on this for UseUmbracoWebsite too?
            app.UseImageSharp();
            app.UseStaticFiles();

            return app;
        }

        
    }
}
