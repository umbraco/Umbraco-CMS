using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Health Check Completed")]
public class HealthCheckCompletedWebhookEvent : WebhookEventBase<HealthCheckCompletedNotification>
{
    public HealthCheckCompletedWebhookEvent(IWebhookFiringService webhookFiringService, IWebhookService webhookService, IOptionsMonitor<WebhookSettings> webhookSettings, IServerRoleAccessor serverRoleAccessor) : base(webhookFiringService, webhookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => "healthCheckCompleted";

    public override object? ConvertNotificationToRequestPayload(HealthCheckCompletedNotification notification) => notification.HealthCheckResults;
}
