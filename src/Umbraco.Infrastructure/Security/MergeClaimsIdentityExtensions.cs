// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Extensions;

/// <summary>
/// Contains extension methods for merging <see cref="System.Security.Claims.ClaimsIdentity"/> instances.
/// </summary>
public static class MergeClaimsIdentityExtensions
{
    // Ignore these Claims when merging, these claims are dynamically added whenever the ticket
    // is re-issued and we don't want to merge old values of these.
    // We do however want to merge these when the SecurityStampValidator refreshes the principal since it's still the same login session
    private static readonly string[] _ignoredClaims = { ClaimTypes.CookiePath, Constants.Security.SessionIdClaimType };

    /// <summary>
    /// Merges all claims from the source <see cref="ClaimsIdentity"/> into the destination <see cref="ClaimsIdentity"/>,
    /// adding only those claims that do not already exist in the destination.
    /// </summary>
    /// <param name="destination">The <see cref="ClaimsIdentity"/> to which claims will be added.</param>
    /// <param name="source">The <see cref="ClaimsIdentity"/> from which claims will be copied.</param>
    public static void MergeAllClaims(this ClaimsIdentity destination, ClaimsIdentity source)
    {
        foreach (Claim claim in source.Claims
                     .Where(claim => !destination.HasClaim(claim.Type, claim.Value)))
        {
            destination.AddClaim(new Claim(claim.Type, claim.Value));
        }
    }

    /// <summary>
    /// Merges claims from the <paramref name="source" /> <see cref="System.Security.Claims.ClaimsIdentity" /> into the <paramref name="destination" /> <see cref="System.Security.Claims.ClaimsIdentity" />.
    /// Only claims that are not present in the destination and are not in the ignored claims list are added.
    /// </summary>
    /// <param name="destination">The <see cref="System.Security.Claims.ClaimsIdentity" /> to which new claims will be added.</param>
    /// <param name="source">The <see cref="System.Security.Claims.ClaimsIdentity" /> from which claims will be merged.</param>
    public static void MergeClaimsFromCookieIdentity(this ClaimsIdentity destination, ClaimsIdentity source)
    {
        foreach (Claim claim in source.Claims
                     .Where(claim => !_ignoredClaims.Contains(claim.Type))
                     .Where(claim => !destination.HasClaim(claim.Type, claim.Value)))
        {
            destination.AddClaim(new Claim(claim.Type, claim.Value));
        }
    }

    /// <summary>
    /// Adds claims from a <see cref="BackOfficeIdentityUser"/> to the specified <see cref="ClaimsIdentity"/>,
    /// excluding any claims whose types are in the ignored claims list and skipping claims that already exist in the destination.
    /// </summary>
    /// <param name="destination">The <see cref="ClaimsIdentity"/> to which new claims will be added.</param>
    /// <param name="source">The <see cref="BackOfficeIdentityUser"/> whose claims will be merged into the destination identity.</param>
    /// <remarks>
    /// Claims are only added if their type is not in the ignored claims list and if an identical claim does not already exist in the destination.
    /// </remarks>
    public static void MergeClaimsFromBackOfficeIdentity(this ClaimsIdentity destination, BackOfficeIdentityUser source)
    {
        foreach (IdentityUserClaim<string> claim in source.Claims
                     .Where(claim => !_ignoredClaims.Contains(claim.ClaimType))
                     .Where(claim => !destination.HasClaim(claim.ClaimType!, claim.ClaimValue!)))
        {
            destination.AddClaim(new Claim(claim.ClaimType!, claim.ClaimValue!));
        }
    }
}
