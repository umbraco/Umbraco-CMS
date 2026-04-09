using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Web.Common.AspNetCore;

/// <summary>
/// An <see cref="ICookieManager"/> implementation for ASP.NET Core.
/// </summary>
public class AspNetCoreCookieManager : ICookieManager
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="AspNetCoreCookieManager"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The <see href="IHttpContextAccessor" />.</param>
    public AspNetCoreCookieManager(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc/>
    public void ExpireCookie(string cookieName)
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return;
        }

        httpContext.Response.Cookies.Delete(cookieName);
    }

    /// <inheritdoc/>
    public string? GetCookieValue(string cookieName) => _httpContextAccessor.HttpContext?.Request.Cookies[cookieName];

    /// <inheritdoc/>
    public void SetCookieValue(string cookieName, string value, bool httpOnly, bool secure, string sameSiteMode)
    {
        SameSiteMode sameSiteModeValue = ParseSameSiteMode(sameSiteMode);
        var options = new CookieOptions
        {
            HttpOnly = httpOnly,
            SameSite = sameSiteModeValue,
            Secure = secure,
        };

        _httpContextAccessor.HttpContext?.Response.Cookies.Append(cookieName, value, options);
    }

    private static SameSiteMode ParseSameSiteMode(string sameSiteMode) =>
        Enum.TryParse(sameSiteMode, ignoreCase: true, out SameSiteMode result)
            ? result
            : throw new ArgumentException($"The provided {nameof(sameSiteMode)} value could not be parsed into as SameSiteMode value.", nameof(sameSiteMode));

    /// <inheritdoc/>
    public bool HasCookie(string cookieName) => GetCookieValue(cookieName) is not null;
}
