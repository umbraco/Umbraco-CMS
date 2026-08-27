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

        if (GovernedEntityType(context) is not string entityType)
        {
            return;
        }

        // A read is never denied, so the header could not appear on one however locked down the entity type is.
        if (ResolveOperation(context) == SchemaOperation.Read)
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
                    $"Present when schema lockdown denied the request, naming the entity type and the operation it "
                    + $"was denied for, for example \"{entityType}:update\". Absent when the response has another cause.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String },
            });
    }

    private static SchemaOperation ResolveOperation(OperationFilterContext context)
    {
        SchemaOperation? declared = context.MethodInfo?
            .GetCustomAttribute<SchemaOperationAttribute>()?.Operation;

        return declared ?? SchemaOperationResolver.Resolve(context.ApiDescription.HttpMethod);
    }

    private static string? GovernedEntityType(OperationFilterContext context)
        => (context.ApiDescription.ActionDescriptor as ControllerActionDescriptor)?
            .ControllerTypeInfo
            .GetCustomAttribute<SchemaEntityTypeAttribute>(inherit: true)?
            .EntityType
            .ToLowerInvariant();
}
