using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Web.Common.AspNetCore;

internal class AspNetCoreSessionManager : ISessionIdResolver, ISessionManager
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AspNetCoreSessionManager(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public string? SessionId
    {
        get
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;

            return IsSessionsAvailable
                ? httpContext?.Session.Id
                : "0";
        }
    }

    /// <summary>
    ///     If session isn't enabled this will throw an exception so we check
    /// </summary>
    private bool IsSessionsAvailable => !(_httpContextAccessor.HttpContext?.Features.Get<ISessionFeature>() is null);

    public string? GetSessionValue(string key)
    {
        if (!IsSessionsAvailable)
        {
            return null;
        }

        return _httpContextAccessor.HttpContext?.Session.GetString(key);
    }

    public void SetSessionValue(string key, string value)
    {
        if (!IsSessionsAvailable)
        {
            return;
        }

        _httpContextAccessor.HttpContext?.Session.SetString(key, value);
    }

    public void ClearSessionValue(string key)
    {
        if (!IsSessionsAvailable)
        {
            return;
        }

        _httpContextAccessor.HttpContext?.Session.Remove(key);
    }
}
