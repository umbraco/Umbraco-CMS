using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Permissions;

namespace Umbraco.Cms.Api.Management.Permissions;

/// <summary>
/// Requires the environment permissions for the entity types a controller manages, denying any change to them that
/// they don't permit.
/// </summary>
/// <remarks>
/// Put it on a controller base class, so every endpoint deriving from it is covered. Which entity types are restricted
/// is decided by the registered <see cref="IEnvironmentPermissionsConfigurator"/> instances.
/// <para>
/// When several entity types are named, every one of them has to permit the operation. A declaration on a derived
/// controller replaces the inherited one.
/// </para>
/// <para>
/// It's a filter, not an authorization requirement, so it runs after the endpoint's authorization policies have
/// passed. Permission checks an action makes itself through <see cref="IAuthorizationService"/> run later, so when
/// both would refuse a request, environment permissions answer. A denial carries the
/// <see cref="Constants.Headers.DeniedPermission"/> header, which tells it apart from any other 403.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class RequiresPermissionsForAttribute : Attribute, IAsyncAuthorizationFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiresPermissionsForAttribute"/> class.
    /// </summary>
    /// <param name="entityTypes">The entity types the controller manages.</param>
    public RequiresPermissionsForAttribute(params string[] entityTypes)
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
        IEnvironmentPermissions environmentPermissions =
            httpContext.RequestServices.GetRequiredService<IEnvironmentPermissions>();

        if (environmentPermissions.DeniedPermissions.Count == 0)
        {
            return Task.CompletedTask;
        }

        RequiresPermissionsAttribute? declared = (context.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo
            .GetCustomAttribute<RequiresPermissionsAttribute>();
        var denied = RequiredPermissionsResolver.ResolvePermissions(EntityTypes, declared, httpContext.Request.Method)
            .FirstOrDefault(environmentPermissions.IsDenied);
        if (denied is null)
        {
            return Task.CompletedTask;
        }

        // A bodyless 403 like every other one in this API, as permissions can return a 403 on the same endpoints. The
        // header names the denied permission verb. It's returned at every runtime level, including during an upgrade,
        // where the maintenance mode filter would otherwise answer 503 ("try again later").
        httpContext.Response.Headers[Constants.Headers.DeniedPermission] = denied;

        context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);

        return Task.CompletedTask;
    }
}
