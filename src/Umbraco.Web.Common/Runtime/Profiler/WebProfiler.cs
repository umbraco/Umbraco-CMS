using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Common.Runtime.Profiler
{
    public class WebProfiler : IProfiler
    {
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private volatile BootPhase _bootPhase;
        private MiniProfiler _startupProfiler;


        private const string BootRequestItemKey = "Umbraco.Core.Logging.WebProfiler__isBootRequest";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private int _first;

        public WebProfiler(IHttpContextAccessor httpContextAccessor)
        {
            // create our own provider, which can provide a profiler even during boot
            _httpContextAccessor = httpContextAccessor;
            _bootPhase = BootPhase.Boot;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks>
        /// Normally we would call MiniProfiler.Current.RenderIncludes(...), but because the requeststate is not set, this method does not work.
        /// We fake the requestIds from the RequestState here.
        /// </remarks>
        public string Render()
        {

            var profiler = MiniProfiler.Current;
            if (profiler == null) return string.Empty;

            var context = _httpContextAccessor.HttpContext;

            var path = (profiler.Options as MiniProfilerOptions)?.RouteBasePath.Value.EnsureTrailingSlash();

            var result = StackExchange.Profiling.Internal.Render.Includes(
                profiler,
                path: context.Request.PathBase + path,
                isAuthorized: true,
                requestIDs: new List<Guid>{ profiler.Id },
                position: RenderPosition.Right,
                showTrivial: profiler.Options.PopupShowTrivial,
                showTimeWithChildren: profiler.Options.PopupShowTimeWithChildren,
                maxTracesToShow: profiler.Options.PopupMaxTracesToShow,
                showControls:profiler.Options.ShowControls,
                startHidden: profiler.Options.PopupStartHidden);

            return result;
        }

        public IDisposable Step(string name)
        {
            return MiniProfiler.Current?.Step(name);
        }

        public void Start()
        {
            MiniProfiler.StartNew();
        }

        public void StartBoot()
        {
            _startupProfiler = MiniProfiler.StartNew("Startup Profiler");
        }

        public void StopBoot()
        {
            _startupProfiler.Stop();
        }

        public void Stop(bool discardResults = false)
        {
            MiniProfiler.Current?.Stop(discardResults);
        }


        public void UmbracoApplicationBeginRequest(HttpContext context)
        {
            if (ShouldProfile(context.Request))
            {
                Start();
            }
        }

        public void UmbracoApplicationEndRequest(HttpContext context)
        {
            if (ShouldProfile(context.Request))
            {
                Stop();

                // if this is the first request, append the startup profiler
                var first = Interlocked.Exchange(ref _first, 1) == 0;
                if (first)
                {

                    var startupDuration = _startupProfiler.Root.DurationMilliseconds.GetValueOrDefault();
                    MiniProfiler.Current.DurationMilliseconds += startupDuration;
                    MiniProfiler.Current.GetTimingHierarchy().First().DurationMilliseconds += startupDuration;
                    MiniProfiler.Current.Root.AddChild(_startupProfiler.Root);

                    _startupProfiler = null;
                }
            }
        }

        private static bool ShouldProfile(HttpRequest request)
        {
            if (new Uri(request.GetEncodedUrl(), UriKind.RelativeOrAbsolute).IsClientSideRequest()) return false;
            if (bool.TryParse(request.Query["umbDebug"], out var umbDebug)) return umbDebug;
            if (bool.TryParse(request.Headers["X-UMB-DEBUG"], out var xUmbDebug)) return xUmbDebug;
            if (bool.TryParse(request.Cookies["UMB-DEBUG"], out var cUmbDebug)) return cUmbDebug;
            return false;
        }

        /// <summary>
        ///     Indicates the boot phase.
        /// </summary>
        private enum BootPhase
        {
            Boot = 0, // boot phase (before the 1st request even begins)
            BootRequest = 1, // request boot phase (during the 1st request)
            Booted = 2 // done booting
        }
    }
}
