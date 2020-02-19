using Umbraco.Core;
using Umbraco.Core.Cookie;
using Umbraco.Core.Migrations;


namespace Umbraco.Web.Migrations.PostMigrations
{
    /// <summary>
    /// Clears Csrf tokens.
    /// </summary>
    public class ClearCsrfCookies : IMigration
    {
        private readonly ICookieManager _cookieManager;

        public ClearCsrfCookies(ICookieManager cookieManager)
        {
            _cookieManager = cookieManager;
        }

        public void Migrate()
        {
            _cookieManager.ExpireCookie(Constants.Web.AngularCookieName);
            _cookieManager.ExpireCookie(Constants.Web.CsrfValidationCookieName);
        }
    }
}
