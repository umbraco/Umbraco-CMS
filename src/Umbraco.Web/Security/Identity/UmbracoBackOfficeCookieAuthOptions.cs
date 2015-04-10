using System.Security.Claims;
using System.Threading.Tasks;
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
        public UmbracoBackOfficeCookieAuthOptions()
            : this(UmbracoConfig.For.UmbracoSettings().Security, GlobalSettings.TimeOutInMinutes, GlobalSettings.UseSSL)
        {            
        }

        public UmbracoBackOfficeCookieAuthOptions(            
            ISecuritySection securitySection, 
            int loginTimeoutMinutes, 
            bool forceSsl, 
            bool useLegacyFormsAuthDataFormat = true)
        {
            AuthenticationType = Constants.Security.BackOfficeAuthenticationType;

            if (useLegacyFormsAuthDataFormat)
            {
                //If this is not explicitly set it will fall back to the default automatically
                TicketDataFormat = new FormsAuthenticationSecureDataFormat(loginTimeoutMinutes);    
            }

            CookieDomain = securitySection.AuthCookieDomain;
            CookieName = securitySection.AuthCookieName;
            CookieHttpOnly = true;
            CookieSecure = forceSsl ? CookieSecureOption.Always : CookieSecureOption.SameAsRequest;

            CookiePath = "/";

            //Custom cookie manager so we can filter requests
            CookieManager = new BackOfficeCookieManager(new SingletonUmbracoContextAccessor());
        }       
    }
}