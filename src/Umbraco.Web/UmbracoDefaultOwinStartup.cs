using System;
using Microsoft.Owin;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Mapping;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.Security;


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
        protected IUmbracoContextAccessor UmbracoContextAccessor => Current.UmbracoContextAccessor;
        protected IGlobalSettings GlobalSettings => Current.Configs.Global();
        protected IUmbracoSettingsSection UmbracoSettings => Current.Configs.Settings();
        protected IRuntimeState RuntimeState => Core.Composing.Current.RuntimeState;
        protected ServiceContext Services => Current.Services;
        protected UmbracoMapper Mapper => Current.Mapper;

        /// <summary>
        /// Main startup method
        /// </summary>
        /// <param name="app"></param>
        public virtual void Configuration(IAppBuilder app)
        {
            app.SanitizeThreadCulture();

            // there's nothing we can do really
            if (RuntimeState.Level == RuntimeLevel.BootFailed)
                return;

            ConfigureServices(app, Services);
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

            // must come after all authentication
            app.UseUmbracoBackOfficeExternalLoginErrors();

            app
                .UseSignalR(GlobalSettings)
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
                Services,
                Mapper,
                UmbracoSettings.Content,
                GlobalSettings,
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
                .UseUmbracoBackOfficeCookieAuthentication(UmbracoContextAccessor, RuntimeState, Services.UserService, GlobalSettings, UmbracoSettings.Security, PipelineStage.Authenticate)
                .UseUmbracoBackOfficeExternalCookieAuthentication(UmbracoContextAccessor, RuntimeState, GlobalSettings, PipelineStage.Authenticate)
                // TODO: this would be considered a breaking change but this must come after all authentication so should be moved within ConfigureMiddleware
                .UseUmbracoPreviewAuthentication(UmbracoContextAccessor, RuntimeState, GlobalSettings, UmbracoSettings.Security, PipelineStage.Authorize);
        }

        public static event EventHandler<OwinMiddlewareConfiguredEventArgs> MiddlewareConfigured;

        internal static void OnMiddlewareConfigured(OwinMiddlewareConfiguredEventArgs args)
        {
            MiddlewareConfigured?.Invoke(null, args);
        }
    }
}
