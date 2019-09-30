using System;
using System.Collections.Generic;
using System.Net;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Services
{
    [HealthCheck("6D7D4E11-758E-4697-BA72-7E02A530CBD4",
        "HTTPS endpoints (TLS 1.2)",
        Description = "Checks whether the TLS 1.2 security protocol can be used when making outbound connections to HTTPS endpoints.",
        Group = "Services")]
    public class Tls12Check : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public Tls12Check(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckTls() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new InvalidOperationException("This check has no executable actions.");
        }

        public HealthCheckStatus CheckTls()
        {
            // When set to 0 (SystemDefault), allows the operating system to choose the best protocol to use, and to block protocols that are not secure (this allows TLS 1.2 by default).
            var success = ServicePointManager.SecurityProtocol == 0 ||
                ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls12);

            var message = success
                ? _textService.Localize("healthcheck/tls12HealthCheckSuccess")
                : _textService.Localize("healthcheck/tls12HealthCheckWarn");

            var actions = new List<HealthCheckAction>();

            return new HealthCheckStatus(message)
            {
                ResultType = success
                    ? StatusResultType.Success
                    : StatusResultType.Warning,
                Actions = actions
            };
        }
    }
}
