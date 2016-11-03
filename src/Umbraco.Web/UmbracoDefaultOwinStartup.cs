using System;
using Microsoft.Owin;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Security.Identity;

[assembly: OwinStartup("UmbracoDefaultOwinStartup", typeof(UmbracoDefaultOwinStartup))]

namespace Umbraco.Web
{
    /// <summary>
    /// The default way to configure OWIN for Umbraco
    /// </summary>
    /// <remarks>
    /// The startup type is specified in appSettings under owin:appStartup
    /// </remarks>
    public class UmbracoDefaultOwinStartup
    {
        /// <summary>
        /// Main startup method
        /// </summary>
        /// <param name="app"></param>
        public virtual void Configuration(IAppBuilder app)
        {
            app.SanitizeThreadCulture();

            // there's nothing we can do really
            if (Core.DI.Current.RuntimeState.Level == RuntimeLevel.BootFailed)
                return;

            ConfigureServices(app, Current.Services);
            ConfigureMiddleware(app);
        }

        /// <summary>
        /// Configures services to be created in the OWIN context (CreatePerOwinContext)
        /// </summary>
        /// <param name="app"></param>
        /// <param name="services"></param>
        protected virtual void ConfigureServices(IAppBuilder app, ServiceContext services)
        {
            app.SetUmbracoLoggerFactory();

            //Configure the Identity user manager for use with Umbraco Back office
            // (EXPERT: an overload accepts a custom BackOfficeUserStore implementation)
            app.ConfigureUserManagerForUmbracoBackOffice(
                services,
                Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider().AsUmbracoMembershipProvider());

            app.ConfigureSignalR();
        }

        /// <summary>
        /// Configures middleware to be used (i.e. app.Use...)
        /// </summary>
        /// <param name="app"></param>
        protected virtual void ConfigureMiddleware(IAppBuilder app)
        {
            //Ensure owin is configured for Umbraco back office authentication. If you have any front-end OWIN
            // cookie configuration, this must be declared after it.
            app
                .UseUmbracoBackOfficeCookieAuthentication(Current.RuntimeState, PipelineStage.Authenticate)
                .UseUmbracoBackOfficeExternalCookieAuthentication(Current.RuntimeState, PipelineStage.Authenticate)
                .UseUmbracoPreviewAuthentication(Current.RuntimeState, PipelineStage.Authorize)
                .FinalizeMiddlewareConfiguration();
        }

        public static event EventHandler<OwinMiddlewareConfiguredEventArgs> MiddlewareConfigured;

        internal static void OnMiddlewareConfigured(OwinMiddlewareConfiguredEventArgs args)
        {
            MiddlewareConfigured?.Invoke(null, args);
        }
    }
}
