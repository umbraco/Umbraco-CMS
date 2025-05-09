using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Content Saved", Constants.WebhookEvents.Types.Content)]
public class ExtendedContentSavedWebhookEvent : ExtendedContentWebhookEventBase<ContentSavedNotification>
{
    private readonly IApiContentResponseBuilder _apiContentBuilder;
    private readonly IPublishedContentCache _contentCache;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;

    public ExtendedContentSavedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IApiContentResponseBuilder apiContentBuilder,
        IPublishedContentCache contentCache,
        IVariationContextAccessor variationContextAccessor,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor,
            outputExpansionStrategyAccessor,
            variationContextAccessor)
    {
        _apiContentBuilder = apiContentBuilder;
        _contentCache = contentCache;
        _variationContextAccessor = variationContextAccessor;
        _outputExpansionStrategyAccessor = outputExpansionStrategyAccessor;
    }

    public override string Alias => Constants.WebhookEvents.Aliases.ContentSaved;

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentSavedNotification notification) =>
        notification.SavedEntities;

    protected override object? ConvertEntityToRequestPayload(IContent entity)
    {
        // Get preview/saved version of content
        IPublishedContent? publishedContent = _contentCache.GetById(true, entity.Key);
        IApiContentResponse? deliveryContent = publishedContent is null ? null : _apiContentBuilder.Build(publishedContent);

        if (deliveryContent == null)
        {
            return null;
        }

        Dictionary<string, object> cultures = BuildCultureProperties(publishedContent!, deliveryContent);

        return new
        {
            deliveryContent.Id,
            deliveryContent.ContentType,
            deliveryContent.Name,
            deliveryContent.CreateDate,
            deliveryContent.UpdateDate,
            Cultures = cultures,
        };
    }
}
