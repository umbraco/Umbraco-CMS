using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class WebhookLogRepository : IWebhookLogRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public WebhookLogRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    public async Task CreateAsync(WebhookLog log)
    {
        WebhookLogDto dto = WebhookLogFactory.CreateDto(log);
        var result = await _scopeAccessor.AmbientScope?.Database.InsertAsync(dto)!;
        var id = Convert.ToInt32(result);
        log.Id = id;
    }

    public Task<PagedModel<WebhookLog>> GetPagedAsync(int skip, int take) => throw new NotImplementedException();
}
