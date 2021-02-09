// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Security
{
    /// <summary>
    /// Health check for the recommended production setup regarding unnecessary headers.
    /// </summary>
    [HealthCheck(
        "92ABBAA2-0586-4089-8AE2-9A843439D577",
        "Excessive Headers",
        Description = "Checks to see if your site is revealing information in its headers that gives away unnecessary details about the technology used to build and host it.",
        Group = "Security")]
    public class ExcessiveHeadersCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private static HttpClient s_httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcessiveHeadersCheck"/> class.
        /// </summary>
        public ExcessiveHeadersCheck(ILocalizedTextService textService, IHostingEnvironment hostingEnvironment)
        {
            _textService = textService;
            _hostingEnvironment = hostingEnvironment;
        }

        private static HttpClient HttpClient => s_httpClient ??= new HttpClient();

        /// <summary>
        /// Get the status for this health check
        /// </summary>
        public override async Task<IEnumerable<HealthCheckStatus>> GetStatus() =>
            await Task.WhenAll(CheckForHeaders());

        /// <summary>
        /// Executes the action and returns it's status
        /// </summary>
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
            => throw new InvalidOperationException("ExcessiveHeadersCheck has no executable actions");

        private async Task<HealthCheckStatus> CheckForHeaders()
        {
            string message;
            var success = false;
            Uri url = _hostingEnvironment.ApplicationMainUrl;

            // Access the site home page and check for the headers
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            try
            {
                using HttpResponseMessage response = await HttpClient.SendAsync(request);

                IEnumerable<string> allHeaders = response.Headers.Select(x => x.Key);
                var headersToCheckFor = new[] { "Server", "X-Powered-By", "X-AspNet-Version", "X-AspNetMvc-Version" };
                var headersFound = allHeaders
                    .Intersect(headersToCheckFor)
                    .ToArray();
                success = headersFound.Any() == false;
                message = success
                    ? _textService.Localize("healthcheck/excessiveHeadersNotFound")
                    : _textService.Localize("healthcheck/excessiveHeadersFound", new[] { string.Join(", ", headersFound) });
            }
            catch (Exception ex)
            {
                message = _textService.Localize("healthcheck/httpsCheckInvalidUrl", new[] { url.ToString(), ex.Message });
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Warning,
                    ReadMoreLink = success ? null : Constants.HealthChecks.DocumentationLinks.Security.ExcessiveHeadersCheck,
                };
        }
   }
}
