using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Security.Authorization;

public class ContextualPermissionAuthorizer : IContextualPermissionAuthorizer
{
    public bool IsDenied(IUser currentUser, ContextualPermissionResource resource)
    {
        return resource.PermissionMatchingBehaviour == PermissionMatchingBehaviour.Any
            ? !resource.Permissions.Any(ContextualMatch)
            : !resource.Permissions.All(ContextualMatch);

        bool ContextualMatch(string permissionToCheck) =>
            currentUser.Groups.SelectMany(g => g.GranularPermissions)
                .Any(definedContextualPermission =>
                    (resource.PermissionScopeMatchingBehaviour == PermissionScopeMatchingBehaviour.Any
                     || (resource.PermissionScopeMatchingBehaviour == PermissionScopeMatchingBehaviour.ContextWideOnly && definedContextualPermission.Key is null))
                    && definedContextualPermission.Context.Equals(resource.Context, StringComparison.InvariantCultureIgnoreCase)
                    && definedContextualPermission.Permission.Equals(permissionToCheck, StringComparison.InvariantCultureIgnoreCase));
    }
}
