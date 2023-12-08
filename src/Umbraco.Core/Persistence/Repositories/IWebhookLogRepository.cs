using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IWebhookLogRepository
{
    Task CreateAsync(WebhookLog log);

    Task<PagedModel<WebhookLog>> GetPagedAsync(int skip, int take);

    Task<IEnumerable<WebhookLog>> GetOlderThanDate(DateTime date);

    Task DeleteByIds(int[] ids);
}
