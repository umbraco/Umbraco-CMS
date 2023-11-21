using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IWebhookService
{
    /// <summary>
    ///     Creates a webhook.
    /// </summary>
    /// <param name="webhook"><see cref="Webhook" /> to create.</param>
    Task<Attempt<Webhook, WebhookOperationStatus>> CreateAsync(Webhook webhook);

    /// <summary>
    ///     Updates a webhook.
    /// </summary>
    /// <param name="webhook"><see cref="Webhook" /> to update.</param>
    Task<Attempt<Webhook, WebhookOperationStatus>> UpdateAsync(Webhook webhook);

    /// <summary>
    ///     Deletes a webhook.
    /// </summary>
    /// <param name="key">The unique key of the webhook.</param>
    Task<Attempt<Webhook?, WebhookOperationStatus>> DeleteAsync(Guid key);

    /// <summary>
    ///     Gets a webhook by its key.
    /// </summary>
    /// <param name="key">The unique key of the webhook.</param>
    Task<Webhook?> GetAsync(Guid key);

    /// <summary>
    ///     Gets all webhooks.
    /// </summary>
    Task<PagedModel<Webhook>> GetAllAsync(int skip, int take);

    /// <summary>
    ///     Gets webhooks by event name.
    /// </summary>
    Task<IEnumerable<Webhook>> GetByAliasAsync(string alias);
}
