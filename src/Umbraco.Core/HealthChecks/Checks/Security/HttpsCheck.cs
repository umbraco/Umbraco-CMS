// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
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
    /// <inheritdoc />
    public override async Task<IEnumerable<HealthCheckStatus>> GetStatusAsync()
        => [
            await CheckIfCurrentSchemeIsHttps(),
            await CheckHttpsConfigurationSetting(),
            await CheckForValidCertificate()
        ];

    /// <inheritdoc />
    public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        => throw new InvalidOperationException("HttpsCheck action requested is either not executable or does not exist");

    /// <summary>
    ///     Custom certificate validation callback that stores the certificate expiry information.
    /// </summary>
    private static bool ServerCertificateCustomValidation(
        HttpRequestMessage requestMessage,
        X509Certificate2? certificate,
        X509Chain? chain,
        SslPolicyErrors sslErrors)
    {
        if (certificate is not null)
        {
            requestMessage.Options.Set(new HttpRequestOptionsKey<int?>(HttpPropertyKeyCertificateDaysToExpiry), (int?)Math.Floor((certificate.NotAfter - DateTime.Now).TotalDays));
        }

        return sslErrors == SslPolicyErrors.None;
    }

    private bool TryGetApplicationUrl(
        [NotNullWhen(true)] out Uri? applicationUrl,
        [NotNullWhen(false)] out HealthCheckStatus? unavailableStatus)
    {
        applicationUrl = _hostingEnvironment.ApplicationMainUrl;
        if (applicationUrl is not null)
        {
            unavailableStatus = null;
            return true;
        }

        unavailableStatus = new HealthCheckStatus(
            _textService.Localize("healthcheck", "httpsCheckNoApplicationUrl"))
        {
            ResultType = StatusResultType.Info,
        };
        return false;
    }

    /// <summary>
    ///     Checks that the site's HTTPS certificate is valid and not nearing expiry by making a HEAD request to the application URL over HTTPS.
    /// </summary>
    /// <remarks>
    ///     Exposed as <c>internal</c> (rather than <c>private</c>) solely to enable direct unit testing of the individual
    ///     check in isolation; calling <see cref="GetStatusAsync" /> would additionally execute the other checks, and with a
    ///     valid application URL configured that would mean a real outbound HTTPS request on every test run.
    /// </remarks>
    internal async Task<HealthCheckStatus> CheckForValidCertificate()
    {
        if (TryGetApplicationUrl(out Uri? applicationUrl, out HealthCheckStatus? unavailable) is false)
        {
            return unavailable;
        }

        string message;
        StatusResultType result;

        // Attempt to access the site over HTTPS to see if it HTTPS is supported and a valid certificate has been configured
        var urlBuilder = new UriBuilder(applicationUrl) { Scheme = Uri.UriSchemeHttps };
        Uri url = urlBuilder.Uri;

        using var request = new HttpRequestMessage(HttpMethod.Head, url);

        try
        {
            using var httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = ServerCertificateCustomValidation,
            });

            using HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                // Got a valid response, check now if the certificate is expiring within the specified amount of days
                int? daysToExpiry = 0;
                if (response.RequestMessage != null && response.RequestMessage.Options.TryGetValue(
                        new HttpRequestOptionsKey<int?>(HttpPropertyKeyCertificateDaysToExpiry),
                        out var certificateDaysToExpiry))
                {
                    daysToExpiry = certificateDaysToExpiry;
                }

                if (daysToExpiry <= 0)
                {
                    result = StatusResultType.Error;
                    message = _textService.Localize("healthcheck", "httpsCheckExpiredCertificate");
                }
                else if (daysToExpiry < NumberOfDaysForExpiryWarning)
                {
                    result = StatusResultType.Warning;
                    message = _textService.Localize("healthcheck", "httpsCheckExpiringCertificate", [daysToExpiry.ToString()]);
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
                message = _textService.Localize("healthcheck", "healthCheckInvalidUrl", [url.AbsoluteUri, response.ReasonPhrase]);
            }
        }
        catch (Exception ex)
        {
            if (ex is WebException exception)
            {
                message = exception.Status == WebExceptionStatus.TrustFailure
                    ? _textService.Localize("healthcheck", "httpsCheckInvalidCertificate", [exception.Message])
                    : _textService.Localize("healthcheck", "healthCheckInvalidUrl", [url.AbsoluteUri, exception.Message]);
            }
            else
            {
                message = _textService.Localize("healthcheck", "healthCheckInvalidUrl", [url.AbsoluteUri, ex.Message]);
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

    /// <summary>
    ///     Checks whether the application URL's scheme is HTTPS.
    /// </summary>
    /// <remarks>
    ///     Exposed as <c>internal</c> (rather than <c>private</c>) solely to enable direct unit testing of the individual
    ///     check in isolation, so that tests can cover the scheme branches without also running <see cref="CheckForValidCertificate" />
    ///     (which would make a real outbound HTTPS request).
    /// </remarks>
    internal Task<HealthCheckStatus> CheckIfCurrentSchemeIsHttps()
    {
        if (TryGetApplicationUrl(out Uri? applicationUrl, out HealthCheckStatus? unavailable) is false)
        {
            return Task.FromResult(unavailable);
        }

        var success = applicationUrl.Scheme == Uri.UriSchemeHttps;

        return Task.FromResult(
            new HealthCheckStatus(_textService.Localize("healthcheck", "httpsCheckIsCurrentSchemeHttps", [success ? string.Empty : "not"]))
            {
                ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                ReadMoreLink = success
                    ? null
                    : Constants.HealthChecks.DocumentationLinks.Security.HttpsCheck.CheckIfCurrentSchemeIsHttps,
            });
    }

    /// <summary>
    ///     Checks whether the <see cref="GlobalSettings.UseHttps" /> configuration setting agrees with the scheme of the application URL.
    /// </summary>
    /// <remarks>
    ///     Exposed as <c>internal</c> (rather than <c>private</c>) solely to enable direct unit testing of the individual
    ///     check in isolation, so that tests can cover the configuration branches without also running <see cref="CheckForValidCertificate" />
    ///     (which would make a real outbound HTTPS request).
    /// </remarks>
    internal Task<HealthCheckStatus> CheckHttpsConfigurationSetting()
    {
        if (TryGetApplicationUrl(out Uri? applicationUrl, out HealthCheckStatus? unavailable) is false)
        {
            return Task.FromResult(unavailable);
        }

        var httpsSettingEnabled = _globalSettings.CurrentValue.UseHttps;

        string resultMessage;
        StatusResultType resultType;
        if (applicationUrl.Scheme != Uri.UriSchemeHttps)
        {
            resultMessage = _textService.Localize("healthcheck", "httpsCheckConfigurationRectifyNotPossible");
            resultType = StatusResultType.Info;
        }
        else
        {
            resultMessage = _textService.Localize("healthcheck", "httpsCheckConfigurationCheckResult", [httpsSettingEnabled.ToString(), httpsSettingEnabled ? string.Empty : "not"]);
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
