using System;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Profiler
{

    public class WebProfiler : IProfiler
    {
        private const string WebProfileCookieKey = "umbracoWebProfiler";

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

        public void UmbracoApplicationBeginRequest(HttpContext context, RuntimeLevel runtimeLevel)
        {
            if (runtimeLevel != RuntimeLevel.Run)
            {
                return;
            }

            if (ShouldProfile(context.Request))
            {
                Start();
                ICookieManager cookieManager = GetCookieManager(context);
                cookieManager.ExpireCookie(WebProfileCookieKey); //Ensure we expire the cookie, so we do not reuse the old potential value saved
            }
        }

        private static ICookieManager GetCookieManager(HttpContext context) => context.RequestServices.GetRequiredService<ICookieManager>();

        public void UmbracoApplicationEndRequest(HttpContext context, RuntimeLevel runtimeLevel)
        {
            if (runtimeLevel != RuntimeLevel.Run)
            {
                return;
            }

            if (ShouldProfile(context.Request))
            {
                Stop();

                if (MiniProfilerContext.Value is not null)
                {
                    // if this is the first request, append the startup profiler
                    var first = Interlocked.Exchange(ref _first, 1) == 0;
                    if (first)
                    {
                        AddSubProfiler(_startupProfiler);

                        _startupProfiler = null;
                    }

                    ICookieManager cookieManager = GetCookieManager(context);
                    var cookieValue = cookieManager.GetCookieValue(WebProfileCookieKey);

                    if (cookieValue is not null)
                    {
                        AddSubProfiler(MiniProfiler.FromJson(cookieValue));
                    }

                    //If it is a redirect to a relative path (local redirect)
                    if (context.Response.StatusCode == (int)HttpStatusCode.Redirect
                        && context.Response.Headers.TryGetValue(Microsoft.Net.Http.Headers.HeaderNames.Location, out var location)
                        && !location.Contains("://"))
                    {
                        MiniProfilerContext.Value.Root.Name = "Before Redirect";
                        cookieManager.SetCookieValue(WebProfileCookieKey, MiniProfilerContext.Value.ToJson());
                    }

                }

            }
        }

        private void AddSubProfiler(MiniProfiler subProfiler)
        {
            var startupDuration = subProfiler.Root.DurationMilliseconds.GetValueOrDefault();
            MiniProfilerContext.Value.DurationMilliseconds += startupDuration;
            MiniProfilerContext.Value.GetTimingHierarchy().First().DurationMilliseconds += startupDuration;
            MiniProfilerContext.Value.Root.AddChild(subProfiler.Root);

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
