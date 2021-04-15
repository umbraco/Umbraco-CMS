using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.BackOffice.Routing;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Cms.Web.Common.Extensions;

namespace Umbraco.Extensions
{
    /// <summary>
    /// <see cref="IUmbracoApplicationBuilder"/> extensions for Umbraco
    /// </summary>
    public static partial class UmbracoApplicationBuilderExtensions
    {
        public static IUmbracoApplicationBuilder UseBackOfficeEndpoints(this IUmbracoApplicationBuilder app)
        {
            // NOTE: This method will have been called after UseRouting, UseAuthentication, UseAuthorization
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
                BackOfficeAreaRoutes backOfficeRoutes = app.AppBuilder.ApplicationServices.GetRequiredService<BackOfficeAreaRoutes>();
                backOfficeRoutes.CreateRoutes(endpoints);
            });

            app.UseUmbracoRuntimeMinificationEndpoints();
            app.UseUmbracoPreviewEndpoints();

            return app;
        }
    }
}
