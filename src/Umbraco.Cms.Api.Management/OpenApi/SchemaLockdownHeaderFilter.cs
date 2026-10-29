using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Api.Management.OpenApi;

/// <summary>
/// Documents the schema lockdown header on the 403 response of every governed operation.
/// </summary>
/// <remarks>
/// It applies to every OpenAPI document, so a package whose controllers declare <see cref="SchemaEntityTypeAttribute"/>
/// gets the header documented in its own document too.
/// </remarks>
internal sealed class SchemaLockdownHeaderFilter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Reads are never denied.
        (string EntityType, SchemaOperation Operation)[] deniable = Array.FindAll(
            GovernedOperations(context),
            x => x.Operation != SchemaOperation.Read);
        if (deniable.Length == 0)
        {
            return;
        }

        if (operation.Responses?.TryGetValue(StatusCodes.Status403Forbidden.ToString(), out IOpenApiResponse? response)
            is not true
            || response is not OpenApiResponse forbidden)
        {
            return;
        }

        forbidden.Headers ??= new Dictionary<string, IOpenApiHeader>();
        forbidden.Headers.TryAdd(
            Constants.Headers.SchemaLockdown,
            new OpenApiHeader
            {
                Description =
                    "Present when schema lockdown denied the request, naming the entity type and the operation it "
                    + $"was denied for, for example \"{deniable[0].EntityType.ToLowerInvariant()}:"
                    + $"{deniable[0].Operation.ToString().ToLowerInvariant()}\". Absent when the response has "
                    + "another cause.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
            });
    }

    private static (string EntityType, SchemaOperation Operation)[] GovernedOperations(OperationFilterContext context)
    {
        if ((context.ApiDescription.ActionDescriptor as ControllerActionDescriptor)?
                .ControllerTypeInfo
                .GetCustomAttribute<SchemaEntityTypeAttribute>(inherit: true) is not SchemaEntityTypeAttribute governed)
        {
            return [];
        }

        SchemaOperationAttribute[] declared = context.MethodInfo?
            .GetCustomAttributes<SchemaOperationAttribute>()
            .ToArray() ?? [];

        return SchemaOperationResolver.Resolve(governed.EntityTypes, declared, context.ApiDescription.HttpMethod);
    }
}
