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

        // Assuming if and endpoint have more then one AuthorizeAttribute, there is a risk the user do not have access while still being authorized.
        // The two is the simple two on ManagementApiControllerBase that just requires access to backoffice and feature flags
        var numberOfAuthorizeAttributes =
            description.MethodInfo.GetCustomAttributes(true).Count(x => x is AuthorizeAttribute)
            + description.MethodInfo.DeclaringType?.GetCustomAttributes(true).Count(x => x is AuthorizeAttribute);

        var hasConstructorInjectingIAuthorizationService = description.MethodInfo.DeclaringType?.GetConstructors()
            .Any(ctor =>
                ctor.GetParameters().Any(parameter => parameter.GetType() == typeof(IAuthorizationService))) ?? false;

        if (numberOfAuthorizeAttributes > 2 || hasConstructorInjectingIAuthorizationService)
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
                    TokenUrl = new Uri(Paths.BackOfficeApi.TokenEndpoint, UriKind.Relative)
                }
            }
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[ManagementApiConfiguration.ApiSecurityName] = apiKeyScheme;

        var schemaRef = new OpenApiSecuritySchemeReference(ManagementApiConfiguration.ApiSecurityName, document);
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement { [schemaRef] = [] });
        return Task.CompletedTask;
    }
}
