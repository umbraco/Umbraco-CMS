using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Extensions;

/// <summary>
///     <see cref="IUmbracoEndpointBuilderContext" /> extensions for Umbraco
/// </summary>
public static partial class UmbracoApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the application to enable Umbraco's content preview endpoints, allowing preview functionality for unpublished or draft content.
    /// </summary>
    /// <param name="app">The Umbraco endpoint builder context to configure.</param>
    /// <returns>The same <see cref="IUmbracoEndpointBuilderContext"/> instance, for chaining.</returns>
    public static IUmbracoEndpointBuilderContext UseUmbracoPreviewEndpoints(this IUmbracoEndpointBuilderContext app)
    {
        PreviewRoutes previewRoutes = app.ApplicationServices.GetRequiredService<PreviewRoutes>();
        previewRoutes.CreateRoutes(app.EndpointRouteBuilder);

        return app;
    }
}
