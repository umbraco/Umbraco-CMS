// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Security;

/// <summary>
///     Health check for the recommended production setup regarding the content-security-policy header.
/// </summary>
[HealthCheck(
    "10BEBF47-C128-4C5E-9680-5059BEAFBBDF",
    "Content Security Policy (CSP)",
    Description = "Checks whether the site contains a Content-Security-Policy (CSP) header.",
    Group = "Security")]
public class CspCheck : BaseHttpHeaderCheck
{
    private const string LocalizationPrefix = "contentSecurityPolicy";

    /// <summary>
    ///     Initializes a new instance of the <see cref="CspCheck" /> class.
    /// </summary>
    public CspCheck(IHostingEnvironment hostingEnvironment, ILocalizedTextService textService)
        : base(hostingEnvironment, textService, "Content-Security-Policy", LocalizationPrefix, false, false)
    {
    }

    /// <inheritdoc />
    protected override string ReadMoreLink => Constants.HealthChecks.DocumentationLinks.Security.CspHeaderCheck;
}
