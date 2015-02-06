using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
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
            : this(UmbracoConfig.For.UmbracoSettings().Security, GlobalSettings.TimeOutInMinutes, GlobalSettings.UseSSL)
        {            
        }

        public UmbracoBackOfficeCookieAuthenticationOptions(ISecuritySection securitySection, int loginTimeoutMinutes, bool forceSsl)
        {
            AuthenticationType = "UmbracoBackOffice";

            TicketDataFormat = new FormsAuthenticationSecureDataFormat(loginTimeoutMinutes);

            CookieDomain = securitySection.AuthCookieDomain;
            CookieName = securitySection.AuthCookieName;
            CookieHttpOnly = true;
            CookieSecure = forceSsl ? CookieSecureOption.Always : CookieSecureOption.SameAsRequest;
            CookiePath = "/";
            LoginPath = new PathString("/umbraco/login"); //TODO: ??

        }       
    }
}