using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Legacy webhook event that fires when media is moved, using the legacy payload format.
/// </summary>
[WebhookEvent("Media Moved", Constants.WebhookEvents.Types.Media)]
public class LegacyMediaMovedWebhookEvent : WebhookEventContentBase<MediaMovedNotification, IMedia>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyMediaMovedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webHookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public LegacyMediaMovedWebhookEvent(
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
    public override string Alias => Constants.WebhookEvents.Aliases.MediaMoved;

    /// <inheritdoc />
    protected override IEnumerable<IMedia> GetEntitiesFromNotification(MediaMovedNotification notification) => notification.MoveInfoCollection.Select(x => x.Entity);

    /// <inheritdoc />
    protected override object ConvertEntityToRequestPayload(IMedia entity) => new DefaultPayloadModel { Id = entity.Key };
}
