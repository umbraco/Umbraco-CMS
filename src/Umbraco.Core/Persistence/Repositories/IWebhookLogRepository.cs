using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IWebhookLogRepository
{
    Task CreateAsync(WebhookLog log);

    Task<PagedModel<WebhookLog>> GetPagedAsync(int skip, int take);

    // TODO (V16): Remove the default implementation on this method.
    async Task<PagedModel<WebhookLog>> GetPagedAsync(Guid webhookKey, int skip, int take)
    {
        // This is very inefficient as the filter/skip/take is in-memory, but it will return the correct data.
        // As it's only here to avoid a breaking change on the interface that is unlikely to have a custom
        // implementation, this seems reasonable.
        PagedModel<WebhookLog> allLogs = await GetPagedAsync(0, int.MaxValue);
        var logsForId = allLogs.Items.Where(x => x.WebhookKey == webhookKey).ToList();
        return new PagedModel<WebhookLog>(logsForId.Count, logsForId.Skip(skip).Take(take));
    }

    Task<IEnumerable<WebhookLog>> GetOlderThanDate(DateTime date);

    Task DeleteByIds(int[] ids);
}
