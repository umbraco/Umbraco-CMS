using System;
using Microsoft.Extensions.Options;
using Microsoft.Owin;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Mapping;
using Umbraco.Net;
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
        protected GlobalSettings GlobalSettings => Current.Factory.GetRequiredService<GlobalSettings>();
        protected SecuritySettings SecuritySettings => Current.Factory.GetRequiredService<IOptions<SecuritySettings>>().Value;
        protected IUserPasswordConfiguration UserPasswordConfig => Current.Factory.GetRequiredService<IUserPasswordConfiguration>();
        protected IRuntimeState RuntimeState => Current.RuntimeState;
        protected ServiceContext Services => Current.Services;
        protected UmbracoMapper Mapper => Current.Mapper;
        protected IIpResolver IpResolver => Current.IpResolver;
        protected IHostingEnvironment HostingEnvironment => Current.HostingEnvironment;
        protected IRequestCache RequestCache => Current.AppCaches.RequestCache;

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
                .UseSignalR(GlobalSettings, HostingEnvironment)
                .FinalizeMiddlewareConfiguration();
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
                .UseUmbracoBackOfficeExternalCookieAuthentication(UmbracoContextAccessor, RuntimeState, GlobalSettings, HostingEnvironment, RequestCache, PipelineStage.Authenticate);
        }

        public static event EventHandler<OwinMiddlewareConfiguredEventArgs> MiddlewareConfigured;

        internal static void OnMiddlewareConfigured(OwinMiddlewareConfiguredEventArgs args)
        {
            MiddlewareConfigured?.Invoke(null, args);
        }
    }
}
