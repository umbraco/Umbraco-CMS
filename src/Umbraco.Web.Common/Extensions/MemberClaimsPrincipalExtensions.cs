using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Umbraco.Extensions;

public static class MemberClaimsPrincipalExtensions
{
    /// <summary>
    /// Tries to get specifically the member identity from the ClaimsPrincipal
    /// </summary>
    /// <remarks>
    /// The identity returned is the one with default authentication type.
    /// </remarks>
    /// <param name="principal">The principal to find the identity in.</param>
    /// <returns>The default authenticated authentication type identity.</returns>
    public static ClaimsIdentity? GetMemberIdentity(this ClaimsPrincipal principal)
        => principal.Identities.FirstOrDefault(x => x.AuthenticationType == IdentityConstants.ApplicationScheme);
}
