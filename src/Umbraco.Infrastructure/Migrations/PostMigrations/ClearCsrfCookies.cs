using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Infrastructure.Migrations.PostMigrations;

/// <summary>
///     Clears Csrf tokens.
/// </summary>
[Obsolete("Removed in the V13, and replaced with a notification handler")]
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
