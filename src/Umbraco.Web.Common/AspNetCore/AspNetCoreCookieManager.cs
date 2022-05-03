using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Web.Common.AspNetCore;

public class AspNetCoreCookieManager : ICookieManager
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AspNetCoreCookieManager(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public void ExpireCookie(string cookieName)
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return;
        }

        var cookieValue = httpContext.Request.Cookies[cookieName];

        httpContext.Response.Cookies.Append(cookieName, cookieValue ?? string.Empty,
            new CookieOptions { Expires = DateTime.Now.AddYears(-1) });
    }

    public string? GetCookieValue(string cookieName) => _httpContextAccessor.HttpContext?.Request.Cookies[cookieName];

    public void SetCookieValue(string cookieName, string value) =>
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(cookieName, value, new CookieOptions());

    public bool HasCookie(string cookieName) => !(GetCookieValue(cookieName) is null);
}
