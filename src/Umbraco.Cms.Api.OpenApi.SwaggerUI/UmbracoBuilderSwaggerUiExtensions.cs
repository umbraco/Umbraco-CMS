using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.OpenApi.SwaggerUI;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for <see cref="IUmbracoBuilder"/> to serve the OpenAPI documents with Swagger UI.
/// </summary>
public static class UmbracoBuilderSwaggerUiExtensions
{
    /// <summary>
    /// Serves the OpenAPI documents with Swagger UI, replacing any other OpenAPI UI.
    /// </summary>
    /// <param name="builder">The Umbraco builder.</param>
    /// <returns>The same <see cref="IUmbracoBuilder"/> for chaining.</returns>
    public static IUmbracoBuilder AddSwaggerUiOpenApiUi(this IUmbracoBuilder builder)
    {
        builder.AddSwaggerUiServices();
        return builder.AddOpenApiUi<SwaggerUiOpenApiUiProvider>();
    }

    /// <summary>
    /// Serves the OpenAPI documents with Swagger UI, unless another OpenAPI UI has been chosen.
    /// </summary>
    /// <param name="builder">The Umbraco builder.</param>
    /// <returns>The same <see cref="IUmbracoBuilder"/> for chaining.</returns>
    internal static IUmbracoBuilder TryAddSwaggerUiOpenApiUi(this IUmbracoBuilder builder)
    {
        builder.AddSwaggerUiServices();
        return builder.TryAddOpenApiUi<SwaggerUiOpenApiUiProvider>();
    }

    private static void AddSwaggerUiServices(this IUmbracoBuilder builder)
        => builder.Services.ConfigureOptions<ConfigureSwaggerUiOptions>();
}
