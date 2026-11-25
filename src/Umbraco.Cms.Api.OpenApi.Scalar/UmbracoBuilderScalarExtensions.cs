using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.OpenApi.Scalar;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for <see cref="IUmbracoBuilder"/> to serve the OpenAPI documents with Scalar.
/// </summary>
public static class UmbracoBuilderScalarExtensions
{
    /// <summary>
    /// Serves the OpenAPI documents with the Scalar API reference, replacing any other OpenAPI UI.
    /// </summary>
    /// <param name="builder">The Umbraco builder.</param>
    /// <returns>The same <see cref="IUmbracoBuilder"/> for chaining.</returns>
    public static IUmbracoBuilder AddScalarOpenApiUi(this IUmbracoBuilder builder)
    {
        builder.Services.ConfigureOptions<ConfigureScalarOptions>();
        return builder.AddOpenApiUi<ScalarOpenApiUiProvider>();
    }
}
