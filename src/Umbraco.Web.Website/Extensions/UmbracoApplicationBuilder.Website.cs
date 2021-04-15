using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Cms.Web.Website.Routing;

namespace Umbraco.Extensions
{
    /// <summary>
    /// <see cref="IApplicationBuilder"/> extensions for the umbraco front-end website
    /// </summary>
    public static partial class UmbracoApplicationBuilderExtensions
    {
        /// <summary>
        /// Sets up routes for the front-end umbraco website
        /// </summary>
        public static IUmbracoApplicationBuilder UseWebsiteEndpoints(this IUmbracoApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (!app.AppBuilder.UmbracoCanBoot())
            {
                return app;
            }

            app.AppBuilder.UseEndpoints(endpoints =>
            {
                FrontEndRoutes surfaceRoutes = app.AppBuilder.ApplicationServices.GetRequiredService<FrontEndRoutes>();
                surfaceRoutes.CreateRoutes(endpoints);

                endpoints.MapDynamicControllerRoute<UmbracoRouteValueTransformer>("/{**slug}");
            });

            return app;
        }
    }
}
