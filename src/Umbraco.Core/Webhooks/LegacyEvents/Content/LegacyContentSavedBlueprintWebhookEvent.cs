using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Legacy webhook event that fires when a content template (blueprint) is saved, using the legacy payload format.
/// </summary>
[WebhookEvent("Content Template [Blueprint] Saved", Constants.WebhookEvents.Types.Content)]
public class LegacyContentSavedBlueprintWebhookEvent : WebhookEventContentBase<ContentSavedBlueprintNotification, IContent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyContentSavedBlueprintWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public LegacyContentSavedBlueprintWebhookEvent(
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
    protected override object ConvertEntityToRequestPayload(IContent entity) => entity;
}
