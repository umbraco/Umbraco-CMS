using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IWebhookRequestService
{
    /// <summary>
    ///     Creates a webhook request.
    /// </summary>
    /// <param name="webhookKey">The key of the webhook.</param>
    /// <param name="eventAlias">The alias of the event that is creating the request.</param>
    /// <param name="payload">The payload you want to send with your request.</param>
    /// <returns>The created <see cref="WebhookRequest" /> webhook</returns>
    Task<WebhookRequest> CreateAsync(Guid webhookKey, string eventAlias, object? payload);

    /// <summary>
    ///     Gets all of the webhook requests in the current database.
    /// </summary>
    /// <returns>An enumerable of <see cref="WebhookRequest" /> objects.</returns>
    Task<IEnumerable<WebhookRequest>> GetAllAsync();

    /// <summary>
    ///     Deletes a webhook request
    /// </summary>
    /// <param name="webhookRequest">The webhook request to be deleted.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task DeleteAsync(WebhookRequest webhookRequest);

    /// <summary>
    ///     Update a webhook request.
    /// </summary>
    /// <param name="webhookRequest">The webhook request you want to update.</param>
    /// <returns>The updated <see cref="WebhookRequest" /> webhook</returns>
    Task<WebhookRequest> UpdateAsync(WebhookRequest webhookRequest);
}
