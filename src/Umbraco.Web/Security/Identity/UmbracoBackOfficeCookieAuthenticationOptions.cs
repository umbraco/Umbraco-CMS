using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Web.Security.Identity
{
    /// <summary>
    /// Umbraco auth cookie options
    /// </summary>
    public sealed class UmbracoBackOfficeCookieAuthenticationOptions : CookieAuthenticationOptions
    {
        public UmbracoBackOfficeCookieAuthenticationOptions()
            : this(UmbracoConfig.For.UmbracoSettings().Security, GlobalSettings.TimeOutInMinutes, GlobalSettings.UseSSL, GlobalSettings.Path)
        {            
        }

        public UmbracoBackOfficeCookieAuthenticationOptions(            
            ISecuritySection securitySection, 
            int loginTimeoutMinutes, 
            bool forceSsl, 
            string cookiePath,
            bool useLegacyFormsAuthDataFormat = true)
        {
            AuthenticationType = "UmbracoBackOffice";

            if (useLegacyFormsAuthDataFormat)
            {
                //If this is not explicitly set it will fall back to the default automatically
                TicketDataFormat = new FormsAuthenticationSecureDataFormat(loginTimeoutMinutes, cookiePath);    
            }

            CookieDomain = securitySection.AuthCookieDomain;
            CookieName = securitySection.AuthCookieName;
            CookieHttpOnly = true;
            CookieSecure = forceSsl ? CookieSecureOption.Always : CookieSecureOption.SameAsRequest;

            //Ensure the cookie path is set so that it isn't transmitted for anything apart from requests to the back office
            CookiePath = cookiePath.EnsureStartsWith('/');

        }       
    }
}