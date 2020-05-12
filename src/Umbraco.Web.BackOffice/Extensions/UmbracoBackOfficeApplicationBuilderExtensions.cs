using System;
using Microsoft.AspNetCore.Builder;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Web.BackOffice.Extensions
{
    public static class UmbracoBackOfficeApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUmbracoBackOffice(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            if (!app.UmbracoCanBoot()) return app;

            app.UseEndpoints(endpoints =>
            {
                // TODO: This is temporary, 'umbraco' cannot be hard coded, needs to use GetUmbracoMvcArea()
                // but actually we need to route all back office stuff in a back office area like we do in v8

                endpoints.MapControllerRoute("Backoffice", "/umbraco/{Action}", new
                {
                    Controller = "BackOffice",
                    Action = "Default"
                });
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
