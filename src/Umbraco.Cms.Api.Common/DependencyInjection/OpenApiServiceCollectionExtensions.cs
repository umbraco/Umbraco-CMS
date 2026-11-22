using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to configure OpenAPI services.
/// </summary>
public static class OpenApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds an OpenAPI document to the documents offered by the OpenAPI UI.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <param name="documentName">The name/identifier of the OpenAPI document.</param>
    /// <param name="documentTitle">The title to display in the UI. Defaults to <paramref name="documentName"/> if not specified.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddOpenApiDocumentToUi(
        this IServiceCollection services,
        string documentName,
        string? documentTitle = null)
        => services.AddOpenApiDocumentToUi(documentName, () => documentTitle);

    /// <summary>
    /// Adds an OpenAPI document to the documents offered by the OpenAPI UI, resolving the title lazily so
    /// callers (such as builder-pattern helpers) can defer it until the UI options are resolved.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <param name="documentName">The name/identifier of the OpenAPI document.</param>
    /// <param name="documentTitleFactory">Factory invoked when the UI options are resolved. Returning <c>null</c> falls back to <paramref name="documentName"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    internal static IServiceCollection AddOpenApiDocumentToUi(
        this IServiceCollection services,
        string documentName,
        Func<string?> documentTitleFactory)
    {
        services.AddOptions<UmbracoOpenApiUiOptions>()
            .Configure<IOptions<UmbracoOpenApiOptions>>((uiOptions, openApiOptions) =>
            {
                var url = openApiOptions.Value.RouteTemplate.Replace("{documentName}", documentName).EnsureStartsWith("/");
                var document = new OpenApiUiDocument(documentName, documentTitleFactory() ?? documentName, url);

                OpenApiUiDocument? existing = uiOptions.Documents.FirstOrDefault(x => x.Name == documentName);
                if (existing is not null)
                {
                    uiOptions.Documents[uiOptions.Documents.IndexOf(existing)] = document;
                }
                else
                {
                    uiOptions.Documents.Add(document);
                }
            });

        return services;
    }
}
