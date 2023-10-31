using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IWebhookRepository
{
    /// <summary>
    ///     Gets all of the webhooks in the current database.
    /// </summary>
    /// <param name="skip">Number of entries to skip.</param>
    /// <param name="take">Number of entries to take.</param>
    /// <returns>A paged model of <see cref="Webhook" /> objects.</returns>
    Task<PagedModel<Webhook>> GetAllAsync(int skip, int take);

    /// <summary>
    ///     Gets all of the webhooks in the current database.
    /// </summary>
    /// <param name="webhook">The webhook you want to create.</param>
    /// <returns>The created <see cref="Webhook" /> webhook</returns>
    Task<Webhook> CreateAsync(Webhook webhook);

    /// <summary>
    ///     Gets a webhook by key
    /// </summary>
    /// <param name="key">The key of the webhook which will be retrieved.</param>
    /// <returns>The <see cref="Webhook" /> webhook with the given key.</returns>
    Task<Webhook?> GetAsync(Guid key);

    /// <summary>
    ///     Gets a webhook by key
    /// </summary>
    /// <param name="eventName">The key of the webhook which will be retrieved.</param>
    /// <returns>The <see cref="Webhook" /> webhook with the given key.</returns>
    Task<PagedModel<Webhook>> GetByEventNameAsync(string eventName);

    /// <summary>
    ///     Gets a webhook by key
    /// </summary>
    /// <param name="webhook">The webhook to be deleted.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task DeleteAsync(Webhook webhook);

    /// <summary>
    ///     Updates a given webhook
    /// </summary>
    /// <param name="webhook">The webhook to be updated.</param>
    /// <returns>The updated <see cref="Webhook" /> webhook.</returns>
    Task UpdateAsync(Webhook webhook);
}
