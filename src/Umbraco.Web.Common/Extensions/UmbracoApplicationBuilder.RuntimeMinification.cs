using System;
using Smidge;
using Smidge.Nuglify;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Extensions
{
    public static partial class UmbracoApplicationBuilderExtensions
    {
        /// <summary>
        /// Enables runtime minification for Umbraco
        /// </summary>
        public static IUmbracoEndpointBuilder UseUmbracoRuntimeMinificationEndpoints(this IUmbracoEndpointBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (!app.RuntimeState.UmbracoCanBoot())
            {
                return app;
            }

            app.AppBuilder.UseSmidge();
            app.AppBuilder.UseSmidgeNuglify();

            return app;
        }
    }
}
