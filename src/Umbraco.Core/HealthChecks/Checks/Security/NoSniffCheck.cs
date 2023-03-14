// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Security;

/// <summary>
///     Health check for the recommended production setup regarding the X-Content-Type-Options header.
/// </summary>
[HealthCheck(
    "1CF27DB3-EFC0-41D7-A1BB-EA912064E071",
    "Content/MIME Sniffing Protection",
    Description = "Checks that your site contains a header used to protect against MIME sniffing vulnerabilities.",
    Group = "Security")]
public class NoSniffCheck : BaseHttpHeaderCheck
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NoSniffCheck" /> class.
    /// </summary>
    public NoSniffCheck(IHostingEnvironment hostingEnvironment, ILocalizedTextService textService)
        : base(hostingEnvironment, textService, "X-Content-Type-Options", "noSniff", false)
    {
    }

    /// <inheritdoc />
    protected override string ReadMoreLink => Constants.HealthChecks.DocumentationLinks.Security.NoSniffCheck;
}
