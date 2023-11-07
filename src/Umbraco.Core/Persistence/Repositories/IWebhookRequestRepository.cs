using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IWebhookRequestRepository
{
    /// <summary>
    ///     Creates a webhook request.
    /// </summary>
    /// <param name="webhookRequest">The webhook request you want to create.</param>
    /// <returns>The created <see cref="WebhookRequest" /> webhook</returns>
    Task<WebhookRequest> CreateAsync(WebhookRequest webhookRequest);
}
