using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin;
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


            
            var authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
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
                                //new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)),
                                new Uri(
                                    HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + 
                                    HttpContext.Current.Request.RawUrl.EnsureStartsWith('/').EnsureEndsWith('/')),
                                credential,
                                graphResourceId);

                            return Task.FromResult(0);
                        }

                    }

                });

        }


    }

    public class NaiveSessionCache : TokenCache
    {
        private static readonly object FileLock = new object();
        string UserObjectId = string.Empty;
        string CacheId = string.Empty;
        public NaiveSessionCache(string userId)
        {
            UserObjectId = userId;
            CacheId = UserObjectId + "_TokenCache";

            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            Load();
        }

        public void Load()
        {
            lock (FileLock)
            {
                this.Deserialize((byte[])HttpContext.Current.Session[CacheId]);
            }
        }

        public void Persist()
        {
            lock (FileLock)
            {
                // reflect changes in the persistent store
                HttpContext.Current.Session[CacheId] = this.Serialize();
                // once the write operation took place, restore the HasStateChanged bit to false
                this.HasStateChanged = false;
            }
        }

        // Empties the persistent store.
        public override void Clear()
        {
            base.Clear();
            System.Web.HttpContext.Current.Session.Remove(CacheId);
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