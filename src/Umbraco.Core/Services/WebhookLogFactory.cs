using System.Net;
using System.Text;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public class WebhookLogFactory : IWebhookLogFactory
{
    public async Task<WebhookLog> CreateAsync(string eventAlias, HttpRequestMessage requestMessage, HttpResponseMessage? httpResponseMessage, int retryCount, Exception? exception, IWebhook webhook, CancellationToken cancellationToken)
    {
        var log = new WebhookLog
        {
            Date = DateTime.UtcNow,
            EventAlias = eventAlias,
            Key = Guid.NewGuid(),
            Url = webhook.Url,
            WebhookKey = webhook.Key,
            RetryCount = retryCount,
            RequestHeaders = $"{requestMessage.Content?.Headers}{requestMessage.Headers}",
            RequestBody = await requestMessage.Content?.ReadAsStringAsync(cancellationToken)!,
            ExceptionOccured = exception is not null,
        };

        if (httpResponseMessage is not null)
        {
            log.StatusCode = MapStatusCodeToMessage(httpResponseMessage.StatusCode);
            log.IsSuccessStatusCode = httpResponseMessage.IsSuccessStatusCode;
            log.ResponseHeaders = $"{httpResponseMessage.Content.Headers}{httpResponseMessage.Headers}";
            log.ResponseBody = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
        }
        else if (exception is HttpRequestException httpRequestException)
        {
            if (httpRequestException.StatusCode is not null)
            {
                log.StatusCode = MapStatusCodeToMessage(httpRequestException.StatusCode.Value);
            }
            else
            {
                log.StatusCode = httpRequestException.HttpRequestError.ToString();
            }

            log.ResponseBody = $"{httpRequestException.HttpRequestError}: {httpRequestException.Message}";
        }
        else if (exception is not null)
        {
            log.ResponseBody = exception.Message;
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
