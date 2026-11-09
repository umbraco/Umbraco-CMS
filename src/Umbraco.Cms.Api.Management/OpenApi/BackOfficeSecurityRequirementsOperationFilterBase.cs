using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.OpenApi;

/// <summary>
/// Serves as a base class for filters that apply back office security requirements to OpenAPI operations.
/// </summary>
public abstract class BackOfficeSecurityRequirementsOperationFilterBase : IOperationFilter
{
    // The AuthorizeAttributes on ManagementApiControllerBase: backoffice access and feature flags.
    private const int BaseAuthorizeAttributeCount = 2;

    protected abstract string ApiName { get; }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor.HasMapToApiAttribute(ApiName) == false)
        {
            return;
        }

        IList<object> endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        if (endpointMetadata.Any(x => x is AllowAnonymousAttribute) is false)
        {
            operation.Responses ??= new OpenApiResponses();
            operation.Responses.Add(
                StatusCodes.Status401Unauthorized.ToString(),
                new OpenApiResponse
                {
                    Description = "The resource is protected and requires an authentication token",
                });

            var schemaRef = new OpenApiSecuritySchemeReference(ManagementApiConfiguration.ApiSecurityName, context.Document);
            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(new OpenApiSecurityRequirement { [schemaRef] = [] });
        }

        var numberOfAuthorizeAttributes = endpointMetadata.Count(x => x is AuthorizeAttribute);

        var hasConstructorInjectingIAuthorizationService = context.MethodInfo.DeclaringType?.GetConstructors()
            .Any(ctor =>
                ctor.GetParameters().Any(parameter => parameter.GetType() == typeof(IAuthorizationService))) ?? false;

        // An endpoint with more AuthorizeAttributes than ManagementApiControllerBase, or one that authorizes in code,
        // may refuse a user who is authenticated.
        if (numberOfAuthorizeAttributes > BaseAuthorizeAttributeCount || hasConstructorInjectingIAuthorizationService)
        {
            operation.Responses ??= new OpenApiResponses();
            operation.Responses.Add(
                StatusCodes.Status403Forbidden.ToString(),
                new OpenApiResponse
                {
                    Description = "The authenticated user does not have access to this resource",
                });
        }
    }
}
