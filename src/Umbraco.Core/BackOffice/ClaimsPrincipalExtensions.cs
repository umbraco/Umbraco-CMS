using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Umbraco.Core;
using Umbraco.Core.BackOffice;

namespace Umbraco.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// This will return the current back office identity if the IPrincipal is the correct type
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static UmbracoBackOfficeIdentity GetUmbracoIdentity(this IPrincipal user)
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
                    // We catch this error because it's what we throw when the required claims are not in the identity.
                    // when that happens something strange is going on, we'll swallow this exception and return null.
                }
            }

            return null;
        }
    }
}
