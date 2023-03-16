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
    Description = "This checks for the presence of the X-XSS-Protection-header.",
    Group = "Security")]
public class XssProtectionCheck : BaseHttpHeaderCheck
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="XssProtectionCheck" /> class.
    /// </summary>
    /// <remarks>
    ///     This check should not find the header in newer browsers as this can cause security vulnerabilities
    ///     https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-XSS-Protection
    /// </remarks>
    public XssProtectionCheck(IHostingEnvironment hostingEnvironment, ILocalizedTextService textService)
        : base(hostingEnvironment, textService, "X-XSS-Protection", "xssProtection", true, true)
    {
    }

    /// <inheritdoc />
    protected override string ReadMoreLink => string.Empty;
}
