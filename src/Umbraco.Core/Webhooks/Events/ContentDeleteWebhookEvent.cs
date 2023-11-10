using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Content was deleted", Constants.WebhookEvents.Types.Content)]
public class ContentDeleteWebhookEvent : WebhookEventContentBase<ContentDeletedNotification, IContent>
{
    public ContentDeleteWebhookEvent(
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

    public override string Alias => Constants.WebhookEvents.Aliases.ContentUnpublish;

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentDeletedNotification notification) =>
        notification.DeletedEntities;

    protected override object ConvertEntityToRequestPayload(IContent entity) => new DefaultPayloadModel { Id = entity.Key };
}
