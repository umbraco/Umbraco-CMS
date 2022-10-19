// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Security;

/// <summary>
///     Health check for the recommended production setup regarding the Strict-Transport-Security header.
/// </summary>
[HealthCheck(
    "E2048C48-21C5-4BE1-A80B-8062162DF124",
    "Cookie hijacking and protocol downgrade attacks Protection (Strict-Transport-Security Header (HSTS))",
    Description = "Checks if your site, when running with HTTPS, contains the Strict-Transport-Security Header (HSTS).",
    Group = "Security")]
public class HstsCheck : BaseHttpHeaderCheck
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="HstsCheck" /> class.
    /// </summary>
    /// <remarks>
    ///     The check is mostly based on the instructions in the OWASP CheatSheet
    ///     (https://github.com/OWASP/CheatSheetSeries/blob/master/cheatsheets/HTTP_Strict_Transport_Security_Cheat_Sheet.md)
    ///     and the blog post of Troy Hunt (https://www.troyhunt.com/understanding-http-strict-transport/)
    ///     If you want do to it perfectly, you have to submit it https://hstspreload.org/,
    ///     but then you should include subdomains and I wouldn't suggest to do that for Umbraco-sites.
    /// </remarks>
    public HstsCheck(IHostingEnvironment hostingEnvironment, ILocalizedTextService textService)
        : base(hostingEnvironment, textService, "Strict-Transport-Security", "hSTS", true)
    {
    }

    /// <inheritdoc />
    protected override string ReadMoreLink => Constants.HealthChecks.DocumentationLinks.Security.HstsCheck;
}
