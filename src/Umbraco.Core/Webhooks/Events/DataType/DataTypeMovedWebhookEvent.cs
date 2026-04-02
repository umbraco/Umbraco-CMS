using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when a data type is moved.
/// </summary>
[WebhookEvent("Data Type Moved")]
public class DataTypeMovedWebhookEvent : WebhookEventBase<DataTypeMovedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeMovedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public DataTypeMovedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.DataTypeMoved;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(DataTypeMovedNotification notification)
        => notification.MoveInfoCollection.Select(moveEvent => new
        {
            Id = moveEvent.Entity.Key,
            NewParentId = moveEvent.NewParentKey,
        });
}
