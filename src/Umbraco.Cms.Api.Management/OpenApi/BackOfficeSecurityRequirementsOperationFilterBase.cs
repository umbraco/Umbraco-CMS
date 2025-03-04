using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.OpenApi;

public abstract class BackOfficeSecurityRequirementsOperationFilterBase : IOperationFilter
{
    protected abstract string ApiName { get; }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.HasMapToApiAttribute(ApiName) == false)
        {
            return;
        }

        if (!context.MethodInfo.GetCustomAttributes(true).Any(x => x is AllowAnonymousAttribute) &&
            !(context.MethodInfo.DeclaringType?.GetCustomAttributes(true).Any(x => x is AllowAnonymousAttribute) ?? false))
        {
            operation.Responses.Add(StatusCodes.Status401Unauthorized.ToString(), new OpenApiResponse
            {
                Description = "The resource is protected and requires an authentication token"
            });

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = ManagementApiConfiguration.ApiSecurityName
                            }
                        }, new string[] { }
                    }
                }
            };
        }

        // Assuming if and endpoint have more then one AuthorizeAttribute, there is a risk the user do not have access while still being authorized.
        // The two is the simple two on ManagementApiControllerBase that just requires access to backoffice and feature flags
        var numberOfAuthorizeAttributes =
            context.MethodInfo.GetCustomAttributes(true).Count(x => x is AuthorizeAttribute)
            + context.MethodInfo.DeclaringType?.GetCustomAttributes(true).Count(x => x is AuthorizeAttribute);


        var hasConstructorInjectingIAuthorizationService = context.MethodInfo.DeclaringType?.GetConstructors()
            .Any(ctor =>
                ctor.GetParameters().Any(parameter => parameter.GetType() == typeof(IAuthorizationService))) ?? false;

        if (numberOfAuthorizeAttributes > 2 || hasConstructorInjectingIAuthorizationService)
        {
            operation.Responses.Add(StatusCodes.Status403Forbidden.ToString(), new OpenApiResponse()
            {
                Description = "The authenticated user does not have access to this resource"
            });
        }
    }
}
