namespace Umbraco.Cms.Core.Web;

public interface ICookieManager
{
    void ExpireCookie(string cookieName);

    string? GetCookieValue(string cookieName);

    [Obsolete("Use overload with the httpOnly parameter instead. Scheduled for removal in V16.")]
    void SetCookieValue(string cookieName, string value) => SetCookieValue(cookieName, value, false);

    void SetCookieValue(string cookieName, string value, bool httpOnly);

    bool HasCookie(string cookieName);
}
