using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Umbraco.Cms.Api.Delivery.OpenApi.Extensions;

/// <summary>
/// Provides extension methods for <see cref="OpenApiSchemaTransformerContext"/>.
/// </summary>
internal static class OpenApiSchemaTransformerContextExtensions
{
    /// <summary>
    /// Gets the OpenAPI document from the context, throwing if it is null.
    /// </summary>
    /// <param name="context">The schema transformer context.</param>
    /// <returns>The OpenAPI document.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the document is null.</exception>
    public static OpenApiDocument GetRequiredDocument(this OpenApiSchemaTransformerContext context)
        => context.Document ?? throw new InvalidOperationException("OpenAPI document context is required for schema registration.");
}
