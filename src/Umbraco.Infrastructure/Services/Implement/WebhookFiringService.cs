using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
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

    public async Task<WebhookResponseModel> Fire(string url, string eventName, object? requestObject) =>
        await _retryService.RetryAsync(
            async () =>
            {
                using var httpClient = new HttpClient();

                var myContent = _jsonSerializer.Serialize(requestObject);
                var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                var byteContent = new ByteArrayContent(buffer);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Umb-Webhook-Event", eventName);

                return await httpClient.PostAsync(url, byteContent);
            },
            _maxRetries);
}
