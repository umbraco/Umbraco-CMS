using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="WebhookLog" /> entities.
/// </summary>
public interface IWebhookLogRepository
{
    /// <summary>
    ///     Creates a new webhook log entry.
    /// </summary>
    /// <param name="log">The webhook log to create.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateAsync(WebhookLog log);

    /// <summary>
    ///     Gets paged webhook logs.
    /// </summary>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    /// <returns>A paged model of webhook logs.</returns>
    Task<PagedModel<WebhookLog>> GetPagedAsync(int skip, int take);

    // TODO (V16): Remove the default implementation on this method.
    /// <summary>
    ///     Gets paged webhook logs for a specific webhook.
    /// </summary>
    /// <param name="webhookKey">The unique key of the webhook.</param>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    /// <returns>A paged model of webhook logs for the specified webhook.</returns>
    async Task<PagedModel<WebhookLog>> GetPagedAsync(Guid webhookKey, int skip, int take)
    {
        // This is very inefficient as the filter/skip/take is in-memory, but it will return the correct data.
        // As it's only here to avoid a breaking change on the interface that is unlikely to have a custom
        // implementation, this seems reasonable.
        PagedModel<WebhookLog> allLogs = await GetPagedAsync(0, int.MaxValue);
        var logsForId = allLogs.Items.Where(x => x.WebhookKey == webhookKey).ToList();
        return new PagedModel<WebhookLog>(logsForId.Count, logsForId.Skip(skip).Take(take));
    }

    /// <summary>
    ///     Gets webhook logs older than the specified date.
    /// </summary>
    /// <param name="date">The cutoff date.</param>
    /// <returns>A collection of webhook logs older than the specified date.</returns>
    Task<IEnumerable<WebhookLog>> GetOlderThanDate(DateTime date);

    /// <summary>
    ///     Deletes webhook logs by their identifiers.
    /// </summary>
    /// <param name="ids">The identifiers of the logs to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteByIds(int[] ids);
}
