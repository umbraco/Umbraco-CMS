// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Security;

/// <summary>
///     Health check for the Umbraco application URL configuration.
/// </summary>
[HealthCheck(
    "6708CA45-E96E-40B8-A40A-0607C1CA7F28",
    "Application URL Configuration",
    Description = "Checks if the Umbraco application URL is configured for your site.",
    Group = "Security")]
public class UmbracoApplicationUrlCheck : HealthCheck
{
    private readonly ILocalizedTextService _textService;
    private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoApplicationUrlCheck" /> class.
    /// </summary>
    /// <param name="textService">The localized text service.</param>
    /// <param name="webRoutingSettings">The web routing settings monitor.</param>
    public UmbracoApplicationUrlCheck(
        ILocalizedTextService textService,
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings)
    {
        _textService = textService;
        _webRoutingSettings = webRoutingSettings;
    }

    /// <inheritdoc />
    public override HealthCheckStatus ExecuteAction(HealthCheckAction action) =>
        throw new InvalidOperationException("UmbracoApplicationUrlCheck has no executable actions");

    /// <inheritdoc />
    public override Task<IEnumerable<HealthCheckStatus>> GetStatusAsync() =>
        Task.FromResult(CheckUmbracoApplicationUrl().Yield());

    private HealthCheckStatus CheckUmbracoApplicationUrl()
    {
        WebRoutingSettings settings = _webRoutingSettings.CurrentValue;
        var url = settings.UmbracoApplicationUrl;

        string resultMessage;
        StatusResultType resultType;

        if (url.IsNullOrWhiteSpace() is false)
        {
            resultMessage = _textService.Localize("healthcheck", "umbracoApplicationUrlCheckResultTrue", [url]);
            resultType = StatusResultType.Success;
        }
        else if (settings.ApplicationUrlDetection == ApplicationUrlDetection.None)
        {
            // No explicit URL and auto-detection is disabled, so the application URL can never be established.
            // Features that require an absolute URL (e.g. password reset and invitation emails) will not work.
            resultMessage = _textService.Localize("healthcheck", "umbracoApplicationUrlCheckResultError");
            resultType = StatusResultType.Error;
        }
        else
        {
            resultMessage = _textService.Localize("healthcheck", "umbracoApplicationUrlCheckResultFalse");
            resultType = StatusResultType.Warning;
        }

        return new HealthCheckStatus(resultMessage)
        {
            ResultType = resultType,
            ReadMoreLink = resultType == StatusResultType.Success
                ? null
                : Constants.HealthChecks.DocumentationLinks.Security.UmbracoApplicationUrlCheck,
        };
    }
}
