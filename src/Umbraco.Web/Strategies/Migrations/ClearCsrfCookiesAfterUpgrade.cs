using System.Web;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Strategies.Migrations
{
    /// <summary>
    /// After upgrade we clear out the csrf tokens
    /// </summary>
    [Weight(-100)]
    public class ClearCsrfCookiesAfterUpgrade : MigrationStartupHander
    {
        protected override void AfterMigration(MigrationRunner sender, MigrationEventArgs e)
        {
            if (e.ProductName != Constants.System.UmbracoMigrationName) return;

            if (HttpContext.Current == null) return;

            var http = new HttpContextWrapper(HttpContext.Current);

            http.ExpireCookie(AngularAntiForgeryHelper.AngularCookieName);
            http.ExpireCookie(AngularAntiForgeryHelper.CsrfValidationCookieName);
        }
    }
}
