using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Declares which schema entity types a controller manages, and denies any change to them the schema lockdown
/// restrictions do not permit.
/// </summary>
/// <remarks>
/// Declaring the entity type is all a controller has to do: the attribute is an authorization filter, and MVC
/// collects filters from base classes, so a newly added endpoint on a governed controller is covered without anyone
/// having to remember to tag it. Whether the entity type is restricted at all is decided solely by the registered
/// <see cref="ISchemaLockdownConfigurator"/> instances, never by this attribute.
/// <para>
/// More than one entity type can be named, for an endpoint that touches several: creating a template for a document
/// type is both. Every named type has to permit the operation, so the most restrictive answer wins.
/// </para>
/// <para>
/// A declaration on a derived controller replaces the one it inherits rather than adding to it, because the attribute
/// does not allow multiples. That is what lets a specific endpoint be governed as something other than whatever its
/// base class manages.
/// </para>
/// <para>
/// Being an MVC filter rather than an authorization requirement is deliberate. Authorization requirements are all
/// evaluated together, so one cannot tell whether the others were satisfied; filters run only once the authorization
/// middleware has admitted the request, so a denial here means schema lockdown is the reason and nothing else was
/// also in the way.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class SchemaEntityTypeAttribute : Attribute, IAsyncAuthorizationFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaEntityTypeAttribute"/> class.
    /// </summary>
    /// <param name="entityTypes">The entity types the controller manages.</param>
    public SchemaEntityTypeAttribute(params string[] entityTypes)
    {
        ArgumentOutOfRangeException.ThrowIfZero(entityTypes.Length);
        foreach (var entityType in entityTypes)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        }

        EntityTypes = entityTypes;
    }

    /// <summary>
    /// Gets the entity types the controller manages.
    /// </summary>
    public IReadOnlyList<string> EntityTypes { get; }

    /// <inheritdoc />
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        HttpContext httpContext = context.HttpContext;
        SchemaOperationAttribute[] declared = DeclaredOperations(context.ActionDescriptor);

        ISchemaRestrictions restrictions =
            httpContext.RequestServices.GetRequiredService<ISchemaRestrictions>();

        (string EntityType, SchemaOperation Operation)? refusal = EntityTypes
            .Select(entityType => (
                EntityType: entityType,
                Operation: ResolveOperation(httpContext, declared, entityType)))
            .Cast<(string EntityType, SchemaOperation Operation)?>()
            .FirstOrDefault(x => restrictions.IsAllowed(x!.Value.EntityType, x.Value.Operation) is false);

        if (refusal is null)
        {
            return Task.CompletedTask;
        }

        (var denied, SchemaOperation operation) = refusal.Value;

        // The denial is an ordinary bodyless 403, as every other one in this API is: a 403 on these endpoints can
        // equally come from the permissions the request has already passed, and one status code cannot describe two
        // bodies. The header names what was denied for whoever is reading the network tab.
        //
        // This holds at every runtime level. An entity type a configurator restricted is restricted during an
        // unattended upgrade as much as after one, so the 503 MaintenanceModeActionFilter would otherwise write -
        // meaning "try again later" - would be untrue. Anything not restricted still falls through to it.
        httpContext.Response.Headers[Constants.Headers.SchemaLockdown] =
            $"{denied.ToLowerInvariant()}:{operation.ToString().ToLowerInvariant()}";

        context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);

        return Task.CompletedTask;
    }

    private static SchemaOperationAttribute[] DeclaredOperations(ActionDescriptor actionDescriptor)
        => (actionDescriptor as ControllerActionDescriptor)?.MethodInfo
            .GetCustomAttributes<SchemaOperationAttribute>()
            .ToArray() ?? [];

    // An entity type named outright wins, then a declaration naming none, then the verb.
    private static SchemaOperation ResolveOperation(
        HttpContext httpContext,
        SchemaOperationAttribute[] declared,
        string entityType)
    {
        SchemaOperationAttribute? match =
            Array.Find(declared, x => string.Equals(x.EntityType, entityType, StringComparison.OrdinalIgnoreCase))
            ?? Array.Find(declared, x => x.EntityType is null);

        return match?.Operation ?? SchemaOperationResolver.Resolve(httpContext.Request.Method);
    }
}
