using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

public class WebhookFiringService : IWebhookFiringService
{
    private readonly IWebhookRequestService _webhookRequestService;

    public WebhookFiringService(IWebhookRequestService webhookRequestService) => _webhookRequestService = webhookRequestService;

    public async Task FireAsync(IWebhook webhook, string eventAlias, object? payload, CancellationToken cancellationToken) =>
        await _webhookRequestService.CreateAsync(webhook.Key, eventAlias, payload);
}


