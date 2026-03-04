using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Legacy webhook event that fires when a content template (blueprint) is deleted, using the legacy payload format.
/// </summary>
[WebhookEvent("Content Template [Blueprint] Deleted", Constants.WebhookEvents.Types.Content)]
public class LegacyContentDeletedBlueprintWebhookEvent : WebhookEventContentBase<ContentDeletedBlueprintNotification, IContent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyContentDeletedBlueprintWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    public LegacyContentDeletedBlueprintWebhookEvent(
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
    public override string Alias => Constants.WebhookEvents.Aliases.ContentDeletedBlueprint;

    /// <inheritdoc />
    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentDeletedBlueprintNotification notification) =>
        notification.DeletedBlueprints;

    /// <inheritdoc />
    protected override object ConvertEntityToRequestPayload(IContent entity) => entity;
}
