using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// Provides security/identity extension methods to IAppBuilder.
    /// </summary>
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Configure Default Identity User Manager for Umbraco
        /// </summary>
        /// <param name="app"></param>
        /// <param name="services"></param>
        /// <param name="contentSettings"></param>
        /// <param name="globalSettings"></param>
        /// <param name="userMembershipProvider"></param>
        public static void ConfigureUserManagerForUmbracoBackOffice(this IAppBuilder app,
            ServiceContext services,
            UmbracoMapper mapper,
            IContentSection contentSettings,
            IGlobalSettings globalSettings,
            MembershipProviderBase userMembershipProvider)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (userMembershipProvider == null) throw new ArgumentNullException(nameof(userMembershipProvider));

            //Configure Umbraco user manager to be created per request
            app.CreatePerOwinContext<BackOfficeUserManager>(
                (options, owinContext) => BackOfficeUserManager.Create(
                    options,
                    services.UserService,
                    services.MemberTypeService,
                    services.EntityService,
                    services.ExternalLoginService,
                    userMembershipProvider,
                    mapper,
                    contentSettings,
                    globalSettings));

            app.SetBackOfficeUserManagerType<BackOfficeUserManager, BackOfficeIdentityUser>();

            //Create a sign in manager per request
            app.CreatePerOwinContext<BackOfficeSignInManager>((options, context) => BackOfficeSignInManager.Create(options, context, globalSettings, app.CreateLogger<BackOfficeSignInManager>()));
        }

        /// <summary>
        /// Configure a custom UserStore with the Identity User Manager for Umbraco
        /// </summary>
        /// <param name="app"></param>
        /// <param name="runtimeState"></param>
        /// <param name="globalSettings"></param>
        /// <param name="userMembershipProvider"></param>
        /// <param name="customUserStore"></param>
        /// <param name="contentSettings"></param>
        public static void ConfigureUserManagerForUmbracoBackOffice(this IAppBuilder app,
            IRuntimeState runtimeState,
            IContentSection contentSettings,
            IGlobalSettings globalSettings,
            MembershipProviderBase userMembershipProvider,
            BackOfficeUserStore customUserStore)
        {
            if (runtimeState == null) throw new ArgumentNullException(nameof(runtimeState));
            if (userMembershipProvider == null) throw new ArgumentNullException(nameof(userMembershipProvider));
            if (customUserStore == null) throw new ArgumentNullException(nameof(customUserStore));

            //Configure Umbraco user manager to be created per request
            app.CreatePerOwinContext<BackOfficeUserManager>(
                (options, owinContext) => BackOfficeUserManager.Create(
                    options,
                    customUserStore,
                    userMembershipProvider,
                    contentSettings));

            app.SetBackOfficeUserManagerType<BackOfficeUserManager, BackOfficeIdentityUser>();

            //Create a sign in manager per request
            app.CreatePerOwinContext<BackOfficeSignInManager>((options, context) => BackOfficeSignInManager.Create(options, context, globalSettings, app.CreateLogger(typeof(BackOfficeSignInManager).FullName)));
        }

        /// <summary>
        /// Configure a custom BackOfficeUserManager for Umbraco
        /// </summary>
        /// <param name="app"></param>
        /// <param name="runtimeState"></param>
        /// <param name="globalSettings"></param>
        /// <param name="userManager"></param>
        public static void ConfigureUserManagerForUmbracoBackOffice<TManager, TUser>(this IAppBuilder app,
            IRuntimeState runtimeState,
            IGlobalSettings globalSettings,
            Func<IdentityFactoryOptions<TManager>, IOwinContext, TManager> userManager)
            where TManager : BackOfficeUserManager<TUser>
            where TUser : BackOfficeIdentityUser
        {
            if (runtimeState == null) throw new ArgumentNullException(nameof(runtimeState));
            if (userManager == null) throw new ArgumentNullException(nameof(userManager));

            //Configure Umbraco user manager to be created per request
            app.CreatePerOwinContext<TManager>(userManager);

            app.SetBackOfficeUserManagerType<TManager, TUser>();

            //Create a sign in manager per request
            app.CreatePerOwinContext<BackOfficeSignInManager>(
                (options, context) => BackOfficeSignInManager.Create(options, context, globalSettings, app.CreateLogger(typeof(BackOfficeSignInManager).FullName)));
        }

        /// <summary>
        /// Ensures that the UmbracoBackOfficeAuthenticationMiddleware is assigned to the pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <param name="umbracoContextAccessor"></param>
        /// <param name="runtimeState"></param>
        /// <param name="userService"></param>
        /// <param name="globalSettings"></param>
        /// <param name="securitySection"></param>
        /// <returns></returns>
        /// <remarks>
        /// By default this will be configured to execute on PipelineStage.Authenticate
        /// </remarks>
        public static IAppBuilder UseUmbracoBackOfficeCookieAuthentication(this IAppBuilder app, IUmbracoContextAccessor umbracoContextAccessor, IRuntimeState runtimeState,IUserService userService, IGlobalSettings globalSettings, ISecuritySection securitySection)
        {
            return app.UseUmbracoBackOfficeCookieAuthentication(umbracoContextAccessor, runtimeState, userService, globalSettings, securitySection, PipelineStage.Authenticate);
        }

        /// <summary>
        /// Ensures that the UmbracoBackOfficeAuthenticationMiddleware is assigned to the pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <param name="umbracoContextAccessor"></param>
        /// <param name="runtimeState"></param>
        /// <param name="userService"></param>
        /// <param name="globalSettings"></param>
        /// <param name="securitySection"></param>
        /// <param name="stage">
        /// Configurable pipeline stage
        /// </param>
        /// <returns></returns>
        public static IAppBuilder UseUmbracoBackOfficeCookieAuthentication(this IAppBuilder app, IUmbracoContextAccessor umbracoContextAccessor, IRuntimeState runtimeState, IUserService userService, IGlobalSettings globalSettings, ISecuritySection securitySection, PipelineStage stage)
        {
            //Create the default options and provider
            var authOptions = app.CreateUmbracoCookieAuthOptions(umbracoContextAccessor, globalSettings, runtimeState, securitySection);

            authOptions.Provider = new BackOfficeCookieAuthenticationProvider(userService, runtimeState, globalSettings)
            {
                // Enables the application to validate the security stamp when the user
                // logs in. This is a security feature which is used when you
                // change a password or add an external login to your account.
                OnValidateIdentity = context =>
                {
                    // capture the current ticket for the request
                    var identity = context.Identity;

                    return SecurityStampValidator
                        .OnValidateIdentity<BackOfficeUserManager, BackOfficeIdentityUser, int>(
                        // This will re-verify the security stamp at a throttled 30 mins
                        // (the standard/default set in aspnet identity).
                        // This ensures that if the security stamp has changed - i.e. passwords,
                        // external logins, or other security profile data changed behind the
                        // scenes while being logged in, that they are logged out and have
                        // to re-verify their identity.
                        TimeSpan.FromMinutes(30),
                        async (manager, user) =>
                        {
                            var regenerated = await manager.GenerateUserIdentityAsync(user);

                            // Keep any custom claims from the original identity
                            regenerated.MergeClaimsFromBackOfficeIdentity(identity);

                            return regenerated;
                        },
                        identity => identity.GetUserId<int>())(context);
                }

            };

            return app.UseUmbracoBackOfficeCookieAuthentication(umbracoContextAccessor, runtimeState, globalSettings, securitySection, authOptions, stage);
        }

        /// <summary>
        /// Ensures that the UmbracoBackOfficeAuthenticationMiddleware is assigned to the pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <param name="umbracoContextAccessor"></param>
        /// <param name="runtimeState"></param>
        /// <param name="globalSettings"></param>
        /// <param name="securitySection"></param>
        /// <param name="cookieOptions">Custom auth cookie options can be specified to have more control over the cookie authentication logic</param>
        /// <param name="stage">
        /// Configurable pipeline stage
        /// </param>
        /// <returns></returns>
        public static IAppBuilder UseUmbracoBackOfficeCookieAuthentication(this IAppBuilder app, IUmbracoContextAccessor umbracoContextAccessor, IRuntimeState runtimeState, IGlobalSettings globalSettings, ISecuritySection securitySection, CookieAuthenticationOptions cookieOptions, PipelineStage stage)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (runtimeState == null) throw new ArgumentNullException(nameof(runtimeState));
            if (cookieOptions == null) throw new ArgumentNullException(nameof(cookieOptions));
            if (cookieOptions.Provider == null)
                throw new ArgumentNullException("cookieOptions.Provider cannot be null.", nameof(cookieOptions));
            if (cookieOptions.Provider is BackOfficeCookieAuthenticationProvider == false)
                throw new ArgumentException($"cookieOptions.Provider must be of type {typeof(BackOfficeCookieAuthenticationProvider)}.", nameof(cookieOptions));

            app.UseUmbracoBackOfficeCookieAuthenticationInternal(cookieOptions, runtimeState, stage);

            //don't apply if app is not ready
            if (runtimeState.Level != RuntimeLevel.Upgrade && runtimeState.Level != RuntimeLevel.Run) return app;

            var cookieAuthOptions = app.CreateUmbracoCookieAuthOptions(
                umbracoContextAccessor, globalSettings, runtimeState, securitySection,
                //This defines the explicit path read cookies from for this middleware
                new[] {$"{globalSettings.Path}/backoffice/UmbracoApi/Authentication/GetRemainingTimeoutSeconds"});
            cookieAuthOptions.Provider = cookieOptions.Provider;

            //This is a custom middleware, we need to return the user's remaining logged in seconds
            app.Use<GetUserSecondsMiddleWare>(
                cookieAuthOptions,
                Current.Configs.Global(),
                Current.Configs.Settings().Security,
                app.CreateLogger<GetUserSecondsMiddleWare>());

            //This is required so that we can read the auth ticket format outside of this pipeline
            app.CreatePerOwinContext<UmbracoAuthTicketDataProtector>(
                (options, context) => new UmbracoAuthTicketDataProtector(cookieOptions.TicketDataFormat));

            return app;
        }

        private static bool _markerSet = false;

        /// <summary>
        /// This registers the exact type of the user manager in owin so we can extract it
        /// when required in order to extract the user manager instance
        /// </summary>
        /// <typeparam name="TManager"></typeparam>
        /// <typeparam name="TUser"></typeparam>
        /// <param name="app"></param>
        /// <remarks>
        /// This is required because a developer can specify a custom user manager and due to generic types the key name will registered
        /// differently in the owin context
        /// </remarks>
        private static void SetBackOfficeUserManagerType<TManager, TUser>(this IAppBuilder app)
            where TManager : BackOfficeUserManager<TUser>
            where TUser : BackOfficeIdentityUser
        {
            if (_markerSet) throw new InvalidOperationException("The back office user manager marker has already been set, only one back office user manager can be configured");

            //on each request set the user manager getter -
            // this is required purely because Microsoft.Owin.IOwinContext is super inflexible with it's Get since it can only be
            // a generic strongly typed instance
            app.Use((context, func) =>
            {
                context.Set(BackOfficeUserManager.OwinMarkerKey, new BackOfficeUserManagerMarker<TManager, TUser>());
                return func();
            });
        }

        private static void UseUmbracoBackOfficeCookieAuthenticationInternal(this IAppBuilder app, CookieAuthenticationOptions options, IRuntimeState runtimeState, PipelineStage stage)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (runtimeState == null) throw new ArgumentNullException(nameof(runtimeState));

            //First the normal cookie middleware
            app.Use(typeof(CookieAuthenticationMiddleware), app, options);
            //don't apply if app is not ready
            if (runtimeState.Level == RuntimeLevel.Upgrade || runtimeState.Level == RuntimeLevel.Run)
            {
                //Then our custom middlewares
                app.Use(typeof(ForceRenewalCookieAuthenticationMiddleware), app, options, Current.UmbracoContextAccessor);
                app.Use(typeof(FixWindowsAuthMiddlware));                
            }

            //Marks all of the above middlewares to execute on Authenticate
            app.UseStageMarker(stage);
        }


        /// <summary>
        /// Ensures that the cookie middleware for validating external logins is assigned to the pipeline with the correct
        /// Umbraco back office configuration
        /// </summary>
        /// <param name="app"></param>
        /// <param name="umbracoContextAccessor"></param>
        /// <param name="runtimeState"></param>
        /// <param name="globalSettings"></param>
        /// <returns></returns>
        /// <remarks>
        /// By default this will be configured to execute on PipelineStage.Authenticate
        /// </remarks>
        public static IAppBuilder UseUmbracoBackOfficeExternalCookieAuthentication(this IAppBuilder app, IUmbracoContextAccessor umbracoContextAccessor, IRuntimeState runtimeState,IGlobalSettings globalSettings)
        {
            return app.UseUmbracoBackOfficeExternalCookieAuthentication(umbracoContextAccessor, runtimeState, globalSettings, PipelineStage.Authenticate);
        }

        /// <summary>
        /// Ensures that the cookie middleware for validating external logins is assigned to the pipeline with the correct
        /// Umbraco back office configuration
        /// </summary>
        /// <param name="app"></param>
        /// <param name="umbracoContextAccessor"></param>
        /// <param name="runtimeState"></param>
        /// <param name="globalSettings"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        public static IAppBuilder UseUmbracoBackOfficeExternalCookieAuthentication(this IAppBuilder app,
            IUmbracoContextAccessor umbracoContextAccessor, IRuntimeState runtimeState,
            IGlobalSettings globalSettings, PipelineStage stage)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (runtimeState == null) throw new ArgumentNullException(nameof(runtimeState));

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.Security.BackOfficeExternalAuthenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                CookieName = Constants.Security.BackOfficeExternalCookieName,
                ExpireTimeSpan = TimeSpan.FromMinutes(5),
                //Custom cookie manager so we can filter requests
                CookieManager = new BackOfficeCookieManager(umbracoContextAccessor, runtimeState, globalSettings),
                CookiePath = "/",
                CookieSecure = globalSettings.UseHttps ? CookieSecureOption.Always : CookieSecureOption.SameAsRequest,
                CookieHttpOnly = true,
                CookieDomain = Current.Configs.Settings().Security.AuthCookieDomain
            }, stage);

            return app;
        }

        /// <summary>
        /// In order for preview to work this needs to be called
        /// </summary>
        /// <param name="app"></param>
        /// <param name="umbracoContextAccessor"></param>
        /// <param name="runtimeState"></param>
        /// <param name="globalSettings"></param>
        /// <param name="securitySettings"></param>
        /// <returns></returns>
        /// <remarks>
        /// This ensures that during a preview request that the back office use is also Authenticated and that the back office Identity
        /// is added as a secondary identity to the current IPrincipal so it can be used to Authorize the previewed document.
        /// </remarks>
        /// <remarks>
        /// By default this will be configured to execute on PipelineStage.PostAuthenticate
        /// </remarks>
        public static IAppBuilder UseUmbracoPreviewAuthentication(this IAppBuilder app, IUmbracoContextAccessor umbracoContextAccessor, IRuntimeState runtimeState, IGlobalSettings globalSettings, ISecuritySection securitySettings)
        {
            return app.UseUmbracoPreviewAuthentication(umbracoContextAccessor, runtimeState, globalSettings, securitySettings, PipelineStage.PostAuthenticate);
        }

        /// <summary>
        /// In order for preview to work this needs to be called
        /// </summary>
        /// <param name="app"></param>
        /// <param name="umbracoContextAccessor"></param>
        /// <param name="runtimeState"></param>
        /// <param name="globalSettings"></param>
        /// <param name="securitySettings"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        /// <remarks>
        /// This ensures that during a preview request that the back office use is also Authenticated and that the back office Identity
        /// is added as a secondary identity to the current IPrincipal so it can be used to Authorize the previewed document.
        /// </remarks>
        public static IAppBuilder UseUmbracoPreviewAuthentication(this IAppBuilder app, IUmbracoContextAccessor umbracoContextAccessor, IRuntimeState runtimeState, IGlobalSettings globalSettings, ISecuritySection securitySettings, PipelineStage stage)
        {
            if (runtimeState.Level != RuntimeLevel.Run) return app;

            var authOptions = app.CreateUmbracoCookieAuthOptions(umbracoContextAccessor, globalSettings, runtimeState, securitySettings);
            app.Use(typeof(PreviewAuthenticationMiddleware),  authOptions, Current.Configs.Global());

            // This middleware must execute at least on PostAuthentication, by default it is on Authorize
            // The middleware needs to execute after the RoleManagerModule executes which is during PostAuthenticate,
            // currently I've had 100% success with ensuring this fires after RoleManagerModule even if this is set
            // to PostAuthenticate though not sure if that's always a guarantee so by default it's Authorize.
            if (stage < PipelineStage.PostAuthenticate)
                throw new InvalidOperationException("The stage specified for UseUmbracoPreviewAuthentication must be greater than or equal to " + PipelineStage.PostAuthenticate);

            app.UseStageMarker(stage);
            return app;
        }

        /// <summary>
        /// Enable the back office to detect and handle errors registered with external login providers
        /// </summary>
        /// <param name="app"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        public static IAppBuilder UseUmbracoBackOfficeExternalLoginErrors(this IAppBuilder app, PipelineStage stage = PipelineStage.Authorize)
        {            
            app.Use(typeof(BackOfficeExternalLoginProviderErrorMiddlware));
            app.UseStageMarker(stage);
            return app;
        }

        public static void SanitizeThreadCulture(this IAppBuilder app)
        {
            Thread.CurrentThread.SanitizeThreadCulture();
        }

        /// <summary>
        /// Create the default umb cookie auth options
        /// </summary>
        /// <param name="app"></param>
        /// <param name="umbracoContextAccessor"></param>
        /// <param name="globalSettings"></param>
        /// <param name="runtimeState"></param>
        /// <param name="securitySettings"></param>
        /// <param name="explicitPaths"></param>
        /// <returns></returns>
        public static UmbracoBackOfficeCookieAuthOptions CreateUmbracoCookieAuthOptions(this IAppBuilder app,
            IUmbracoContextAccessor umbracoContextAccessor,
            IGlobalSettings globalSettings, IRuntimeState runtimeState, ISecuritySection securitySettings, string[] explicitPaths = null)
        {
            //this is how aspnet wires up the default AuthenticationTicket protector so we'll use the same code
            var ticketDataFormat = new TicketDataFormat(
                app.CreateDataProtector(typeof (CookieAuthenticationMiddleware).FullName,
                    Constants.Security.BackOfficeAuthenticationType,
                    "v1"));

            var authOptions = new UmbracoBackOfficeCookieAuthOptions(
                explicitPaths,
                umbracoContextAccessor,
                securitySettings,
                globalSettings,
                runtimeState,
                ticketDataFormat);

            return authOptions;
        }
    }
}
