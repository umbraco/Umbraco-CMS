using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Umbraco.Core.BackOffice;

namespace Umbraco.Extensions
{
    public static class HttpContextExtensions
    {
        public static void SetPrincipalForRequest(this HttpContext context, ClaimsPrincipal principal)
        {
            context.User = principal;
        }

        /// <summary>
        /// This will return the current back office identity.
        /// </summary>
        /// <param name="http"></param>        
        /// <returns>
        /// Returns the current back office identity if an admin is authenticated otherwise null
        /// </returns>
        public static UmbracoBackOfficeIdentity GetCurrentIdentity(this HttpContext http)
        {
            if (http == null) throw new ArgumentNullException(nameof(http));
            if (http.User == null) return null; //there's no user at all so no identity

            // If it's already a UmbracoBackOfficeIdentity
            var backOfficeIdentity = http.User.GetUmbracoIdentity();
            if (backOfficeIdentity != null) return backOfficeIdentity;

            return null;
        }
    }
}
