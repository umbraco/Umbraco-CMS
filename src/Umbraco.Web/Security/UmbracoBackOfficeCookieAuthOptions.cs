using System;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// Umbraco auth cookie options
    /// </summary>
    public sealed class UmbracoBackOfficeCookieAuthOptions : CookieAuthenticationOptions
    {
        public int LoginTimeoutMinutes { get; }
        
        public UmbracoBackOfficeCookieAuthOptions(
            string[] explicitPaths,
            IUmbracoContextAccessor umbracoContextAccessor,
            ISecuritySection securitySection,
            IGlobalSettings globalSettings,
            IRuntimeState runtimeState,
            ISecureDataFormat<AuthenticationTicket> secureDataFormat)
        {
            var secureDataFormat1 = secureDataFormat ?? throw new ArgumentNullException(nameof(secureDataFormat));
            LoginTimeoutMinutes = globalSettings.TimeOutInMinutes;
            AuthenticationType = Constants.Security.BackOfficeAuthenticationType;
            
            SlidingExpiration = true;
            ExpireTimeSpan = TimeSpan.FromMinutes(LoginTimeoutMinutes);
            CookieDomain = securitySection.AuthCookieDomain;
            CookieName = securitySection.AuthCookieName;
            CookieHttpOnly = true;
            CookieSecure = globalSettings.UseHttps ? CookieSecureOption.Always : CookieSecureOption.SameAsRequest;
            CookiePath = "/";

            TicketDataFormat = new UmbracoSecureDataFormat(LoginTimeoutMinutes, secureDataFormat1);

            //Custom cookie manager so we can filter requests
            CookieManager = new BackOfficeCookieManager(umbracoContextAccessor, runtimeState, globalSettings, explicitPaths);
        }
        
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
