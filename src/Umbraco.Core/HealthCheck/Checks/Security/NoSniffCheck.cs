﻿using Microsoft.Extensions.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.HealthCheck;

namespace Umbraco.Core.HealthCheck.Checks.Security
{
    [HealthCheck(
        "1CF27DB3-EFC0-41D7-A1BB-EA912064E071",
        "Content/MIME Sniffing Protection",
        Description = "Checks that your site contains a header used to protect against MIME sniffing vulnerabilities.",
        Group = "Security")]
    public class NoSniffCheck : BaseHttpHeaderCheck
    {
        public NoSniffCheck(IConfiguration configuration, IRequestAccessor requestAccessor, ILocalizedTextService textService)
            : base(configuration, requestAccessor, textService, "X-Content-Type-Options", "nosniff", "noSniff", false)
        {
        }
    }
}
