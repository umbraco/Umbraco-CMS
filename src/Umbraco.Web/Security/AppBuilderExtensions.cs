using System;
using System.Threading;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Web.Composing;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// Provides security/identity extension methods to IAppBuilder.
    /// </summary>
    public static class AppBuilderExtensions
    {

        // TODO: Migrate this!

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
        public static IAppBuilder UseUmbracoBackOfficeExternalCookieAuthentication(this IAppBuilder app, IUmbracoContextAccessor umbracoContextAccessor, IRuntimeState runtimeState,GlobalSettings globalSettings, IHostingEnvironment hostingEnvironment, IRequestCache requestCache)
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
            GlobalSettings globalSettings, IHostingEnvironment hostingEnvironment, IRequestCache requestCache, PipelineStage stage)
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
                CookieDomain = new SecuritySettings().AuthCookieDomain // TODO inject settings
            }, stage);

            app.UseStageMarker(stage);
            return app;
        }

        public static void SanitizeThreadCulture(this IAppBuilder app)
        {
            Thread.CurrentThread.SanitizeThreadCulture();
        }

    }
}
