using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web.Security.Identity;
using Umbraco.Web.UI.App_Start;

[assembly: OwinStartup("UmbracoStandardOwinStartup2", typeof(UmbracoStandardOwinStartup2))]

namespace Umbraco.Web.UI.App_Start
{
    /// <summary>
    /// A custom way to configure OWIN for Umbraco
    /// </summary>
    /// <remarks>
    /// The startup type is specified in appSettings under owin:appStartup - change it to "CustomUmbracoStartup" to use this class
    /// 
    /// This startup class would allow you to customize the Identity IUserStore and/or IUserManager for the Umbraco Backoffice
    /// </remarks>
    public class UmbracoCustomOwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            //Configure the Identity user manager and user store for use with Umbraco Back office

            // *** EXPERT: overloads of this method allow you to specify a custom UserStore or even a custom UserManager!
            // *** If you plan to implement your own custom 2 factor auth, you will need a custom implementation of:
            // *** BackOfficeUserManager & BackOfficeUserStore. Your custom BackOfficeUserManager will need to override/implement: 
            // ***      - SupportsUserTwoFactor to return true
            // ***      - Umbraco.Web.Security.Identity.IUmbracoBackOfficeTwoFactorOptions
            // *** The result view returned from IUmbracoBackOfficeTwoFactorOptions.GetTwoFactorView will be the angular 
            // *** view displayed to the user to enter the 2 factor authentication code. You will need to create/implement
            // *** the custom angular view and all logic to handle the REST call to verify the code and log the user in 
            // *** based on the username provided on the $scope of your view.
            // *** Your custom BackOfficeUserStore will need to override/implement: 
            // ***      - Microsoft.AspNet.Identity.IUserTwoFactorStore<BackOfficeIdentityUser, int>            

            app.ConfigureUserManagerForUmbracoBackOffice(
                ApplicationContext.Current,
                global::Umbraco.Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider().AsUmbracoMembershipProvider());
            
