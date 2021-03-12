using System;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Http;
using StackExchange.Profiling;
using Umbraco.Cms.Core.Logging;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Profiler
{

    public class WebProfiler : IProfiler
    {
        public static readonly AsyncLocal<MiniProfiler> MiniProfilerContext = new AsyncLocal<MiniProfiler>(x =>
        {
            _ = x;
        });
        private MiniProfiler _startupProfiler;
        private int _first;



        public IDisposable Step(string name)
        {
            return MiniProfiler.Current?.Step(name);
        }

        public void Start()
        {
            MiniProfiler.StartNew();
            MiniProfilerContext.Value = MiniProfiler.Current;
        }

        public void StartBoot() => _startupProfiler = MiniProfiler.StartNew("Startup Profiler");

        public void StopBoot() => _startupProfiler.Stop();

        public void Stop(bool discardResults = false) => MiniProfilerContext.Value?.Stop(discardResults);


        public void UmbracoApplicationBeginRequest(HttpContext context)
        {
            if (ShouldProfile(context.Request))
                Start();
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
                    MiniProfilerContext.Value.DurationMilliseconds += startupDuration;
                    MiniProfilerContext.Value.GetTimingHierarchy().First().DurationMilliseconds += startupDuration;
                    MiniProfilerContext.Value.Root.AddChild(_startupProfiler.Root);

                    _startupProfiler = null;
                }
            }
        }

        private static bool ShouldProfile(HttpRequest request)
        {
            if (request.IsClientSideRequest()) return false;
            if (bool.TryParse(request.Query["umbDebug"], out var umbDebug)) return umbDebug;
            if (bool.TryParse(request.Headers["X-UMB-DEBUG"], out var xUmbDebug)) return xUmbDebug;
            if (bool.TryParse(request.Cookies["UMB-DEBUG"], out var cUmbDebug)) return cUmbDebug;
            return false;
        }
    }
}
