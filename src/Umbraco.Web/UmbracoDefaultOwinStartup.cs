using System;
using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Logging;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Logging;
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

            //Configure the Identity user manager for use with Umbraco Back office 
            // (EXPERT: an overload accepts a custom BackOfficeUserStore implementation)
            app.ConfigureUserManagerForUmbracoBackOffice(
                ApplicationContext,
                Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider().AsUmbracoMembershipProvider());
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
                .UseUmbracoBackOfficeCookieAuthentication(ApplicationContext, PipelineStage.Authenticate)
                .UseUmbracoBackOfficeExternalCookieAuthentication(ApplicationContext, PipelineStage.Authenticate)
                .UseUmbracoPreviewAuthentication(ApplicationContext, PipelineStage.Authorize)
                .FinalizeMiddlewareConfiguration();
        }

        /// <summary>
        /// Raised when the middelware has been configured
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
