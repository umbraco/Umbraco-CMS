using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     Authorizes content access.
/// </summary>
public interface IContentPermissionAuthorizer
{
    /// <summary>
    ///     Authorizes whether the current user has access to the specified content item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="contentKey">The key of the content item to check for.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IUser currentUser, Guid contentKey, string permissionToCheck)
        => IsDeniedAsync(currentUser, contentKey.Yield(), new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the specified content item(s).
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="contentKeys">The keys of the content items to check for.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IUser currentUser, IEnumerable<Guid> contentKeys, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorizes whether the current user has access to the descendants of the specified content item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="parentKey">The key of the parent content item.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedWithDescendantsAsync(IUser currentUser, Guid parentKey, string permissionToCheck)
        => IsDeniedWithDescendantsAsync(currentUser, parentKey, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the descendants of the specified content item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="parentKey">The key of the parent content item.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedWithDescendantsAsync(IUser currentUser, Guid parentKey, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorizes whether the current user has access to the root item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedAtRootLevelAsync(IUser currentUser, string permissionToCheck)
        => IsDeniedAtRootLevelAsync(currentUser, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the root item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAtRootLevelAsync(IUser currentUser, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorizes whether the current user has access to the recycle bin item.
    /// </summary>
    /// <param name="currentUser">The current user'.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAtRecycleBinLevelAsync(IUser currentUser, string permissionToCheck)
        => IsDeniedAtRecycleBinLevelAsync(currentUser, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorizes whether the current user has access to the recycle bin item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAtRecycleBinLevelAsync(IUser currentUser, ISet<string> permissionsToCheck);

    Task<bool> IsDeniedForCultures(IUser currentUser, ISet<string> culturesToCheck);

    /// <summary>
    ///     Filters the specified content keys to only those the user has access to.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="contentKeys">The keys of the content items to filter.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>Returns the keys of content items the user has access to.</returns>
    /// <remarks>
    ///     The default implementation falls back to calling <see cref="IsDeniedAsync(IUser, IEnumerable{Guid}, ISet{string})"/>
    ///     for each key individually. Override this method for better performance with batch authorization.
    /// </remarks>
    // TODO (V18): Remove default implementation.
    async Task<ISet<Guid>> FilterAuthorizedAsync(IUser currentUser, IEnumerable<Guid> contentKeys, ISet<string> permissionsToCheck)
    {
        var authorizedKeys = new HashSet<Guid>();
        foreach (Guid key in contentKeys)
        {
            if (await IsDeniedAsync(currentUser, [key], permissionsToCheck) == false)
            {
                authorizedKeys.Add(key);
            }
        }

        return authorizedKeys;
    }
}
