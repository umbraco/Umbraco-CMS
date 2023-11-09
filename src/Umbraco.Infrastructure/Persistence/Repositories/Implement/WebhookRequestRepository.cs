using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class WebhookRequestRepository : IWebhookRequestRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public WebhookRequestRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    public async Task<WebhookRequest> CreateAsync(WebhookRequest webhookRequest)
    {
        WebhookRequestDto dto = WebhookRequestFactory.CreateDto(webhookRequest);
        var result = await _scopeAccessor.AmbientScope?.Database.InsertAsync(dto)!;
        var id = Convert.ToInt32(result);
        webhookRequest.Id = id;
        return webhookRequest;
    }

    public async Task DeleteAsync(WebhookRequest webhookRequest)
    {
        Sql<ISqlContext> sql = _scopeAccessor.AmbientScope!.Database.SqlContext.Sql()
            .Delete<WebhookRequestDto>()
            .Where<WebhookRequestDto>(x => x.Id == webhookRequest.Id);

        await _scopeAccessor.AmbientScope?.Database.ExecuteAsync(sql)!;
    }

    public async Task<IEnumerable<WebhookRequest>> GetAllAsync()
    {
        Sql<ISqlContext>? sql = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql()
            .Select<WebhookRequestDto>()
            .From<WebhookRequestDto>();

        List<WebhookRequestDto>? webhookDtos = await _scopeAccessor.AmbientScope?.Database.FetchAsync<WebhookRequestDto>(sql)!;

        return webhookDtos.Select(WebhookRequestFactory.CreateModel);
    }

    public async Task<WebhookRequest> UpdateAsync(WebhookRequest webhookRequest)
    {
        WebhookRequestDto dto = WebhookRequestFactory.CreateDto(webhookRequest);
        await _scopeAccessor.AmbientScope?.Database.UpdateAsync(dto)!;
        return webhookRequest;
    }
}
