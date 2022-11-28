// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Security;

/// <summary>
///     Health check for the recommended production setup regarding the X-XSS-Protection header.
/// </summary>
[HealthCheck(
    "F4D2B02E-28C5-4999-8463-05759FA15C3A",
    "Cross-site scripting Protection (X-XSS-Protection header)",
    Description = "This header enables the Cross-site scripting (XSS) filter in your browser. It checks for the presence of the X-XSS-Protection-header.",
    Group = "Security")]
public class XssProtectionCheck : BaseHttpHeaderCheck
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="XssProtectionCheck" /> class.
    /// </summary>
    /// <remarks>
    ///     The check is mostly based on the instructions in the OWASP CheatSheet
    ///     (https://www.owasp.org/index.php/HTTP_Strict_Transport_Security_Cheat_Sheet)
    ///     and the blog post of Troy Hunt (https://www.troyhunt.com/understanding-http-strict-transport/)
    ///     If you want do to it perfectly, you have to submit it https://hstspreload.appspot.com/,
    ///     but then you should include subdomains and I wouldn't suggest to do that for Umbraco-sites.
    /// </remarks>
    public XssProtectionCheck(IHostingEnvironment hostingEnvironment, ILocalizedTextService textService)
        : base(hostingEnvironment, textService, "X-XSS-Protection", "xssProtection", true)
    {
    }

    /// <inheritdoc />
    protected override string ReadMoreLink => Constants.HealthChecks.DocumentationLinks.Security.XssProtectionCheck;
}
