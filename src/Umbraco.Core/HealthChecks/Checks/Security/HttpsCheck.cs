// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Security;

/// <summary>
///     Health checks for the recommended production setup regarding HTTPS.
/// </summary>
[HealthCheck(
    "EB66BB3B-1BCD-4314-9531-9DA2C1D6D9A7",
    "HTTPS Configuration",
    Description = "Checks if your site is configured to work over HTTPS and if the Umbraco related configuration for that is correct.",
    Group = "Security")]
public class HttpsCheck : HealthCheck
{
    private const int NumberOfDaysForExpiryWarning = 14;
    private const string HttpPropertyKeyCertificateDaysToExpiry = "CertificateDaysToExpiry";

    private static HttpClient? _httpClient;
    private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;

    private readonly ILocalizedTextService _textService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpsCheck" /> class.
    /// </summary>
    /// <param name="textService">The text service.</param>
    /// <param name="globalSettings">The global settings.</param>
    /// <param name="hostingEnvironment">The hosting environment.</param>
    public HttpsCheck(
        ILocalizedTextService textService,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IHostingEnvironment hostingEnvironment)
    {
        _textService = textService;
        _globalSettings = globalSettings;
        _hostingEnvironment = hostingEnvironment;
    }

    private static HttpClient _httpClientEnsureInitialized => _httpClient ??= new HttpClient(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = ServerCertificateCustomValidation,
    });

    /// <inheritdoc />
    public override async Task<IEnumerable<HealthCheckStatus>> GetStatus() =>
        await Task.WhenAll(
            CheckIfCurrentSchemeIsHttps(),
            CheckHttpsConfigurationSetting(),
            CheckForValidCertificate());

    /// <inheritdoc />
    public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        => throw new InvalidOperationException(
            "HttpsCheck action requested is either not executable or does not exist");

    private static bool ServerCertificateCustomValidation(
        HttpRequestMessage requestMessage,
        X509Certificate2? certificate,
        X509Chain? chain,
        SslPolicyErrors sslErrors)
    {
        if (certificate is not null)
        {
            requestMessage.Properties[HttpPropertyKeyCertificateDaysToExpiry] =
                (int)Math.Floor((certificate.NotAfter - DateTime.Now).TotalDays);
        }

        return sslErrors == SslPolicyErrors.None;
    }

    private async Task<HealthCheckStatus> CheckForValidCertificate()
    {
        string message;
        StatusResultType result;

        // Attempt to access the site over HTTPS to see if it HTTPS is supported and a valid certificate has been configured
        var urlBuilder = new UriBuilder(_hostingEnvironment.ApplicationMainUrl) { Scheme = Uri.UriSchemeHttps };
        Uri url = urlBuilder.Uri;

        var request = new HttpRequestMessage(HttpMethod.Head, url);

        try
        {
            using HttpResponseMessage response = await _httpClientEnsureInitialized.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                // Got a valid response, check now if the certificate is expiring within the specified amount of days
                int? daysToExpiry = 0;
                if (request.Properties.TryGetValue(
                    HttpPropertyKeyCertificateDaysToExpiry,
                    out var certificateDaysToExpiry))
                {
                    daysToExpiry = (int?)certificateDaysToExpiry;
                }

                if (daysToExpiry <= 0)
                {
                    result = StatusResultType.Error;
                    message = _textService.Localize("healthcheck", "httpsCheckExpiredCertificate");
                }
                else if (daysToExpiry < NumberOfDaysForExpiryWarning)
                {
                    result = StatusResultType.Warning;
                    message = _textService.Localize("healthcheck", "httpsCheckExpiringCertificate", new[] { daysToExpiry.ToString() });
                }
                else
                {
                    result = StatusResultType.Success;
                    message = _textService.Localize("healthcheck", "httpsCheckValidCertificate");
                }
            }
            else
            {
                result = StatusResultType.Error;
                message = _textService.Localize("healthcheck", "healthCheckInvalidUrl", new[] { url.AbsoluteUri, response.ReasonPhrase });
            }
        }
        catch (Exception ex)
        {
            if (ex is WebException exception)
            {
                message = exception.Status == WebExceptionStatus.TrustFailure
                    ? _textService.Localize("healthcheck", "httpsCheckInvalidCertificate", new[] { exception.Message })
                    : _textService.Localize("healthcheck", "healthCheckInvalidUrl", new[] { url.AbsoluteUri, exception.Message });
            }
            else
            {
                message = _textService.Localize("healthcheck", "healthCheckInvalidUrl", new[] { url.AbsoluteUri, ex.Message });
            }

            result = StatusResultType.Error;
        }

        return new HealthCheckStatus(message)
        {
            ResultType = result,
            ReadMoreLink = result == StatusResultType.Success
                ? null
                : Constants.HealthChecks.DocumentationLinks.Security.HttpsCheck.CheckIfCurrentSchemeIsHttps,
        };
    }

    private Task<HealthCheckStatus> CheckIfCurrentSchemeIsHttps()
    {
        Uri uri = _hostingEnvironment.ApplicationMainUrl;
        var success = uri.Scheme == Uri.UriSchemeHttps;

        return Task.FromResult(
            new HealthCheckStatus(_textService.Localize("healthcheck", "httpsCheckIsCurrentSchemeHttps", new[] { success ? string.Empty : "not" }))
            {
                ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                ReadMoreLink = success
                    ? null
                    : Constants.HealthChecks.DocumentationLinks.Security.HttpsCheck.CheckIfCurrentSchemeIsHttps,
            });
    }

    private Task<HealthCheckStatus> CheckHttpsConfigurationSetting()
    {
        var httpsSettingEnabled = _globalSettings.CurrentValue.UseHttps;
        Uri uri = _hostingEnvironment.ApplicationMainUrl;

        string resultMessage;
        StatusResultType resultType;
        if (uri.Scheme != Uri.UriSchemeHttps)
        {
            resultMessage = _textService.Localize("healthcheck", "httpsCheckConfigurationRectifyNotPossible");
            resultType = StatusResultType.Info;
        }
        else
        {
            resultMessage = _textService.Localize("healthcheck", "httpsCheckConfigurationCheckResult", new[] { httpsSettingEnabled.ToString(), httpsSettingEnabled ? string.Empty : "not" });
            resultType = httpsSettingEnabled ? StatusResultType.Success : StatusResultType.Error;
        }

        return Task.FromResult(new HealthCheckStatus(resultMessage)
        {
            ResultType = resultType,
            ReadMoreLink = resultType == StatusResultType.Success
                ? null
                : Constants.HealthChecks.DocumentationLinks.Security.HttpsCheck.CheckHttpsConfigurationSetting,
        });
    }
}
