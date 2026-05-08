using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IUmbracoBuilder"/> to register custom OpenAPI documents.
/// </summary>
public static class UmbracoBuilderOpenApiExtensions
{
    /// <summary>
    /// Registers a custom OpenAPI document with Umbraco's defaults applied: a <c>ShouldInclude</c> predicate
    /// that filters endpoints by <c>[MapToApi(documentName)]</c> and the Umbraco schema ID convention. Adds the
    /// document to the OpenAPI UI dropdown.
    /// </summary>
    /// <param name="builder">The Umbraco builder.</param>
    /// <param name="documentName">The name/identifier of the OpenAPI document. Matches the <c>[MapToApi]</c> value on controllers that should be included.</param>
    /// <param name="documentTitle">
    /// Optional human-readable title. When supplied, used as the OpenAPI document's <c>Info.Title</c> and as
    /// the label in the OpenAPI UI dropdown. Falls back to <paramref name="documentName"/> when null.
    /// </param>
    /// <param name="configure">
    /// Optional callback to configure the <see cref="OpenApiOptions"/> further. Runs after the Umbraco
    /// defaults, so anything set here overrides them. Use this to set the document <c>Info</c>, opt into
    /// <c>AddBackofficeSecurityRequirements()</c>, add transformers, or replace <c>ShouldInclude</c>.
    /// </param>
    /// <param name="jsonOptionsName">
    /// Optional named <c>JsonOptions</c> to apply to this OpenAPI document. Leave <c>null</c> to use the
    /// default HTTP JSON options.
    /// </param>
    /// <returns>The same <see cref="IUmbracoBuilder"/> for chaining.</returns>
    public static IUmbracoBuilder AddBackOfficeOpenApiDocument(
        this IUmbracoBuilder builder,
        string documentName,
        string? documentTitle = null,
        Action<OpenApiOptions>? configure = null,
        string? jsonOptionsName = null)
    {
        builder.Services.AddOpenApi(
            documentName,
            options =>
            {
                options.ShouldInclude = apiDescription =>
                    apiDescription.ActionDescriptor.HasMapToApiAttribute(documentName);

                options.CreateSchemaReferenceId = UmbracoSchemaIdGenerator.CreateSchemaReferenceId;

                if (documentTitle is not null)
                {
                    options.AddDocumentTransformer((document, _, _) =>
                    {
                        document.Info.Title = documentTitle;
                        return Task.CompletedTask;
                    });
                }

                configure?.Invoke(options);
            });

        builder.Services.AddOpenApiDocumentToUi(documentName, documentTitle);

        if (jsonOptionsName is not null)
        {
            builder.Services.ReplaceOpenApiSchemaService(documentName, jsonOptionsName);
        }

        return builder;
    }
}
