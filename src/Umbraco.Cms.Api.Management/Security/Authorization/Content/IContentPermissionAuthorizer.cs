using System.Security.Principal;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Content;

/// <summary>
///     Authorizes content access.
/// </summary>
public interface IContentPermissionAuthorizer
{
    /// <summary>
    ///     Authorizes whether the current user has access to the specified content item.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="contentKey">The key of the content item to check for.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedAsync(IPrincipal currentUser, Guid contentKey, string permissionToCheck)
        => IsDeniedAsync(currentUser, contentKey.Yield(), new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the specified content item(s).
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="contentKeys">The keys of the content items to check for.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IPrincipal currentUser, IEnumerable<Guid> contentKeys, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorizes whether the current user has access to the descendants of the specified content item.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="parentKey">The key of the parent content item.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedWithDescendantsAsync(IPrincipal currentUser, Guid parentKey, string permissionToCheck)
        => IsDeniedWithDescendantsAsync(currentUser, parentKey, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the descendants of the specified content item.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="parentKey">The key of the parent content item.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedWithDescendantsAsync(IPrincipal currentUser, Guid parentKey, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorizes whether the current user has access to the root item.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedAtRootLevelAsync(IPrincipal currentUser, string permissionToCheck)
        => IsDeniedAtRootLevelAsync(currentUser, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the root item.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAtRootLevelAsync(IPrincipal currentUser, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorizes whether the current user has access to the recycle bin item.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedAtRecycleBinLevelAsync(IPrincipal currentUser, string permissionToCheck)
        => IsDeniedAtRecycleBinLevelAsync(currentUser, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the recycle bin item.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAtRecycleBinLevelAsync(IPrincipal currentUser, ISet<string> permissionsToCheck);

    Task<bool> IsDeniedForCultures(IPrincipal currentUser, ISet<string> culturesToCheck);
}
