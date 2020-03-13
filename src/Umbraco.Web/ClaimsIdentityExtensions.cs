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
                userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? claimsIdentity.FindFirst("sub")?.Value;
            }

            return userId;
        }

        public static string GetUserName(this IIdentity identity)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            string username = null;
            if (identity is ClaimsIdentity claimsIdentity)
            {
                username = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value
                           ?? claimsIdentity.FindFirst("preferred_username")?.Value;
            }

            return username;
        }
    }
}
