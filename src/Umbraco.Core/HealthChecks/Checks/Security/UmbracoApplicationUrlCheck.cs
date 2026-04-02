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
        var url = _webRoutingSettings.CurrentValue.UmbracoApplicationUrl;

        string resultMessage;
        StatusResultType resultType;
        var success = false;

        if (url.IsNullOrWhiteSpace())
        {
            resultMessage = _textService.Localize("healthcheck", "umbracoApplicationUrlCheckResultFalse");
            resultType = StatusResultType.Warning;
        }
        else
        {
            resultMessage = _textService.Localize("healthcheck", "umbracoApplicationUrlCheckResultTrue", new[] { url });
            resultType = StatusResultType.Success;
            success = true;
        }

        return new HealthCheckStatus(resultMessage)
        {
            ResultType = resultType,
            ReadMoreLink = success
                ? null
                : Constants.HealthChecks.DocumentationLinks.Security.UmbracoApplicationUrlCheck,
        };
    }
}
