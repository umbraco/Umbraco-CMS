using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when an element is deleted.
/// </summary>
[WebhookEvent("Element Deleted", Constants.WebhookEvents.Types.Element)]
public class ElementDeletedWebhookEvent : WebhookEventContentBase<ElementDeletedNotification, IElement>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementDeletedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public ElementDeletedWebhookEvent(
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
    public override string Alias => Constants.WebhookEvents.Aliases.ElementDelete;

    /// <inheritdoc />
    protected override IEnumerable<IElement> GetEntitiesFromNotification(ElementDeletedNotification notification) =>
        notification.DeletedEntities;

    /// <inheritdoc />
    protected override object ConvertEntityToRequestPayload(IElement entity)
        => new DefaultPayloadModel { Id = entity.Key };
}
