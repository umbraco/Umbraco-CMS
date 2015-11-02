using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Security.Identity
{
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Sets the OWIN logger to use Umbraco's logging system
        /// </summary>
        /// <param name="app"></param>
        public static void SetUmbracoLoggerFactory(this IAppBuilder app)
        {
            app.SetLoggerFactory(new OwinLoggerFactory());
        }

        #region Backoffice

        /// <summary>
        /// Configure Default Identity User Manager for Umbraco
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appContext"></param>
        /// <param name="userMembershipProvider"></param>
        public static void ConfigureUserManagerForUmbracoBackOffice(this IAppBuilder app,
            ApplicationContext appContext,
            MembershipProviderBase userMembershipProvider)
        {
            if (appContext == null) throw new ArgumentNullException("appContext");
            if (userMembershipProvider == null) throw new ArgumentNullException("userMembershipProvider");

            //Don't proceed if the app is not ready
            if (appContext.IsUpgrading == false && appContext.IsConfigured == false) return;

            //Configure Umbraco user manager to be created per request
            app.CreatePerOwinContext<BackOfficeUserManager>(
                (options, owinContext) => BackOfficeUserManager.Create(
                    options,
                    appContext.Services.UserService,
                    appContext.Services.ExternalLoginService,
                    userMembershipProvider));

            //Create a sign in manager per request
            app.CreatePerOwinContext<BackOfficeSignInManager>((options, context) => BackOfficeSignInManager.Create(options, context, app.CreateLogger<BackOfficeSignInManager>()));
        }

        /// <summary>
        /// Configure a custom UserStore with the Identity User Manager for Umbraco
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appContext"></param>
        /// <param name="userMembershipProvider"></param>
        /// <param name="customUserStore"></param>
        public static void ConfigureUserManagerForUmbracoBackOffice(this IAppBuilder app,
            ApplicationContext appContext,
            MembershipProviderBase userMembershipProvider,
            BackOfficeUserStore customUserStore)
        {
            if (appContext == null) throw new ArgumentNullException("appContext");
            if (userMembershipProvider == null) throw new ArgumentNullException("userMembershipProvider");
            if (customUserStore == null) throw new ArgumentNullException("customUserStore");

            //Don't proceed if the app is not ready
            if (appContext.IsUpgrading == false && appContext.IsConfigured == false) return;

            //Configure Umbraco user manager to be created per request
            app.CreatePerOwinContext<BackOfficeUserManager>(
                (options, owinContext) => BackOfficeUserManager.Create(
                    options,
                    customUserStore,
                    userMembershipProvider));

            //Create a sign in manager per request
            app.CreatePerOwinContext<BackOfficeSignInManager>((options, context) => BackOfficeSignInManager.Create(options, context, app.CreateLogger(typeof(BackOfficeSignInManager).FullName)));
        }

        /// <summary>
        /// Configure a custom BackOfficeUserManager for Umbraco
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appContext"></param>
        /// <param name="userManager"></param>
        public static void ConfigureUserManagerForUmbracoBackOffice<TManager, TUser>(this IAppBuilder app,
            ApplicationContext appContext,
            Func<IdentityFactoryOptions<TManager>, IOwinContext, TManager> userManager)
            where TManager : BackOfficeUserManager<TUser>
            where TUser : BackOfficeIdentityUser
        {
            if (appContext == null) throw new ArgumentNullException("appContext");
            if (userManager == null) throw new ArgumentNullException("userManager");

            //Don't proceed if the app is not ready
            if (appContext.IsUpgrading == false && appContext.IsConfigured == false) return;

            //Configure Umbraco user manager to be created per request
            app.CreatePerOwinContext<TManager>(userManager);

            //Create a sign in manager per request
            app.CreatePerOwinContext<BackOfficeSignInManager>((options, context) => BackOfficeSignInManager.Create(options, context, app.CreateLogger(typeof(BackOfficeSignInManager).FullName)));
        }

        /// <summary>
        /// Ensures that the UmbracoBackOfficeAuthenticationMiddleware is assigned to the pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appContext"></param>
        /// <returns></returns>
        public static IAppBuilder UseUmbracoBackOfficeCookieAuthentication(this IAppBuilder app, ApplicationContext appContext)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (appContext == null) throw new ArgumentNullException("appContext");

            //Don't proceed if the app is not ready
            if (appContext.IsUpgrading == false && appContext.IsConfigured == false) return app;

            var authOptions = new UmbracoBackOfficeCookieAuthOptions(
                UmbracoConfig.For.UmbracoSettings().Security,
                GlobalSettings.TimeOutInMinutes,
                GlobalSettings.UseSSL)
            {
                Provider = new BackOfficeCookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user 
                    // logs in. This is a security feature which is used when you 
                    // change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator
                        .OnValidateIdentity<BackOfficeUserManager, BackOfficeIdentityUser, int>(
                            TimeSpan.FromMinutes(30),
                            (manager, user) => user.GenerateUserIdentityAsync(manager),
                            identity => identity.GetUserId<int>()),                    
                }
            };

            //This is a custom middleware, we need to return the user's remaining logged in seconds
            app.Use<GetUserSecondsMiddleWare>(
                authOptions,
                UmbracoConfig.For.UmbracoSettings().Security,
                app.CreateLogger<GetUserSecondsMiddleWare>());

            app.UseCookieAuthentication(authOptions);

            return app;
        }

        /// <summary>
        /// Ensures that the cookie middleware for validating external logins is assigned to the pipeline with the correct
        /// Umbraco back office configuration
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appContext"></param>
        /// <returns></returns>
        public static IAppBuilder UseUmbracoBackOfficeExternalCookieAuthentication(this IAppBuilder app, ApplicationContext appContext)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (appContext == null) throw new ArgumentNullException("appContext");

            //Don't proceed if the app is not ready
            if (appContext.IsUpgrading == false && appContext.IsConfigured == false) return app;

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.Security.BackOfficeExternalAuthenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                CookieName = Constants.Security.BackOfficeExternalCookieName,
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
        #endregion

        public static void SanitizeThreadCulture(this IAppBuilder app)
        {
            // get the current culture
            var currentCulture = CultureInfo.CurrentCulture;

            // at the top of any culture should be the invariant culture - find it
            // doing an .Equals comparison ensure that we *will* find it and not loop
            // endlessly
            var invariantCulture = currentCulture;
            while (invariantCulture.Equals(CultureInfo.InvariantCulture) == false)
                invariantCulture = invariantCulture.Parent;

            // now that invariant culture should be the same object as CultureInfo.InvariantCulture
            // yet for some reasons, sometimes it is not - and this breaks anything that loops on
            // culture.Parent until a reference equality to CultureInfo.InvariantCulture. See, for
            // example, the following code in PerformanceCounterLib.IsCustomCategory:
            //
            // CultureInfo culture = CultureInfo.CurrentCulture;
            // while (culture != CultureInfo.InvariantCulture)
            // {
            //     library = GetPerformanceCounterLib(machine, culture);
            //     if (library.IsCustomCategory(category))
            //         return true;
            //     culture = culture.Parent;
            // }
            //
            // The reference comparisons never succeeds, hence the loop never ends, and the
            // application hangs.
            //
            // granted, that comparison should probably be a .Equals comparison, but who knows
            // how many times the framework assumes that it can do a reference comparison? So,
            // better fix the cultures.

            if (ReferenceEquals(invariantCulture, CultureInfo.InvariantCulture))
                return;

            // if we do not have equality, fix cultures by replacing them with a culture with
            // the same name, but obtained here and now, with a proper invariant top culture

            var thread = Thread.CurrentThread;
            thread.CurrentCulture = CultureInfo.GetCultureInfo(thread.CurrentCulture.Name);
            thread.CurrentUICulture = CultureInfo.GetCultureInfo(thread.CurrentUICulture.Name);
        }
    }
}