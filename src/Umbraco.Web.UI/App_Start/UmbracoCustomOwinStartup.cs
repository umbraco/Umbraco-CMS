//using Microsoft.Owin;
//using Microsoft.Owin.Logging;
//using Owin;
//using Umbraco.Core;
//using Umbraco.Core.Logging;
//using Umbraco.Core.Security;
//using Umbraco.Web.Security.Identity;
//using Umbraco.Web.UI;

////To use this startup class, change the appSetting value in the web.config called 
//// "owin:appStartup" to be "CustomUmbracoOwinStartup"

//[assembly: OwinStartup("UmbracoCustomOwinStartup", typeof(UmbracoCustomOwinStartup))]

//namespace Umbraco.Web.UI
//{
//    /// <summary>
//    /// A custom way to configure OWIN for Umbraco
//    /// </summary>
//    /// <remarks>
//    /// The startup type is specified in appSettings under owin:appStartup - change it to "CustomUmbracoStartup" to use this class
//    /// 
//    /// This startup class would allow you to customize the Identity IUserStore and/or IUserManager for the Umbraco Backoffice
//    /// </remarks>
//    public class UmbracoCustomOwinStartup
//    {
//        public void Configuration(IAppBuilder app)
//        {
//            app.SetLoggerFactory(new OwinLoggerFactory());

//            //Configure the Identity user manager for use with Umbraco Back office 
//            // (EXPERT: an overload accepts a custom BackOfficeUserStore implementation)
//            app.ConfigureUserManagerForUmbracoBackOffice(
//                ApplicationContext.Current,
//                Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider().AsUmbracoMembershipProvider());

//            //Ensure owin is configured for Umbraco back office authentication
//            app
//                .UseUmbracoBackOfficeCookieAuthentication()
//                .UseUmbracoBackOfficeExternalCookieAuthentication();

//            /* 
//             * Configure external logins for the back office:
//             * 
//             * Depending on the authentication sources you would like to enable, you will need to install 
//             * certain Nuget packages. 
//             * 
//             * For Google auth:             Install-Package UmbracoCms.Identity.Google
//             * For Facebook auth:           Install-Package UmbracoCms.Identity.Facebook
//             * For Microsoft auth:          Install-Package UmbracoCms.Identity.MicrosoftAccount
//             * For ActiveDirectory auth:    Install-Package UmbracoCms.Identity.ActiveDirectory
//             * 
//             * There are many more providers such as Twitter, Yahoo, ActiveDirectory, etc... most information can
//             * be found here: http://www.asp.net/web-api/overview/security/external-authentication-services
//             * 
//             * For sample code on using external providers with the Umbraco back office, install one of the 
//             * packages listed above to review it's code samples 
//             *  
//             */

//            app.ConfigureBackOfficeGoogleAuth(
//                "1072120697051-p41pro11srud3o3n90j7m00geq426jqt.apps.googleusercontent.com",
//                "cs_LJTXh2rtI01C5OIt9WFkt");
//        }

        
//    }
//}
