using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages permissions for content access.
/// </summary>
public interface IContentPermissionService
{
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
}
