using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.OpenApi;

/// <summary>
/// Documents the 403 response and the schema lockdown header of every operation schema lockdown can deny.
/// </summary>
/// <remarks>
/// It applies to every OpenAPI document, so any controller that declares <see cref="SchemaEntityTypeAttribute"/> gets
/// them documented in its own document too.
/// </remarks>
internal sealed class SchemaLockdownHeaderFilter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        string[] deniable = GovernedPermissions(context);
        if (deniable.Length == 0)
        {
            return;
        }

        // Schema lockdown is a reason to refuse an authenticated user in its own right, so the forbidden response is added
        // when nothing else documented one.
        operation.Responses ??= new OpenApiResponses();
        if (operation.Responses.TryGetValue(StatusCodes.Status403Forbidden.ToString(), out IOpenApiResponse? response)
            is false)
        {
            response = new OpenApiResponse { Description = "The authenticated user does not have access to this resource" };
            operation.Responses[StatusCodes.Status403Forbidden.ToString()] = response;
        }

        if (response is not OpenApiResponse forbidden)
        {
            return;
        }

        forbidden.Headers ??= new Dictionary<string, IOpenApiHeader>();
        forbidden.Headers.TryAdd(
            Constants.Headers.SchemaLockdown,
            new OpenApiHeader
            {
                Description =
                    "Present when schema lockdown denied the request, naming the permission verb it was denied, for "
                    + $"example \"{deniable[0]}\". Absent when the response has another cause.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
            });
    }

    private static string[] GovernedPermissions(OperationFilterContext context)
    {
        if ((context.ApiDescription.ActionDescriptor as ControllerActionDescriptor)?
                .ControllerTypeInfo
                .GetCustomAttribute<SchemaEntityTypeAttribute>(inherit: true) is not SchemaEntityTypeAttribute governed)
        {
            return [];
        }

        SchemaOperationAttribute? declared = context.MethodInfo?.GetCustomAttribute<SchemaOperationAttribute>();

        return SchemaOperationResolver.ResolvePermissions(governed.EntityTypes, declared, context.ApiDescription.HttpMethod);
    }
}
