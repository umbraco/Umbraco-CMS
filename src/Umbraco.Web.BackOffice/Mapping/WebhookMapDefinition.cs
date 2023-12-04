using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Web.Common.Models;

namespace Umbraco.Cms.Web.BackOffice.Mapping;

public class WebhookMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<WebhookViewModel, IWebhook>((_, _) => new Webhook(string.Empty), Map);
        mapper.Define<IWebhookEvent, WebhookEventViewModel>((_, _) => new WebhookEventViewModel(), Map);
        mapper.Define<WebhookLog, WebhookLogViewModel>((_, _) => new WebhookLogViewModel(), Map);
    }

    // Umbraco.Code.MapAll -CreateDate -DeleteDate -Id -Key -UpdateDate
    private void Map(WebhookViewModel source, IWebhook target, MapperContext context)
    {
        target.ContentTypeKeys = source.ContentTypeKeys;
        target.Events = source.Events.Select(x => x.Alias).ToArray();
        target.Url = source.Url;
        target.Enabled = source.Enabled;
        target.Key = source.Key ?? Guid.NewGuid();
        target.Headers = source.Headers;
    }

    // Umbraco.Code.MapAll
    private void Map(IWebhookEvent source, WebhookEventViewModel target, MapperContext context)
    {
        target.EventName = source.EventName;
        target.EventType = source.EventType;
        target.Alias = source.Alias;
    }

    // Umbraco.Code.MapAll
    private void Map(WebhookLog source, WebhookLogViewModel target, MapperContext context)
    {
        target.Date = source.Date;
        target.EventAlias = source.EventAlias;
        target.Key = source.Key;
        target.RequestBody = source.RequestBody ?? string.Empty;
        target.ResponseBody = source.ResponseBody;
        target.RetryCount = source.RetryCount;
        target.StatusCode = source.StatusCode;
        target.Url = source.Url;
        target.RequestHeaders = source.RequestHeaders;
        target.ResponseHeaders = source.ResponseHeaders;
        target.WebhookKey = source.WebhookKey;
    }
}
