using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.Security;

namespace Umbraco.Cms.Api.Delivery.OpenApi.Transformers;

/// <summary>
/// Transformer that adds member authentication security requirements to OpenAPI documents.
/// </summary>
internal class MemberAuthenticationSecurityRequirementsTransformer : IOpenApiOperationTransformer, IOpenApiDocumentTransformer
{
    private const string AuthSchemeName = "UmbracoMember";

    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Name = AuthSchemeName,
            Type = SecuritySchemeType.OAuth2,
            Description = "Umbraco Member Authentication",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(Paths.MemberApi.AuthorizationEndpoint, UriKind.Relative),
                    TokenUrl = new Uri(Paths.MemberApi.TokenEndpoint, UriKind.Relative),
                },
            },
        };

        document.AddComponent(AuthSchemeName, securityScheme);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
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
