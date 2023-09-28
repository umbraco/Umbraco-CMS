using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IWebhookLogRepository
{
    Task CreateAsync(WebhookLog log);

    Task<PagedModel<WebhookLog>> GetPagedAsync(int skip, int take);
}
