namespace Umbraco.Cms.Core.Web;

public interface ICookieManager
{
    void ExpireCookie(string cookieName);

    string? GetCookieValue(string cookieName);

    void SetCookieValue(string cookieName, string value);

    bool HasCookie(string cookieName);
}
