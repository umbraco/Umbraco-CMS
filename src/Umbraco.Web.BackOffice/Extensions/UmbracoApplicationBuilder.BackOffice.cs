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

            if (!app.RuntimeState.UmbracoCanBoot())
            {
                return app;
            }

            BackOfficeAreaRoutes backOfficeRoutes = app.ApplicationServices.GetRequiredService<BackOfficeAreaRoutes>();
            backOfficeRoutes.CreateRoutes(app.EndpointRouteBuilder);

            app.UseUmbracoRuntimeMinificationEndpoints();
            app.UseUmbracoPreviewEndpoints();

            return app;
        }
    }
}
