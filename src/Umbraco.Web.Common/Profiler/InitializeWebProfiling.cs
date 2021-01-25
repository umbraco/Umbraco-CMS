// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Web.Common.Lifetime;

namespace Umbraco.Web.Common.Profiler
{
    /// <summary>
    /// Initialized the web profiling. Ensures the boot process profiling is stopped.
    /// </summary>
    public class InitializeWebProfiling : INotificationHandler<UmbracoApplicationStarting>
    {
        private readonly bool _profile;
        private readonly WebProfiler _profiler;
        private readonly IUmbracoRequestLifetime _umbracoRequestLifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeWebProfiling"/> class.
        /// </summary>
        public InitializeWebProfiling(IProfiler profiler, IUmbracoRequestLifetime umbracoRequestLifetime, ILogger<InitializeWebProfiling> logger)
        {
            _umbracoRequestLifetime = umbracoRequestLifetime;
            _profile = true;

            // although registered in UmbracoBuilderExtensions.AddUmbraco, ensure that we have not
            // been replaced by another component, and we are still "the" profiler
            _profiler = profiler as WebProfiler;
            if (_profiler != null)
            {
                return;
            }

            // if VoidProfiler was registered, let it be known
            if (profiler is NoopProfiler)
            {
                logger.LogInformation(
                    "Profiler is VoidProfiler, not profiling (must run debug mode to profile).");
            }

            _profile = false;
        }

        /// <inheritdoc/>
        public void Handle(UmbracoApplicationStarting notification)
        {
            if (_profile)
            {
                _umbracoRequestLifetime.RequestStart += (sender, context) => _profiler.UmbracoApplicationBeginRequest(context);

                _umbracoRequestLifetime.RequestEnd += (sender, context) => _profiler.UmbracoApplicationEndRequest(context);

                // Stop the profiling of the booting process
                _profiler.StopBoot();
            }
        }
    }
}
