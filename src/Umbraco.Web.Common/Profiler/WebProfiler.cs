using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Profiler;

public class WebProfiler : IProfiler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WebProfiler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }


    public static readonly AsyncLocal<MiniProfiler> MiniProfilerContext = new(x =>
    {
        _ = x;
    });

    private const string WebProfileCookieKey = "umbracoWebProfiler";

    private int _first;
    private MiniProfiler? _startupProfiler;

    public IDisposable? Step(string name) =>
        MiniProfiler.Current?.Step(name);
    bool IsEnabled => true;

    public void Start()
    {
        var name = $"{_httpContextAccessor.HttpContext?.Request.Method} {_httpContextAccessor.HttpContext?.Request.GetDisplayUrl()}";

        MiniProfiler.StartNew(name);

        MiniProfilerContext.Value = MiniProfiler.Current!;
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
                    AddSubProfiler(MiniProfiler.FromJson(cookieValue)!);
                }

                // If it is a redirect to a relative path (local redirect)
                if (context.Response.StatusCode == (int)HttpStatusCode.Redirect
                    && context.Response.Headers.TryGetValue(HeaderNames.Location, out StringValues location)
                    && !location.Contains("://"))
                {
                    MiniProfilerContext.Value.Root.Name = "Before Redirect";
                    cookieManager.SetCookieValue(WebProfileCookieKey, MiniProfilerContext.Value.ToJson(), false);
                }
            }
        }
    }

    private static ICookieManager GetCookieManager(HttpContext context) =>
        context.RequestServices.GetRequiredService<ICookieManager>();

    private bool ShouldProfile(HttpRequest request)
    {
        if (request.IsClientSideRequest())
        {
            return false;
        }

        IOptions<MiniProfilerOptions>? miniprofilerOptions = _httpContextAccessor.HttpContext?.RequestServices?.GetService<IOptions<MiniProfilerOptions>>();
        if (miniprofilerOptions is not null && miniprofilerOptions.Value.IgnoredPaths.Contains(request.Path))
        {
            return false;
        }

        IWebProfilerService? webProfilerService = _httpContextAccessor.HttpContext?.RequestServices?.GetService<IWebProfilerService>();

        if (webProfilerService is not null)
        {
            Attempt<bool, WebProfilerOperationStatus> shouldProfile = webProfilerService.GetStatus().GetAwaiter().GetResult();

            if (shouldProfile.Success)
            {
                return shouldProfile.Result;
            }
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
