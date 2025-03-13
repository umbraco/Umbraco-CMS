using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Content Template [Blueprint] Saved", Constants.WebhookEvents.Types.Content)]
public class ContentSavedBlueprintWebhookEvent : WebhookEventContentBase<ContentSavedBlueprintNotification, IContent>
{
    public ContentSavedBlueprintWebhookEvent(
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

    public override string Alias => Constants.WebhookEvents.Aliases.ContentSavedBlueprint;

    protected override IEnumerable<IContent>
        GetEntitiesFromNotification(ContentSavedBlueprintNotification notification)
            => new List<IContent> { notification.SavedBlueprint };

    protected override object ConvertEntityToRequestPayload(IContent entity) => entity;
}
