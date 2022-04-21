using Umbraco.Cms.Core.Web;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.Migrations.PostMigrations
{
    /// <summary>
    /// Clears CSRF tokens for the current request.
    /// </summary>
    public class ClearCsrfCookies : MigrationBase
    {
        private readonly ICookieManager _cookieManager;

        public ClearCsrfCookies(IMigrationContext context, ICookieManager cookieManager)
            : base(context) => _cookieManager = cookieManager;

        protected override void Migrate()
        {
            _cookieManager.ExpireCookie(Constants.Web.AngularCookieName);
            _cookieManager.ExpireCookie(Constants.Web.CsrfValidationCookieName);
        }
    }
}
