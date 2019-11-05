using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;

namespace Umbraco.Core.Security
{
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// This will return the current back office identity if the IPrincipal is the correct type
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        internal static UmbracoBackOfficeIdentity GetUmbracoIdentity(this IPrincipal user)
        {
            //If it's already a UmbracoBackOfficeIdentity
            if (user.Identity is UmbracoBackOfficeIdentity backOfficeIdentity) return backOfficeIdentity;

            //Check if there's more than one identity assigned and see if it's a UmbracoBackOfficeIdentity and use that
            if (user is ClaimsPrincipal claimsPrincipal)
            {
                backOfficeIdentity = claimsPrincipal.Identities.OfType<UmbracoBackOfficeIdentity>().FirstOrDefault();
                if (backOfficeIdentity != null) return backOfficeIdentity;
            }

            //Otherwise convert to a UmbracoBackOfficeIdentity if it's auth'd and has the back office session
            if (user.Identity is ClaimsIdentity claimsIdentity && claimsIdentity.IsAuthenticated && claimsIdentity.HasClaim(x => x.Type == Constants.Security.SessionIdClaimType))
            {
                try
                {
                    return UmbracoBackOfficeIdentity.FromClaimsIdentity(claimsIdentity);
                }
                catch (InvalidOperationException)
                {
                }
            }

            return null;
        }

        /// <summary>
        /// Ensures that the thread culture is set based on the back office user's culture
        /// </summary>
        /// <param name="identity"></param>
        internal static void EnsureCulture(this IIdentity identity)
        {
            if (identity is UmbracoBackOfficeIdentity umbIdentity && umbIdentity.IsAuthenticated)
            {
                Thread.CurrentThread.CurrentUICulture =
                    Thread.CurrentThread.CurrentCulture = UserCultures.GetOrAdd(umbIdentity.Culture, s => new CultureInfo(s));
            }
        }


        /// <summary>
        /// Used so that we aren't creating a new CultureInfo object for every single request
        /// </summary>
        private static readonly ConcurrentDictionary<string, CultureInfo> UserCultures = new ConcurrentDictionary<string, CultureInfo>();

    }
}
