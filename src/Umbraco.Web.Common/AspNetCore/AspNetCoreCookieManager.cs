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

        var cookieValue = httpContext.Request.Cookies[cookieName];

        httpContext.Response.Cookies.Append(
            cookieName,
            cookieValue ?? string.Empty,
            new CookieOptions
            {
                Expires = DateTime.Now.AddYears(-1),
            });
    }

    /// <inheritdoc/>
    public string? GetCookieValue(string cookieName) => _httpContextAccessor.HttpContext?.Request.Cookies[cookieName];

    /// <inheritdoc/>
    [Obsolete("Use overload with the secure and sameSiteMode parameters instead. Scheduled for removal in V17.")]
    public void SetCookieValue(string cookieName, string value, bool httpOnly) =>
        SetCookieValue(cookieName, value, httpOnly, false, SameSiteMode.Unspecified.ToString());

    /// <inheritdoc/>
    public void SetCookieValue(string cookieName, string value, bool httpOnly, bool secure, string sameSiteMode)
    {
        SameSiteMode sameSiteModeValue = ParseSameSiteMode(sameSiteMode);
        _httpContextAccessor.HttpContext?.Response.Cookies.Append(
            cookieName,
            value,
            new CookieOptions
            {
                HttpOnly = httpOnly,
                SameSite = sameSiteModeValue,
                Secure = secure,
            });
    }

    private static SameSiteMode ParseSameSiteMode(string sameSiteMode) =>
        Enum.TryParse(sameSiteMode, ignoreCase: true, out SameSiteMode result)
            ? result
            : throw new ArgumentException($"The provided {nameof(sameSiteMode)} value could not be parsed into as SameSiteMode value.", nameof(sameSiteMode));

    /// <inheritdoc/>
    public bool HasCookie(string cookieName) => GetCookieValue(cookieName) is not null;
}
