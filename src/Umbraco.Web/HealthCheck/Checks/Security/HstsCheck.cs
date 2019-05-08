using Umbraco.Core;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    [HealthCheck(
        "E2048C48-21C5-4BE1-A80B-8062162DF124",
        "Cookie hijacking and protocol downgrade attacks Protection (Strict-Transport-Security Header (HSTS))",
        Description = "Checks if your site, when running with HTTPS, contains the Strict-Transport-Security Header (HSTS). If not, it adds with a default of 100 days.",
        Group = "Security")]
    public class HstsCheck : BaseHttpHeaderCheck
    {
        // The check is mostly based on the instructions in the OWASP CheatSheet
        // (https://github.com/OWASP/CheatSheetSeries/blob/master/cheatsheets/HTTP_Strict_Transport_Security_Cheat_Sheet.md)
        // and the blog post of Troy Hunt (https://www.troyhunt.com/understanding-http-strict-transport/)
        // If you want do to it perfectly, you have to submit it https://hstspreload.org/,
        // but then you should include subdomains and I wouldn't suggest to do that for Umbraco-sites.
        public HstsCheck(IRuntimeState runtime, ILocalizedTextService textService)
            : base(runtime, textService, "Strict-Transport-Security", "max-age=10886400", "hSTS", true)
        {
        }
    }
}
