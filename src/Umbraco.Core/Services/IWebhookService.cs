using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IWebhookService
{
    /// <summary>
    ///     Creates a webhook.
    /// </summary>
    /// <param name="webhook"><see cref="IWebhook" /> to create.</param>
    Task<Attempt<IWebhook, WebhookOperationStatus>> CreateAsync(IWebhook webhook);

    /// <summary>
    ///     Updates a webhook.
    /// </summary>
    /// <param name="webhook"><see cref="IWebhook" /> to update.</param>
    Task<Attempt<IWebhook, WebhookOperationStatus>> UpdateAsync(IWebhook webhook);

    /// <summary>
    ///     Deletes a webhook.
    /// </summary>
    /// <param name="key">The unique key of the webhook.</param>
    Task<Attempt<IWebhook?, WebhookOperationStatus>> DeleteAsync(Guid key);

    /// <summary>
    ///     Gets a webhook by its key.
    /// </summary>
    /// <param name="key">The unique key of the webhook.</param>
    Task<IWebhook?> GetAsync(Guid key);

    /// <summary>
    ///     Gets all webhooks.
    /// </summary>
    Task<PagedModel<IWebhook>> GetAllAsync(int skip, int take);

    /// <summary>
    ///     Gets webhooks by event name.
    /// </summary>
    Task<IEnumerable<IWebhook>> GetByAliasAsync(string alias);
}
