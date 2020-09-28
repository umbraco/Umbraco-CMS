using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Net;
using Umbraco.Web.Common.Lifetime;
using Umbraco.Web.Common.Middleware;

namespace Umbraco.Web.Common.Profiler
{
    internal sealed class WebProfilerComponent : IComponent
    {
        private readonly bool _profile;
        private readonly WebProfiler _profiler;
        private readonly IUmbracoApplicationLifetime _umbracoApplicationLifetime;
        private readonly IUmbracoRequestLifetime _umbracoRequestLifetime;
        private readonly List<Action> _terminate = new List<Action>();

        public WebProfilerComponent(IProfiler profiler, ILogger<WebProfilerComponent> logger, IUmbracoRequestLifetime umbracoRequestLifetime,
            IUmbracoApplicationLifetime umbracoApplicationLifetime)
        {
            _umbracoRequestLifetime = umbracoRequestLifetime;
            _umbracoApplicationLifetime = umbracoApplicationLifetime;
            _profile = true;

            // although registered in WebRuntime.Compose, ensure that we have not
            // been replaced by another component, and we are still "the" profiler
            _profiler = profiler as WebProfiler;
            if (_profiler != null) return;

            // if VoidProfiler was registered, let it be known
            if (profiler is VoidProfiler)
                logger.LogInformation(
                    "Profiler is VoidProfiler, not profiling (must run debug mode to profile).");
            _profile = false;
        }

        public void Initialize()
        {
            if (!_profile) return;

            // bind to ApplicationInit - ie execute the application initialization for *each* application
            // it would be a mistake to try and bind to the current application events
            _umbracoApplicationLifetime.ApplicationInit += InitializeApplication;
        }

        public void Terminate()
        {
            _umbracoApplicationLifetime.ApplicationInit -= InitializeApplication;
            foreach (var t in _terminate) t();
        }

        private void InitializeApplication(object sender, EventArgs args)
        {
            void requestStart(object sender, HttpContext context) => _profiler.UmbracoApplicationBeginRequest(context);
            _umbracoRequestLifetime.RequestStart += requestStart;
            _terminate.Add(() => _umbracoRequestLifetime.RequestStart -= requestStart);

            void requestEnd(object sender, HttpContext context) => _profiler.UmbracoApplicationEndRequest(context);
            _umbracoRequestLifetime.RequestEnd += requestEnd;
            _terminate.Add(() => _umbracoRequestLifetime.RequestEnd -= requestEnd);

            // Stop the profiling of the booting process
            _profiler.StopBoot();
        }
    }
}
