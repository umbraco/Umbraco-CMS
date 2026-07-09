using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

/// <summary>
/// Webhook event that fires when the content recycle bin is emptied.
/// </summary>
[WebhookEvent("Content Recycle Bin Emptied", Constants.WebhookEvents.Types.Content)]
public class ContentEmptiedRecycleBinWebhookEvent : WebhookEventContentBase<ContentEmptiedRecycleBinNotification, IContent>
{
    private readonly IApiContentBuilder _apiContentBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentEmptiedRecycleBinWebhookEvent"/> class.
    /// </summary>
    /// <param name="webhookFiringService">The webhook firing service.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <param name="webhookSettings">The webhook settings.</param>
    /// <param name="serverRoleAccessor">The server role accessor.</param>
    /// <param name="apiContentBuilder">The API content builder.</param>
    public ContentEmptiedRecycleBinWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IApiContentBuilder apiContentBuilder)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor)
    {
        _apiContentBuilder = apiContentBuilder;
    }

    /// <inheritdoc />
    public override string Alias => Constants.WebhookEvents.Aliases.ContentEmptiedRecycleBin;

    /// <inheritdoc />
    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentEmptiedRecycleBinNotification notification) =>
        notification.DeletedEntities;

    /// <inheritdoc />
    protected override object ConvertEntityToRequestPayload(IContent entity)
        => new DefaultPayloadModel { Id = entity.Key };
}
