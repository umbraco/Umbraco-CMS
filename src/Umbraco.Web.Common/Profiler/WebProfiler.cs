using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Profiler;

public class WebProfiler : IProfiler
{
    public static readonly AsyncLocal<MiniProfiler> MiniProfilerContext = new(x =>
    {
        _ = x;
    });

    private const string WebProfileCookieKey = "umbracoWebProfiler";

    private int _first;
    private MiniProfiler? _startupProfiler;

    public IDisposable? Step(string name) => MiniProfiler.Current?.Step(name);

    public void Start()
    {
        MiniProfiler.StartNew();
        MiniProfilerContext.Value = MiniProfiler.Current;
    }

    public void Stop(bool discardResults = false) => MiniProfilerContext.Value?.Stop(discardResults);

    public void StartBoot() => _startupProfiler = MiniProfiler.StartNew("Startup Profiler");

    public void StopBoot() => _startupProfiler?.Stop();

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
            cookieManager.ExpireCookie(
                WebProfileCookieKey); // Ensure we expire the cookie, so we do not reuse the old potential value saved
        }
    }

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
                    if (_startupProfiler is not null)
                    {
                        AddSubProfiler(_startupProfiler);
                    }

                    _startupProfiler = null;
                }

                ICookieManager cookieManager = GetCookieManager(context);
                var cookieValue = cookieManager.GetCookieValue(WebProfileCookieKey);

                if (cookieValue is not null)
                {
                    AddSubProfiler(MiniProfiler.FromJson(cookieValue));
                }

                // If it is a redirect to a relative path (local redirect)
                if (context.Response.StatusCode == (int)HttpStatusCode.Redirect
                    && context.Response.Headers.TryGetValue(HeaderNames.Location, out StringValues location)
                    && !location.Contains("://"))
                {
                    MiniProfilerContext.Value.Root.Name = "Before Redirect";
                    cookieManager.SetCookieValue(WebProfileCookieKey, MiniProfilerContext.Value.ToJson());
                }
            }
        }
    }

    private static ICookieManager GetCookieManager(HttpContext context) =>
        context.RequestServices.GetRequiredService<ICookieManager>();

    private static bool ShouldProfile(HttpRequest request)
    {
        if (request.IsClientSideRequest())
        {
            return false;
        }

        if (bool.TryParse(request.Query["umbDebug"], out var umbDebug))
        {
            return umbDebug;
        }

        if (bool.TryParse(request.Headers["X-UMB-DEBUG"], out var xUmbDebug))
        {
            return xUmbDebug;
        }

        if (bool.TryParse(request.Cookies["UMB-DEBUG"], out var cUmbDebug))
        {
            return cUmbDebug;
        }

        return false;
    }

    private void AddSubProfiler(MiniProfiler subProfiler)
    {
        var startupDuration = subProfiler.Root.DurationMilliseconds.GetValueOrDefault();
        if (MiniProfilerContext.Value is not null)
        {
            MiniProfilerContext.Value.DurationMilliseconds += startupDuration;
            MiniProfilerContext.Value.GetTimingHierarchy().First().DurationMilliseconds += startupDuration;
            MiniProfilerContext.Value.Root.AddChild(subProfiler.Root);
        }
    }
}
