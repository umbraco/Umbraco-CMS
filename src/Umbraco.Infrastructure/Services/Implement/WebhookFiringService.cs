using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

public class WebhookFiringService : IWebhookFiringService
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly WebhookSettings _webhookSettings;
    private readonly IWebhookLogService _webhookLogService;
    private readonly IWebhookLogFactory _webhookLogFactory;

    public WebhookFiringService(
        IJsonSerializer jsonSerializer,
        IOptions<WebhookSettings> webhookSettings,
        IWebhookLogService webhookLogService,
        IWebhookLogFactory webhookLogFactory)
    {
        _jsonSerializer = jsonSerializer;
        _webhookLogService = webhookLogService;
        _webhookLogFactory = webhookLogFactory;
        _webhookSettings = webhookSettings.Value;
    }

    // TODO: Add queing instead of processing directly in thread
    // as this just makes save and publish longer
    public async Task FireAsync(Webhook webhook, string eventName, object? payload, CancellationToken cancellationToken, TimeSpan? retryDelay = null)
    {
        for (var retry = 0; retry < _webhookSettings.MaximumRetries; retry++)
        {
            try
            {
                HttpResponseMessage response = await SendRequestAsync(webhook, eventName, payload, retry, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return;
                }

                // Retry after a delay, if specified
                if (retryDelay is not null)
                {
                    await Task.Delay(retryDelay.Value, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                // Retry after a delay, if specified
                if (retryDelay != null)
                {
                    await Task.Delay(retryDelay.Value, cancellationToken);
                }
            }
        }
    }

    private async Task<HttpResponseMessage> SendRequestAsync(Webhook webhook, string eventName, object? payload, int retryCount, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();

        var serializedObject = _jsonSerializer.Serialize(payload);
        var buffer = System.Text.Encoding.UTF8.GetBytes(serializedObject);
        var byteContent = new ByteArrayContent(buffer);
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Umb-Webhook-Event", eventName);
        foreach (KeyValuePair<string, string> header in webhook.Headers)
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        HttpResponseMessage response = await httpClient.PostAsync(webhook.Url, byteContent);

        var webhookResponseModel = new WebhookResponseModel
        {
            HttpResponseMessage = response,
            RetryCount = retryCount,
        };


        WebhookLog log = await _webhookLogFactory.CreateAsync(eventName, webhookResponseModel, webhook, cancellationToken);
        await _webhookLogService.CreateAsync(log);

        return response;
    }
}


