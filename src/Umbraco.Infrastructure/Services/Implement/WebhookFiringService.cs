using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

/// <summary>
/// Provides functionality to trigger or send webhooks within the application.
/// </summary>
public class WebhookFiringService : IWebhookFiringService
{
    private readonly IWebhookRequestService _webhookRequestService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookFiringService"/> class, responsible for handling webhook firing operations.
    /// </summary>
    /// <param name="webhookRequestService">The service used to process and send webhook requests.</param>
    public WebhookFiringService(IWebhookRequestService webhookRequestService) => _webhookRequestService = webhookRequestService;

    /// <summary>
    /// Asynchronously triggers the specified webhook using the provided event alias and payload.
    /// </summary>
    /// <param name="webhook">The webhook instance to be triggered.</param>
    /// <param name="eventAlias">The alias identifying the event that triggers the webhook.</param>
    /// <param name="payload">The data payload to include with the webhook request.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous firing operation.</returns>
    public async Task FireAsync(IWebhook webhook, string eventAlias, object? payload, CancellationToken cancellationToken) =>
        await _webhookRequestService.CreateAsync(webhook.Key, eventAlias, payload);
}


