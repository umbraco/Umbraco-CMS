using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class WebhookRepository : EntityRepositoryBase<Guid, Webhook>, IWebhookRepository
{
    public WebhookRepository(IScopeAccessor scopeAccessor, AppCaches appCaches, ILogger<EntityRepositoryBase<Guid, Webhook>> logger) : base(scopeAccessor, appCaches, logger)
    {
    }

    protected override Webhook? PerformGet(Guid key)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);
        sql.Where(GetBaseWhereClause(), new { key });

        WebhookDto? dto = Database.FirstOrDefault<WebhookDto>(sql);

        if (dto is not null)
        {
            return DtoToEntity(dto);
        }

        return null;
    }

    protected override IEnumerable<Webhook> PerformGetAll(params Guid[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);

        List<WebhookDto> dtos = Database.Fetch<WebhookDto>(sql);

        return dtos.Select(DtoToEntity);
    }

    protected override IEnumerable<Webhook> PerformGetByQuery(IQuery<Webhook> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<Webhook>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        List<WebhookDto>? dtos = Database.Fetch<WebhookDto>(sql);

        return dtos.Select(DtoToEntity);
    }

    protected override void PersistNewItem(Webhook entity)
    {
        entity.AddingEntity();

        WebhookDto webhookDto = WebhookFactory.BuildDto(entity);

        var id = Convert.ToInt32(Database.Insert(webhookDto));
        entity.Id = id;
        IEnumerable<WebhookEntityKeyDto> buildEntityKey2WebhookDtos = WebhookFactory.BuildEntityKey2WebhookDtos(entity, id);
        Database.InsertBulk(buildEntityKey2WebhookDtos);

        entity.ResetDirtyProperties();
    }

    protected override void PersistUpdatedItem(Webhook entity)
    {
        entity.UpdatingEntity();

        WebhookDto dto = WebhookFactory.BuildDto(entity);
        Database.Update(dto);

        entity.ResetDirtyProperties();
    }

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = Sql();

        sql = isCount
            ? sql.SelectCount()
            : sql.Select<WebhookDto>();

        sql
            .From<WebhookDto>();

        return sql;
    }

    protected override string GetBaseWhereClause() => "key = @key";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string>
        {
            $"DELETE FROM {Constants.DatabaseSchema.Tables.Webhook} WHERE key = @key",
        };
        return list;
    }

    protected override Guid GetEntityId(Webhook entity)
        => entity.Key;

    protected override void PersistDeletedItem(Webhook entity)
    {
        IEnumerable<string> deletes = GetDeleteClauses();
        foreach (var delete in deletes)
        {
            Database.Execute(delete, new { key = GetEntityId(entity) });
        }

        entity.DeleteDate = DateTime.Now;
    }

    private Webhook DtoToEntity(WebhookDto dto)
    {
        List<WebhookEntityKeyDto> webhookEntityKeyDtos = Database.Fetch<WebhookEntityKeyDto>("WHERE webhookId = @webhookId", new { webhookId = dto.Id });
        Webhook entity = WebhookFactory.BuildEntity(dto, webhookEntityKeyDtos);

        // reset dirty initial properties (U4-1946)
        entity.ResetDirtyProperties(false);

        return entity;
    }
}
