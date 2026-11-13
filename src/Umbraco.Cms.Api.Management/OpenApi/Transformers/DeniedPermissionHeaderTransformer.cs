using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Management.Permissions;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.OpenApi.Transformers;

/// <summary>
/// Documents the 403 response and the denied permission header of every operation environment permissions can deny.
/// </summary>
/// <remarks>
/// It is added with the backoffice security requirements, so any document using them documents its controllers with
/// <see cref="RequiresPermissionsForAttribute"/>, or actions with <see cref="RequiresPermissionsAttribute"/>, too.
/// </remarks>
internal sealed class DeniedPermissionHeaderTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<string> deniable = GovernedPermissions(context);
        if (deniable.Count == 0)
        {
            return Task.CompletedTask;
        }

        // Environment permissions are a reason to refuse an authenticated user in their own right, so the forbidden
        // response is added when nothing else documented one.
        operation.Responses ??= new OpenApiResponses();
        if (operation.Responses.TryGetValue(StatusCodes.Status403Forbidden.ToString(), out IOpenApiResponse? response)
            is false)
        {
            response = new OpenApiResponse { Description = "The authenticated user does not have access to this resource" };
            operation.Responses[StatusCodes.Status403Forbidden.ToString()] = response;
        }

        if (response is not OpenApiResponse forbidden)
        {
            return Task.CompletedTask;
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

        return Task.CompletedTask;
    }

    private static IReadOnlyList<string> GovernedPermissions(OpenApiOperationTransformerContext context)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor descriptor)
        {
            return [];
        }

        if (descriptor.MethodInfo.GetCustomAttribute<RequiresPermissionsAttribute>() is RequiresPermissionsAttribute declared)
        {
            return declared.Permissions;
        }

        var permission = descriptor.ControllerTypeInfo
            .GetCustomAttribute<RequiresPermissionsForAttribute>(inherit: true)?
            .InferPermission(context.Description.HttpMethod);

        return permission is null ? [] : [permission];
    }
}
