using System.Security.Claims;

namespace Umbraco.Cms.Api.Management.Models;

public class LoginProviderUserLink
{
    public required string ClaimsPrincipalUserId { get; set; }

    public required string LoginProvider { get; set; }
}
