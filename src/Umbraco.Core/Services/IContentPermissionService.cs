using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages permissions for content access.
/// </summary>
public interface IContentPermissionService
{
    // TODO (V19): Remove the default implementations from this interface.

    /// <summary>
    ///     Authorize that a user has access to a content item.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="contentKey">The identifier of the content item to check for access.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>A task resolving into a <see cref="ContentAuthorizationStatus"/>.</returns>
    Task<ContentAuthorizationStatus> AuthorizeAccessAsync(IUser user, Guid contentKey, string permissionToCheck)
        => AuthorizeAccessAsync(user, contentKey.Yield(), new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorize that a user has access to content items.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="contentKeys">The identifiers of the content items to check for access.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>A task resolving into a <see cref="ContentAuthorizationStatus"/>.</returns>
    Task<ContentAuthorizationStatus> AuthorizeAccessAsync(IUser user, IEnumerable<Guid> contentKeys, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorize that a user has access to the descendant items of a content item.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="parentKey">The identifier of the parent content item to check its descendants for access.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>A task resolving into a <see cref="ContentAuthorizationStatus"/>.</returns>
    Task<ContentAuthorizationStatus> AuthorizeDescendantsAccessAsync(IUser user, Guid parentKey, string permissionToCheck)
        => AuthorizeDescendantsAccessAsync(user, parentKey, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorize that a user has access to the descendant items of a content item.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="parentKey">The identifier of the parent content item to check its descendants for access.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>A task resolving into a <see cref="ContentAuthorizationStatus"/>.</returns>
    Task<ContentAuthorizationStatus> AuthorizeDescendantsAccessAsync(IUser user, Guid parentKey, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorize that a user is allowed to perform action on the content root item.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>A task resolving into a <see cref="ContentAuthorizationStatus"/>.</returns>
    Task<ContentAuthorizationStatus> AuthorizeRootAccessAsync(IUser user, string permissionToCheck)
        => AuthorizeRootAccessAsync(user, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorize that a user is allowed to perform actions on the content root item.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>A task resolving into a <see cref="ContentAuthorizationStatus"/>.</returns>
    Task<ContentAuthorizationStatus> AuthorizeRootAccessAsync(IUser user, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorize that a user is allowed to perform action on the content bin item.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>A task resolving into a <see cref="ContentAuthorizationStatus"/>.</returns>
    Task<ContentAuthorizationStatus> AuthorizeBinAccessAsync(IUser user, string permissionToCheck)
        => AuthorizeBinAccessAsync(user, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorize that a user is allowed to perform actions on the content bin item.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>A task resolving into a <see cref="ContentAuthorizationStatus"/>.</returns>
    Task<ContentAuthorizationStatus> AuthorizeBinAccessAsync(IUser user, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorize that a user has access to specific cultures
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="culturesToCheck">The collection of cultures to authorize.</param>
    /// <returns>A task resolving into a <see cref="ContentAuthorizationStatus"/>.</returns>
    Task<ContentAuthorizationStatus> AuthorizeCultureAccessAsync(IUser user, ISet<string> culturesToCheck);

    /// <summary>
    ///     Filters content keys to only those the user has access to.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="contentKeys">The identifiers of the content items to filter.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>A task resolving into the set of authorized content keys.</returns>
    // TODO (V18): Remove default implementation.
    Task<ISet<Guid>> FilterAuthorizedAccessAsync(IUser user, IEnumerable<Guid> contentKeys, ISet<string> permissionsToCheck)
        => Task.FromResult<ISet<Guid>>(new HashSet<Guid>());

    /// <summary>
    ///     Gets the effective permissions for a user on the specified content items.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to get permissions for.</param>
    /// <param name="contentKeys">The identifiers of the content items to get permissions for.</param>
    /// <returns>A task resolving into the effective permissions for each content item.</returns>
    // TODO (V19): Remove the default implementation.
    async Task<IEnumerable<NodePermissions>> GetPermissionsAsync(IUser user, IEnumerable<Guid> contentKeys)
    {
        // This default delegates to IUserService.GetDocumentPermissionsAsync, which resolves permissions using the same
        // underlying algorithm as the optimised implementation in ContentPermissionService.
        // The results are functionally equivalent; this default simply takes a less direct route.
        // It exists for backward compatibility: custom IContentPermissionService implementations that predate this method
        // will fall back here and retain the pre-existing IUserService behaviour without breaking.
        IUserService userService = StaticServiceProvider.Instance.GetRequiredService<IUserService>();
        Attempt<IEnumerable<NodePermissions>, UserOperationStatus> result = await userService.GetDocumentPermissionsAsync(user.Key, contentKeys);
        return result.Success ? result.Result : [];
    }

    /// <summary>
    ///     Filters the fallback permissions for a user. Fallback permissions are the user group default permissions
    ///     used by the UI when no granular per-document permissions are assigned.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to filter permissions for.</param>
    /// <param name="fallbackPermissions">The fallback permissions aggregated from the user's groups.</param>
    /// <returns>A task resolving into the filtered set of fallback permissions.</returns>
    // TODO (V19): Remove the default implementation.
    // Default passes through unchanged for backward compatibility with custom implementations
    // that predate this method.
    Task<ISet<string>> FilterFallbackPermissionsAsync(IUser user, ISet<string> fallbackPermissions)
        => Task.FromResult(fallbackPermissions);
}
