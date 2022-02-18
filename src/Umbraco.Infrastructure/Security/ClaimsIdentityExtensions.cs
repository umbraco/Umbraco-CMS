// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using System.Security.Claims;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Extensions
{
    public static class MergeClaimsIdentityExtensions
    {
        // We used to ignore CookiePath and SessionIdClaimType, but these claims are only issued at login
        // meaning if we don't merge these claims you'll be logged out, after the claims are merged
        // since your session ID disappears
        private static readonly string[] s_ignoredClaims = Array.Empty<string>();

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
                .Where(claim => !s_ignoredClaims.Contains(claim.Type))
                .Where(claim => !destination.HasClaim(claim.Type, claim.Value)))
            {
                destination.AddClaim(new Claim(claim.Type, claim.Value));
            }
        }

        public static void MergeClaimsFromBackOfficeIdentity(this ClaimsIdentity destination, BackOfficeIdentityUser source)
        {
            foreach (Microsoft.AspNetCore.Identity.IdentityUserClaim<string> claim in source.Claims
                .Where(claim => !s_ignoredClaims.Contains(claim.ClaimType))
                .Where(claim => !destination.HasClaim(claim.ClaimType, claim.ClaimValue)))
            {
                destination.AddClaim(new Claim(claim.ClaimType, claim.ClaimValue));
            }
        }
    }
}
