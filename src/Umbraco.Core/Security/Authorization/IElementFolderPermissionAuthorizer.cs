using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     Authorizes element folder access.
/// </summary>
public interface IElementFolderPermissionAuthorizer
{
    /// <summary>
    ///     Authorizes whether the current user has access to the specified element folder item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="folderKey">The key of the element folder item to check for.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is denied, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IUser currentUser, Guid folderKey, string permissionToCheck)
        => IsDeniedAsync(currentUser, folderKey.Yield(), new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the specified element folder item(s).
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="folderKeys">The keys of the element folder items to check for.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is denied, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IUser currentUser, IEnumerable<Guid> folderKeys, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorizes whether the current user has access to the root item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is denied, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAtRootLevelAsync(IUser currentUser, string permissionToCheck)
        => IsDeniedAtRootLevelAsync(currentUser, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the root item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is denied, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAtRootLevelAsync(IUser currentUser, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorizes whether the current user has access to the recycle bin item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is denied, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAtRecycleBinLevelAsync(IUser currentUser, string permissionToCheck)
        => IsDeniedAtRecycleBinLevelAsync(currentUser, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the recycle bin item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is denied, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAtRecycleBinLevelAsync(IUser currentUser, ISet<string> permissionsToCheck);
}
