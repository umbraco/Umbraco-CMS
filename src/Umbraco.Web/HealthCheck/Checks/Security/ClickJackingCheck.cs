using Umbraco.Core;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    [HealthCheck(
        "ED0D7E40-971E-4BE8-AB6D-8CC5D0A6A5B0",
        "Click-Jacking Protection",
        Description = "Checks if your site is allowed to be IFRAMEd by another site and thus would be susceptible to click-jacking.",
        Group = "Security")]
    public class ClickJackingCheck : BaseHttpHeaderCheck
    {
        public ClickJackingCheck(IRuntimeState runtime, ILocalizedTextService textService)
            : base(runtime, textService, "X-Frame-Options", "sameorigin", "clickJacking", true)
        {
        }
    }
}
