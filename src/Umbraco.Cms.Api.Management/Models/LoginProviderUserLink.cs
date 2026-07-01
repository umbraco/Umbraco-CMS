using System.Security.Claims;

namespace Umbraco.Cms.Api.Management.Models;

/// <summary>
/// Represents a link between a user and a login provider.
/// </summary>
public class LoginProviderUserLink
{
    /// <summary>
    /// Gets or sets the unique identifier of the user as obtained from the claims principal.
    /// </summary>
    public required string ClaimsPrincipalUserId { get; set; }

    /// <summary>
    /// Gets or sets the login provider associated with the user link.
    /// </summary>
    public required string LoginProvider { get; set; }
}
