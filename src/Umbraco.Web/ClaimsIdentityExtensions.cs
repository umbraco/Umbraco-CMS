using System;
using System.Security.Claims;
using System.Security.Principal;

namespace Umbraco.Web
{
    public static class ClaimsIdentityExtensions
    {
        public static string GetUserId(this IIdentity identity)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            string userId = null;
            if (identity is ClaimsIdentity claimsIdentity)
            {
                userId = claimsIdentity.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? claimsIdentity.FindFirstValue("sub");
            }

            return userId;
        }

        public static string GetUserName(this IIdentity identity)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            string username = null;
            if (identity is ClaimsIdentity claimsIdentity)
            {
                username = claimsIdentity.FindFirstValue(ClaimTypes.Name)
                           ?? claimsIdentity.FindFirstValue("preferred_username");
            }

            return username;
        }

        public static string FindFirstValue(this ClaimsIdentity identity, string claimType)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            return identity.FindFirst(claimType)?.Value;
        }
    }
}
