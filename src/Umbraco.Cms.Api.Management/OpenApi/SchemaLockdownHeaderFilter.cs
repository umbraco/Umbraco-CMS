using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Api.Management.OpenApi;

/// <summary>
/// Documents the schema lockdown header on the forbidden response of every governed operation.
/// </summary>
/// <remarks>
/// The denial itself is a bodyless 403, because a 403 on these operations can equally come from the permissions the
/// request has already passed, and one status code cannot describe two bodies. A header can say "may be present",
/// which is what is actually true, so that is where the reason goes.
/// </remarks>
internal sealed class SchemaLockdownHeaderFilter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.DocumentName != ManagementApiConfiguration.ApiName)
        {
            return;
        }

        // A read is never denied, so an operation that only reads could not carry the header however locked down
        // its entity types are.
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
                    + $"was denied for, for example \"{deniable[0].EntityType}:"
                    + $"{deniable[0].Operation.ToString().ToLowerInvariant()}\". Absent when the response has "
                    + "another cause.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
            });
    }

    // Mirrors what the filter resolves per request: an entity type named outright wins, then a declaration naming
    // none, then the verb.
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

        return governed.EntityTypes
            .Select(entityType =>
            {
                SchemaOperationAttribute? match =
                    Array.Find(declared, x => string.Equals(x.EntityType, entityType, StringComparison.OrdinalIgnoreCase))
                    ?? Array.Find(declared, x => x.EntityType is null);

                return (
                    EntityType: entityType.ToLowerInvariant(),
                    Operation: match?.Operation ?? SchemaOperationResolver.Resolve(context.ApiDescription.HttpMethod));
            })
            .ToArray();
    }
}
