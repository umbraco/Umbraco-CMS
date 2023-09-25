using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages permissions for media access.
/// </summary>
public interface IMediaPermissionsService
{
    /// <summary>
    ///     Authorize that the current user has access to these media items.
    /// </summary>
    /// <param name="performingUser">The user performing the operation.</param>
    /// <param name="mediaKeys">The identifiers of the media items to check for access.</param>
    /// <returns>A task resolving into a <see cref="MediaAuthorizationStatus"/>.</returns>
    Task<MediaAuthorizationStatus> AuthorizeAccessAsync(IUser performingUser, IEnumerable<Guid> mediaKeys);

    /// <summary>
    ///     Authorize that the current user has access to the media root item.
    /// </summary>
    /// <param name="performingUser">The user performing the operation.</param>
    /// <returns>A task resolving into a <see cref="MediaAuthorizationStatus"/>.</returns>
    Task<MediaAuthorizationStatus> AuthorizeRootAccessAsync(IUser performingUser);

    /// <summary>
    ///     Authorize that the current user has access to the media bin item.
    /// </summary>
    /// <param name="performingUser">The user performing the operation.</param>
    /// <returns>A task resolving into a <see cref="MediaAuthorizationStatus"/>.</returns>
    Task<MediaAuthorizationStatus> AuthorizeBinAccessAsync(IUser performingUser);
}
