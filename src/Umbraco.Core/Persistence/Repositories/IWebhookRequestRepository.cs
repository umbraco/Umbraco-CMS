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

    /// <summary>
    ///     Deletes a webhook request
    /// </summary>
    /// <param name="webhookRequest">The webhook request to be deleted.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task DeleteAsync(WebhookRequest webhookRequest);

    /// <summary>
    ///     Gets all of the webhook requests in the current database.
    /// </summary>
    /// <returns>An enumerable of <see cref="WebhookRequest" /> objects.</returns>
    Task<IEnumerable<WebhookRequest>> GetAllAsync();

    /// <summary>
    ///     Update a webhook request.
    /// </summary>
    /// <param name="webhookRequest">The webhook request you want to update.</param>
    /// <returns>The updated <see cref="WebhookRequest" /> webhook</returns>
    Task<WebhookRequest> UpdateAsync(WebhookRequest webhookRequest);
}
