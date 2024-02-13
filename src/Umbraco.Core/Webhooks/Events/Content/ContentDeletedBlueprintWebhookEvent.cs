using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Content Template [Blueprint] Deleted", Constants.WebhookEvents.Types.Content)]
public class ContentDeletedBlueprintWebhookEvent : WebhookEventContentBase<ContentDeletedBlueprintNotification, IContent>
{
    public ContentDeletedBlueprintWebhookEvent(
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

    public override string Alias => Constants.WebhookEvents.Aliases.ContentDeletedBlueprint;

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentDeletedBlueprintNotification notification) =>
        notification.DeletedBlueprints;

    protected override object ConvertEntityToRequestPayload(IContent entity) => entity;
}
