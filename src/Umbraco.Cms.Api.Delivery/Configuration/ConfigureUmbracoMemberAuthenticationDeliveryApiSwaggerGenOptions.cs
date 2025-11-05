using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.Security;

namespace Umbraco.Cms.Api.Delivery.Configuration;

/// <summary>
/// This configures member authentication for the Delivery API in Swagger. Consult the docs for
/// member authentication within the Delivery API for instructions on how to use this.
/// </summary>
/// <remarks>
/// This class is not used by the core CMS due to the required installation dependencies (local login page among other things).
/// </remarks>
public class ConfigureUmbracoMemberAuthenticationDeliveryApiSwaggerGenOptions : IConfigureNamedOptions<OpenApiOptions>
{
    private const string AuthSchemeName = "UmbracoMember";

    /// <inheritdoc />
    public void Configure(OpenApiOptions options)
    {
        // No default configuration
    }

    /// <inheritdoc />
    public void Configure(string? name, OpenApiOptions options)
    {
        if (name != DeliveryApiConfiguration.ApiName)
        {
            return;
        }

        // add security requirements for content API operations
        options.AddDocumentTransformer<DeliveryApiSecurityFilter>();
        options.AddOperationTransformer<DeliveryApiSecurityFilter>();
    }

    private sealed class DeliveryApiSecurityFilter : IOpenApiOperationTransformer, IOpenApiDocumentTransformer
    {
        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            var apiKeyScheme = new OpenApiSecurityScheme
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

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[AuthSchemeName] = apiKeyScheme;

            var schemaRef = new OpenApiSecuritySchemeReference(AuthSchemeName, document);
            document.Security ??= new List<OpenApiSecurityRequirement>();
            document.Security.Add(new OpenApiSecurityRequirement { [schemaRef] = [] });

            return Task.CompletedTask;
        }

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
}
