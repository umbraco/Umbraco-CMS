using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Api.Management.Mapping.Webhook;

public class WebhookEventMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper) => mapper.Define<IWebhookEvent, WebhookEventViewModel>((_, _) => new WebhookEventViewModel(), Map);

    // Umbraco.Code.MapAll
    private void Map(IWebhookEvent source, WebhookEventViewModel target, MapperContext context)
    {
        target.EventName = source.EventName;
        target.EventType = source.EventType;
        target.Alias = source.Alias;
    }
}
