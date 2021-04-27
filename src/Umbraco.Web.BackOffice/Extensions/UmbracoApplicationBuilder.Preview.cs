using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.BackOffice.Routing;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Extensions
{
    /// <summary>
    /// <see cref="IUmbracoEndpointBuilder"/> extensions for Umbraco
    /// </summary>
    public static partial class UmbracoApplicationBuilderExtensions
    {
        public static IUmbracoEndpointBuilder UseUmbracoPreviewEndpoints(this IUmbracoEndpointBuilder app)
        {
            PreviewRoutes previewRoutes = app.ApplicationServices.GetRequiredService<PreviewRoutes>();
            previewRoutes.CreateRoutes(app.EndpointRouteBuilder);

            return app;
        }
    }
}
