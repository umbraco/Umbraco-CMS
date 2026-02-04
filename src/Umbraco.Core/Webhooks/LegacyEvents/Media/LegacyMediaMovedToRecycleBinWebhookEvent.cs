using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Legacy webhook event that fires when media is moved to the recycle bin, using the legacy payload format.
/// </summary>
[WebhookEvent("Media Moved to Recycle Bin", Constants.WebhookEvents.Types.Media)]
public class LegacyMediaMovedToRecycleBinWebhookEvent : WebhookEventContentBase<MediaMovedToRecycleBinNotification, IMedia>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyMediaMovedToRecycleBinWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public LegacyMediaMovedToRecycleBinWebhookEvent(
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
    public override string Alias => Constants.WebhookEvents.Aliases.MediaMovedToRecycleBin;

    /// <inheritdoc />
    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaMovedToRecycleBinNotification notification) => notification.MoveInfoCollection.Select(x => x.Entity);

    /// <inheritdoc />
    protected override object ConvertEntityToRequestPayload(IMedia entity) => new DefaultPayloadModel { Id = entity.Key };
}
