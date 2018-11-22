namespace Umbraco.Web.HealthCheck.Checks.Security
{
    [HealthCheck(
        "F4D2B02E-28C5-4999-8463-05759FA15C3A",
        "Cross-site scripting Protection (X-XSS-Protection header)",
        Description = "This header enables the Cross-site scripting (XSS) filter in your browser. It checks for the presence of the X-XSS-Protection-header.",
        Group = "Security")]
    public class XssProtectionCheck : BaseHttpHeaderCheck
    {
        // The check is mostly based on the instructions in the OWASP CheatSheet
        // (https://www.owasp.org/index.php/HTTP_Strict_Transport_Security_Cheat_Sheet)
        // and the blogpost of Troy Hunt (https://www.troyhunt.com/understanding-http-strict-transport/)
        // If you want do to it perfectly, you have to submit it https://hstspreload.appspot.com/,
        // but then you should include subdomains and I wouldn't suggest to do that for Umbraco-sites.
        public XssProtectionCheck(HealthCheckContext healthCheckContext)
            : base(healthCheckContext, "X-XSS-Protection", "1; mode=block", "xssProtection", true)
        {
        }
    }
}
