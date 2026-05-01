using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to configure OpenAPI services.
/// </summary>
public static class OpenApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds an OpenAPI document to the OpenAPI UI document selector dropdown.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <param name="documentName">The name/identifier of the OpenAPI document.</param>
    /// <param name="documentTitle">The title to display in the UI dropdown. Defaults to <paramref name="documentName"/> if not specified.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddOpenApiDocumentToUi(
        this IServiceCollection services,
        string documentName,
        string? documentTitle = null)
    {
        services.AddOptions<SwaggerUIOptions>()
            .Configure<IOptions<UmbracoOpenApiOptions>>((swaggerUiOptions, openApiOptions) =>
            {
                var openApiRoute = openApiOptions.Value.RouteTemplate.Replace("{documentName}", documentName).EnsureStartsWith("/");
                swaggerUiOptions.SwaggerEndpoint(openApiRoute, documentTitle ?? documentName);
                swaggerUiOptions.ConfigObject.Urls = swaggerUiOptions.ConfigObject.Urls.OrderBy(x => x.Name);
            });

        return services;
    }
}
