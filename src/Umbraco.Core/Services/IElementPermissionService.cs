using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages permissions for element access.
/// </summary>
public interface IElementPermissionService
{
    /// <summary>
    ///     Authorize that a user has access to an element.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="elementKey">The identifier of the element to check for access.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>A task resolving into a <see cref="ElementAuthorizationStatus"/>.</returns>
    Task<ElementAuthorizationStatus> AuthorizeAccessAsync(IUser user, Guid elementKey, string permissionToCheck)
        => AuthorizeAccessAsync(user, elementKey.Yield(), new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorize that a user has access to elements.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="elementKeys">The identifiers of the elements to check for access.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>A task resolving into a <see cref="ElementAuthorizationStatus"/>.</returns>
    Task<ElementAuthorizationStatus> AuthorizeAccessAsync(IUser user, IEnumerable<Guid> elementKeys, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorize that a user has access to the descendant items of an element.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="parentKey">The identifier of the parent element to check its descendants for access.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>A task resolving into a <see cref="ElementAuthorizationStatus"/>.</returns>
    Task<ElementAuthorizationStatus> AuthorizeDescendantsAccessAsync(IUser user, Guid parentKey, string permissionToCheck)
        => AuthorizeDescendantsAccessAsync(user, parentKey, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorize that a user has access to the descendant items of an element.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="parentKey">The identifier of the parent element to check its descendants for access.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>A task resolving into a <see cref="ElementAuthorizationStatus"/>.</returns>
    Task<ElementAuthorizationStatus> AuthorizeDescendantsAccessAsync(IUser user, Guid parentKey, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorize that a user is allowed to perform action on the element root.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>A task resolving into a <see cref="ElementAuthorizationStatus"/>.</returns>
    Task<ElementAuthorizationStatus> AuthorizeRootAccessAsync(IUser user, string permissionToCheck)
        => AuthorizeRootAccessAsync(user, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorize that a user is allowed to perform actions on the element root.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>A task resolving into a <see cref="ElementAuthorizationStatus"/>.</returns>
    Task<ElementAuthorizationStatus> AuthorizeRootAccessAsync(IUser user, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorize that a user is allowed to perform action on the element recycle bin.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>A task resolving into a <see cref="ElementAuthorizationStatus"/>.</returns>
    Task<ElementAuthorizationStatus> AuthorizeBinAccessAsync(IUser user, string permissionToCheck)
        => AuthorizeBinAccessAsync(user, new HashSet<string> { permissionToCheck });

    /// <summary>
    ///     Authorize that a user is allowed to perform actions on the element recycle bin.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>A task resolving into a <see cref="ElementAuthorizationStatus"/>.</returns>
    Task<ElementAuthorizationStatus> AuthorizeBinAccessAsync(IUser user, ISet<string> permissionsToCheck);

    /// <summary>
    ///     Authorize that a user has access to specific cultures.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="culturesToCheck">The collection of cultures to authorize.</param>
    /// <returns>A task resolving into a <see cref="ElementAuthorizationStatus"/>.</returns>
    Task<ElementAuthorizationStatus> AuthorizeCultureAccessAsync(IUser user, ISet<string> culturesToCheck);
}