using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using StackExchange.Profiling;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Web.Common.Middleware;

namespace Umbraco.Web.Common.Runtime.Profiler
{
    public class WebProfiler : IProfiler
    {
        private const string BootRequestItemKey = "Umbraco.Core.Logging.WebProfiler__isBootRequest";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly WebProfilerProvider _provider;
        private int _first;

        public WebProfiler(IHttpContextAccessor httpContextAccessor)
        {
            // create our own provider, which can provide a profiler even during boot
            _provider = new WebProfilerProvider();
            _httpContextAccessor = httpContextAccessor;
        }

        public string Render() => MiniProfiler.Current
            .RenderIncludes(_httpContextAccessor.HttpContext, RenderPosition.Right).ToString();

        public IDisposable Step(string name) => MiniProfiler.Current?.Step(name);

        public void Start()
        {
            MiniProfiler.StartNew();
        }

        public void Stop(bool discardResults = false)
        {
            MiniProfiler.Current?.Stop(discardResults);
        }

        public void UmbracoApplicationBeginRequest(HttpContext context)
        {
            // if this is the first request, notify our own provider that this request is the boot request
            var first = Interlocked.Exchange(ref _first, 1) == 0;
            if (first)
            {
                _provider.BeginBootRequest();
                context.Items[BootRequestItemKey] = true;
                // and no need to start anything, profiler is already there
            }
            // else start a profiler, the normal way
            else if (ShouldProfile(context.Request))
                Start();
        }

        public void UmbracoApplicationEndRequest(HttpContext context)
        {
            // if this is the boot request, or if we should profile this request, stop
            // (the boot request is always profiled, no matter what)
            if (context.Items.TryGetValue(BootRequestItemKey, out var isBoot) && isBoot.Equals(true))
            {
                _provider.EndBootRequest();
                Stop();
            }
            else if (ShouldProfile(context.Request))
            {
                Stop();
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
    }
}
