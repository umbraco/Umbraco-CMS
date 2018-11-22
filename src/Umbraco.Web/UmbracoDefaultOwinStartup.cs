using System;
using Microsoft.Owin;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Security;
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

            ConfigureServices(app);
            ConfigureMiddleware(app);
        }

        /// <summary>
        /// Configures services to be created in the OWIN context (CreatePerOwinContext)
        /// </summary>
        /// <param name="app"></param>
        protected virtual void ConfigureServices(IAppBuilder app)
        {
            app.SetUmbracoLoggerFactory();
            ConfigureUmbracoUserManager(app);
        }

        /// <summary>
        /// Configures middleware to be used (i.e. app.Use...)
        /// </summary>
        /// <param name="app"></param>
        protected virtual void ConfigureMiddleware(IAppBuilder app)
        {

            // Configure OWIN for authentication. 
            ConfigureUmbracoAuthentication(app);

            app
                .UseSignalR()
                .FinalizeMiddlewareConfiguration();
        }

        /// <summary>
        /// Configure the Identity user manager for use with Umbraco Back office
        /// </summary>
        /// <param name="app"></param>
        protected virtual void ConfigureUmbracoUserManager(IAppBuilder app)
        {
            // (EXPERT: an overload accepts a custom BackOfficeUserStore implementation)
            app.ConfigureUserManagerForUmbracoBackOffice(
                ApplicationContext,
                Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider().AsUmbracoMembershipProvider());
        }

        /// <summary>
        /// Configure external/OAuth login providers
        /// </summary>
        /// <param name="app"></param>
        protected virtual void ConfigureUmbracoAuthentication(IAppBuilder app)
        {
            // Ensure owin is configured for Umbraco back office authentication.
            // Front-end OWIN cookie configuration must be declared after this code.
            app
                .UseUmbracoBackOfficeCookieAuthentication(ApplicationContext, PipelineStage.Authenticate)
                .UseUmbracoBackOfficeExternalCookieAuthentication(ApplicationContext, PipelineStage.Authenticate)
                .UseUmbracoPreviewAuthentication(ApplicationContext, PipelineStage.Authorize);
        }

        /// <summary>
        /// Raised when the middleware has been configured
        /// </summary>
        public static event EventHandler<OwinMiddlewareConfiguredEventArgs> MiddlewareConfigured;

        protected virtual ApplicationContext ApplicationContext
        {
            get { return ApplicationContext.Current; }
        }

        internal static void OnMiddlewareConfigured(OwinMiddlewareConfiguredEventArgs args)
        {
            var handler = MiddlewareConfigured;
            if (handler != null) handler(null, args);
        }
    }
}
