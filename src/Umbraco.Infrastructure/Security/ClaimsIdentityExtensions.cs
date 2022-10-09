// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Extensions;

public static class MergeClaimsIdentityExtensions
{
    // Ignore these Claims when merging, these claims are dynamically added whenever the ticket
    // is re-issued and we don't want to merge old values of these.
    // We do however want to merge these when the SecurityStampValidator refreshes the principal since it's still the same login session
    private static readonly string[] _ignoredClaims = { ClaimTypes.CookiePath, Constants.Security.SessionIdClaimType };

    public static void MergeAllClaims(this ClaimsIdentity destination, ClaimsIdentity source)
    {
        foreach (Claim claim in source.Claims
                     .Where(claim => !destination.HasClaim(claim.Type, claim.Value)))
        {
            destination.AddClaim(new Claim(claim.Type, claim.Value));
        }
    }

    public static void MergeClaimsFromCookieIdentity(this ClaimsIdentity destination, ClaimsIdentity source)
    {
        foreach (Claim claim in source.Claims
                     .Where(claim => !_ignoredClaims.Contains(claim.Type))
                     .Where(claim => !destination.HasClaim(claim.Type, claim.Value)))
        {
            destination.AddClaim(new Claim(claim.Type, claim.Value));
        }
    }

    public static void MergeClaimsFromBackOfficeIdentity(this ClaimsIdentity destination, BackOfficeIdentityUser source)
    {
        foreach (IdentityUserClaim<string> claim in source.Claims
                     .Where(claim => !_ignoredClaims.Contains(claim.ClaimType))
                     .Where(claim => !destination.HasClaim(claim.ClaimType, claim.ClaimValue)))
        {
            destination.AddClaim(new Claim(claim.ClaimType, claim.ClaimValue));
        }
    }
}
