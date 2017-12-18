using System.Web;
using Umbraco.Core.Events;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Migrations;

namespace Umbraco.Web.Strategies.Migrations
{
    /// <summary>
    /// After upgrade we clear out the csrf tokens
    /// </summary>
    public class ClearCsrfCookiesAfterUpgrade : IPostMigration
    {
        public void Migrated(MigrationRunner sender, MigrationEventArgs args)
        {
            if (args.ProductName != Constants.System.UmbracoMigrationName) return;
            if (HttpContext.Current == null) return;

            var http = new HttpContextWrapper(HttpContext.Current);
            http.ExpireCookie(AngularAntiForgeryHelper.AngularCookieName);
            http.ExpireCookie(AngularAntiForgeryHelper.CsrfValidationCookieName);
        }
    }
}
