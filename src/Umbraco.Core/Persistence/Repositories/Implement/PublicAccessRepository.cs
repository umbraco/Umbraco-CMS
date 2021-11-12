using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class PublicAccessRepository : NPocoRepositoryBase<Guid, PublicAccessEntry>, IPublicAccessRepository
    {
        public PublicAccessRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger)
            : base(scopeAccessor, cache, logger)
        { }

        protected override IRepositoryCachePolicy<PublicAccessEntry, Guid> CreateCachePolicy()
        {
            return new FullDataSetRepositoryCachePolicy<PublicAccessEntry, Guid>(GlobalIsolatedCache, ScopeAccessor, GetEntityId, /*expires:*/ false);
        }

        protected override PublicAccessEntry PerformGet(Guid id)
        {
            //return from GetAll - this will be cached as a collection
            return GetMany().FirstOrDefault(x => x.Key == id);
        }

        protected override IEnumerable<PublicAccessEntry> PerformGetAll(params Guid[] ids)
        {
            var sql = GetBaseQuery(false);

            if (ids.Any())
            {
                sql.WhereIn<AccessDto>(x => x.Id, ids);
            }

            sql.OrderBy<AccessDto>(x => x.NodeId);
            
            var dtos = Database.FetchOneToMany<AccessDto>(x => x.Rules, sql);
            return dtos.Select(PublicAccessEntryFactory.BuildEntity);
        }

        protected override IEnumerable<PublicAccessEntry> PerformGetByQuery(IQuery<PublicAccessEntry> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<PublicAccessEntry>(sqlClause, query);
            var sql = translator.Translate();
            
            var dtos = Database.FetchOneToMany<AccessDto>(x => x.Rules, sql);
            return dtos.Select(PublicAccessEntryFactory.BuildEntity);
        }

       protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            return Sql()
                .SelectAll()
                .From<AccessDto>()
                .LeftJoin<AccessRuleDto>()
                .On<AccessDto, AccessRuleDto>(left => left.Id, right => right.AccessId);
        }

        protected override string GetBaseWhereClause()
        {
            return $"{Constants.DatabaseSchema.Tables.Access}.id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
            {
                "DELETE FROM umbracoAccessRule WHERE accessId = @id",
                "DELETE FROM umbracoAccess WHERE id = @id"
            };
            return list;
        }

        protected override Guid NodeObjectTypeId => throw new NotImplementedException();

        protected override void PersistNewItem(PublicAccessEntry entity)
        {
            entity.AddingEntity();
            foreach (var rule in entity.Rules)
                rule.AddingEntity();
            
            var dto = PublicAccessEntryFactory.BuildDto(entity);

            Database.Insert(dto);
            //update the id so HasEntity is correct
            entity.Id = entity.Key.GetHashCode();

            foreach (var rule in dto.Rules)
            {
                rule.AccessId = entity.Key;
                Database.Insert(rule);
            }

            //update the id so HasEntity is correct
            foreach (var rule in entity.Rules)
                rule.Id = rule.Key.GetHashCode();

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(PublicAccessEntry entity)
        {
            entity.UpdatingEntity();
            foreach (var rule in entity.Rules)
            {
                if (rule.HasIdentity)
                    rule.UpdatingEntity();
                else
                    rule.AddingEntity();
            }
            
            var dto = PublicAccessEntryFactory.BuildDto(entity);

            Database.Update(dto);

            foreach (var removedRule in entity.RemovedRules)
            {
                Database.Delete<AccessRuleDto>("WHERE id=@Id", new { Id = removedRule });
            }

            foreach (var rule in entity.Rules)
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
                        UpdateDate = rule.UpdateDate
                    });
                    //update the id so HasEntity is correct
                    rule.Id = rule.Key.GetHashCode();
                }
            }

            entity.ResetDirtyProperties();
        }

        protected override Guid GetEntityId(PublicAccessEntry entity)
        {
            return entity.Key;
        }
    }
}
