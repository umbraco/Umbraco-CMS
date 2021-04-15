using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.BackOffice.Routing;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Extensions
{
    /// <summary>
    /// <see cref="IUmbracoApplicationBuilder"/> extensions for Umbraco
    /// </summary>
    public static partial class UmbracoApplicationBuilderExtensions
    {
        public static IUmbracoApplicationBuilder UseUmbracoPreviewEndpoints(this IUmbracoApplicationBuilder app)
        {
            app.AppBuilder.UseEndpoints(endpoints =>
            {
                PreviewRoutes previewRoutes = app.AppBuilder.ApplicationServices.GetRequiredService<PreviewRoutes>();
                previewRoutes.CreateRoutes(endpoints);
            });

            return app;
        }
    }
}
