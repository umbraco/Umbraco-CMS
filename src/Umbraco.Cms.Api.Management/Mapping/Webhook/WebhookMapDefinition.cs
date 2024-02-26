using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Web.Common.Models;

namespace Umbraco.Cms.Web.BackOffice.Mapping;

public class WebhookMapDefinition : IMapDefinition
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILocalizedTextService _localizedTextService;

    public WebhookMapDefinition(IHostingEnvironment hostingEnvironment, ILocalizedTextService localizedTextService)
    {
        _hostingEnvironment = hostingEnvironment;
        _localizedTextService = localizedTextService;
    }

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IWebhookEvent, WebhookEventResponseModel>((_, _) => new WebhookEventResponseModel(), Map);
        mapper.Define<WebhookLog, WebhookLogViewModel>((_, _) => new WebhookLogViewModel(), Map);
        mapper.Define<CreateWebhookRequestModel, IWebhook>((_, _) => new Webhook(string.Empty), Map);
        mapper.Define<UpdateWebhookRequestModel, IWebhook>((_, _) => new Webhook(string.Empty), Map);
    }

    // Umbraco.Code.MapAll
    private void Map(IWebhookEvent source, WebhookEventResponseModel target, MapperContext context)
    {
        target.EventName = source.EventName;
        target.EventType = source.EventType;
        target.Alias = source.Alias;
    }

    // Umbraco.Code.MapAll -CreateDate -DeleteDate -Id -UpdateDate
    private void Map(CreateWebhookRequestModel source, IWebhook target, MapperContext context)
    {
        target.Url = source.Url;
        target.Enabled = source.Enabled;
        target.ContentTypeKeys = source.ContentTypeKeys;
        target.Events = source.Events;
        target.Headers = source.Headers;
        target.Key = source.Id ?? Guid.NewGuid();
    }

    // Umbraco.Code.MapAll -CreateDate -DeleteDate -Id -UpdateDate -Key
    private void Map(UpdateWebhookRequestModel source, IWebhook target, MapperContext context)
    {
        target.Url = source.Url;
        target.Enabled = source.Enabled;
        target.ContentTypeKeys = source.ContentTypeKeys;
        target.Events = source.Events;
        target.Headers = source.Headers;
    }

    // Umbraco.Code.MapAll
    private void Map(WebhookLog source, WebhookLogViewModel target, MapperContext context)
    {
        target.Date = source.Date;
        target.EventAlias = source.EventAlias;
        target.Key = source.Key;
        target.RequestBody = source.RequestBody ?? string.Empty;
        target.RetryCount = source.RetryCount;
        target.Url = source.Url;
        target.RequestHeaders = source.RequestHeaders;
        target.WebhookKey = source.WebhookKey;

        if (_hostingEnvironment.IsDebugMode)
        {
            target.ExceptionOccured = source.ExceptionOccured;
            target.ResponseBody = source.ResponseBody;
            target.ResponseHeaders = source.ResponseHeaders;
            target.StatusCode = source.StatusCode;
        }
        else
        {
            target.ResponseBody = _localizedTextService.Localize("webhooks", "toggleDebug", Thread.CurrentThread.CurrentUICulture);
            target.StatusCode = source.StatusCode is "OK (200)" ? source.StatusCode : _localizedTextService.Localize("webhooks", "statusNotOk", Thread.CurrentThread.CurrentUICulture);
        }
    }
}
