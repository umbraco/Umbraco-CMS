using System.Security.Principal;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Media;

/// <summary>
///     Authorizes media access.
/// </summary>
public interface IMediaAuthorizer
{
    /// <summary>
    ///     Authorizes whether the current user has access to the specified media item(s).
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="mediaKeys">The keys of the media items to check for.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedAsync(IPrincipal currentUser, IEnumerable<Guid> mediaKeys);
}
