using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Delivery.OpenApi.Transformers;

/// <summary>
/// Transforms the OpenAPI document to include API key security scheme.
/// </summary>
internal class ApiKeyTransformer : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
{
    private const string AuthSchemeName = "ApiKeyAuth";

    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var apiKeyScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            Name = Constants.DeliveryApi.HeaderNames.ApiKey,
            In = ParameterLocation.Header,
            Description = "API key specified through configuration to authorize access to the API.",
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[AuthSchemeName] = apiKeyScheme;

        var schemaRef = new OpenApiSecuritySchemeReference(AuthSchemeName, document);
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement { [schemaRef] = [] });

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var schemaRef = new OpenApiSecuritySchemeReference(AuthSchemeName, context.Document);
        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement { [schemaRef] = [] });

        return Task.CompletedTask;
    }
}
