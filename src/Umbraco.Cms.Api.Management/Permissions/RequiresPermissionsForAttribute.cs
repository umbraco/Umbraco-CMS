using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Permissions;

namespace Umbraco.Cms.Api.Management.Permissions;

/// <summary>
/// Requires the environment permissions for what a controller manages, denying any change to it that they don't
/// permit.
/// </summary>
/// <remarks>
/// Put it on a controller base class, so every endpoint deriving from it is covered. Which permission verbs are denied
/// is decided by the registered <see cref="IEnvironmentPermissionsConfigurator"/> instances.
/// <para>
/// The permission verb an action needs is the declared resource followed by the change its HTTP method makes: with
/// <c>[RequiresPermissionsFor(Constants.Permissions.Entities.DocumentType.Resource)]</c>, a POST needs
/// <see cref="Constants.Permissions.Entities.DocumentType.Create"/>. A package names its own resource, so its verbs
/// don't collide with Umbraco's.
/// </para>
/// <para>
/// An action with <see cref="RequiresPermissionsAttribute"/> is checked against its declared verbs instead, and this
/// attribute leaves it alone. That's also how an action that changes more than the controller manages declares what
/// it needs.
/// </para>
/// <para>
/// A declaration on a derived controller replaces the inherited one.
/// </para>
/// <para>
/// It's a filter, not an authorization requirement, so it runs after the endpoint's authorization policies have
/// passed. Permission checks an action makes itself through
/// <see cref="Microsoft.AspNetCore.Authorization.IAuthorizationService"/> run later, so when both would refuse a
/// request, environment permissions answer. A denial carries the <see cref="Constants.Headers.DeniedPermission"/>
/// header, which tells it apart from any other 403.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class RequiresPermissionsForAttribute : Attribute, IAsyncAuthorizationFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiresPermissionsForAttribute"/> class.
    /// </summary>
    /// <param name="resource">
    /// The resource whose permission verbs the controller's actions need, for example
    /// <see cref="Constants.Permissions.Entities.DocumentType.Resource"/>.
    /// </param>
    public RequiresPermissionsForAttribute(string resource)
    {
        // The resource is joined to the operation with a dot, so a trailing one is redundant.
        var trimmed = resource?.TrimEnd('.');
        ArgumentException.ThrowIfNullOrWhiteSpace(trimmed, nameof(resource));

        Resource = trimmed;
    }

    /// <summary>
    /// Gets the resource whose permission verbs the controller's actions need.
    /// </summary>
    public string Resource { get; }

    /// <inheritdoc />
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // An action that declares its permissions is checked by that declaration instead.
        if ((context.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo
            .IsDefined(typeof(RequiresPermissionsAttribute)) is true)
        {
            return Task.CompletedTask;
        }

        EnvironmentPermissionsAuthorization.Authorize(
            context,
            () => InferPermission(context.HttpContext.Request.Method) is string permission ? [permission] : []);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Infers the permission verb an action needs from its HTTP method.
    /// </summary>
    /// <param name="httpMethod">The HTTP method the action responds to.</param>
    /// <returns>The resource followed by the HTTP method's change, or <c>null</c> for a method that changes nothing.</returns>
    /// <remarks>Both enforcement and the OpenAPI document use this, so they can't disagree.</remarks>
    internal string? InferPermission(string? httpMethod)
        => InferOperation(httpMethod) is string operation
            ? PermissionVerbs.For(Resource, operation)
            : null;

    /// <summary>
    /// Infers the change an HTTP method makes.
    /// </summary>
    /// <param name="httpMethod">The HTTP method the action responds to.</param>
    /// <returns>
    /// <c>null</c> for GET, HEAD and OPTIONS, which change nothing. Otherwise one of
    /// <see cref="Constants.Permissions.Operations"/>: PUT, PATCH and any unrecognised method are treated as
    /// <see cref="Constants.Permissions.Operations.Update"/>.
    /// </returns>
    private static string? InferOperation(string? httpMethod)
    {
        var method = httpMethod ?? string.Empty;

        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
        {
            return null;
        }

        if (HttpMethods.IsPost(method))
        {
            return Constants.Permissions.Operations.Create;
        }

        if (HttpMethods.IsDelete(method))
        {
            return Constants.Permissions.Operations.Delete;
        }

        return Constants.Permissions.Operations.Update;
    }
}
