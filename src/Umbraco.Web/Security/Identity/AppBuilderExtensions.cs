using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        /// Called at the end of configuring middleware
        /// </summary>
        /// <param name="app"></param>
        /// <remarks>
        /// This could be used for something else in the future - maybe to inform Umbraco that middleware is done/ready, but for
        /// now this is used to raise the custom event
        /// 
        /// This is an extension method in case developer entirely replace the UmbracoDefaultOwinStartup class, in which case they will
        /// need to ensure they call this extension method in their startup class.
        /// 
        /// TODO: Move this method in v8, it doesn't belong in this namespace/extension class
        /// </remarks>
        public static void FinalizeMiddlewareConfiguration(this IAppBuilder app)
        {
            UmbracoDefaultOwinStartup.OnMiddlewareConfigured(new OwinMiddlewareConfiguredEventArgs(app));
        }

        /// <summary>
        /// Sets the OWIN logger to use Umbraco's logging system
        /// </summary>
        /// <param name="app"></param>
        /// <remarks>
        /// TODO: Move this method in v8, it doesn't belong in this namespace/extension class
        /// </remarks>
        public static void SetUmbracoLoggerFactory(this IAppBuilder app)
        {
            app.SetLoggerFactory(new OwinLoggerFactory());
        }
        
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
        /// <remarks>
        /// By default this will be configured to execute on PipelineStage.Authenticate
        /// </remarks>
        public static IAppBuilder UseUmbracoBackOfficeCookieAuthentication(this IAppBuilder app, ApplicationContext appContext)
        {
            return app.UseUmbracoBackOfficeCookieAuthentication(appContext, PipelineStage.Authenticate);
        }

        /// <summary>
        /// Ensures that the UmbracoBackOfficeAuthenticationMiddleware is assigned to the pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appContext"></param>
        /// <param name="stage">
        /// Configurable pipeline stage
        /// </param>
        /// <returns></returns>
        public static IAppBuilder UseUmbracoBackOfficeCookieAuthentication(this IAppBuilder app, ApplicationContext appContext, PipelineStage stage)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (appContext == null) throw new ArgumentNullException("appContext");

            var cookieAuthProvider = new BackOfficeCookieAuthenticationProvider
            {
                // Enables the application to validate the security stamp when the user 
                // logs in. This is a security feature which is used when you 
                // change a password or add an external login to your account.  
                OnValidateIdentity = SecurityStampValidator
                    .OnValidateIdentity<BackOfficeUserManager, BackOfficeIdentityUser, int>(
                        TimeSpan.FromMinutes(30),
                        (manager, user) => user.GenerateUserIdentityAsync(manager),
                        identity => identity.GetUserId<int>()),
            };

            var authOptions = CreateCookieAuthOptions();
            authOptions.Provider = cookieAuthProvider;

            app.UseUmbracoBackOfficeCookieAuthentication(authOptions, appContext, stage);

            //don't apply if app isnot ready
            if (appContext.IsUpgrading || appContext.IsConfigured)
            {
                var getSecondsOptions = CreateCookieAuthOptions(
                    //This defines the explicit path read cookies from for this middleware
                    new[] {string.Format("{0}/backoffice/UmbracoApi/Authentication/GetRemainingTimeoutSeconds", GlobalSettings.Path)});
                getSecondsOptions.Provider = cookieAuthProvider;

                //This is a custom middleware, we need to return the user's remaining logged in seconds
                app.Use<GetUserSecondsMiddleWare>(
                    getSecondsOptions,
                    UmbracoConfig.For.UmbracoSettings().Security,
                    app.CreateLogger<GetUserSecondsMiddleWare>());
            }

            return app;
        }

        internal static IAppBuilder UseUmbracoBackOfficeCookieAuthentication(this IAppBuilder app, CookieAuthenticationOptions options, ApplicationContext appContext, PipelineStage stage = PipelineStage.Authenticate)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            //First the normal cookie middleware
            app.Use(typeof(CookieAuthenticationMiddleware), app, options);
            //don't apply if app isnot ready
            if (appContext.IsUpgrading || appContext.IsConfigured)
            {
                //Then our custom middlewares
                app.Use(typeof(ForceRenewalCookieAuthenticationMiddleware), app, options, new SingletonUmbracoContextAccessor());
                app.Use(typeof(FixWindowsAuthMiddlware));                
            }

            //Marks all of the above middlewares to execute on Authenticate
            app.UseStageMarker(stage);

            return app;
        }


        /// <summary>
        /// Ensures that the cookie middleware for validating external logins is assigned to the pipeline with the correct
        /// Umbraco back office configuration
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appContext"></param>
        /// <returns></returns>
        /// <remarks>
        /// By default this will be configured to execute on PipelineStage.Authenticate
        /// </remarks>
        public static IAppBuilder UseUmbracoBackOfficeExternalCookieAuthentication(this IAppBuilder app, ApplicationContext appContext)
        {
            return app.UseUmbracoBackOfficeExternalCookieAuthentication(appContext, PipelineStage.Authenticate);
        }

        /// <summary>
        /// Ensures that the cookie middleware for validating external logins is assigned to the pipeline with the correct
        /// Umbraco back office configuration
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appContext"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        public static IAppBuilder UseUmbracoBackOfficeExternalCookieAuthentication(this IAppBuilder app, ApplicationContext appContext, PipelineStage stage)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (appContext == null) throw new ArgumentNullException("appContext");

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
            }, stage);

            return app;
        }

        /// <summary>
        /// In order for preview to work this needs to be called
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appContext"></param>
        /// <returns></returns>
        /// <remarks>
        /// This ensures that during a preview request that the back office use is also Authenticated and that the back office Identity
        /// is added as a secondary identity to the current IPrincipal so it can be used to Authorize the previewed document.
        /// </remarks>
        /// <remarks>
        /// By default this will be configured to execute on PipelineStage.PostAuthenticate
        /// </remarks>
        public static IAppBuilder UseUmbracoPreviewAuthentication(this IAppBuilder app, ApplicationContext appContext)
        {
            return app.UseUmbracoPreviewAuthentication(appContext, PipelineStage.PostAuthenticate);
        }

        /// <summary>
        /// In order for preview to work this needs to be called
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appContext"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        /// <remarks>
        /// This ensures that during a preview request that the back office use is also Authenticated and that the back office Identity
        /// is added as a secondary identity to the current IPrincipal so it can be used to Authorize the previewed document.
        /// </remarks>
        public static IAppBuilder UseUmbracoPreviewAuthentication(this IAppBuilder app, ApplicationContext appContext, PipelineStage stage)
        {
            //don't apply if app isnot ready
            if (appContext.IsConfigured)
            {
                var authOptions = CreateCookieAuthOptions();                
                app.Use(typeof(PreviewAuthenticationMiddleware),  authOptions);

                //This middleware must execute at least on PostAuthentication, by default it is on Authorize
                // The middleware needs to execute after the RoleManagerModule executes which is during PostAuthenticate, 
                // currently I've had 100% success with ensuring this fires after RoleManagerModule even if this is set
                // to PostAuthenticate though not sure if that's always a guarantee so by default it's Authorize.
                if (stage < PipelineStage.PostAuthenticate)
                {
                    throw new InvalidOperationException("The stage specified for UseUmbracoPreviewAuthentication must be greater than or equal to " + PipelineStage.PostAuthenticate);
                }

                
                app.UseStageMarker(stage);
            }

            return app;
        }

        public static void SanitizeThreadCulture(this IAppBuilder app)
        {
            Thread.CurrentThread.SanitizeThreadCulture();
        }

        /// <summary>
        /// Create the default umb cookie auth options
        /// </summary>
        /// <param name="explicitPaths"></param>
        /// <returns></returns>
        private static UmbracoBackOfficeCookieAuthOptions CreateCookieAuthOptions(string[] explicitPaths = null)
        {
            var authOptions = new UmbracoBackOfficeCookieAuthOptions(
                explicitPaths,
                UmbracoConfig.For.UmbracoSettings().Security,
                GlobalSettings.TimeOutInMinutes,
                GlobalSettings.UseSSL);
            return authOptions;
        }
    }
}