using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     A custom user store that uses Umbraco member data
/// </summary>
public interface IMemberUserStore : IUserStore<MemberIdentityUser>
{
    /// <summary>
    /// Returns the published member content associated with the specified <see cref="MemberIdentityUser"/>.
    /// </summary>
    /// <param name="user">The member identity user whose published member content is to be retrieved.</param>
    /// <returns>The published member content if found; otherwise, <c>null</c>.</returns>
    IPublishedContent? GetPublishedMember(MemberIdentityUser user);
}
