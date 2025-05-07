using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Content Recycle Bin Emptied", Constants.WebhookEvents.Types.Content)]
public class LegacyContentEmptiedRecycleBinWebhookEvent : WebhookEventContentBase<ContentEmptiedRecycleBinNotification, IContent>
{
    private readonly IApiContentBuilder _apiContentBuilder;

    public LegacyContentEmptiedRecycleBinWebhookEvent(
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

    public override string Alias => Constants.WebhookEvents.Aliases.ContentEmptiedRecycleBin;

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentEmptiedRecycleBinNotification notification) =>
        notification.DeletedEntities;

    protected override object? ConvertEntityToRequestPayload(IContent entity) => entity;
}
