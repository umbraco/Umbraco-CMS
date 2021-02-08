// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Services;

namespace Umbraco.Core.HealthChecks.Checks.Security
{
    /// <summary>
    /// Health checks for the recommended production setup regarding https.
    /// </summary>
    [HealthCheck(
        "EB66BB3B-1BCD-4314-9531-9DA2C1D6D9A7",
        "HTTPS Configuration",
        Description = "Checks if your site is configured to work over HTTPS and if the Umbraco related configuration for that is correct.",
        Group = "Security")]
    public class HttpsCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;

        private static HttpClient s_httpClient;
        private static HttpClientHandler s_httpClientHandler;
        private static int s_certificateDaysToExpiry;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpsCheck"/> class.
        /// </summary>
        public HttpsCheck(
            ILocalizedTextService textService,
            IOptionsMonitor<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment)
        {
            _textService = textService;
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
        }

        private static HttpClient HttpClient => s_httpClient ??= new HttpClient(HttpClientHandler);

        private static HttpClientHandler HttpClientHandler => s_httpClientHandler ??= new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = ServerCertificateCustomValidation
        };

        /// <summary>
        /// Get the status for this health check
        /// </summary>
        public override async Task<IEnumerable<HealthCheckStatus>> GetStatus() =>
            await Task.WhenAll(
                CheckIfCurrentSchemeIsHttps(),
                CheckHttpsConfigurationSetting(),
                CheckForValidCertificate());

        /// <summary>
        /// Executes the action and returns it's status
        /// </summary>
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
            => throw new InvalidOperationException("HttpsCheck action requested is either not executable or does not exist");

        private static bool ServerCertificateCustomValidation(HttpRequestMessage requestMessage, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslErrors)
        {
            if (!(certificate is null) && s_certificateDaysToExpiry == default)
            {
                s_certificateDaysToExpiry = (int)Math.Floor((certificate.NotAfter - DateTime.Now).TotalDays);
            }

            return sslErrors == SslPolicyErrors.None;
        }

        private async Task<HealthCheckStatus> CheckForValidCertificate()
        {
            string message;
            StatusResultType result;

            // Attempt to access the site over HTTPS to see if it HTTPS is supported
            // and a valid certificate has been configured
            var url = _hostingEnvironment.ApplicationMainUrl.ToString().Replace("http:", "https:");

            var request = new HttpRequestMessage(HttpMethod.Head, url);

            try
            {
                using HttpResponseMessage response = await HttpClient.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Got a valid response, check now for if certificate expiring within 14 days
                    // Hat-tip: https://stackoverflow.com/a/15343898/489433
                    const int numberOfDaysForExpiryWarning = 14;

                    if (s_certificateDaysToExpiry <= 0)
                    {
                        result = StatusResultType.Error;
                        message = _textService.Localize("healthcheck/httpsCheckExpiredCertificate");
                    }
                    else if (s_certificateDaysToExpiry < numberOfDaysForExpiryWarning)
                    {
                        result = StatusResultType.Warning;
                        message = _textService.Localize("healthcheck/httpsCheckExpiringCertificate", new[] { s_certificateDaysToExpiry.ToString() });
                    }
                    else
                    {
                        result = StatusResultType.Success;
                        message = _textService.Localize("healthcheck/httpsCheckValidCertificate");
                    }
                }
                else
                {
                    result = StatusResultType.Error;
                    message = _textService.Localize("healthcheck/healthCheckInvalidUrl", new[] { url, response.ReasonPhrase });
                }
            }
            catch (Exception ex)
            {
                var exception = ex as WebException;
                if (exception != null)
                {
                    message = exception.Status == WebExceptionStatus.TrustFailure
                        ? _textService.Localize("healthcheck/httpsCheckInvalidCertificate", new[] { exception.Message })
                        : _textService.Localize("healthcheck/healthCheckInvalidUrl", new[] { url, exception.Message });
                }
                else
                {
                    message = _textService.Localize("healthcheck/healthCheckInvalidUrl", new[] { url, ex.Message });
                }

                result = StatusResultType.Error;
            }

            return new HealthCheckStatus(message)
            {
                ResultType = result,
                ReadMoreLink = result == StatusResultType.Success
                    ? null
                    : Constants.HealthChecks.DocumentationLinks.Security.HttpsCheck.CheckIfCurrentSchemeIsHttps
            };
        }

        private Task<HealthCheckStatus> CheckIfCurrentSchemeIsHttps()
        {
            Uri uri = _hostingEnvironment.ApplicationMainUrl;
            var success = uri.Scheme == "https";

            return Task.FromResult(new HealthCheckStatus(_textService.Localize("healthcheck/httpsCheckIsCurrentSchemeHttps", new[] { success ? string.Empty : "not" }))
            {
                ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                ReadMoreLink = success ? null : Constants.HealthChecks.DocumentationLinks.Security.HttpsCheck.CheckIfCurrentSchemeIsHttps
            });
        }

        private Task<HealthCheckStatus> CheckHttpsConfigurationSetting()
        {
            bool httpsSettingEnabled = _globalSettings.CurrentValue.UseHttps;
            Uri uri = _hostingEnvironment.ApplicationMainUrl;

            string resultMessage;
            StatusResultType resultType;
            if (uri.Scheme != "https")
            {
                resultMessage = _textService.Localize("healthcheck/httpsCheckConfigurationRectifyNotPossible");
                resultType = StatusResultType.Info;
            }
            else
            {
                resultMessage = _textService.Localize(
                    "healthcheck/httpsCheckConfigurationCheckResult",
                    new[] { httpsSettingEnabled.ToString(), httpsSettingEnabled ? string.Empty : "not" });
                resultType = httpsSettingEnabled ? StatusResultType.Success : StatusResultType.Error;
            }

            return Task.FromResult(new HealthCheckStatus(resultMessage)
            {
                ResultType = resultType,
                ReadMoreLink = resultType == StatusResultType.Success
                    ? null
                    : Constants.HealthChecks.DocumentationLinks.Security.HttpsCheck.CheckHttpsConfigurationSetting
            });
        }
    }
}
