using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Logging
{
    internal class WebProfilerComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        // the profiler is too important to be composed in a component,
        // it is composed first thing in WebRuntime.Compose - this component
        // only initializes it if needed.
        //
        //public override void Compose(Composition Composition)
        //{
        //    composition.Container.RegisterSingleton<IProfiler, WebProfiler>();
        //}

        private WebProfiler _profiler;

        public void Initialize(IProfiler profiler, ILogger logger, IRuntimeState runtime)
        {
            // although registered in WebRuntime.Compose, ensure that we have not
            // been replaced by another component, and we are still "the" profiler
            _profiler = profiler as WebProfiler;
            if (_profiler == null)
            {
                // if VoidProfiler was registered, let it be known
                var vp = profiler as VoidProfiler;
                if (vp != null)
                    logger.Info<WebProfilerComponent>("Profiler is VoidProfiler, not profiling (must run debug mode to profile).");
                return;
            }

            // bind to ApplicationInit - ie execute the application initialization for *each* application
            // it would be a mistake to try and bind to the current application events
            UmbracoApplicationBase.ApplicationInit += InitializeApplication;
        }

        private void InitializeApplication(object sender, EventArgs args)
        {
            var app = sender as HttpApplication;
            if (app == null) return;

            // for *each* application (this will run more than once)
            app.BeginRequest += (s, a) => _profiler.UmbracoApplicationBeginRequest(s, a);
            app.EndRequest += (s, a) => _profiler.UmbracoApplicationEndRequest(s, a);
        }
    }
}
