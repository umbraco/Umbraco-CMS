using System;
using System.Security.Claims;
using System.Security.Principal;

namespace Umbraco.Cms.Core
{
    public static class ClaimsIdentityExtensions
    {
        public static T GetUserId<T>(this IIdentity identity)
        {
            var strId = identity.GetUserId();
            var converted = strId.TryConvertTo<T>();
            return converted.ResultOr(default);
        }

        /// <summary>
        /// Returns the user id from the <see cref="IIdentity"/> of either the claim type <see cref="ClaimTypes.NameIdentifier"/> or "sub"
        /// </summary>
        /// <param name="identity"></param>
        /// <returns>
        /// The string value of the user id if found otherwise null
        /// </returns>
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

        /// <summary>
        /// Returns the user name from the <see cref="IIdentity"/> of either the claim type <see cref="ClaimTypes.Name"/> or "preferred_username"
        /// </summary>
        /// <param name="identity"></param>
        /// <returns>
        /// The string value of the user name if found otherwise null
        /// </returns>
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

        /// <summary>
        /// Returns the first claim value found in the <see cref="ClaimsIdentity"/> for the given claimType
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="claimType"></param>
        /// <returns>
        /// The string value of the claim if found otherwise null
        /// </returns>
        public static string FindFirstValue(this ClaimsIdentity identity, string claimType)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            return identity.FindFirst(claimType)?.Value;
        }
    }
}
