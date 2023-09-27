using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class WebhookRepository : IWebhookRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public WebhookRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    public async Task<PagedModel<Webhook>> GetAllAsync(int skip, int take)
    {
        Sql<ISqlContext>? sql = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql()
            .Select<WebhookDto>()
            .From<WebhookDto>();

        List<WebhookDto>? webhookDtos = await _scopeAccessor.AmbientScope?.Database.FetchAsync<WebhookDto>(sql)!;

        IEnumerable<Webhook>? webhooks = webhookDtos?.Skip(skip).Take(take).Select(DtoToEntity).WhereNotNull();

        return new PagedModel<Webhook>
        {
            Items = webhooks ?? Enumerable.Empty<Webhook>(),
            Total = webhookDtos?.Count ?? 0,
        };
    }

    public async Task<Webhook> CreateAsync(Webhook webhook)
    {
        webhook.AddingEntity();

        WebhookDto webhookDto = WebhookFactory.BuildDto(webhook);

        var result = await _scopeAccessor.AmbientScope?.Database.InsertAsync(webhookDto)!;

        var id = Convert.ToInt32(result);
        webhook.Id = id;

        IEnumerable<Event2WebhookDto> entityKeys = WebhookFactory.BuildEvent2WebhookDto(webhook);
        await _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(entityKeys)!;
        await _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(WebhookFactory.BuildEntityKey2WebhookDto(webhook))!;

        webhook.ResetDirtyProperties();

        return webhook;
    }

    public async Task<Webhook?> GetAsync(Guid key)
    {
        Sql<ISqlContext>? sql = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql()
            .Select<WebhookDto>()
            .From<WebhookDto>()
            .Where<WebhookDto>(x => x.Key == key);

        WebhookDto? webhookDto = await _scopeAccessor.AmbientScope?.Database.FirstOrDefaultAsync<WebhookDto>(sql)!;

        return DtoToEntity(webhookDto);
    }

    public async Task DeleteAsync(Webhook webhook)
    {
        Sql<ISqlContext> sql = _scopeAccessor.AmbientScope!.Database.SqlContext.Sql()
            .Delete<WebhookDto>()
            .Where<WebhookDto>(x => x.Key == webhook.Key);

        await _scopeAccessor.AmbientScope?.Database.ExecuteAsync(sql)!;

        webhook.DeleteDate = DateTime.Now;
    }

    public async Task UpdateAsync(Webhook webhook)
    {
        webhook.UpdatingEntity();

        WebhookDto dto = WebhookFactory.BuildDto(webhook);
        await _scopeAccessor.AmbientScope?.Database.UpdateAsync(dto)!;

        // Delete and re-insert the many to one references (event & entity keys)
        DeleteManyToOneReferences(dto.Id);
        InsertManyToOneReferences(webhook);

        webhook.ResetDirtyProperties();
    }

    private void DeleteManyToOneReferences(int webhookId)
    {
        _scopeAccessor.AmbientScope?.Database.Delete<EntityKey2WebhookDto>("WHERE webhookId = @webhookId", new { webhookId });
        _scopeAccessor.AmbientScope?.Database.Delete<Event2WebhookDto>("WHERE webhookId = @webhookId", new { webhookId });
    }

    private void InsertManyToOneReferences(Webhook webhook)
    {
        IEnumerable<EntityKey2WebhookDto> buildEntityKey2WebhookDtos = WebhookFactory.BuildEntityKey2WebhookDto(webhook);
        IEnumerable<Event2WebhookDto> buildEvent2WebhookDtos = WebhookFactory.BuildEvent2WebhookDto(webhook);

        _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(buildEntityKey2WebhookDtos);
        _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(buildEvent2WebhookDtos);
    }

    private Webhook? DtoToEntity(WebhookDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        List<EntityKey2WebhookDto>? webhookEntityKeyDtos = _scopeAccessor.AmbientScope?.Database.Fetch<EntityKey2WebhookDto>("WHERE webhookId = @webhookId", new { webhookId = dto.Id });
        List<Event2WebhookDto>? event2WebhookDtos = _scopeAccessor.AmbientScope?.Database.Fetch<Event2WebhookDto>("WHERE webhookId = @webhookId", new { webhookId = dto.Id });
        Webhook entity = WebhookFactory.BuildEntity(dto, webhookEntityKeyDtos, event2WebhookDtos);

        // reset dirty initial properties (U4-1946)
        entity.ResetDirtyProperties(false);

        return entity;
    }
}
