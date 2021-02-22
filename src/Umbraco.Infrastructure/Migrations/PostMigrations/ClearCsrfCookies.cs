using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Web;
using Umbraco.Core;
using Umbraco.Core.Migrations;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Web.Migrations.PostMigrations
{
    /// <summary>
    /// Clears Csrf tokens.
    /// </summary>
    public class ClearCsrfCookies : IMigration
    {
        private readonly ICookieManager _cookieManager;

        public ClearCsrfCookies(IMigrationContext context, ICookieManager cookieManager)
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
