using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    [HealthCheck(
        "ED0D7E40-971E-4BE8-AB6D-8CC5D0A6A5B0",
        "Click-Jacking Protection",
        Description = "Checks if your site allowed to be IFRAMed by another site and thus would be susceptible to click-jacking.",
        Group = "Security")]
    public class ClickJackingCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public ClickJackingCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        /// <summary>
        /// Get the status for this health check
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[] { CheckForFrameOptionsHeader() };
        }

        /// <summary>
        /// Executes the action and returns it's status
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "checkForFrameOptionsHeader":
                    return CheckForFrameOptionsHeader();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckForFrameOptionsHeader()
        {
            var message = string.Empty;
            var success = false;
            var url = HealthCheckContext.HttpContext.Request.Url;

            // Access the site home page and check for the click-jack protection header
            using (var webClient = new WebClient())
            {
                var address = string.Format("http://{0}:{1}", url.Host.ToLower(), url.Port);
                try
                {
                    webClient.DownloadString(address);
                    success = webClient.ResponseHeaders.AllKeys.Contains("X-Frame-Options");
                    message = success
                        ? _textService.Localize("healthcheck/clickJackingCheckHeaderFound")
                        : _textService.Localize("healthcheck/clickJackingCheckHeaderNotFound");
                }
                catch (Exception ex)
                {
                    message = _textService.Localize("healthcheck/httpsCheckInvalidUrl", new[] { address, ex.Message });
                }
            }

            var actions = new List<HealthCheckAction>();

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }
   }
}