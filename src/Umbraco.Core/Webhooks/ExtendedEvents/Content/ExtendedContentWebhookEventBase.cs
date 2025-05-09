using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

public abstract class ExtendedContentWebhookEventBase<TNotification> : WebhookEventContentBase<TNotification, IContent>
    where TNotification : INotification
{
    private readonly IOutputExpansionStrategyAccessor _outputExpansionStrategyAccessor;
    private readonly IVariationContextAccessor _variationContextAccessor;

    public ExtendedContentWebhookEventBase(
        IWebhookFiringService webhookFiringService,
        IWebhookService webhookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor,
        IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor,
        IVariationContextAccessor variationContextAccessor)
        : base(webhookFiringService, webhookService, webhookSettings, serverRoleAccessor)
    {
        _outputExpansionStrategyAccessor = outputExpansionStrategyAccessor;
        _variationContextAccessor = variationContextAccessor;
    }

    public Dictionary<string, object> BuildCultureProperties(
        IPublishedContent publishedContent,
        IApiContentResponse deliveryContent)
    {
        var cultures = new Dictionary<string, object>();

        // just to be safe that messing with the variationContext doesn't screw things up
        VariationContext? originalVariationContext = _variationContextAccessor.VariationContext;

        try
        {
            foreach (KeyValuePair<string, IApiContentRoute> culture in deliveryContent.Cultures)
            {
                _variationContextAccessor.VariationContext = new VariationContext(culture.Key);

                IDictionary<string, object?> properties =
                    _outputExpansionStrategyAccessor.TryGetValue(out IOutputExpansionStrategy? outputExpansionStrategy)
                        ? outputExpansionStrategy.MapContentProperties(publishedContent!)
                        : new Dictionary<string, object?>();

                cultures.Add(culture.Key, new { culture.Value.Path, culture.Value.StartItem, properties, });
            }
        }
        finally
        {
            _variationContextAccessor.VariationContext = originalVariationContext;
        }

        return cultures;
    }
}
