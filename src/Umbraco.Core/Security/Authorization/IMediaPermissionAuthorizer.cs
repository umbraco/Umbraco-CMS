using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     Authorizes media access.
/// </summary>
public interface IMediaPermissionAuthorizer
{
    /// <summary>
    ///     Authorizes whether the current user has access to the specified media item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="mediaKey">The key of the media item to check for.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IUser currentUser, Guid mediaKey)
        => IsDeniedAsync(currentUser, mediaKey.Yield());

    /// <summary>
    ///     Authorizes whether the current user has access to the specified media item(s).
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="mediaKeys">The keys of the media items to check for.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IUser currentUser, IEnumerable<Guid> mediaKeys);

    /// <summary>
    ///     Authorizes whether the current user has access to the root item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAtRootLevelAsync(IUser currentUser);

    /// <summary>
    ///     Authorizes whether the current user has access to the recycle bin item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAtRecycleBinLevelAsync(IUser currentUser);
}
