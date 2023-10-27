using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

public class WebhookFiringService : IWebhookFiringService
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IRetryService _retryService;
    private readonly int _maxRetries = 5;

    public WebhookFiringService(IJsonSerializer jsonSerializer, IRetryService retryService)
    {
        _jsonSerializer = jsonSerializer;
        _retryService = retryService;
    }

    public async Task<WebhookResponseModel> Fire(Webhook webhook, string eventName, object? requestObject) =>
        await _retryService.RetryAsync(
            async () =>
            {
                using var httpClient = new HttpClient();

                var serializedObject = _jsonSerializer.Serialize(requestObject);
                var buffer = System.Text.Encoding.UTF8.GetBytes(serializedObject);
                var byteContent = new ByteArrayContent(buffer);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Umb-Webhook-Event", eventName);
                foreach (KeyValuePair<string, string> header in webhook.Headers)
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }

                return await httpClient.PostAsync(webhook.Url, byteContent);
            },
            _maxRetries);
}
