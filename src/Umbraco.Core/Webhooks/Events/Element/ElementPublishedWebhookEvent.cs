using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when an element is published.
/// </summary>
[WebhookEvent("Element Published", Constants.WebhookEvents.Types.Element)]
public class ElementPublishedWebhookEvent : WebhookEventContentBase<ElementPublishedNotification, IElement>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementPublishedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public ElementPublishedWebhookEvent(
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
    public override string Alias => Constants.WebhookEvents.Aliases.ElementPublish;

    /// <inheritdoc />
    protected override IEnumerable<IElement> GetEntitiesFromNotification(ElementPublishedNotification notification) =>
        notification.PublishedEntities;

    /// <inheritdoc />
    protected override object? ConvertEntityToRequestPayload(IElement entity)
        => new
        {
            Id = entity.Key,
            Cultures = entity.PublishCultureInfos?.Values.Select(cultureInfo => new
            {
                cultureInfo.Culture,
                cultureInfo.Date,
            }),
        };
}
