using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Permissions;

/// <summary>
/// Denies the action unless the environment permissions permit every declared permission verb.
/// </summary>
/// <remarks>
/// It works on any action, with or without <see cref="RequiresPermissionsForAttribute"/> on the controller. On a
/// controller that has it, it replaces the verbs that would be inferred from the HTTP verb: the action needs exactly
/// the declared verbs, for example
/// <c>[RequiresPermissions([Constants.Permissions.Template.Create, Constants.Permissions.DocumentType.Update])]</c>,
/// or none, as <c>[RequiresPermissions([])]</c>, for an action that changes nothing.
/// <para>
/// Only the environment permissions are checked, not the current user's permissions.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RequiresPermissionsAttribute : Attribute, IAsyncAuthorizationFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiresPermissionsAttribute"/> class.
    /// </summary>
    /// <param name="permissions">The permission verbs the action needs, for example from <see cref="Constants.Permissions"/>.</param>
    public RequiresPermissionsAttribute(string[] permissions)
    {
        foreach (var permission in permissions)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        }

        Permissions = permissions.Distinct(StringComparer.Ordinal).ToArray();
    }

    /// <summary>
    /// Gets the permission verbs the action needs.
    /// </summary>
    public IReadOnlyList<string> Permissions { get; }

    /// <inheritdoc />
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        EnvironmentPermissionsAuthorization.Authorize(context, () => Permissions);

        return Task.CompletedTask;
    }
}
