using System.Security.Principal;

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
    Task<bool> IsAuthorizedAsync(IPrincipal currentUser, Guid contentKey, char permissionToCheck)
        => IsAuthorizedAsync(currentUser, new[] { contentKey }, new[] { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the specified content item(s).
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="contentKeys">The keys of the content items to check for.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedAsync(IPrincipal currentUser, IEnumerable<Guid> contentKeys, IReadOnlyList<char> permissionsToCheck);

    /// <summary>
    ///     Authorizes whether the current user has access to the descendants of the specified content item.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="parentKey">The key of the parent content item.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedWithDescendantsAsync(IPrincipal currentUser, Guid parentKey, IReadOnlyList<char> permissionsToCheck);

    /// <summary>
    ///     Authorizes whether the current user has access to the root item.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedAtRootLevelAsync(IPrincipal currentUser, char permissionToCheck)
        => IsAuthorizedAtRootLevelAsync(currentUser, new[] { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the root item.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedAtRootLevelAsync(IPrincipal currentUser, IReadOnlyList<char> permissionsToCheck);

    /// <summary>
    ///     Authorizes whether the current user has access to the recycle bin item.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedAtRecycleBinLevelAsync(IPrincipal currentUser, char permissionToCheck)
        => IsAuthorizedAtRecycleBinLevelAsync(currentUser, new[] { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the recycle bin item.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedAtRecycleBinLevelAsync(IPrincipal currentUser, IReadOnlyList<char> permissionsToCheck);
}
