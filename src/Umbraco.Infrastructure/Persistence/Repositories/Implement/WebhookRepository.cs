namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

using Microsoft.EntityFrameworkCore;
using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

public class WebhookRepository : IWebhookRepository
{
    private readonly IScopeAccessor _scopeAccessor;
    private readonly IEFCoreScopeAccessor<UmbracoDbContext> _coreScopeAccessor;

    public WebhookRepository(
        IScopeAccessor scopeAccessor,
        IEFCoreScopeAccessor<UmbracoDbContext> coreScopeAccessor)
    {
        _scopeAccessor = scopeAccessor;
        _coreScopeAccessor = coreScopeAccessor;
    }

    public async Task<PagedModel<IWebhook>> GetAllAsync(int skip, int take)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            return new PagedModel<IWebhook>
            {
                Items = Enumerable.Empty<IWebhook>(),
                Total = 0,
            };
        }

        Sql<ISqlContext>? sql = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
            .Select<WebhookDto>()
            .From<WebhookDto>();

        List<WebhookDto>? webhookDtos = await _scopeAccessor.AmbientScope.Database.FetchAsync<WebhookDto>(sql);

        return new PagedModel<IWebhook>
        {
            Items = await DtosToEntities(webhookDtos.Skip(skip).Take(take)),
            Total = webhookDtos.Count,
        };
    }

    public async Task<IWebhook> CreateAsync(IWebhook webhook)
    {
        webhook.AddingEntity();
        WebhookDto webhookDto = WebhookFactory.BuildDto(webhook);

        var result = await _scopeAccessor.AmbientScope?.Database.InsertAsync(webhookDto)!;

        var id = Convert.ToInt32(result);
        webhook.Id = id;

        await _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(WebhookFactory.BuildEvent2WebhookDto(webhook))!;
        await _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(WebhookFactory.BuildEntityKey2WebhookDto(webhook))!;
        await _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(WebhookFactory.BuildHeaders2WebhookDtos(webhook))!;

        return webhook;
    }

    public async Task<IWebhook?> GetAsync(Guid key)
    {
        IEfCoreScope<UmbracoDbContext>? scope = _coreScopeAccessor.AmbientScope;
        if (scope is null)
        {
            return null;
        }

        Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.WebhookDto? webhookDto = await scope.ExecuteWithContextAsync<Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.WebhookDto?>(async db =>
        {
            return await db.Webhooks
                .Where(x => x.Key == key)
                .Include(x => x.Webhook2ContentTypeKeys)
                .Include(x => x.Webhook2Events)
                .Include(x => x.Webhook2Headers)
                .FirstOrDefaultAsync();
        });

        return webhookDto is null ? null : WebhookFactory.BuildEntity(webhookDto);
    }

    public async Task<PagedModel<IWebhook>> GetByIdsAsync(IEnumerable<Guid> keys)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            return new PagedModel<IWebhook>
            {
                Items = Enumerable.Empty<IWebhook>(),
                Total = 0,
            };
        }

        Sql<ISqlContext>? sql = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
            .Select<WebhookDto>()
            .From<WebhookDto>()
            .WhereIn<WebhookDto>(x => x.Key, keys);

        List<WebhookDto>? webhookDtos = await _scopeAccessor.AmbientScope?.Database.FetchAsync<WebhookDto>(sql)!;

        return new PagedModel<IWebhook>
        {
            Items = await DtosToEntities(webhookDtos),
            Total = webhookDtos.Count,
        };
    }

    public async Task<PagedModel<IWebhook>> GetByAliasAsync(string alias)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            return new PagedModel<IWebhook>
            {
                Items = Enumerable.Empty<IWebhook>(),
                Total = 0,
            };
        }

        Sql<ISqlContext> sql = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
            .SelectAll()
            .From<WebhookDto>()
            .InnerJoin<Webhook2EventsDto>()
            .On<WebhookDto, Webhook2EventsDto>(left => left.Id, right => right.WebhookId)
            .Where<Webhook2EventsDto>(x => x.Event == alias);

        List<WebhookDto>? webhookDtos = await _scopeAccessor.AmbientScope?.Database.FetchAsync<WebhookDto>(sql)!;

        return new PagedModel<IWebhook>
        {
            Items = await DtosToEntities(webhookDtos),
            Total = webhookDtos.Count,
        };
    }

    public async Task DeleteAsync(IWebhook webhook)
    {
        Sql<ISqlContext> sql = _scopeAccessor.AmbientScope!.Database.SqlContext.Sql()
            .Delete<WebhookDto>()
            .Where<WebhookDto>(x => x.Key == webhook.Key);

        await _scopeAccessor.AmbientScope?.Database.ExecuteAsync(sql)!;
    }

    public async Task UpdateAsync(IWebhook webhook)
    {
        webhook.UpdatingEntity();
        WebhookDto dto = WebhookFactory.BuildDto(webhook);
        await _scopeAccessor.AmbientScope?.Database.UpdateAsync(dto)!;

        // Delete and re-insert the many to one references (event & entity keys)
        DeleteManyToOneReferences(dto.Id);
        InsertManyToOneReferences(webhook);
    }

    private void DeleteManyToOneReferences(int webhookId)
    {
        IUmbracoDatabase? database = _scopeAccessor.AmbientScope?.Database;
        if (database is null)
        {
            return;
        }

        Sql<ISqlContext> sql = database.SqlContext.Sql()
            .Delete<Webhook2ContentTypeKeysDto>()
            .Where<Webhook2ContentTypeKeysDto>(x => x.WebhookId == webhookId);
        database.Execute(sql);

        sql = database.SqlContext.Sql()
            .Delete<Webhook2EventsDto>()
            .Where<Webhook2EventsDto>(x => x.WebhookId == webhookId);
        database.Execute(sql);

        sql = database.SqlContext.Sql()
            .Delete<Webhook2HeadersDto>()
            .Where<Webhook2HeadersDto>(x => x.WebhookId == webhookId);
        database.Execute(sql);
    }

    private void InsertManyToOneReferences(IWebhook webhook)
    {
        IEnumerable<Webhook2ContentTypeKeysDto> buildEntityKey2WebhookDtos = WebhookFactory.BuildEntityKey2WebhookDto(webhook);
        IEnumerable<Webhook2EventsDto> buildEvent2WebhookDtos = WebhookFactory.BuildEvent2WebhookDto(webhook);
        IEnumerable<Webhook2HeadersDto> header2WebhookDtos = WebhookFactory.BuildHeaders2WebhookDtos(webhook);

        _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(buildEntityKey2WebhookDtos);
        _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(buildEvent2WebhookDtos);
        _scopeAccessor.AmbientScope?.Database.InsertBulkAsync(header2WebhookDtos);
    }

    private async Task<IEnumerable<IWebhook>> DtosToEntities(IEnumerable<WebhookDto> dtos)
    {
        List<IWebhook> result = new();

        foreach (WebhookDto webhook in dtos)
        {
            result.Add(await DtoToEntity(webhook));
        }

        return result;
    }

    private async Task<IWebhook> DtoToEntity(WebhookDto dto)
    {
        List<Webhook2ContentTypeKeysDto>? webhookEntityKeyDtos = null;
        List<Webhook2EventsDto>? event2WebhookDtos = null;
        List<Webhook2HeadersDto>? headersWebhookDtos = null;
        IUmbracoDatabase? database = _scopeAccessor.AmbientScope?.Database;
        if (database is not null)
        {
            Sql<ISqlContext> sql = database.SqlContext.Sql()
                .Select<Webhook2ContentTypeKeysDto>()
                .From<Webhook2ContentTypeKeysDto>()
                .Where<Webhook2ContentTypeKeysDto>(x => x.WebhookId == dto.Id);
            webhookEntityKeyDtos = await database.FetchAsync<Webhook2ContentTypeKeysDto>(sql);
            sql = database.SqlContext.Sql()
                .Select<Webhook2EventsDto>()
                .From<Webhook2EventsDto>()
                .Where<Webhook2EventsDto>(x => x.WebhookId == dto.Id);
            event2WebhookDtos = await database.FetchAsync<Webhook2EventsDto>(sql);
            sql = database.SqlContext.Sql()
                .Select<Webhook2HeadersDto>()
                .From<Webhook2HeadersDto>()
                .Where<Webhook2HeadersDto>(x => x.WebhookId == dto.Id);
            headersWebhookDtos = await database.FetchAsync<Webhook2HeadersDto>(sql);
        }

        Webhook entity = WebhookFactory.BuildEntity(dto, webhookEntityKeyDtos, event2WebhookDtos, headersWebhookDtos);

        return entity;
    }
}
