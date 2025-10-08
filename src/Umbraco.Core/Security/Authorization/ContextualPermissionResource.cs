using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security.Authorization;

public class ContextualPermissionResource : IPermissionResource
{
    public static ContextualPermissionResource WithPermission(string permission, string context) =>
        WithAllPermissions(permission.Yield(), context);

    public static ContextualPermissionResource WithContextWidePermission(string permission, string context) =>
        WithAllContextWidePermissions(permission.Yield(), context);

    public static ContextualPermissionResource WithAnyContextWidePermissions(IEnumerable<string> permissions, string context) =>
        new(permissions, context, PermissionMatchingBehaviour.Any, PermissionScopeMatchingBehaviour.ContextWideOnly);

    public static ContextualPermissionResource WithAllContextWidePermissions(IEnumerable<string> permissions, string context) =>
        new(permissions, context, PermissionMatchingBehaviour.All, PermissionScopeMatchingBehaviour.ContextWideOnly);

    public static ContextualPermissionResource WithAnyPermissions(IEnumerable<string> permissions, string context) =>
        new(permissions, context, PermissionMatchingBehaviour.Any, PermissionScopeMatchingBehaviour.Any);

    public static ContextualPermissionResource WithAllPermissions(IEnumerable<string> permissions, string context) =>
        new(permissions, context, PermissionMatchingBehaviour.All, PermissionScopeMatchingBehaviour.Any);

    public static ContextualPermissionResource WithSetup(
        IEnumerable<string> permissions,
        string context,
        PermissionMatchingBehaviour permissionMatchingBehaviour,
        PermissionScopeMatchingBehaviour permissionScopeMatchingBehaviour) =>
        new(permissions, context, permissionMatchingBehaviour, permissionScopeMatchingBehaviour);

    private ContextualPermissionResource(
        IEnumerable<string> permissions,
        string context,
        PermissionMatchingBehaviour permissionMatchingBehaviour,
        PermissionScopeMatchingBehaviour permissionScopeMatchingBehaviour)
    {
        Permissions = permissions;
        Context = context;
        PermissionMatchingBehaviour = permissionMatchingBehaviour;
        PermissionScopeMatchingBehaviour = permissionScopeMatchingBehaviour;
    }

    public IEnumerable<string> Permissions { get; }

    public string Context { get; }

    public PermissionMatchingBehaviour PermissionMatchingBehaviour { get; }

    public PermissionScopeMatchingBehaviour PermissionScopeMatchingBehaviour { get; }
}

public enum PermissionMatchingBehaviour
{
    Any,
    All,
}

public enum PermissionScopeMatchingBehaviour
{
    /// <summary>
    /// Both context wide and granular
    /// </summary>
    Any,

    /// <summary>
    /// Only Context wide
    /// </summary>
    ContextWideOnly,
}
