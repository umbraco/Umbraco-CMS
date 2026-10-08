using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     Authorizes element access.
/// </summary>
public interface IElementPermissionAuthorizer
{
    /// <summary>
    ///     Authorizes whether the current user has access to the specified element item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="elementKey">The key of the element item to check for.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is denied, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IUser currentUser, Guid elementKey, string permissionToCheck)
        => IsDeniedAsync(currentUser, elementKey.Yield(), new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the specified element item(s).
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="elementKeys">The keys of the element items to check for.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is denied, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IUser currentUser, IEnumerable<Guid> elementKeys, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorizes whether the current user has access to the descendants of the specified element item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="parentKey">The key of the parent element item.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is denied, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedWithDescendantsAsync(IUser currentUser, Guid parentKey, string permissionToCheck)
        => IsDeniedWithDescendantsAsync(currentUser, parentKey, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the descendants of the specified element item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="parentKey">The key of the parent element item.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is denied, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedWithDescendantsAsync(IUser currentUser, Guid parentKey, ISet<string> permissionsToCheck);

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

    /// <summary>
    ///     Authorizes whether the current user has access to the specified cultures.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="culturesToCheck">The cultures to check for access.</param>
    /// <returns>Returns <c>true</c> if authorization is denied, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedForCultures(IUser currentUser, ISet<string> culturesToCheck);
}