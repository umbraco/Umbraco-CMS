using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Cms.Web.Website.Middleware;
using Umbraco.Cms.Web.Website.Routing;

namespace Umbraco.Extensions
{
    /// <summary>
    /// <see cref="IApplicationBuilder"/> extensions for the umbraco front-end website
    /// </summary>
    public static partial class UmbracoApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds all required middleware to run the website
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IUmbracoApplicationBuilder WithWebsite(this IUmbracoApplicationBuilder builder)
        {
            builder.AppBuilder.UseMiddleware<PublicAccessMiddleware>();
            return builder;
        }

        /// <summary>
        /// Sets up routes for the front-end umbraco website
        /// </summary>
        public static IUmbracoEndpointBuilder UseWebsiteEndpoints(this IUmbracoEndpointBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (!builder.RuntimeState.UmbracoCanBoot())
            {
                return builder;
            }

            FrontEndRoutes surfaceRoutes = builder.ApplicationServices.GetRequiredService<FrontEndRoutes>();
            surfaceRoutes.CreateRoutes(builder.EndpointRouteBuilder);
            builder.EndpointRouteBuilder.MapDynamicControllerRoute<UmbracoRouteValueTransformer>("/{**slug}");

            return builder;
        }
    }
}
