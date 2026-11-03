using System.Net;
using System.Text;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Implements <see cref="IWebhookLogFactory" /> to create webhook log entries.
/// </summary>
public class WebhookLogFactory : IWebhookLogFactory
{
    /// <inheritdoc />
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
            RequestBody = await ReadBodyAsync(requestMessage.Content, cancellationToken),
            ExceptionOccured = exception is not null,
        };

        if (httpResponseMessage is not null)
        {
            log.StatusCode = MapStatusCodeToMessage(httpResponseMessage.StatusCode);
            log.IsSuccessStatusCode = httpResponseMessage.IsSuccessStatusCode;
            log.ResponseHeaders = $"{httpResponseMessage.Content.Headers}{httpResponseMessage.Headers}";
            log.ResponseBody = await ReadBodyAsync(httpResponseMessage.Content, cancellationToken);
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

    /// <summary>
    ///     Reads a request or response body as text for logging.
    /// </summary>
    /// <remarks>
    ///     Unlike <see cref="HttpContent.ReadAsStringAsync()" />, this does not throw when the content declares a
    ///     charset that can't be resolved; the body is decoded as UTF-8 instead.
    /// </remarks>
    private static async Task<string> ReadBodyAsync(HttpContent? content, CancellationToken cancellationToken)
    {
        if (content is null)
        {
            return string.Empty;
        }

        byte[] bytes = await content.ReadAsByteArrayAsync(cancellationToken);
        return GetEncoding(content.Headers.ContentType?.CharSet).GetString(bytes);
    }

    private static Encoding GetEncoding(string? charset)
    {
        if (string.IsNullOrWhiteSpace(charset))
        {
            return Encoding.UTF8;
        }

        try
        {
            return Encoding.GetEncoding(charset.Trim('"'));
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8;
        }
    }

    /// <summary>
    ///     Maps an HTTP status code to a human-readable message.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>A formatted string containing the status code name and numeric value.</returns>
    private string MapStatusCodeToMessage(HttpStatusCode statusCode) => $"{statusCode.ToString()} ({(int)statusCode})";

    /// <summary>
    ///     Calculates and formats the headers from an HTTP response message.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message.</param>
    /// <returns>A formatted string containing all headers.</returns>
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
