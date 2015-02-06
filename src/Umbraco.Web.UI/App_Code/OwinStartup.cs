using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;

using Owin;
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

        public void Configuration(IAppBuilder app)
        {
            ////Single method to configure the Identity user manager for use with Umbraco
            //app.ConfigureUserManagerForUmbraco<UmbracoApplicationUser>();

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

        }


    }
}