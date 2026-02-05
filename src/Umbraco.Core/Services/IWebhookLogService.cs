using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for managing webhook log entries.
/// </summary>
public interface IWebhookLogService
{
    /// <summary>
    /// Creates a new webhook log entry.
    /// </summary>
    /// <param name="webhookLog">The <see cref="WebhookLog"/> to create.</param>
    /// <returns>A task that represents the asynchronous operation, containing the created <see cref="WebhookLog"/>.</returns>
    Task<WebhookLog> CreateAsync(WebhookLog webhookLog);

    /// <summary>
    /// Gets a paged collection of all webhook logs.
    /// </summary>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <returns>A task that represents the asynchronous operation, containing a paged model of <see cref="WebhookLog"/> objects.</returns>
    Task<PagedModel<WebhookLog>> Get(int skip = 0, int take = int.MaxValue);

    /// <summary>
    /// Gets a paged collection of webhook logs for a specific webhook.
    /// </summary>
    /// <param name="webhookKey">The unique key of the webhook.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <returns>A task that represents the asynchronous operation, containing a paged model of <see cref="WebhookLog"/> objects.</returns>
    // TODO (V16): Remove the default implementation on this method.
    async Task<PagedModel<WebhookLog>> Get(Guid webhookKey, int skip = 0, int take = int.MaxValue)
    {
        // This is very inefficient as the filter/skip/take is in-memory, but it will return the correct data.
        // As it's only here to avoid a breaking change on the interface that is unlikely to have a custom
        // implementation, this seems reasonable.
        PagedModel<WebhookLog> allLogs = await Get(0, int.MaxValue);
        var logsForId = allLogs.Items.Where(x => x.WebhookKey == webhookKey).ToList();
        return new PagedModel<WebhookLog>(logsForId.Count, logsForId.Skip(skip).Take(take));
    }
}
