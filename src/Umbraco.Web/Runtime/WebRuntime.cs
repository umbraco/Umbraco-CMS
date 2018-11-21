using System;
using System.Web;
using LightInject;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Runtime;
using Umbraco.Web.Logging;

namespace Umbraco.Web.Runtime
{
    /// <summary>
    /// Represents the Web Umbraco runtime.
    /// </summary>
    /// <remarks>On top of CoreRuntime, handles all of the web-related runtime aspects of Umbraco.</remarks>
    public class WebRuntime : CoreRuntime
    {
        private readonly UmbracoApplicationBase _umbracoApplication;
        private IProfiler _webProfiler;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRuntime"/> class.
        /// </summary>
        /// <param name="umbracoApplication"></param>
        public WebRuntime(UmbracoApplicationBase umbracoApplication)
            : base()
        {
            _umbracoApplication = umbracoApplication;
        }

        /// <inheritdoc/>
        public override void Boot(ServiceContainer container)
        {
            // create and start asap to profile boot
            var debug = GlobalSettings.DebugMode;
            if (debug)
            {
                _webProfiler = new WebProfiler();
                _webProfiler.Start();
            }
            else
            {
                // should let it be null, that's how MiniProfiler is meant to work,
                // but our own IProfiler expects an instance so let's get one
                _webProfiler = new VoidProfiler();
            }

            // some components may want to initialize with the UmbracoApplicationBase
            // well, they should not - we should not do this
            // TODO remove this eventually.
            container.RegisterInstance(_umbracoApplication);

            // replace CoreRuntime's IProfiler registration
            container.RegisterSingleton(_ => _webProfiler);

            // throws if not full-trust
            new AspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted).Demand();

            base.Boot(container);

            // now (and only now) is the time to switch over to perWebRequest scopes
            if (!(container.ScopeManagerProvider is MixedLightInjectScopeManagerProvider smp))
                throw new Exception("Container.ScopeManagerProvider is not MixedLightInjectScopeManagerProvider.");
            smp.EnablePerWebRequestScope();
        }
    }
}

