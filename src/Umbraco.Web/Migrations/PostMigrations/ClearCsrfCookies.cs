using System.Web;
using Umbraco.Core.Migrations;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Migrations.PostMigrations
{
    /// <summary>
    /// Clears Csrf tokens.
    /// </summary>
    public class ClearCsrfCookies : IMigration
    {
        public void Migrate()
        {
            if (HttpContext.Current == null) return;

            var http = new HttpContextWrapper(HttpContext.Current);
            http.ExpireCookie(AngularAntiForgeryHelper.AngularCookieName);
            http.ExpireCookie(AngularAntiForgeryHelper.CsrfValidationCookieName);
        }
    }
}
