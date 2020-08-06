using Umbraco.Core;

namespace Umbraco.Web
{
    public static class CookieManagerExtensions
    {
        public static string GetPreviewCookieValue(this ICookieManager cookieManager)
        {
            return cookieManager.GetCookieValue(Constants.Web.PreviewCookieName);
        }

    }

}
