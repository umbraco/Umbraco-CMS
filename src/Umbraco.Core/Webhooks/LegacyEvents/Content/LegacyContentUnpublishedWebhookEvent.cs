using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Content Unpublished", Constants.WebhookEvents.Types.Content)]
public class LegacyContentUnpublishedWebhookEvent : WebhookEventContentBase<ContentUnpublishedNotification, IContent>
{
    public LegacyContentUnpublishedWebhookEvent(
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

    public override string Alias => Constants.WebhookEvents.Aliases.ContentUnpublish;

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentUnpublishedNotification notification) => notification.UnpublishedEntities;

    protected override object ConvertEntityToRequestPayload(IContent entity) => new DefaultPayloadModel { Id = entity.Key };
}
