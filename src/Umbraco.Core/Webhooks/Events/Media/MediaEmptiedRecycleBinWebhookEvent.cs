using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when the media recycle bin is emptied.
/// </summary>
[WebhookEvent("Media Recycle Bin Emptied", Constants.WebhookEvents.Types.Media)]
public class MediaEmptiedRecycleBinWebhookEvent : WebhookEventContentBase<MediaEmptiedRecycleBinNotification, IMedia>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaEmptiedRecycleBinWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public MediaEmptiedRecycleBinWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(
            webhookFiringService,
            webHookService,
            webhookSettings,
            serverRoleAccessor)
    {
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.MediaEmptiedRecycleBin;

    /// <inheritdoc />
    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaEmptiedRecycleBinNotification notification)
        => notification.DeletedEntities;

    /// <inheritdoc />
    protected override object ConvertEntityToRequestPayload(IMedia entity)
        => new DefaultPayloadModel { Id = entity.Key };
}
