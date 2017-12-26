using System.Web;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Scoping;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Migrations
{
    /// <summary>
    /// After upgrade we clear out the csrf tokens
    /// </summary>
    public class ClearCsrfCookiesAfterUpgrade : IPostMigration
    {
        public void Execute(string name, IScope scope, SemVersion originVersion, SemVersion targetVersion, ILogger logger)
        {
            if (name != Constants.System.UmbracoUpgradePlanName) return;
            if (HttpContext.Current == null) return;

            var http = new HttpContextWrapper(HttpContext.Current);
            http.ExpireCookie(AngularAntiForgeryHelper.AngularCookieName);
            http.ExpireCookie(AngularAntiForgeryHelper.CsrfValidationCookieName);
        }
    }
}
