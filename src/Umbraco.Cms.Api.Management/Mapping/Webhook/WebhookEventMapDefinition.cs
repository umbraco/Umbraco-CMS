using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Api.Management.Mapping.Webhook;

/// <summary>
/// Provides the mapping configuration used to transform webhook event data within the API management context.
/// </summary>
public class WebhookEventMapDefinition : IMapDefinition
{
    /// <summary>
    /// Configures the mapping between <see cref="IWebhookEvent"/> domain objects and <see cref="WebhookEventViewModel"/> view models for webhook events.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance used to define the mapping configuration.</param>
    public void DefineMaps(IUmbracoMapper mapper) => mapper.Define<IWebhookEvent, WebhookEventViewModel>((_, _) => new WebhookEventViewModel(), Map);

    // Umbraco.Code.MapAll
    private void Map(IWebhookEvent source, WebhookEventViewModel target, MapperContext context)
    {
        target.EventName = source.EventName;
        target.EventType = source.EventType;
        target.Alias = source.Alias;
    }
}
