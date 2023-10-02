﻿using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Web.Common.Models;

namespace Umbraco.Cms.Web.BackOffice.Mapping;

public class WebhookMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<WebhookViewModel, Webhook>((_, _) => new Webhook(string.Empty), Map);
        mapper.Define<Webhook, WebhookViewModel>((_, _) => new WebhookViewModel(), Map);
        mapper.Define<IWebhookEvent, WebhookEventViewModel>((_, _) => new WebhookEventViewModel(), Map);
    }

    // Umbraco.Code.MapAll -CreateDate -DeleteDate -Id -Key -UpdateDate
    private void Map(WebhookViewModel source, Webhook target, MapperContext context)
    {
        target.EntityKeys = source.EntityKeys;
        target.Events = source.Events;
        target.Url = source.Url;
        target.Enabled = source.Enabled;
        target.Key = source.Key ?? Guid.NewGuid();
    }

    // Umbraco.Code.MapAll
    private void Map(Webhook source, WebhookViewModel target, MapperContext context)
    {
        target.EntityKeys = source.EntityKeys;
        target.Events = source.Events;
        target.Url = source.Url;
        target.Enabled = source.Enabled;
        target.Key = source.Key;
    }

    // Umbraco.Code.MapAll
    private void Map(IWebhookEvent source, WebhookEventViewModel target, MapperContext context) => target.EventName = source.EventName;
}