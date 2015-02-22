using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;

namespace Umbraco.Web.Security.Identity
{
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Configure Identity User Manager for Umbraco
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appContext"></param>
        /// <param name="userMembershipProvider"></param>
        public static void ConfigureUserManagerForUmbracoBackOffice(this IAppBuilder app, ApplicationContext appContext, MembershipProviderBase userMembershipProvider)
        {
            //Don't proceed if the app is not ready
            if (appContext.IsConfigured == false
                || appContext.DatabaseContext == null
                || appContext.DatabaseContext.IsDatabaseConfigured == false) return;

            //Configure Umbraco user manager to be created per request
            app.CreatePerOwinContext<BackOfficeUserManager>(
                (options, owinContext) => BackOfficeUserManager.Create(
                    options, 
                    owinContext, 
                    appContext.Services.UserService,
                    appContext.Services.ExternalLoginService,
                    userMembershipProvider));

            //Configure Umbraco role manager to be created per request
            app.CreatePerOwinContext<BackOfficeRoleManager>((options, owinContext) => BackOfficeRoleManager.Create(options));
        }

        /// <summary>
        /// Ensures that the UmbracoBackOfficeAuthenticationMiddleware is assigned to the pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IAppBuilder UseUmbracoBackOfficeCookieAuthentication(this IAppBuilder app)
        {
            if (app == null) throw new ArgumentNullException("app");


            app.UseCookieAuthentication(new UmbracoBackOfficeCookieAuthenticationOptions(
                    UmbracoConfig.For.UmbracoSettings().Security,
                    GlobalSettings.TimeOutInMinutes,
                    GlobalSettings.UseSSL)
            {
                Provider = new CookieAuthenticationProvider
                {                
                    //// Enables the application to validate the security stamp when the user 
                    //// logs in. This is a security feature which is used when you 
                    //// change a password or add an external login to your account.  
                    //OnValidateIdentity = SecurityStampValidator
                    //    .OnValidateIdentity<UmbracoMembersUserManager<UmbracoApplicationUser>, UmbracoApplicationUser, int>(
                    //        TimeSpan.FromMinutes(30),
                    //        (manager, user) => user.GenerateUserIdentityAsync(manager),
                    //        identity => identity.GetUserId<int>())
                }
            });

            return app;
        }

        /// <summary>
        /// Ensures that the cookie middleware for validating external logins is assigned to the pipeline with the correct
        /// Umbraco back office configuration
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IAppBuilder UseUmbracoBackOfficeExternalCookieAuthentication(this IAppBuilder app)
        {
            if (app == null) throw new ArgumentNullException("app");

            app.SetDefaultSignInAsAuthenticationType("UmbracoExternalCookie");
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.Security.BackOfficeExternalAuthenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                CookieName = Constants.Security.BackOfficeExternalAuthenticationType,
                ExpireTimeSpan = TimeSpan.FromMinutes(5),
                //Custom cookie manager so we can filter requests
                CookieManager = new BackOfficeCookieManager(new SingletonUmbracoContextAccessor()),
                CookiePath = "/",
                CookieSecure = GlobalSettings.UseSSL ? CookieSecureOption.Always : CookieSecureOption.SameAsRequest,
                CookieHttpOnly = true,
                CookieDomain = UmbracoConfig.For.UmbracoSettings().Security.AuthCookieDomain
            });

            return app;
        }

    }
}