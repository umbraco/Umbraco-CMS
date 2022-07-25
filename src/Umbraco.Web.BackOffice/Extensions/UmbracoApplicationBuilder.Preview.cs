using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.BackOffice.Routing;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Extensions;

/// <summary>
///     <see cref="IUmbracoEndpointBuilderContext" /> extensions for Umbraco
/// </summary>
public static partial class UmbracoApplicationBuilderExtensions
{
    public static IUmbracoEndpointBuilderContext UseUmbracoPreviewEndpoints(this IUmbracoEndpointBuilderContext app)
    {
        PreviewRoutes previewRoutes = app.ApplicationServices.GetRequiredService<PreviewRoutes>();
        previewRoutes.CreateRoutes(app.EndpointRouteBuilder);

        return app;
    }
}
