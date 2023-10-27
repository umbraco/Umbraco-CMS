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

        return new PagedModel<Webhook>
        {
            Items = await DtosToEntities(webhookDtos.Skip(skip).Take(take)),
            Total = webhookDtos.Count,
        };
    }

    public async Task<Webhook> CreateAsync(Webhook webhook)
    {
        WebhookDto webhookDto = WebhookFactory.BuildDto(webhook);

        var result = await _scopeAccessor.AmbientScope?.Database.InsertAsync(webhookDto)!;

        var id = Convert.ToInt32(result);
        webhook.Id = id;

        await _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(WebhookFactory.BuildEvent2WebhookDto(webhook))!;
        await _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(WebhookFactory.BuildEntityKey2WebhookDto(webhook))!;
        await _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(WebhookFactory.BuildHeaders2WebhookDtos(webhook))!;

        return webhook;
    }

    public async Task<Webhook?> GetAsync(Guid key)
    {
        Sql<ISqlContext>? sql = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql()
            .Select<WebhookDto>()
            .From<WebhookDto>()
            .Where<WebhookDto>(x => x.Key == key);

        WebhookDto? webhookDto = await _scopeAccessor.AmbientScope?.Database.FirstOrDefaultAsync<WebhookDto>(sql)!;

        return webhookDto is null ? null : await DtoToEntity(webhookDto);
    }

    public async Task<PagedModel<Webhook>> GetByEventNameAsync(string eventName)
    {
        Sql<ISqlContext>? sql = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql()
            .SelectAll()
            .From<WebhookDto>()
            .InnerJoin<Event2WebhookDto>()
            .On<WebhookDto, Event2WebhookDto>(left => left.Id, right => right.WebhookId)
            .Where<Event2WebhookDto>(x => x.Event == eventName);

        List<WebhookDto>? webhookDtos = await _scopeAccessor.AmbientScope?.Database.FetchAsync<WebhookDto>(sql)!;

        return new PagedModel<Webhook>
        {
            Items = await DtosToEntities(webhookDtos),
            Total = webhookDtos.Count,
        };
    }

    public async Task DeleteAsync(Webhook webhook)
    {
        Sql<ISqlContext> sql = _scopeAccessor.AmbientScope!.Database.SqlContext.Sql()
            .Delete<WebhookDto>()
            .Where<WebhookDto>(x => x.Key == webhook.Key);

        await _scopeAccessor.AmbientScope?.Database.ExecuteAsync(sql)!;
    }

    public async Task UpdateAsync(Webhook webhook)
    {
        WebhookDto dto = WebhookFactory.BuildDto(webhook);
        await _scopeAccessor.AmbientScope?.Database.UpdateAsync(dto)!;

        // Delete and re-insert the many to one references (event & entity keys)
        DeleteManyToOneReferences(dto.Id);
        InsertManyToOneReferences(webhook);
    }

    private void DeleteManyToOneReferences(int webhookId)
    {
        _scopeAccessor.AmbientScope?.Database.Delete<EntityKey2WebhookDto>("WHERE webhookId = @webhookId", new { webhookId });
        _scopeAccessor.AmbientScope?.Database.Delete<Event2WebhookDto>("WHERE webhookId = @webhookId", new { webhookId });
        _scopeAccessor.AmbientScope?.Database.Delete<Headers2WebhookDto>("WHERE webhookId = @webhookId", new { webhookId });
    }

    private void InsertManyToOneReferences(Webhook webhook)
    {
        IEnumerable<EntityKey2WebhookDto> buildEntityKey2WebhookDtos = WebhookFactory.BuildEntityKey2WebhookDto(webhook);
        IEnumerable<Event2WebhookDto> buildEvent2WebhookDtos = WebhookFactory.BuildEvent2WebhookDto(webhook);
        IEnumerable<Headers2WebhookDto> header2WebhookDtos = WebhookFactory.BuildHeaders2WebhookDtos(webhook);

        _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(buildEntityKey2WebhookDtos);
        _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(buildEvent2WebhookDtos);
        _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(header2WebhookDtos);
    }

    private async Task<IEnumerable<Webhook>> DtosToEntities(IEnumerable<WebhookDto> dtos)
    {
        List<Webhook> result = new();

        foreach (WebhookDto webhook in dtos)
        {
            result.Add(await DtoToEntity(webhook));
        }

        return result;
    }

    private async Task<Webhook> DtoToEntity(WebhookDto dto)
    {
        List<EntityKey2WebhookDto>? webhookEntityKeyDtos = await _scopeAccessor.AmbientScope?.Database.FetchAsync<EntityKey2WebhookDto>("WHERE webhookId = @webhookId", new { webhookId = dto.Id })!;
        List<Event2WebhookDto>? event2WebhookDtos = await _scopeAccessor.AmbientScope?.Database.FetchAsync<Event2WebhookDto>("WHERE webhookId = @webhookId", new { webhookId = dto.Id })!;
        List<Headers2WebhookDto>? headersWebhookDtos = await _scopeAccessor.AmbientScope?.Database.FetchAsync<Headers2WebhookDto>("WHERE webhookId = @webhookId", new { webhookId = dto.Id })!;
        Webhook entity = WebhookFactory.BuildEntity(dto, webhookEntityKeyDtos, event2WebhookDtos, headersWebhookDtos);

        return entity;
    }
}
