using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages permissions for element container access.
/// </summary>
public interface IElementContainerPermissionService
{
    /// <summary>
    ///     Authorize that a user has access to an element container.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="containerKey">The identifier of the element container to check for access.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>A task resolving into a <see cref="ElementAuthorizationStatus"/>.</returns>
    Task<ElementAuthorizationStatus> AuthorizeAccessAsync(IUser user, Guid containerKey, string permissionToCheck)
        => AuthorizeAccessAsync(user, containerKey.Yield(), new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorize that a user has access to element containers.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="containerKeys">The identifiers of the element containers to check for access.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>A task resolving into a <see cref="ElementAuthorizationStatus"/>.</returns>
    Task<ElementAuthorizationStatus> AuthorizeAccessAsync(IUser user, IEnumerable<Guid> containerKeys, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorize that a user is allowed to perform action on the element container root.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>A task resolving into a <see cref="ElementAuthorizationStatus"/>.</returns>
    Task<ElementAuthorizationStatus> AuthorizeRootAccessAsync(IUser user, string permissionToCheck)
        => AuthorizeRootAccessAsync(user, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorize that a user is allowed to perform actions on the element container root.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>A task resolving into a <see cref="ElementAuthorizationStatus"/>.</returns>
    Task<ElementAuthorizationStatus> AuthorizeRootAccessAsync(IUser user, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorize that a user is allowed to perform action on the element container recycle bin.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>A task resolving into a <see cref="ElementAuthorizationStatus"/>.</returns>
    Task<ElementAuthorizationStatus> AuthorizeBinAccessAsync(IUser user, string permissionToCheck)
        => AuthorizeBinAccessAsync(user, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorize that a user is allowed to perform actions on the element container recycle bin.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>A task resolving into a <see cref="ElementAuthorizationStatus"/>.</returns>
    Task<ElementAuthorizationStatus> AuthorizeBinAccessAsync(IUser user, ISet<string> permissionsToCheck);
}
