using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Owin;
using Umbraco.Core;
using Umbraco.Web.Security.Identity;
//using Microsoft.Owin.Security.Facebook;
//using Microsoft.Owin.Security.Google;
//using Microsoft.Owin.Security.OpenIdConnect;
//using Microsoft.Owin.Security.MicrosoftAccount;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Umbraco.Web.UI
{
    public static class UmbracoBackOfficeAuthExtensions
    {
        /*

        ///  <summary>
        ///  Configure microsoft account sign-in
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
        ///      Microsoft.Owin.Security.MicrosoftAccount
        /// 
        ///  Microsoft account documentation for ASP.Net Identity can be found:
        ///  
        ///  http://www.asp.net/web-api/overview/security/external-authentication-services#MICROSOFT
        ///  http://blogs.msdn.com/b/webdev/archive/2012/09/19/configuring-your-asp-net-application-for-microsoft-oauth-account.aspx
        ///  
        ///  Microsoft apps can be created here:
        ///  
        ///  http://go.microsoft.com/fwlink/?LinkID=144070
        ///  
        ///  </remarks>
        public static void ConfigureBackOfficeMicrosoftAuth(this IAppBuilder app, string clientId, string clientSecret,
            string caption = "Microsoft", string style = "btn-microsoft", string icon = "fa-windows")
        {
            var msOptions = new MicrosoftAccountAuthenticationOptions
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                SignInAsAuthenticationType = Constants.Security.BackOfficeExternalAuthenticationType
            };
            msOptions.ForUmbracoBackOffice(style, icon);
            msOptions.Caption = caption;
            app.UseMicrosoftAccountAuthentication(msOptions);
        }

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
            var googleOptions = new GoogleOAuth2AuthenticationOptions
            {
                ClientId = clientId,
                ClientSecret = clientSecret, 
                //In order to allow using different google providers on the front-end vs the back office,
                // these settings are very important to make them distinguished from one another.
                SignInAsAuthenticationType = Constants.Security.BackOfficeExternalAuthenticationType,
                //  By default this is '/signin-google', you will need to change that default value in your
                //  Google developer settings for your web-app in the "REDIRECT URIS" setting
                CallbackPath = new PathString("/umbraco-google-signin")
            };
            googleOptions.ForUmbracoBackOffice(style, icon);
            googleOptions.Caption = caption;
            app.UseGoogleAuthentication(googleOptions);
        }

        ///  <summary>
        ///  Configure facebook sign-in
        ///  </summary>
        ///  <param name="app"></param>
        ///  <param name="appId"></param>
        ///  <param name="appSecret"></param>
        /// <param name="caption"></param>
        /// <param name="style"></param>
        /// <param name="icon"></param>
        /// <remarks>
        ///  
        ///  Nuget installation:
        ///      Microsoft.Owin.Security.Facebook
        /// 
        ///  Facebook account documentation for ASP.Net Identity can be found:
        ///  
        ///  http://www.asp.net/web-api/overview/security/external-authentication-services#FACEBOOK
        ///  
        ///  Facebook apps can be created here:
        /// 
        ///  https://developers.facebook.com/
        ///  
        ///  </remarks>
        public static void ConfigureBackOfficeFacebookAuth(this IAppBuilder app, string appId, string appSecret,
            string caption = "Facebook", string style = "btn-facebook", string icon = "fa-facebook")
        {
            var fbOptions = new FacebookAuthenticationOptions
            {
                AppId = appId,
                AppSecret = appSecret,
                //In order to allow using different google providers on the front-end vs the back office,
                // these settings are very important to make them distinguished from one another.
                SignInAsAuthenticationType = Constants.Security.BackOfficeExternalAuthenticationType,
                //  By default this is '/signin-facebook', you will need to change that default value in your
                //  Facebook developer settings for your app in the Advanced settings under "Client OAuth Login"
                //  in the "Valid OAuth redirect URIs", specify the full URL, for example: http://mysite.com/umbraco-facebook-signin
                CallbackPath = new PathString("/umbraco-facebook-signin")
            };
            fbOptions.ForUmbracoBackOffice(style, icon);
            fbOptions.Caption = caption;
            app.UseFacebookAuthentication(fbOptions);
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
        ///  <param name="appKey"></param>
        ///  <param name="authType">
        ///  This by default is 'OpenIdConnect' but that doesn't match what ASP.Net Identity actually stores in the
        ///  loginProvider field in the database which looks something like this (for example): 
        ///      https://sts.windows.net/3bb0b4c5-364f-4394-ad36-0f29f95e5ggg/
        ///  and is based on your AD setup. This value needs to match in order for accounts to 
        ///  detected as linked/un-linked in the back office. 
        ///  </param>
        /// <param name="caption"></param>
        /// <param name="style"></param>
        /// <param name="icon"></param>
        /// <remarks>
        ///  
        ///  Nuget installation:
        ///      Install-Package Microsoft.Owin.Security.OpenIdConnect
        ///      Install-Package Microsoft.IdentityModel.Clients.ActiveDirectory
        /// 
        ///  ActiveDirectory account documentation for ASP.Net Identity can be found:
        ///  
        ///  https://github.com/AzureADSamples/WebApp-WebAPI-OpenIDConnect-DotNet
        /// 
        ///  This configuration requires the NaiveSessionCache class below which will need to be un-commented
        /// 
        ///  </remarks>
        public static void ConfigureBackOfficeActiveDirectoryAuth(this IAppBuilder app, 
            string tenant, string clientId, string postLoginRedirectUri, string appKey,
            string authType,
            string caption = "Active Directory", string style = "btn-microsoft", string icon = "fa-windows")
        {         
            const string aadInstance = "https://login.windows.net/{0}";
            const string graphResourceId = "https://graph.windows.net";

            var authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
            var adOptions = new OpenIdConnectAuthenticationOptions
            {
                AuthenticationType = authType,
                SignInAsAuthenticationType = Constants.Security.BackOfficeExternalAuthenticationType,
                ClientId = clientId,
                Authority = authority,
                PostLogoutRedirectUri = postLoginRedirectUri,
                Notifications = new OpenIdConnectAuthenticationNotifications()
                {
                    // If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.
                    AuthorizationCodeReceived = (context) =>
                    {
                        var credential = new ClientCredential(clientId, appKey);
                        var userObjectId = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                        var authContext = new AuthenticationContext(authority, new NaiveSessionCache(userObjectId));
                        var result = authContext.AcquireTokenByAuthorizationCode(
                            context.Code,
                            //NOTE: This URL needs to match EXACTLY the same path that is configured in the AD configuration. 
                            new Uri(
                                HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) +
                                HttpContext.Current.Request.RawUrl.EnsureStartsWith('/').EnsureEndsWith('/')),
                            credential,
                            graphResourceId);

                        return Task.FromResult(0);
                    }

                }

            };
            adOptions.ForUmbracoBackOffice(style, icon);
            adOptions.Caption = caption;
            app.UseOpenIdConnectAuthentication(adOptions);
        }
    
         */

    }
    
    

    /*

    /// <summary>
    /// A Session cache token storage which is required to initialize the AD Identity provider on startup
    /// </summary>
    /// <remarks>    
    /// Based on the examples from the AD samples: 
    /// https://github.com/AzureADSamples/WebApp-WebAPI-OpenIDConnect-DotNet/blob/master/TodoListWebApp/Utils/NaiveSessionCache.cs
    /// 
    /// There are some newer examples of different token storage including persistent storage here:
    /// It would appear that this is better for whatever reason: https://github.com/OfficeDev/O365-WebApp-SingleTenant/blob/master/O365-WebApp-SingleTenant/Models/ADALTokenCache.cs
    /// 
    /// The type of token storage will be dependent on your requirements but this should be fine for standard installations
    /// </remarks>
    public class NaiveSessionCache : TokenCache
    {
        private static readonly object FileLock = new object();
        readonly string _cacheId;
        public NaiveSessionCache(string userId)
        {
            _cacheId = userId + "_TokenCache";

            AfterAccess = AfterAccessNotification;
            BeforeAccess = BeforeAccessNotification;
            Load();
        }

        public void Load()
        {
            lock (FileLock)
            {
                Deserialize((byte[])HttpContext.Current.Session[_cacheId]);
            }
        }

        public void Persist()
        {
            lock (FileLock)
            {
                // reflect changes in the persistent store
                HttpContext.Current.Session[_cacheId] = Serialize();
                // once the write operation took place, restore the HasStateChanged bit to false
                HasStateChanged = false;
            }
        }

        // Empties the persistent store.
        public override void Clear()
        {
            base.Clear();
            HttpContext.Current.Session.Remove(_cacheId);
        }

        public override void DeleteItem(TokenCacheItem item)
        {
            base.DeleteItem(item);
            Persist();
        }

        // Triggered right before ADAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after ADAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (HasStateChanged)
            {
                Persist();
            }
        }
    }
    
    */
}