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

internal class PublicAccessRepository : EntityRepositoryBase<Guid, PublicAccessEntry>, IPublicAccessRepository
{
    public PublicAccessRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<PublicAccessRepository> logger)
        : base(scopeAccessor, cache, logger)
    {
    }

    protected override IRepositoryCachePolicy<PublicAccessEntry, Guid> CreateCachePolicy() =>
        new FullDataSetRepositoryCachePolicy<PublicAccessEntry, Guid>(GlobalIsolatedCache, ScopeAccessor, GetEntityId, /*expires:*/ false);

    protected override PublicAccessEntry? PerformGet(Guid id) =>

        // return from GetAll - this will be cached as a collection
        GetMany().FirstOrDefault(x => x.Key == id);

    protected override IEnumerable<PublicAccessEntry> PerformGetAll(params Guid[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);

        if (ids?.Any() ?? false)
        {
            sql.WhereIn<AccessDto>(x => x.Id, ids);
        }

        sql.OrderBy<AccessDto>(x => x.NodeId);

        List<AccessDto>? dtos = Database.FetchOneToMany<AccessDto>(x => x.Rules, sql);
        return dtos.Select(PublicAccessEntryFactory.BuildEntity);
    }

    protected override IEnumerable<PublicAccessEntry> PerformGetByQuery(IQuery<PublicAccessEntry> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<PublicAccessEntry>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        List<AccessDto>? dtos = Database.FetchOneToMany<AccessDto>(x => x.Rules, sql);
        return dtos.Select(PublicAccessEntryFactory.BuildEntity);
    }

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount) =>
        Sql()
            .SelectAll()
            .From<AccessDto>()
            .LeftJoin<AccessRuleDto>()
            .On<AccessDto, AccessRuleDto>(left => left.Id, right => right.AccessId);

    protected override string GetBaseWhereClause() => $"{Constants.DatabaseSchema.Tables.Access}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string>
        {
            "DELETE FROM umbracoAccessRule WHERE accessId = @id", "DELETE FROM umbracoAccess WHERE id = @id",
        };
        return list;
    }

    protected override void PersistNewItem(PublicAccessEntry entity)
    {
        entity.AddingEntity();
        foreach (PublicAccessRule rule in entity.Rules)
        {
            rule.AddingEntity();
        }

        AccessDto dto = PublicAccessEntryFactory.BuildDto(entity);

        Database.Insert(dto);

        // update the id so HasEntity is correct
        entity.Id = entity.Key.GetHashCode();

        foreach (AccessRuleDto rule in dto.Rules)
        {
            rule.AccessId = entity.Key;
            Database.Insert(rule);
        }

        // update the id so HasEntity is correct
        foreach (PublicAccessRule rule in entity.Rules)
        {
            rule.Id = rule.Key.GetHashCode();
        }

        entity.ResetDirtyProperties();
    }

    protected override void PersistUpdatedItem(PublicAccessEntry entity)
    {
        entity.UpdatingEntity();
        foreach (PublicAccessRule rule in entity.Rules)
        {
            if (rule.HasIdentity)
            {
                rule.UpdatingEntity();
            }
            else
            {
                rule.AddingEntity();
            }
        }

        AccessDto dto = PublicAccessEntryFactory.BuildDto(entity);

        Database.Update(dto);

        foreach (Guid removedRule in entity.RemovedRules)
        {
            Database.Delete<AccessRuleDto>("WHERE id=@Id", new { Id = removedRule });
        }

        foreach (PublicAccessRule rule in entity.Rules)
        {
            if (rule.HasIdentity)
            {
                var count = Database.Update(dto.Rules.Single(x => x.Id == rule.Key));
                if (count == 0)
                {
                    throw new InvalidOperationException("No rows were updated for the access rule");
                }
            }
            else
            {
                Database.Insert(new AccessRuleDto
                {
                    Id = rule.Key,
                    AccessId = dto.Id,
                    RuleValue = rule.RuleValue,
                    RuleType = rule.RuleType,
                    CreateDate = rule.CreateDate,
                    UpdateDate = rule.UpdateDate,
                });

                // update the id so HasEntity is correct
                rule.Id = rule.Key.GetHashCode();
            }
        }

        entity.ResetDirtyProperties();
    }

    protected override Guid GetEntityId(PublicAccessEntry entity) => entity.Key;
}
