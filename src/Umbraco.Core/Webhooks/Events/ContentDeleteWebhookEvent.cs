using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

public class ContentDeleteWebhookEvent : WebhookEventContentBase<ContentDeletedNotification, IContent>
{
    public ContentDeleteWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebHookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(
            webhookFiringService,
            webHookService,
            webhookSettings,
            serverRoleAccessor,
            Constants.WebhookEvents.Names.ContentDelete,
            WebhookEventType.Content)
    {
    }

    public override string Alias => Constants.WebhookEvents.Aliases.ContentUnpublish;

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentDeletedNotification notification) =>
        notification.DeletedEntities;

    protected override object ConvertEntityToRequestPayload(IContent entity) => new DefaultPayloadModel { Id = entity.Key };
}
