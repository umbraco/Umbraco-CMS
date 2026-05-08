using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Extension methods for <see cref="IUmbracoBuilder"/> to register custom OpenAPI documents.
/// </summary>
public static class UmbracoBuilderOpenApiExtensions
{
    /// <summary>
    /// Registers a custom OpenAPI document with Umbraco's defaults applied.
    /// </summary>
    /// <param name="builder">The Umbraco builder.</param>
    /// <param name="documentName">The document name. Matches the <c>[MapToApi]</c> value on controllers to include.</param>
    /// <param name="configure">Optional callback to customize the document.</param>
    /// <returns>The same <see cref="IUmbracoBuilder"/> for chaining.</returns>
    public static IUmbracoBuilder AddBackOfficeOpenApiDocument(
        this IUmbracoBuilder builder,
        string documentName,
        Action<BackOfficeOpenApiDocumentBuilder>? configure = null)
    {
        var documentBuilder = new BackOfficeOpenApiDocumentBuilder(documentName);
        configure?.Invoke(documentBuilder);
        documentBuilder.Build(builder);
        return builder;
    }
}
