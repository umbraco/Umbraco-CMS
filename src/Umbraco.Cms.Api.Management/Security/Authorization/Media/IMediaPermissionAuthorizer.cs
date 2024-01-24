using System.Security.Principal;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Media;

/// <summary>
///     Authorizes media access.
/// </summary>
public interface IMediaPermissionAuthorizer
{
    /// <summary>
    ///     Authorizes whether the current user has access to the specified media item.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="mediaKey">The key of the media item to check for.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedAsync(IPrincipal currentUser, Guid mediaKey)
        => IsDeniedAsync(currentUser, mediaKey.Yield());

    /// <summary>
    ///     Authorizes whether the current user has access to the specified media item(s).
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="mediaKeys">The keys of the media items to check for.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IPrincipal currentUser, IEnumerable<Guid> mediaKeys);

    /// <summary>
    ///     Authorizes whether the current user has access to the root item.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAtRootLevelAsync(IPrincipal currentUser);

    /// <summary>
    ///     Authorizes whether the current user has access to the recycle bin item.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAtRecycleBinLevelAsync(IPrincipal currentUser);
}
