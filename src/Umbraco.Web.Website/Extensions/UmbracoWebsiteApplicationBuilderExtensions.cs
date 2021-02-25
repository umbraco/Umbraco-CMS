using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.Website.Routing;

namespace Umbraco.Extensions
{
    /// <summary>
    /// <see cref="IApplicationBuilder"/> extensions for the umbraco front-end website
    /// </summary>
    public static class UmbracoWebsiteApplicationBuilderExtensions
    {
        /// <summary>
        /// Sets up services and routes for the front-end umbraco website
        /// </summary>
        public static IApplicationBuilder UseUmbracoWebsite(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (!app.UmbracoCanBoot())
            {
                return app;
            }

            app.UseUmbracoRoutes();

            return app;
        }

        /// <summary>
        /// Sets up routes for the umbraco front-end
        /// </summary>
        public static IApplicationBuilder UseUmbracoRoutes(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                FrontEndRoutes surfaceRoutes = app.ApplicationServices.GetRequiredService<FrontEndRoutes>();
                surfaceRoutes.CreateRoutes(endpoints);

                endpoints.MapDynamicControllerRoute<UmbracoRouteValueTransformer>("/{**slug}");
            });

            return app;
        }
    }
}
