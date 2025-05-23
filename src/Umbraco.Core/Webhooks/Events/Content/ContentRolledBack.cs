using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Content Rolled Back", Constants.WebhookEvents.Types.Content)]
public class ContentRolledBackWebhookEvent : WebhookEventContentBase<ContentRolledBackNotification, IContent>
{
    private readonly IPublishedContentCache _contentCache;
    private readonly IApiContentBuilder _apiContentBuilder;

    public ContentRolledBackWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IPublishedContentCache contentCache,
        IApiContentBuilder apiContentBuilder)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor)
    {
        _contentCache = contentCache;
        _apiContentBuilder = apiContentBuilder;
    }

    public override string Alias => Constants.WebhookEvents.Aliases.ContentRolledBack;

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentRolledBackNotification notification) =>
        new List<IContent> { notification.Entity };

    protected override object? ConvertEntityToRequestPayload(IContent entity)
        => new DefaultPayloadModel { Id = entity.Key };
}
