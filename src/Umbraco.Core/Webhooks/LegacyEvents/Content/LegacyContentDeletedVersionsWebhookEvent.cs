using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Legacy webhook event that fires when content versions are deleted, using the legacy payload format.
/// </summary>
[WebhookEvent("Content Versions Deleted", Constants.WebhookEvents.Types.Content)]
public class LegacyContentDeletedVersionsWebhookEvent : WebhookEventBase<ContentDeletedVersionsNotification>
{
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyContentDeletedVersionsWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="idKeyMap">The ID to key mapping service.</param>
    public LegacyContentDeletedVersionsWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IIdKeyMap idKeyMap)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor)
    {
        _idKeyMap = idKeyMap;
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ContentDeletedVersions;

    /// <inheritdoc />
    /// <remarks>
    /// The legacy payload identifies the content by its integer id, so a key that cannot be resolved has no payload
    /// to send and the webhook is not fired.
    /// </remarks>
    public override bool ShouldFireWebhookForNotification(ContentDeletedVersionsNotification notificationObject)
        => TryGetId(notificationObject.Key, out _);

    /// <inheritdoc />
    public override object? ConvertNotificationToRequestPayload(ContentDeletedVersionsNotification notification)
    {
        if (TryGetId(notification.Key, out int id) is false)
        {
            return null;
        }

        return new
        {
            Id = id,
            notification.DeletePriorVersions,
            notification.SpecificVersion,
            notification.DateToRetain
        };
    }

    private bool TryGetId(Guid key, out int id)
    {
        // TODO (V20): await this once the webhook payload contract goes async.
        Attempt<int> attempt = _idKeyMap.GetIdForKeyAsync(key, UmbracoObjectTypes.Document).GetAwaiter().GetResult();
        id = attempt.Result;
        return attempt.Success;
    }
}
