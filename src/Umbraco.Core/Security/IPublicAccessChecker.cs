using System.Security.Claims;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Provides functionality to check public access rules for content items.
/// </summary>
public interface IPublicAccessChecker
{
    /// <summary>
    ///     Tests the current member access level to a given content item.
    /// </summary>
    /// <param name="publishedContentId">The ID of the content item.</param>
    /// <returns>The access level for the content item.</returns>
    Task<PublicAccessStatus> HasMemberAccessToContentAsync(int publishedContentId);

    /// <summary>
    ///     Tests member access level to a given content item.
    /// </summary>
    /// <param name="publishedContentId">The ID of the content item.</param>
    /// <param name="claimsPrincipal">The member claims to test against the content item.</param>
    /// <returns>The access level for the content item.</returns>
    Task<PublicAccessStatus> HasMemberAccessToContentAsync(int publishedContentId, ClaimsPrincipal claimsPrincipal) => Task.FromResult(PublicAccessStatus.AccessDenied);
}
