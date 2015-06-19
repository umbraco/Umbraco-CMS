using System.Web;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Web.WebApi.Filters;
using umbraco.interfaces;

namespace Umbraco.Web.Strategies.Migrations
{
    /// <summary>
    /// After upgrade we clear out the csrf tokens
    /// </summary>
    public class ClearCsrfCookiesAfterUpgrade : MigrationStartupHander
    {
        protected override void AfterMigration(MigrationRunner sender, MigrationEventArgs e)
        {
            if (HttpContext.Current == null) return;

            var http = new HttpContextWrapper(HttpContext.Current);

            http.ExpireCookie(AngularAntiForgeryHelper.AngularCookieName);
            http.ExpireCookie(AngularAntiForgeryHelper.CsrfValidationCookieName);
        }
    }
}