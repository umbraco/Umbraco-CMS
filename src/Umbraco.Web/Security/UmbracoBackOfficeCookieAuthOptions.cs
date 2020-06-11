using System;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// Umbraco auth cookie options
    /// </summary>
    public sealed class UmbracoBackOfficeCookieAuthOptions : CookieAuthenticationOptions
    {
        public int LoginTimeoutMinutes { get; }

        /// <summary>
        /// Creates the cookie options for saving the auth cookie
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public CookieOptions CreateRequestCookieOptions(IOwinContext ctx, AuthenticationTicket ticket)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (ticket == null) throw new ArgumentNullException(nameof(ticket));

            var issuedUtc = ticket.Properties.IssuedUtc ?? SystemClock.UtcNow;
            var expiresUtc = ticket.Properties.ExpiresUtc ?? issuedUtc.Add(ExpireTimeSpan);

            var cookieOptions = new CookieOptions
            {
                Path = "/",
                Domain = this.CookieDomain ?? null,
                HttpOnly = true,
                Secure = this.CookieSecure == CookieSecureOption.Always
                                         || (this.CookieSecure == CookieSecureOption.SameAsRequest && ctx.Request.IsSecure),
            };

            if (ticket.Properties.IsPersistent)
            {
                cookieOptions.Expires = expiresUtc.UtcDateTime;
            }

            return cookieOptions;
        }

    }
}
