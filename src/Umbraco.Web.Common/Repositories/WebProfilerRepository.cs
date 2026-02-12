using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Repositories;

internal sealed class WebProfilerRepository : IWebProfilerRepository
{
    private const string CookieName = "UMB-DEBUG";
    private const string HeaderName = "X-UMB-DEBUG";
    private const string QueryName = "umbDebug";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICookieManager _cookieManager;
    private readonly GlobalSettings _globalSettings;

    public WebProfilerRepository(IHttpContextAccessor httpContextAccessor, ICookieManager cookieManager, IOptions<GlobalSettings> globalSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _cookieManager = cookieManager;
        _globalSettings = globalSettings.Value;
    }

    public void SetStatus(int userId, bool status)
    {
        if (status)
        {
            // This cookie enables debug profiling on the front-end without needing query strings or headers.
            // It uses SameSite=Strict, so it only works when the BackOffice and front-end share the same domain.
            // It's marked httpOnly to prevent JavaScript access (the server reads it, not client-side code).
            // No expiration is set, so it's a session cookie and will be deleted when the browser closes.
            // For cross-site setups, use the query string (?umbDebug=true) or header (X-UMB-DEBUG) instead.
            _cookieManager.SetCookieValue(
                CookieName,
                "1",
                httpOnly: true,
                secure: _globalSettings.UseHttps,
                sameSiteMode: "Strict");
        }
        else
        {
            _cookieManager.ExpireCookie(CookieName);
        }
    }

    public bool GetStatus(int userId)
    {

        var request = _httpContextAccessor.GetRequiredHttpContext().Request;
        if (bool.TryParse(request.Query[QueryName], out var umbDebug))
        {
            return umbDebug;
        }

        if (bool.TryParse(request.Headers[HeaderName], out var xUmbDebug))
        {
            return xUmbDebug;
        }

        return _cookieManager.HasCookie(CookieName);
    }
}
