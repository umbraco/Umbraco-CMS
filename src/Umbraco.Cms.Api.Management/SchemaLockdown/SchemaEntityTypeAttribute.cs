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
/// Declares which schema entity types a controller manages, and denies any change to them that schema lockdown
/// doesn't permit.
/// </summary>
/// <remarks>
/// Put it on a controller base class, so every endpoint deriving from it is covered. Which entity types are restricted
/// is decided by the registered <see cref="ISchemaLockdownConfigurator"/> instances.
/// <para>
/// When several entity types are named, every one of them has to permit the operation. A declaration on a derived
/// controller replaces the inherited one.
/// </para>
/// <para>
/// It's a filter, not an authorization requirement, so it only runs once the request has passed authorization, and a
/// denial always means schema lockdown was the reason.
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
        ISchemaRestrictions restrictions =
            httpContext.RequestServices.GetRequiredService<ISchemaRestrictions>();

        if (EntityTypes.Any(restrictions.IsRestricted) is false)
        {
            return Task.CompletedTask;
        }

        SchemaOperationAttribute[] declared = DeclaredOperations(context.ActionDescriptor);
        (string EntityType, SchemaOperation Operation)[] governed =
            SchemaOperationResolver.Resolve(EntityTypes, declared, httpContext.Request.Method);

        var refusal = Array.FindIndex(governed, x => restrictions.IsAllowed(x.EntityType, x.Operation) is false);
        if (refusal < 0)
        {
            return Task.CompletedTask;
        }

        (var denied, SchemaOperation operation) = governed[refusal];

        // A bodyless 403 like every other one in this API, as permissions can return a 403 on the same endpoints. The
        // header says what was denied. It's returned at every runtime level, including during an upgrade, where the
        // maintenance mode filter would otherwise answer 503 ("try again later").
        httpContext.Response.Headers[Constants.Headers.SchemaLockdown] =
            $"{denied.ToLowerInvariant()}:{operation.ToString().ToLowerInvariant()}";

        context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);

        return Task.CompletedTask;
    }

    private static SchemaOperationAttribute[] DeclaredOperations(ActionDescriptor actionDescriptor)
        => (actionDescriptor as ControllerActionDescriptor)?.MethodInfo
            .GetCustomAttributes<SchemaOperationAttribute>()
            .ToArray() ?? [];
}
