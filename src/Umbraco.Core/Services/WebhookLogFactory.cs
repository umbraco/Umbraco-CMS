using System.Net;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Core.Services;

public class WebhookLogFactory : IWebhookLogFactory
{
    public async Task<WebhookLog> CreateAsync(string eventAlias, WebhookResponseModel responseModel, IWebhook webhook, CancellationToken cancellationToken)
    {
        var log = new WebhookLog
        {
            Date = DateTime.UtcNow,
            EventAlias = eventAlias,
            Key = Guid.NewGuid(),
            Url = webhook.Url,
            WebhookKey = webhook.Key,
        };

        if (responseModel.HttpResponseMessage is not null)
        {
            log.RequestBody = await responseModel.HttpResponseMessage!.RequestMessage!.Content!.ReadAsStringAsync(cancellationToken);
            log.ResponseBody = await responseModel.HttpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
            log.StatusCode = MapStatusCodeToMessage(responseModel.HttpResponseMessage.StatusCode);
            log.RetryCount = responseModel.RetryCount;
            log.ResponseHeaders = responseModel.HttpResponseMessage.Headers.ToString();
            log.RequestHeaders = responseModel.HttpResponseMessage.RequestMessage.Headers.ToString();
        }

        return log;
    }

    private string MapStatusCodeToMessage(HttpStatusCode statusCode) => $"{statusCode.ToString()} ({(int)statusCode})";
}
