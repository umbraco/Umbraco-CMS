using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web.Security.Identity;
using Umbraco.Web.UI;

[assembly: OwinStartup(typeof(OwinStartup))]

namespace Umbraco.Web.UI
{

    /// <summary>
    /// Summary description for Startup
    /// </summary>
    public class OwinStartup
    {

        public async Task DoStuff()
        {
            var client = new HttpClient();

            using (var request = await client.PostAsJsonAsync("", "123"))                        
            {
                
            }
        }

        public void Configuration(IAppBuilder app)
        {
            
            



            //Single method to configure the Identity user manager for use with Umbraco
            app.ConfigureUserManagerForUmbracoBackOffice(
                ApplicationContext.Current,
                Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider().AsUmbracoMembershipProvider());

            //// Enable the application to use a cookie to store information for the 
            //// signed in user and to use a cookie to temporarily store information 
            //// about a user logging in with a third party login provider 
            //// Configure the sign in cookie
            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //{
            //    AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,

            //    Provider = new CookieAuthenticationProvider
            //    {
            //        // Enables the application to validate the security stamp when the user 
            //        // logs in. This is a security feature which is used when you 
            //        // change a password or add an external login to your account.  
            //        OnValidateIdentity = SecurityStampValidator
            //            .OnValidateIdentity<UmbracoMembersUserManager<UmbracoApplicationUser>, UmbracoApplicationUser, int>(
            //                TimeSpan.FromMinutes(30),
            //                (manager, user) => user.GenerateUserIdentityAsync(manager),
            //                identity => identity.GetUserId<int>())
            //    }
            //});

            //Ensure owin is configured for Umbraco back office authentication - this must
            // be configured AFTER the standard UseCookieConfiguration above.
            app
                .UseUmbracoBackOfficeCookieAuthentication()
                .UseUmbracoBackOfficeExternalCookieAuthentication();

            //app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            
            //app.UseGoogleAuthentication(
            //    clientId: "1072120697051-07jlhgrd5hodsfe7dgqimdie8qc1omet.apps.googleusercontent.com",
            //    clientSecret: "Ue9swN0lEX9rwxzQz1Y_tFzg");

            var googleOptions = new GoogleOAuth2AuthenticationOptions
            {
           
            };
            googleOptions.Description.Properties["SocialStyle"] = "btn-google-plus";
            googleOptions.Description.Properties["SocialIcon"] = "fa-google-plus";
            googleOptions.Caption = "Google";
            app.UseGoogleAuthentication(googleOptions);

            //AD docs are here:
            // https://github.com/AzureADSamples/WebApp-WebAPI-OpenIDConnect-DotNet
          
            var authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
            var adOptions = new OpenIdConnectAuthenticationOptions
            {
                //NOTE: This by default is 'OpenIdConnect' but that doesn't match what identity actually stores in the
                // loginProvider field in the database which is something like: https://sts.windows.net/1234....
                // which is something based on your AD setup. This value needs to match in order for accounts to detected as linked/un-linked
                // in the back office.
                AuthenticationType = "https://sts.windows.net/3bb0b4c5-364f-4394-ad36-0f29f95e5ddd/",

                ClientId = clientId,
                Authority = authority,
                PostLogoutRedirectUri = postLoginRedirectUri,                
                Notifications = new OpenIdConnectAuthenticationNotifications()
                {
                    //
                    // If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.
                    //
                    AuthorizationCodeReceived = (context) =>
                    {
                        var code = context.Code;

                        var credential = new ClientCredential(clientId, appKey);
                        var userObjectId = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                        var authContext = new AuthenticationContext(authority, new NaiveSessionCache(userObjectId));
                        AuthenticationResult result = authContext.AcquireTokenByAuthorizationCode(
                            code,
                            //NOTE: This URL needs to match EXACTLY the same path that is configured in the AD 
                            // configuration. 
                            new Uri(
                                HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) +
                                HttpContext.Current.Request.RawUrl.EnsureStartsWith('/').EnsureEndsWith('/')),
                            credential,
                            graphResourceId);

                        return Task.FromResult(0);
                    }

                }

            };
            adOptions.Description.Properties["SocialStyle"] = "btn-microsoft";
            adOptions.Description.Properties["SocialIcon"] = "fa-windows";
            adOptions.Caption = "Active Directory";
            app.UseOpenIdConnectAuthentication(adOptions);

        }


    }

    //NOTE: Not sure exactly what this is for but it is found in the AD source demo:
    // https://github.com/AzureADSamples/WebApp-WebAPI-OpenIDConnect-DotNet/blob/master/TodoListWebApp/Utils/NaiveSessionCache.cs
    public class NaiveSessionCache : TokenCache
    {
        private static readonly object FileLock = new object();
        readonly string _userObjectId = string.Empty;
        readonly string _cacheId = string.Empty;
        public NaiveSessionCache(string userId)
        {
            _userObjectId = userId;
            _cacheId = _userObjectId + "_TokenCache";

            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            Load();
        }

        public void Load()
        {
            lock (FileLock)
            {
                this.Deserialize((byte[])HttpContext.Current.Session[_cacheId]);
            }
        }

        public void Persist()
        {
            lock (FileLock)
            {
                // reflect changes in the persistent store
                HttpContext.Current.Session[_cacheId] = this.Serialize();
                // once the write operation took place, restore the HasStateChanged bit to false
                this.HasStateChanged = false;
            }
        }

        // Empties the persistent store.
        public override void Clear()
        {
            base.Clear();
            System.Web.HttpContext.Current.Session.Remove(_cacheId);
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
            if (this.HasStateChanged)
            {
                Persist();
            }
        }
    }

}