using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     A custom user store that uses Umbraco member data
/// </summary>
public interface IMemberUserStore : IUserStore<MemberIdentityUser>
{
    IPublishedContent? GetPublishedMember(MemberIdentityUser user);
}
