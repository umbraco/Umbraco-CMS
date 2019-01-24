using System;
using System.Collections.Generic;
using System.Net;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    [HealthCheck("6D7D4E11-758E-4697-BA72-7E02A530CBD4",
        "TLS Check",
        Description = "Checks that your site is set up to use at least TLS 1.2 for outgoing connections.",
        Group = "Security")]
    public class TlsCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public TlsCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckTls() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new InvalidOperationException("TlsCheck has no executable actions");
        }

        public HealthCheckStatus CheckTls()
        {
            bool success = (int)ServicePointManager.SecurityProtocol >= (int)SecurityProtocolType.Tls12;

            string message = success
                ? _textService.Localize("healthcheck/tlsHealthCheckSuccess")
                : _textService.Localize("healthcheck/tlsHealthCheckWarn");

            var actions = new List<HealthCheckAction>();

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success
                        ? StatusResultType.Success
                        : StatusResultType.Warning,
                    Actions = actions
                };
        }
    }
}
