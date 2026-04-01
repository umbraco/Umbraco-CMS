// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Web.Common.AspNetCore;

/// <summary>
///     Logs the application URL configuration status at startup, providing guidance
///     when the URL is not configured and auto-detection is disabled.
/// </summary>
internal sealed class ApplicationUrlConfigurationNotificationHandler
    : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private readonly ILogger<ApplicationUrlConfigurationNotificationHandler> _logger;
    private readonly IOptionsMonitor<WebRoutingSettings> _webRoutingSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApplicationUrlConfigurationNotificationHandler" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="webRoutingSettings">The web routing settings monitor.</param>
    public ApplicationUrlConfigurationNotificationHandler(
        ILogger<ApplicationUrlConfigurationNotificationHandler> logger,
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings)
    {
        _logger = logger;
        _webRoutingSettings = webRoutingSettings;
    }

    /// <inheritdoc/>
    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        WebRoutingSettings settings = _webRoutingSettings.CurrentValue;

        if (settings.UmbracoApplicationUrl is not null)
        {
            _logger.LogInformation(
                "Application URL configured as {ApplicationMainUrl}.",
                settings.UmbracoApplicationUrl);
            return;
        }

        if (settings.ApplicationUrlDetection == ApplicationUrlDetection.None)
        {
            _logger.LogWarning(
                "Application URL auto-detection is disabled and no explicit URL is configured. "
                + "Email links (invitations, password resets) will not be available. "
                + "Set Umbraco:CMS:WebRouting:UmbracoApplicationUrl in configuration, "
                + "or change Umbraco:CMS:WebRouting:ApplicationUrlDetection to 'FirstRequest' or 'EveryRequest'.");
            return;
        }

        _logger.LogInformation(
            "Application URL auto-detection is enabled ({DetectionMode}). ",
            settings.ApplicationUrlDetection);
    }
}
