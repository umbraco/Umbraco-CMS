using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages permissions for media access.
/// </summary>
public interface IMediaPermissionService
{
    /// <summary>
    ///     Authorize that a user has access to a media item.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="mediaKey">The identifier of the media item to check for access.</param>
    /// <returns>A task resolving into a <see cref="MediaAuthorizationStatus"/>.</returns>
    Task<MediaAuthorizationStatus> AuthorizeAccessAsync(IUser user, Guid mediaKey)
        => AuthorizeAccessAsync(user, mediaKey.Yield());

    /// <summary>
    ///     Authorize that a user has access to media items.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="mediaKeys">The identifiers of the media items to check for access.</param>
    /// <returns>A task resolving into a <see cref="MediaAuthorizationStatus"/>.</returns>
    Task<MediaAuthorizationStatus> AuthorizeAccessAsync(IUser user, IEnumerable<Guid> mediaKeys);

    /// <summary>
    ///     Authorize that a user has access to the media root item.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <returns>A task resolving into a <see cref="MediaAuthorizationStatus"/>.</returns>
    Task<MediaAuthorizationStatus> AuthorizeRootAccessAsync(IUser user);

    /// <summary>
    ///     Authorize that a user has access to the media bin item.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <returns>A task resolving into a <see cref="MediaAuthorizationStatus"/>.</returns>
    Task<MediaAuthorizationStatus> AuthorizeBinAccessAsync(IUser user);
}
