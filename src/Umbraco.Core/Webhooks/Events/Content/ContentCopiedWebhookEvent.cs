using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when content is copied.
/// </summary>
[WebhookEvent("Content Copied", Constants.WebhookEvents.Types.Content)]
public class ContentCopiedWebhookEvent : WebhookEventBase<ContentCopiedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentCopiedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public ContentCopiedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ContentCopied;

    /// <inheritdoc />
    public override object ConvertNotificationToRequestPayload(ContentCopiedNotification notification)
    {
        return new
        {
            Id = notification.Copy.Key,
            Original = notification.Original.Key,
            Parent = notification.ParentKey,
            notification.RelateToOriginal,
        };
    }
}
