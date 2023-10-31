﻿using System.Net.Http.Headers;
using System.Text;
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
    private readonly IWebhookBackgroundTaskQueue _webhookBackgroundTaskQueue;

    public WebhookFiringService(
        IJsonSerializer jsonSerializer,
        IOptions<WebhookSettings> webhookSettings,
        IWebhookLogService webhookLogService,
        IWebhookLogFactory webhookLogFactory,
        IWebhookBackgroundTaskQueue webhookBackgroundTaskQueue)
    {
        _jsonSerializer = jsonSerializer;
        _webhookLogService = webhookLogService;
        _webhookLogFactory = webhookLogFactory;
        _webhookBackgroundTaskQueue = webhookBackgroundTaskQueue;
        _webhookSettings = webhookSettings.Value;
    }

    // TODO: Add queing instead of processing directly in thread
    // as this just makes save and publish longer
    public async Task FireAsync(Webhook webhook, string eventName, object? payload, CancellationToken cancellationToken) =>
        await _webhookBackgroundTaskQueue.QueueBackgroundWorkItemAsync(async () => await SendRequestAsync(webhook, eventName, payload, 0));

    private async Task<WebhookResponseModel> SendRequestAsync(Webhook webhook, string eventName, object? payload, int retryCount)
    {
        using var httpClient = new HttpClient();

        var serializedObject = _jsonSerializer.Serialize(payload);
        var stringContent = new StringContent(serializedObject, Encoding.UTF8, "application/json");
        stringContent.Headers.TryAddWithoutValidation("Umb-Webhook-Event", eventName);

        foreach (KeyValuePair<string, string> header in webhook.Headers)
        {
            stringContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        HttpResponseMessage response = await httpClient.PostAsync(webhook.Url, stringContent);

        var webhookResponseModel = new WebhookResponseModel
        {
            HttpResponseMessage = response,
            RetryCount = retryCount,
        };


        WebhookLog log = await _webhookLogFactory.CreateAsync(eventName, webhookResponseModel, webhook);
        await _webhookLogService.CreateAsync(log);

        return webhookResponseModel;
    }
}