            //Ensure owin is configured for Umbraco back office authentication
            app
                .UseUmbracoBackOfficeCookieAuthentication(ApplicationContext.Current)
                .UseUmbracoBackOfficeExternalCookieAuthentication(ApplicationContext.Current);
            /* 
             * Configure external logins for the back office:
             * 
             * Depending on the authentication sources you would like to enable, you will need to install 
             * certain Nuget packages. 
             * 
             * For Google auth:					Install-Package UmbracoCms.IdentityExtensions.Google
             * For Facebook auth:					Install-Package UmbracoCms.IdentityExtensions.Facebook
             * For Microsoft auth:					Install-Package UmbracoCms.IdentityExtensions.Microsoft
             * For Azure ActiveDirectory auth:		Install-Package UmbracoCms.IdentityExtensions.AzureActiveDirectory
             * 
             * There are many more providers such as Twitter, Yahoo, ActiveDirectory, etc... most information can
             * be found here: http://www.asp.net/web-api/overview/security/external-authentication-services
             * 
             * For sample code on using external providers with the Umbraco back office, install one of the 
             * packages listed above to review it's code samples 
             *  
             */
            /*
             * To configure a simple auth token server for the back office:
             *             
             * By default the CORS policy is to allow all requests
             * 
             *      app.UseUmbracoBackOfficeTokenAuth(new BackOfficeAuthServerProviderOptions());
             *      
             * If you want to have a custom CORS policy for the token server you can provide
             * a custom CORS policy, example: 
             * 
             *      app.UseUmbracoBackOfficeTokenAuth(
             *          new BackOfficeAuthServerProviderOptions()
             *              {
             *             		//Modify the CorsPolicy as required
             *                  CorsPolicy = new CorsPolicy()
             *                  {
             *                      AllowAnyHeader = true,
             *                      AllowAnyMethod = true,
             *                      Origins = { "http://mywebsite.com" }                
             *                  }
             *              });
             */
        }
    }

    /// <summary>
    /// The standard way to configure OWIN for Umbraco
    /// </summary>
    /// <remarks>
    /// The startup type is specified in appSettings under owin:appStartup - change it to "StandardUmbracoStartup" to use this class
    /// </remarks>
    public class UmbracoStandardOwinStartup2 : UmbracoDefaultOwinStartup
    {
        public override void Configuration(IAppBuilder app)
        {
            //ensure the default options are configured
            base.Configuration(app);

            //
            app.ConfigureBackOfficeGoogleAuth(
                "1072120697051-p41pro11srud3o3n90j7m00geq426jqt.apps.googleusercontent.com",
                "cs_LJTXh2rtI01C5OIt9WFkt");

            /* 
             * Configure external logins for the back office:
             * 
             * Depending on the authentication sources you would like to enable, you will need to install 
             * certain Nuget packages. 
             * 
             * For Google auth:					Install-Package UmbracoCms.IdentityExtensions.Google
             * For Facebook auth:					Install-Package UmbracoCms.IdentityExtensions.Facebook
             * For Microsoft auth:					Install-Package UmbracoCms.IdentityExtensions.Microsoft
             * For Azure ActiveDirectory auth:		Install-Package UmbracoCms.IdentityExtensions.AzureActiveDirectory
             * 
             * There are many more providers such as Twitter, Yahoo, ActiveDirectory, etc... most information can
             * be found here: http://www.asp.net/web-api/overview/security/external-authentication-services
             * 
             * For sample code on using external providers with the Umbraco back office, install one of the 
             * packages listed above to review it's code samples 
             *  
             */

            /*
             * To configure a simple auth token server for the back office:
             *             
             * By default the CORS policy is to allow all requests
             * 
             *      app.UseUmbracoBackOfficeTokenAuth(new BackOfficeAuthServerProviderOptions());
             *      
             * If you want to have a custom CORS policy for the token server you can provide
             * a custom CORS policy, example: 
             * 
             *      app.UseUmbracoBackOfficeTokenAuth(
             *          new BackOfficeAuthServerProviderOptions()
             *              {
             *             		//Modify the CorsPolicy as required
             *                  CorsPolicy = new CorsPolicy()
             *                  {
             *                      AllowAnyHeader = true,
             *                      AllowAnyMethod = true,
             *                      Origins = { "http://mywebsite.com" }                
             *                  }
             *              });
             */

        }
    }

    public static class UmbracoGoogleAuthExtensions
    {
        ///  <summary>
        ///  Configure google sign-in
        ///  </summary>
        ///  <param name="app"></param>
        ///  <param name="clientId"></param>
        ///  <param name="clientSecret"></param>
        /// <param name="caption"></param>
        /// <param name="style"></param>
        /// <param name="icon"></param>
        /// <remarks>
        ///  
        ///  Nuget installation:
        ///      Microsoft.Owin.Security.Google
        /// 
        ///  Google account documentation for ASP.Net Identity can be found:
        ///  
        ///  http://www.asp.net/web-api/overview/security/external-authentication-services#GOOGLE
        ///  
        ///  Google apps can be created here:
        ///  
        ///  https://developers.google.com/accounts/docs/OpenIDConnect#getcredentials
        ///  
        ///  </remarks>
        public static void ConfigureBackOfficeGoogleAuth(this IAppBuilder app, string clientId, string clientSecret,
            string caption = "Google", string style = "btn-google-plus", string icon = "fa-google-plus")
        {
            var googleOptions = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                //In order to allow using different google providers on the front-end vs the back office,
                // these settings are very important to make them distinguished from one another.
                SignInAsAuthenticationType = global::Umbraco.Core.Constants.Security.BackOfficeExternalAuthenticationType,
                //  By default this is '/signin-google', you will need to change that default value in your
                //  Google developer settings for your web-app in the "REDIRECT URIS" setting
                CallbackPath = new PathString("/umbraco-google-signin")
            };

            googleOptions.SetExternalSignInAutoLinkOptions(
                new ExternalSignInAutoLinkOptions(
                    autoLinkExternalAccount: true));

            googleOptions.ForUmbracoBackOffice(style, icon);
            googleOptions.Caption = caption;
            app.UseGoogleAuthentication(googleOptions);
        }

        ///  <summary>
        ///  Configure ActiveDirectory sign-in
        ///  </summary>
        ///  <param name="app"></param>
        ///  <param name="tenant"></param>
        ///  <param name="clientId"></param>
        ///  <param name="postLoginRedirectUri">
        ///  The URL that will be redirected to after login is successful, example: http://mydomain.com/umbraco/;
        ///  </param>
        ///  <param name="issuerId">
        /// 
        ///  This is the "Issuer Id" for you Azure AD application. This a GUID value and can be found
        ///  in the Azure portal when viewing your configured application and clicking on 'View endpoints'
        ///  which will list all of the API endpoints. Each endpoint will contain a GUID value, this is
        ///  the Issuer Id which must be used for this value.        
        /// 
        ///  If this value is not set correctly then accounts won't be able to be detected 
        ///  for un-linking in the back office. 
        /// 
        ///  </param>
        /// <param name="caption"></param>
        /// <param name="style"></param>
        /// <param name="icon"></param>
        /// <remarks>
        ///  ActiveDirectory account documentation for ASP.Net Identity can be found:
        ///  https://github.com/AzureADSamples/WebApp-WebAPI-OpenIDConnect-DotNet
        ///  </remarks>
        public static void ConfigureBackOfficeAzureActiveDirectoryAuth(this IAppBuilder app,
            string tenant, string clientId, string postLoginRedirectUri, Guid issuerId,
            string caption = "Active Directory", string style = "btn-microsoft", string icon = "fa-windows")
        {
            var authority = string.Format(
                CultureInfo.InvariantCulture,
                "https://login.windows.net/{0}",
                tenant);

            var adOptions = new OpenIdConnectAuthenticationOptions
            {
                SignInAsAuthenticationType = global::Umbraco.Core.Constants.Security.BackOfficeExternalAuthenticationType,
                ClientId = clientId,
                Authority = authority,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    RedirectToIdentityProvider = notification =>
                    {
                        return Task.FromResult(0);
                    }
                }
            };

            adOptions.SetExternalSignInAutoLinkOptions(new ExternalSignInAutoLinkOptions());

            adOptions.ForUmbracoBackOffice(style, icon);
            adOptions.Caption = caption;
            //Need to set the auth tyep as the issuer path
            adOptions.AuthenticationType = string.Format(
                CultureInfo.InvariantCulture,
                "https://sts.windows.net/{0}/",
                issuerId);
            app.UseOpenIdConnectAuthentication(adOptions);
        }    

    }
}