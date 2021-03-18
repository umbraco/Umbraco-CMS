using System.Linq;
using System.Security.Claims;
using Umbraco.Core.Models.Identity;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Security
{
    internal static class ClaimsIdentityExtensions
    {
        // Ignore these Claims when merging, these claims are dynamically added whenever the ticket
        // is re-issued and we don't want to merge old values of these.
        private static readonly string[] IgnoredClaims = new[] { ClaimTypes.CookiePath, Constants.Security.SessionIdClaimType };

        internal static void MergeClaimsFromBackOfficeIdentity(this ClaimsIdentity destination, ClaimsIdentity source)
        {
            foreach (var claim in source.Claims
                .Where(claim => !IgnoredClaims.Contains(claim.Type))
                .Where(claim => !destination.HasClaim(claim.Type, claim.Value)))
            {
                destination.AddClaim(new Claim(claim.Type, claim.Value));
            }
        }

        internal static void MergeClaimsFromBackOfficeIdentity(this ClaimsIdentity destination, BackOfficeIdentityUser source)
        {
            foreach (var claim in source.Claims
                .Where(claim => !IgnoredClaims.Contains(claim.ClaimType))
                .Where(claim => !destination.HasClaim(claim.ClaimType, claim.ClaimValue)))
            {
                destination.AddClaim(new Claim(claim.ClaimType, claim.ClaimValue));
            }
        }
    }
}
