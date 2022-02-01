using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    [HealthCheck(
        "92ABBAA2-0586-4089-8AE2-9A843439D577",
        "Excessive Headers",
        Description = "Checks to see if your site is revealing information in its headers that gives away unnecessary details about the technology used to build and host it.",
        Group = "Security")]
    public class ExcessiveHeadersCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        private readonly IRuntimeState _runtime;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExcessiveHeadersCheck(ILocalizedTextService textService, IRuntimeState runtime, IHttpContextAccessor httpContextAccessor)
        {
            _textService = textService;
            _runtime = runtime;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get the status for this health check
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[] { CheckForHeaders() };
        }

        /// <summary>
        /// Executes the action and returns it's status
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new InvalidOperationException("ExcessiveHeadersCheck has no executable actions");
        }

        private HealthCheckStatus CheckForHeaders()
        {
            var message = string.Empty;
            var success = false;
            var url = _runtime.ApplicationUrl.GetLeftPart(UriPartial.Authority);

            // Access the site home page and check for the headers
            var request = WebRequest.Create(url);
            request.Method = "HEAD";
            try
            {
                var response = request.GetResponse();
                var allHeaders = response.Headers.AllKeys;

                var headersToCheckFor = new List<string> {"Server", "X-Powered-By", "X-AspNet-Version", "X-AspNetMvc-Version" };

                // Ignore if server header is present and it's set to cloudflare
                if (allHeaders.InvariantContains("Server") && response.Headers["Server"].InvariantEquals("cloudflare"))
                {
                    headersToCheckFor.Remove("Server");
                }

                var headersFound = allHeaders
                    .Intersect(headersToCheckFor)
                    .ToArray();
                success = headersFound.Any() == false;
                message = success
                    ? _textService.Localize("healthcheck", "excessiveHeadersNotFound")
                    : _textService.Localize("healthcheck", "excessiveHeadersFound", new [] { string.Join(", ", headersFound) });
            }
            catch (Exception ex)
            {
                message = _textService.Localize("healthcheck", "healthCheckInvalidUrl", new[] { url.ToString(), ex.Message });
            }

            var actions = new List<HealthCheckAction>();
            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Warning,
                    Actions = actions
                };
        }
   }
}
