using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IWebhookRepository
{
    /// <summary>
    ///     Gets all of the webhooks in the current database.
    /// </summary>
    /// <param name="skip">Number of entries to skip.</param>
    /// <param name="take">Number of entries to take.</param>
    /// <returns>A paged model of <see cref="IWebhook" /> objects.</returns>
    Task<PagedModel<IWebhook>> GetAllAsync(int skip, int take);

    /// <summary>
    ///     Gets all of the webhooks in the current database.
    /// </summary>
    /// <param name="webhook">The webhook you want to create.</param>
    /// <returns>The created <see cref="IWebhook" /> webhook</returns>
    Task<IWebhook> CreateAsync(IWebhook webhook);

    /// <summary>
    ///     Gets a webhook by key
    /// </summary>
    /// <param name="key">The key of the webhook which will be retrieved.</param>
    /// <returns>The <see cref="IWebhook" /> webhook with the given key.</returns>
    Task<IWebhook?> GetAsync(Guid key);

    /// <summary>
    ///     Gets a webhook by key
    /// </summary>
    /// <param name="alias">The alias of an event, which is referenced by a webhook.</param>
    /// <returns>
    ///     A paged model of <see cref="IWebhook" />
    /// </returns>
    Task<PagedModel<IWebhook>> GetByAliasAsync(string alias);

    /// <summary>
    ///     Gets a webhook by key
    /// </summary>
    /// <param name="webhook">The webhook to be deleted.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task DeleteAsync(IWebhook webhook);

    /// <summary>
    ///     Updates a given webhook
    /// </summary>
    /// <param name="webhook">The webhook to be updated.</param>
    /// <returns>The updated <see cref="IWebhook" /> webhook.</returns>
    Task UpdateAsync(IWebhook webhook);
}
