using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Management.Permissions;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.OpenApi;

/// <summary>
/// Documents the 403 response and the denied permission header of every operation environment permissions can deny.
/// </summary>
/// <remarks>
/// It applies to every OpenAPI document, so any controller with <see cref="RequiresPermissionsForAttribute"/>, or action
/// with <see cref="RequiresPermissionsAttribute"/>, gets them documented in its own document too.
/// </remarks>
internal sealed class DeniedPermissionHeaderFilter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        IReadOnlyList<string> deniable = GovernedPermissions(context);
        if (deniable.Count == 0)
        {
            return;
        }

        // Environment permissions are a reason to refuse an authenticated user in its own right, so the forbidden response is added
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
            Constants.Headers.DeniedPermission,
            new OpenApiHeader
            {
                Description =
                    "Present when environment permissions denied the request, naming the permission verb it was denied, for "
                    + $"example \"{deniable[0]}\". Absent when the response has another cause.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
            });
    }

    private static IReadOnlyList<string> GovernedPermissions(OperationFilterContext context)
    {
        if (context.MethodInfo?.GetCustomAttribute<RequiresPermissionsAttribute>() is RequiresPermissionsAttribute declared)
        {
            return declared.Permissions;
        }

        return (context.ApiDescription.ActionDescriptor as ControllerActionDescriptor)?
            .ControllerTypeInfo
            .GetCustomAttribute<RequiresPermissionsForAttribute>(inherit: true)?
            .InferPermission(context.ApiDescription.HttpMethod) is string permission
            ? [permission]
            : [];
    }
}
