// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Security;

[HealthCheck(
    "6708CA45-E96E-40B8-A40A-0607C1CA7F28",
    "Application URL Configuration",
    Description = "Checks if the Umbraco application URL is configured for your site.",
    Group = "Security")]
public class UmbracoApplicationUrlCheck : HealthCheck
{
    private readonly ILocalizedTextService _textService;
    private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;

    public UmbracoApplicationUrlCheck(
        ILocalizedTextService textService,
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings)
    {
        _textService = textService;
        _webRoutingSettings = webRoutingSettings;
    }

    /// <summary>
    ///     Executes the action and returns its status
    /// </summary>
    public override HealthCheckStatus ExecuteAction(HealthCheckAction action) =>
        throw new InvalidOperationException("UmbracoApplicationUrlCheck has no executable actions");

    /// <summary>
    ///     Get the status for this health check
    /// </summary>
    public override Task<IEnumerable<HealthCheckStatus>> GetStatus() =>
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
