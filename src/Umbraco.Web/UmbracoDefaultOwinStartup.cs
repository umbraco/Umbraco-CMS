using System;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Logging;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Web.Security.Identity;
using Umbraco.Web.SignalR;

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
            ConfigureSignalR(app);
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

        private IHubContext _previewHubContext;

        /// <summary>
        /// Configures SignalR.
        /// </summary>
        /// <param name="app"></param>
        protected virtual void ConfigureSignalR(IAppBuilder app)
        {
            app.MapSignalR();

            _previewHubContext = GlobalHost.ConnectionManager.GetHubContext<PreviewHub>();
            PreviewHub.Initialize(_previewHubContext);
        }

        protected virtual ApplicationContext ApplicationContext => ApplicationContext.Current;

        public static event EventHandler<OwinMiddlewareConfiguredEventArgs> MiddlewareConfigured;

        internal static void OnMiddlewareConfigured(OwinMiddlewareConfiguredEventArgs args)
        {
            MiddlewareConfigured?.Invoke(null, args);
        }
    }
}
