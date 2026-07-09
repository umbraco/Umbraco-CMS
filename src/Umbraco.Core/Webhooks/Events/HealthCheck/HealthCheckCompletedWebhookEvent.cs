using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when health checks are completed.
/// </summary>
[WebhookEvent("Health Check Completed")]
public class HealthCheckCompletedWebhookEvent : WebhookEventBase<HealthCheckCompletedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckCompletedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public HealthCheckCompletedWebhookEvent(IWebhookFiringService webhookFiringService, IWebhookService webhookService, IOptionsMonitor<WebhookSettings> webhookSettings, IServerRoleAccessor serverRoleAccessor) : base(webhookFiringService, webhookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.HealthCheckCompleted;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(HealthCheckCompletedNotification notification) =>
        new
        {
            notification.HealthCheckResults.AllChecksSuccessful,
            Results = notification.HealthCheckResults.ResultsAsDictionary.Select(result => new
            {
                result.Key,
                Statusus = result.Value.Select(x => new
                {
                    ResultType = x.ResultType.ToString(),
                    x.Message,
                }),
            }),
        };
}
