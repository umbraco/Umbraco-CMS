using System.Net;
using System.Text;
using Umbraco.Cms.Core.Models;

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
            RetryCount = responseModel.RetryCount,
        };

        if (responseModel.HttpResponseMessage is not null)
        {
            if (responseModel.HttpResponseMessage.RequestMessage?.Content is not null)
            {
                log.RequestBody = await responseModel.HttpResponseMessage.RequestMessage.Content.ReadAsStringAsync(cancellationToken);
                log.RequestHeaders = CalculateHeaders(responseModel.HttpResponseMessage);
            }

            log.ResponseBody = await responseModel.HttpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
            log.ResponseHeaders = responseModel.HttpResponseMessage.Headers.ToString();
            log.StatusCode = MapStatusCodeToMessage(responseModel.HttpResponseMessage.StatusCode);
        }

        return log;
    }

    private string MapStatusCodeToMessage(HttpStatusCode statusCode) => $"{statusCode.ToString()} ({(int)statusCode})";

    private string CalculateHeaders(HttpResponseMessage responseMessage)
    {
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = responseMessage.RequestMessage!.Headers.Concat(responseMessage.RequestMessage.Content!.Headers);

        var result = new StringBuilder();

        foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
        {
            result.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}\n");
        }

        return result.ToString();
    }
}
