using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Core.Services;

public class WebhookLogFactory : IWebhookLogFactory
{
    public async Task<WebhookLog> CreateAsync(string eventName, WebhookResponseModel responseModel, Webhook webhook, CancellationToken cancellationToken)
    {
        var log = new WebhookLog
        {
            Date = DateTime.UtcNow,
            EventName = eventName,
            Key = Guid.NewGuid(),
            Url = webhook.Url,
        };

        if (responseModel.HttpResponseMessage is not null)
        {
            log.RequestBody = await responseModel.HttpResponseMessage!.RequestMessage!.Content!.ReadAsStringAsync(cancellationToken);
            log.ResponseBody = await responseModel.HttpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
            log.StatusCode = responseModel.HttpResponseMessage.StatusCode.ToString();
            log.RetryCount = responseModel.RetryCount;
            log.ResponseHeaders = responseModel.HttpResponseMessage.Headers.ToString();
            log.RequestHeaders = responseModel.HttpResponseMessage.RequestMessage.Headers.ToString();
        }

        return log;
    }
}
