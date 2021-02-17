using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Umbraco.Core;
using Umbraco.Core.Security;

namespace Umbraco.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// This will return the current back office identity if the IPrincipal is the correct type and authenticated.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static ClaimsIdentity GetUmbracoIdentity(this IPrincipal user)
        {
            // Check if the identity is a ClaimsIdentity, and that's it's authenticated and has all required claims.
            if (user.Identity is ClaimsIdentity claimsIdentity
                && claimsIdentity.IsAuthenticated
                && claimsIdentity.VerifyBackOfficeIdentity(out ClaimsIdentity umbracoIdentity))
            {
                if (claimsIdentity.AuthenticationType == Constants.Security.BackOfficeAuthenticationType)
                {
                    return claimsIdentity;
                }
                return umbracoIdentity;
            }

            return null;
        }

        /// <summary>
        /// Returns the remaining seconds on an auth ticket for the user based on the claim applied to the user durnig authentication
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static double GetRemainingAuthSeconds(this IPrincipal user) => user.GetRemainingAuthSeconds(DateTimeOffset.UtcNow);

        /// <summary>
        /// Returns the remaining seconds on an auth ticket for the user based on the claim applied to the user durnig authentication
        /// </summary>
        /// <param name="user"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public static double GetRemainingAuthSeconds(this IPrincipal user, DateTimeOffset now)
        {
            var claimsPrincipal = user as ClaimsPrincipal;
            if (claimsPrincipal == null) return 0;

            var ticketExpires = claimsPrincipal.FindFirst(Constants.Security.TicketExpiresClaimType)?.Value;
            if (ticketExpires.IsNullOrWhiteSpace()) return 0;

            var utcExpired = DateTimeOffset.Parse(ticketExpires, null, DateTimeStyles.RoundtripKind);

            var secondsRemaining = utcExpired.Subtract(now).TotalSeconds;
            return secondsRemaining;
        }
    }
}
