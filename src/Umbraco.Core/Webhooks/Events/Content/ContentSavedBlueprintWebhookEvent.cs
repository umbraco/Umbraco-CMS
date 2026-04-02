using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when a content template (blueprint) is saved.
/// </summary>
[WebhookEvent("Content Template [Blueprint] Saved", Constants.WebhookEvents.Types.Content)]
public class ContentSavedBlueprintWebhookEvent : WebhookEventContentBase<ContentSavedBlueprintNotification, IContent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentSavedBlueprintWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
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

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ContentSavedBlueprint;

    /// <inheritdoc />
    protected override IEnumerable<IContent>
        GetEntitiesFromNotification(ContentSavedBlueprintNotification notification)
            => new List<IContent> { notification.SavedBlueprint };

    /// <inheritdoc />
    protected override object ConvertEntityToRequestPayload(IContent entity)
        => new DefaultPayloadModel { Id = entity.Key };
}
