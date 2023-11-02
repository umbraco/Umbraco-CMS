using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages permissions for content access.
/// </summary>
public interface IContentPermissionService
{
    /// <summary>
    ///     Authorize that the current user has access to the specified content item.
    /// </summary>
    /// <param name="performingUser">The user performing the operation.</param>
    /// <param name="contentKey">The identifier of the content item to check for access.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    /// <returns>A task resolving into a <see cref="ContentAuthorizationStatus"/>.</returns>
    Task<ContentAuthorizationStatus> AuthorizeAccessAsync(IUser performingUser, Guid contentKey, char permissionToCheck)
        => AuthorizeAccessAsync(performingUser, new[] { contentKey }, new[] { permissionToCheck });

    /// <summary>
    ///     Authorize that the current user has access to these content items.
    /// </summary>
    /// <param name="performingUser">The user performing the operation.</param>
    /// <param name="contentKeys">The identifiers of the content items to check for access.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>A task resolving into a <see cref="ContentAuthorizationStatus"/>.</returns>
    Task<ContentAuthorizationStatus> AuthorizeAccessAsync(IUser performingUser, IEnumerable<Guid> contentKeys, IReadOnlyList<char> permissionsToCheck);

    /// <summary>
    ///     Authorize that the current user has access the descendant items of the given content item.
    /// </summary>
    /// <param name="performingUser">The user performing the operation.</param>
    /// <param name="parentKey">The identifier of the parent content item to check its descendants for access.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    /// <returns>A task resolving into a <see cref="ContentAuthorizationStatus"/>.</returns>
    Task<ContentAuthorizationStatus> AuthorizeDescendantsAccessAsync(IUser performingUser, Guid parentKey, IReadOnlyList<char> permissionsToCheck);

    /// <summary>
    ///     Authorize that the current user has access to the content root item.
    /// </summary>
    /// <param name="performingUser">The user performing the operation.</param>
    /// <returns>A task resolving into a <see cref="ContentAuthorizationStatus"/>.</returns>
    Task<ContentAuthorizationStatus> AuthorizeRootAccessAsync(IUser performingUser);

    /// <summary>
    ///     Authorize that the current user has access to the content bin item.
    /// </summary>
    /// <param name="performingUser">The user performing the operation.</param>
    /// <returns>A task resolving into a <see cref="ContentAuthorizationStatus"/>.</returns>
    Task<ContentAuthorizationStatus> AuthorizeBinAccessAsync(IUser performingUser);
}
