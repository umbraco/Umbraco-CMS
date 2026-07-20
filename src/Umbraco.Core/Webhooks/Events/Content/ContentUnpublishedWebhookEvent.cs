using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when content is unpublished.
/// </summary>
[WebhookEvent("Content Unpublished", Constants.WebhookEvents.Types.Content)]
public class ContentUnpublishedWebhookEvent : WebhookEventContentBase<ContentUnpublishedNotification, IContent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentUnpublishedWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public ContentUnpublishedWebhookEvent(
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
    public override string Alias => Constants.WebhookEvents.Aliases.ContentUnpublish;

    /// <inheritdoc />
    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentUnpublishedNotification notification) => notification.UnpublishedEntities;

    /// <inheritdoc />
    protected override object ConvertEntityToRequestPayload(IContent entity)
        => new DefaultPayloadModel { Id = entity.Key };
}
