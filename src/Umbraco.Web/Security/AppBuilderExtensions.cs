using System;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.Mapping;
using Umbraco.Net;
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
        /// Ensures that the cookie middleware for validating external logins is assigned to the pipeline with the correct
        /// Umbraco back office configuration
        /// </summary>
        /// <param name="app"></param>
        /// <param name="umbracoContextAccessor"></param>
        /// <param name="runtimeState"></param>
        /// <param name="globalSettings"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="requestCache"></param>
        /// <returns></returns>
        /// <remarks>
        /// By default this will be configured to execute on PipelineStage.Authenticate
        /// </remarks>
        public static IAppBuilder UseUmbracoBackOfficeExternalCookieAuthentication(this IAppBuilder app, IUmbracoContextAccessor umbracoContextAccessor, IRuntimeState runtimeState,IGlobalSettings globalSettings, IHostingEnvironment hostingEnvironment, IRequestCache requestCache)
        {
            return app.UseUmbracoBackOfficeExternalCookieAuthentication(umbracoContextAccessor, runtimeState, globalSettings, hostingEnvironment, requestCache, PipelineStage.Authenticate);
        }

        /// <summary>
        /// Ensures that the cookie middleware for validating external logins is assigned to the pipeline with the correct
        /// Umbraco back office configuration
        /// </summary>
        /// <param name="app"></param>
        /// <param name="umbracoContextAccessor"></param>
        /// <param name="runtimeState"></param>
        /// <param name="globalSettings"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="requestCache"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        public static IAppBuilder UseUmbracoBackOfficeExternalCookieAuthentication(this IAppBuilder app,
            IUmbracoContextAccessor umbracoContextAccessor, IRuntimeState runtimeState,
            IGlobalSettings globalSettings, IHostingEnvironment hostingEnvironment, IRequestCache requestCache, PipelineStage stage)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (runtimeState == null) throw new ArgumentNullException(nameof(runtimeState));
            if (hostingEnvironment == null) throw new ArgumentNullException(nameof(hostingEnvironment));

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = Constants.Security.BackOfficeExternalAuthenticationType,
                AuthenticationMode = AuthenticationMode.Passive,
                CookieName = Constants.Security.BackOfficeExternalCookieName,
                ExpireTimeSpan = TimeSpan.FromMinutes(5),
                CookiePath = "/",
                CookieSecure = globalSettings.UseHttps ? CookieSecureOption.Always : CookieSecureOption.SameAsRequest,
                CookieHttpOnly = true,
                CookieDomain = Current.Configs.Security().AuthCookieDomain
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
        /// <param name="ioHelper"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="requestCache"></param>
        /// <returns></returns>
        /// <remarks>
        /// This ensures that during a preview request that the back office use is also Authenticated and that the back office Identity
        /// is added as a secondary identity to the current IPrincipal so it can be used to Authorize the previewed document.
        /// </remarks>
        /// <remarks>
        /// By default this will be configured to execute on PipelineStage.PostAuthenticate
        /// </remarks>
        public static IAppBuilder UseUmbracoPreviewAuthentication(this IAppBuilder app, IUmbracoContextAccessor umbracoContextAccessor, IRuntimeState runtimeState, IGlobalSettings globalSettings, ISecuritySettings securitySettings, IHostingEnvironment hostingEnvironment, IRequestCache requestCache)
        {
            return app.UseUmbracoPreviewAuthentication(umbracoContextAccessor, runtimeState, globalSettings, securitySettings, hostingEnvironment, requestCache, PipelineStage.PostAuthenticate);
        }

        /// <summary>
        /// In order for preview to work this needs to be called
        /// </summary>
        /// <param name="app"></param>
        /// <param name="umbracoContextAccessor"></param>
        /// <param name="runtimeState"></param>
        /// <param name="globalSettings"></param>
        /// <param name="securitySettings"></param>
        /// <param name="ioHelper"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="requestCache"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        /// <remarks>
        /// This ensures that during a preview request that the back office use is also Authenticated and that the back office Identity
        /// is added as a secondary identity to the current IPrincipal so it can be used to Authorize the previewed document.
        /// </remarks>
        public static IAppBuilder UseUmbracoPreviewAuthentication(this IAppBuilder app, IUmbracoContextAccessor umbracoContextAccessor, IRuntimeState runtimeState, IGlobalSettings globalSettings, ISecuritySettings securitySettings, IHostingEnvironment hostingEnvironment, IRequestCache requestCache, PipelineStage stage)
        {
            if (runtimeState.Level != RuntimeLevel.Run) return app;

            //var authOptions = app.CreateUmbracoCookieAuthOptions(umbracoContextAccessor, globalSettings, runtimeState, securitySettings, hostingEnvironment, requestCache);
            app.Use(typeof(PreviewAuthenticationMiddleware), /*authOptions*/null, globalSettings, hostingEnvironment);

            // This middleware must execute at least on PostAuthentication, by default it is on Authorize
            // The middleware needs to execute after the RoleManagerModule executes which is during PostAuthenticate,
            // currently I've had 100% success with ensuring this fires after RoleManagerModule even if this is set
            // to PostAuthenticate though not sure if that's always a guarantee so by default it's Authorize.
            if (stage < PipelineStage.PostAuthenticate)
                throw new InvalidOperationException("The stage specified for UseUmbracoPreviewAuthentication must be greater than or equal to " + PipelineStage.PostAuthenticate);

            app.UseStageMarker(stage);
            return app;
        }

        public static void SanitizeThreadCulture(this IAppBuilder app)
        {
            Thread.CurrentThread.SanitizeThreadCulture();
        }

        public static IAppBuilder CreatePerOwinContext<T>(this IAppBuilder app, Func<T> createCallback)
            where T : class, IDisposable
        {
            return CreatePerOwinContext<T>(app, (options, context) => createCallback());
        }

        public static IAppBuilder CreatePerOwinContext<T>(this IAppBuilder app,
            Func<IdentityFactoryOptions<T>, IOwinContext, T> createCallback) where T : class, IDisposable
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            return app.CreatePerOwinContext(createCallback, (options, instance) => instance.Dispose());
        }

        public static IAppBuilder CreatePerOwinContext<T>(this IAppBuilder app,
            Func<IdentityFactoryOptions<T>, IOwinContext, T> createCallback,
            Action<IdentityFactoryOptions<T>, T> disposeCallback) where T : class, IDisposable
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (createCallback == null) throw new ArgumentNullException(nameof(createCallback));
            if (disposeCallback == null) throw new ArgumentNullException(nameof(disposeCallback));

            app.Use(typeof(IdentityFactoryMiddleware<T, IdentityFactoryOptions<T>>),
                new IdentityFactoryOptions<T>
                {
                    DataProtectionProvider = app.GetDataProtectionProvider(),
                    Provider = new IdentityFactoryProvider<T>
                    {
                        OnCreate = createCallback,
                        OnDispose = disposeCallback
                    }
                });
            return app;
        }
    }
}
