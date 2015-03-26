using Microsoft.Owin;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web.Security.Identity;
using Umbraco.Web.UI;

[assembly: OwinStartup("StandardUmbracoStartup", typeof(StandardUmbracoOwinStartup))]

namespace Umbraco.Web.UI
{
    /// <summary>
    /// The standard way to configure OWIN for Umbraco
    /// </summary>
    /// <remarks>
    /// The startup type is specified in appSettings under owin:appStartup - change it to "StandardUmbracoStartup" to use this class
    /// </remarks>
    public class StandardUmbracoOwinStartup : DefaultUmbracoOwinStartup
    {
        public override void Configuration(IAppBuilder app)
        {
            //ensure the default options are configured
            base.Configuration(app);

            /* 
             * Configure external logins for the back office:
             * 
             * Depending on the authentication sources you would like to enable, you will need to install 
             * certain Nuget packages. 
             * 
             * For Google auth:     Install-Package Microsoft.Owin.Security.Google
             * For Facebook auth:   Install-Package Microsoft.Owin.Security.Facebook
             * For Microsoft auth:  Install-Package Microsoft.Owin.Security.MicrosoftAccount
             * 
             * There are many more providers such as Twitter, Yahoo, ActiveDirectory, etc... most information can
             * be found here: http://www.asp.net/web-api/overview/security/external-authentication-services
             * 
             *  The source for these methods is located in ~/App_Code/IdentityAuthExtensions.cs, you will need to un-comment
             *  the methods that you would like to use. Each method contains documentation and links to
             *  documentation for reference. You can also tweak the code in those extension
             *  methods to suit your needs. 
             */

            //app.ConfigureBackOfficeGoogleAuth("YOUR_APP_ID", "YOUR_APP_SECRET");
            //app.ConfigureBackOfficeFacebookAuth("YOUR_APP_ID", "YOUR_APP_SECRET");
            //app.ConfigureBackOfficeMicrosoftAuth("YOUR_CLIENT_ID", "YOUR_CLIENT_SECRET");
            //app.ConfigureBackOfficeActiveDirectoryAuth("YOUR_TENANT", "YOUR_CLIENT_ID", "YOUR_POST_LOGIN_REDIRECT_URL", "YOUR_APP_KEY", "YOUR_AUTH_TYPE");
        }
    }
}