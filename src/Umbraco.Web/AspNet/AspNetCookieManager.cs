using System.Web;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Web
{
    public class AspNetCookieManager : ICookieManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspNetCookieManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void ExpireCookie(string cookieName)
        {
            _httpContextAccessor.HttpContext?.ExpireCookie(cookieName);
        }

        public string GetCookieValue(string cookieName)
        {
            return _httpContextAccessor.HttpContext?.Request.GetCookieValue(cookieName);
        }

        public void SetCookieValue(string cookieName, string value)
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Set(new HttpCookie(cookieName, value));
        }

        public bool HasCookie(string cookieName)
        {
            return !(GetCookieValue(cookieName) is null);
        }
    }
}
