using Umbraco.Core;

namespace Umbraco.Web
{
    public static class CookieManagerExtensions
    {
        public static string GetPreviewCookieValue(this ICookieManager cookieManager)
        {
            return cookieManager.GetCookieValue(Constants.Web.PreviewCookieName);
        }

        /// <summary>
        /// Does a preview cookie exist ?
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool HasPreviewCookie(this ICookieManager cookieManager)
        {
            return cookieManager.HasCookie(Constants.Web.PreviewCookieName);
        }
    }

}
