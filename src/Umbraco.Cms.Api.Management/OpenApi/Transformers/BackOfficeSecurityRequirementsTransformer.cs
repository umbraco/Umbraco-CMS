using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.Security;
using Umbraco.Cms.Api.Management.DependencyInjection;

namespace Umbraco.Cms.Api.Management.OpenApi.Transformers;

/// <summary>
/// Transforms OpenAPI operations to include back-office security requirements.
/// </summary>
public class BackOfficeSecurityRequirementsTransformer : IOpenApiOperationTransformer,
    IOpenApiDocumentTransformer
{
    /// <summary>
    /// The number of base AuthorizeAttribute instances on ManagementApiControllerBase
    /// (backoffice access + feature flags).
    /// </summary>
    private const int BaseAuthorizeAttributeCount = 2;

    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor description ||
            description.MethodInfo.GetCustomAttributes(true).Any(x => x is AllowAnonymousAttribute) ||
            description.MethodInfo.DeclaringType?.GetCustomAttributes(true).Any(x => x is AllowAnonymousAttribute) ==
            true)
        {
            return Task.CompletedTask;
        }

        operation.Responses ??= new OpenApiResponses();
        operation.Responses.Add(
            StatusCodes.Status401Unauthorized.ToString(),
            new OpenApiResponse { Description = "The resource is protected and requires an authentication token" });

        var schemaRef = new OpenApiSecuritySchemeReference(
            ManagementApiConfiguration.ApiSecurityName,
            context.Document);
        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement { [schemaRef] = [] });

        // If an endpoint has more than the base number of AuthorizeAttributes (defined on ManagementApiControllerBase),
        // or if the controller injects IAuthorizationService for programmatic authorization checks, there's a
        // possibility the user may be authenticated but not authorized.
        var numberOfAuthorizeAttributes =
            description.MethodInfo.GetCustomAttributes(true).Count(x => x is AuthorizeAttribute)
            + description.MethodInfo.DeclaringType?.GetCustomAttributes(true).Count(x => x is AuthorizeAttribute);

        if (numberOfAuthorizeAttributes > BaseAuthorizeAttributeCount ||
            InjectsAuthorizationService(description.MethodInfo.DeclaringType))
        {
            operation.Responses.Add(
                StatusCodes.Status403Forbidden.ToString(),
                new OpenApiResponse { Description = "The authenticated user does not have access to this resource" });
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var apiKeyScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Name = "Umbraco",
            In = ParameterLocation.Header,
            Description = "Umbraco Authentication",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl =
                        new Uri(Paths.BackOfficeApi.AuthorizationEndpoint, UriKind.Relative),
                    TokenUrl = new Uri(Paths.BackOfficeApi.TokenEndpoint, UriKind.Relative),
                },
            },
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[ManagementApiConfiguration.ApiSecurityName] = apiKeyScheme;

        var schemaRef = new OpenApiSecuritySchemeReference(ManagementApiConfiguration.ApiSecurityName, document);
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement { [schemaRef] = [] });
        return Task.CompletedTask;
    }

    private static bool InjectsAuthorizationService(Type? type)
    {
        if (type is null)
        {
            return false;
        }

        return type.GetConstructors()
            .Any(ctor => ctor.GetParameters()
                .Any(parameter => parameter.ParameterType == typeof(IAuthorizationService)));
    }
}
