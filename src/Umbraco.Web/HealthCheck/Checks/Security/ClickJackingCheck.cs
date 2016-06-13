using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    using System.Text.RegularExpressions;

    [HealthCheck(
        "ED0D7E40-971E-4BE8-AB6D-8CC5D0A6A5B0",
        "Click-Jacking Protection",
        Description = "Checks if your site is allowed to be IFRAMed by another site and thus would be susceptible to click-jacking.",
        Group = "Security")]
    public class ClickJackingCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        private const string XFrameOptionsHeader = "X-Frame-Options";

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
            throw new InvalidOperationException("ClickJackingCheck has no executable actions");
        }

        private HealthCheckStatus CheckForFrameOptionsHeader()
        {
            var message = string.Empty;
            var success = false;
            var url = HealthCheckContext.HttpContext.Request.Url;

            // Access the site home page and check for the click-jack protection header or meta tag
            var address = string.Format("http://{0}:{1}", url.Host.ToLower(), url.Port);
            var request = WebRequest.Create(address);
            request.Method = "GET";
            try
            {
                var response = request.GetResponse();

                // Check first for header
                success = DoHeadersContainFrameOptions(response);

                // If not found, check for meta-tag
                if (success == false)
                {
                    success = DoMetaTagsContainFrameOptions(response);
                }

                message = success
                    ? _textService.Localize("healthcheck/clickJackingCheckHeaderFound")
                    : _textService.Localize("healthcheck/clickJackingCheckHeaderNotFound");
            }
            catch (Exception ex)
            {
                message = _textService.Localize("healthcheck/httpsCheckInvalidUrl", new[] { address, ex.Message });
            }

            var actions = new List<HealthCheckAction>();

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private static bool DoHeadersContainFrameOptions(WebResponse response)
        {
            return response.Headers.AllKeys.Contains(XFrameOptionsHeader);
        }

        private static bool DoMetaTagsContainFrameOptions(WebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                if (stream == null) return false;
                using (var reader = new StreamReader(stream))
                {
                    var html = reader.ReadToEnd();
                    var metaTags = ParseMetaTags(html);
                    return metaTags.ContainsKey(XFrameOptionsHeader);
                }
            }
        }

        private static Dictionary<string, string> ParseMetaTags(string html)
        {
            var regex = new Regex("<meta http-equiv=\"(.+?)\" content=\"(.+?)\">");

            return regex.Matches(html)
                .Cast<Match>()
                .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);
        }
    }
}