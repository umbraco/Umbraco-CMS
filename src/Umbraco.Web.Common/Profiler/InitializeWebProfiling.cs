using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Web.Common.Lifetime;

namespace Umbraco.Web.Common.Profiler
{
    public class InitializeWebProfiling : INotificationHandler<UmbracoApplicationStarting>
    {
        private readonly bool _profile;
        private readonly WebProfiler _profiler;
        private readonly IUmbracoRequestLifetime _umbracoRequestLifetime;
        private readonly List<Action> _terminate = new List<Action>();
        public InitializeWebProfiling(IProfiler profiler, IUmbracoRequestLifetime umbracoRequestLifetime, ILogger<InitializeWebProfiling> logger)
        {
            _umbracoRequestLifetime = umbracoRequestLifetime;
            _profile = true;

            // although registered in WebRuntime.Compose, ensure that we have not
            // been replaced by another component, and we are still "the" profiler
            _profiler = profiler as WebProfiler;
            if (_profiler != null) return;

            // if VoidProfiler was registered, let it be known
            if (profiler is NoopProfiler)
                logger.LogInformation(
                    "Profiler is VoidProfiler, not profiling (must run debug mode to profile).");
            _profile = false;
        }

        /// <inheritdoc/>
        public Task HandleAsync(UmbracoApplicationStarting notification, CancellationToken cancellationToken)
        {
            if (_profile)
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

            return Task.CompletedTask;
        }
    }
}
