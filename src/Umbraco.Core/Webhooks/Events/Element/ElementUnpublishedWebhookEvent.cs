using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when an element is unpublished.
/// </summary>
[WebhookEvent("Element Unpublished", Constants.WebhookEvents.Types.Element)]
public class ElementUnpublishedWebhookEvent : WebhookEventContentBase<ElementUnpublishedNotification, IElement>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementUnpublishedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public ElementUnpublishedWebhookEvent(
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
    public override string Alias => Constants.WebhookEvents.Aliases.ElementUnpublish;

    /// <inheritdoc />
    protected override IEnumerable<IElement> GetEntitiesFromNotification(ElementUnpublishedNotification notification) =>
        notification.UnpublishedEntities;

    /// <inheritdoc />
    protected override object ConvertEntityToRequestPayload(IElement entity)
        => new DefaultPayloadModel { Id = entity.Key };
}
