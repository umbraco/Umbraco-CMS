using Umbraco.Core.Cookie;

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
    }
}
