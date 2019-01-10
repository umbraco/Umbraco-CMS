using System;
using System.Web;
using Umbraco.Core.Components;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Logging
{
    internal sealed class WebProfilerComponent : IComponent
    {
        private readonly WebProfiler _profiler;
        private readonly bool _profile;

        public WebProfilerComponent(IProfiler profiler, ILogger logger)
        {
            _profile = true;

            // although registered in WebRuntime.Compose, ensure that we have not
            // been replaced by another component, and we are still "the" profiler
            _profiler = profiler as WebProfiler;
            if (_profiler != null) return;

            // if VoidProfiler was registered, let it be known
            if (profiler is VoidProfiler)
                logger.Info<WebProfilerComponent>("Profiler is VoidProfiler, not profiling (must run debug mode to profile).");
            _profile = false;
        }

        public void Initialize()
        {
            if (!_profile) return;

            // bind to ApplicationInit - ie execute the application initialization for *each* application
            // it would be a mistake to try and bind to the current application events
            UmbracoApplicationBase.ApplicationInit += InitializeApplication;
        }

        public void Terminate()
        { }

        private void InitializeApplication(object sender, EventArgs args)
        {
            if (!(sender is HttpApplication app)) return;

            // for *each* application (this will run more than once)
            app.BeginRequest += (s, a) => _profiler.UmbracoApplicationBeginRequest(s, a);
            app.EndRequest += (s, a) => _profiler.UmbracoApplicationEndRequest(s, a);
        }
    }
}
