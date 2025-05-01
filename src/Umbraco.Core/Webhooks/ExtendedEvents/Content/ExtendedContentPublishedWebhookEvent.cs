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

[WebhookEvent("Content Published", Constants.WebhookEvents.Types.Content)]
public class ExtendedContentPublishedWebhookEvent : WebhookEventContentBase<ContentPublishedNotification, IContent>
{
    private readonly IApiContentResponseBuilder _apiContentBuilder;
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;
    private readonly IVariationContextAccessor _variationContextAccessor;

    public ExtendedContentPublishedWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IApiContentResponseBuilder apiContentBuilder,
        IPublishedContentCache publishedContentCache,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor,
        IVariationContextAccessor variationContextAccessor)
        : base(
            webhookFiringService,
            webhookService,
            webhookSettings,
            serverRoleAccessor)
    {
        _apiContentBuilder = apiContentBuilder;
        _publishedContentCache = publishedContentCache;
        _outputExpansionStrategyAccessor = outputExpansionStrategyAccessor;
        _variationContextAccessor = variationContextAccessor;
    }

    public override string Alias => Constants.WebhookEvents.Aliases.ContentPublish;

    protected override IEnumerable<IContent> GetEntitiesFromNotification(ContentPublishedNotification notification) =>
        notification.PublishedEntities;

    protected override object? ConvertEntityToRequestPayload(IContent entity)
    {
        IPublishedContent? publishedContent = _publishedContentCache.GetById(entity.Key);
        IApiContentResponse? deliveryContent =
            publishedContent is null ? null : _apiContentBuilder.Build(publishedContent);

        if (deliveryContent == null)
        {
            return null;
        }

        var cultures = new Dictionary<string, object>();

        // just to be safe that messing with the variationContext doesn't screw things up
        VariationContext? originalVariationContext = _variationContextAccessor.VariationContext;

        foreach (KeyValuePair<string, IApiContentRoute> culture in deliveryContent.Cultures)
        {
            _variationContextAccessor.VariationContext = new VariationContext(culture.Key);

            IDictionary<string, object?> properties =
                _outputExpansionStrategyAccessor.TryGetValue(out IOutputExpansionStrategy? outputExpansionStrategy)
                    ? outputExpansionStrategy.MapContentProperties(publishedContent!)
                    : new Dictionary<string, object?>();

            cultures.Add(culture.Key, new
            {
                culture.Value.Path,
                culture.Value.StartItem,
                properties,
            });
        }

        _variationContextAccessor.VariationContext = originalVariationContext;

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
