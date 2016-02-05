using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Web.Security.Identity
{
    /// <summary>
    /// Umbraco auth cookie options
    /// </summary>
    public sealed class UmbracoBackOfficeCookieAuthOptions : CookieAuthenticationOptions
    {
        public int LoginTimeoutMinutes { get; private set; }

        public UmbracoBackOfficeCookieAuthOptions()
            : this(UmbracoConfig.For.UmbracoSettings().Security, GlobalSettings.TimeOutInMinutes, GlobalSettings.UseSSL)
        {            
        }
        
        public UmbracoBackOfficeCookieAuthOptions(
            string[] explicitPaths,
            ISecuritySection securitySection,
            int loginTimeoutMinutes,
            bool forceSsl,
            bool useLegacyFormsAuthDataFormat = true)            
        {
            LoginTimeoutMinutes = loginTimeoutMinutes;
            AuthenticationType = Constants.Security.BackOfficeAuthenticationType;

            if (useLegacyFormsAuthDataFormat)
            {
                //If this is not explicitly set it will fall back to the default automatically
                TicketDataFormat = new FormsAuthenticationSecureDataFormat(LoginTimeoutMinutes);
            }

            SlidingExpiration = true;
            ExpireTimeSpan = TimeSpan.FromMinutes(LoginTimeoutMinutes);
            CookieDomain = securitySection.AuthCookieDomain;
            CookieName = securitySection.AuthCookieName;
            CookieHttpOnly = true;
            CookieSecure = forceSsl ? CookieSecureOption.Always : CookieSecureOption.SameAsRequest;
            CookiePath = "/";            

            //Custom cookie manager so we can filter requests
            CookieManager = new BackOfficeCookieManager(new SingletonUmbracoContextAccessor(), explicitPaths);
        }

        public UmbracoBackOfficeCookieAuthOptions(
            ISecuritySection securitySection,
            int loginTimeoutMinutes,
            bool forceSsl,
            bool useLegacyFormsAuthDataFormat = true)
            : this(null, securitySection, loginTimeoutMinutes, forceSsl, useLegacyFormsAuthDataFormat)
        {
        }

        /// <summary>
        /// Creates the cookie options for saving the auth cookie
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public CookieOptions CreateRequestCookieOptions(IOwinContext ctx, AuthenticationTicket ticket)
        {
            if (ctx == null) throw new ArgumentNullException("ctx");
            if (ticket == null) throw new ArgumentNullException("ticket");

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