using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.Models;

namespace Umbraco.Cms.Web.BackOffice.Mapping;

public class WebhookMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<WebhookViewModel, Webhook>((_, _) => new Webhook(string.Empty, WebhookEvent.ContentDelete), Map);
        mapper.Define<Webhook, WebhookViewModel>((_, _) => new WebhookViewModel(), Map);
    }

    // Umbraco.Code.MapAll -CreateDate -DeleteDate -Id -Key -UpdateDate
    private void Map(WebhookViewModel source, Webhook target, MapperContext context)
    {
        target.EntityKeys = source.EntityKeys;
        target.Event = source.Event;
        target.Url = source.Url;
    }

    // Umbraco.Code.MapAll
    private void Map(Webhook source, WebhookViewModel target, MapperContext context)
    {
        target.EntityKeys = source.EntityKeys;
        target.Event = source.Event;
        target.Url = source.Url;
    }
}
