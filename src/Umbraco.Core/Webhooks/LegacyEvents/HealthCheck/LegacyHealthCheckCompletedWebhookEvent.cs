using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Legacy webhook event that fires when a health check is completed, using the legacy payload format.
/// </summary>
[WebhookEvent("Health Check Completed")]
public class LegacyHealthCheckCompletedWebhookEvent : WebhookEventBase<HealthCheckCompletedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyHealthCheckCompletedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public LegacyHealthCheckCompletedWebhookEvent(IWebhookFiringService webhookFiringService, IWebhookService webhookService, IOptionsMonitor<WebhookSettings> webhookSettings, IServerRoleAccessor serverRoleAccessor) : base(webhookFiringService, webhookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.HealthCheckCompleted;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(HealthCheckCompletedNotification notification) =>
        new
        {
            notification.HealthCheckResults.AllChecksSuccessful,
            notification.HealthCheckResults.ResultsAsDictionary
        };
}
