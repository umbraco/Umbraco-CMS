using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Logging;
using Owin;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Web.Logging;

namespace Umbraco.Web
{
    /// <summary>
    /// Provides general extension methods to IAppBuilder.
    /// </summary>
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Called at the end of configuring middleware
        /// </summary>
        /// <param name="app">The app builder.</param>
        /// <remarks>
        /// This could be used for something else in the future - maybe to inform Umbraco that middleware is done/ready, but for
        /// now this is used to raise the custom event
        ///
        /// This is an extension method in case developer entirely replace the UmbracoDefaultOwinStartup class, in which case they will
        /// need to ensure they call this extension method in their startup class.
        /// </remarks>
        public static void FinalizeMiddlewareConfiguration(this IAppBuilder app)
        {
            UmbracoDefaultOwinStartup.OnMiddlewareConfigured(new OwinMiddlewareConfiguredEventArgs(app));
        }

        /// <summary>
        /// Sets the OWIN logger to use Umbraco's logging system.
        /// </summary>
        /// <param name="app">The app builder.</param>
        public static void SetUmbracoLoggerFactory(this IAppBuilder app)
        {
            app.SetLoggerFactory(new OwinLoggerFactory());
        }

        /// <summary>
        /// Configures SignalR.
        /// </summary>
        /// <param name="app">The app builder.</param>
        /// <param name="globalSettings"></param>
        /// <param name="hostingEnvironment"></param>
        public static IAppBuilder UseSignalR(this IAppBuilder app, GlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
        {
            var umbracoPath = globalSettings.GetUmbracoMvcArea(hostingEnvironment);
            var signalrPath = HttpRuntime.AppDomainAppVirtualPath + umbracoPath + "/BackOffice/signalr";
            return app.MapSignalR(signalrPath, new HubConfiguration { EnableDetailedErrors = true });
        }
    }
}
