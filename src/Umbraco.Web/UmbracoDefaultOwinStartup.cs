using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Security;
using Umbraco.Web;
using Umbraco.Web.Security.Identity;

[assembly: OwinStartup("UmbracoDefaultOwinStartup", typeof(UmbracoDefaultOwinStartup))]

namespace Umbraco.Web
{
    /// <summary>
    /// The default way to configure OWIN for Umbraco
    /// </summary>
    /// <remarks>
    /// The startup type is specified in appSettings under owin:appStartup
    /// </remarks>
    public class UmbracoDefaultOwinStartup
    {
        public virtual void Configuration(IAppBuilder app)
        {
            app.SetUmbracoLoggerFactory();

            //Configure the Identity user manager for use with Umbraco Back office 
            // (EXPERT: an overload accepts a custom BackOfficeUserStore implementation)
            app.ConfigureUserManagerForUmbracoBackOffice(
                ApplicationContext.Current,
                Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider().AsUmbracoMembershipProvider());

            //Ensure owin is configured for Umbraco back office authentication. If you have any front-end OWIN
            // cookie configuration, this must be declared after it.
            app
                .UseUmbracoBackOfficeCookieAuthentication()
                .UseUmbracoBackOfficeExternalCookieAuthentication();
        }
    }
}
