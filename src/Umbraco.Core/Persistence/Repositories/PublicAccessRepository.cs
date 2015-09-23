using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class PublicAccessRepository : PetaPocoRepositoryBase<Guid, PublicAccessEntry>, IPublicAccessRepository
    {
        public PublicAccessRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
            _options = new RepositoryCacheOptions
            {
                //We want to ensure that a zero count gets cached, even if there is nothing in the db we don't want it to lookup nothing each time
                GetAllCacheAllowZeroCount = true,
                //Set to 1000 just to ensure that all of them are cached, The GetAll on this repository gets called *A lot*, we want max performance
                GetAllCacheThresholdLimit = 1000,
                //Override to false so that a Count check against the db is NOT performed when doing a GetAll without params, we just want to 
                // return the raw cache without validation. The GetAll on this repository gets called *A lot*, we want max performance
                GetAllCacheValidateCount = false
            };
        }

        private readonly RepositoryCacheOptions _options;

        protected override PublicAccessEntry PerformGet(Guid id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });

            var taskDto = Database.Fetch<AccessDto, AccessRuleDto, AccessDto>(new AccessRulesRelator().Map, sql).FirstOrDefault();
            if (taskDto == null)
                return null;

            var factory = new PublicAccessEntryFactory();
            var entity = factory.BuildEntity(taskDto);
            return entity;
        }

        protected override IEnumerable<PublicAccessEntry> PerformGetAll(params Guid[] ids)
        {
            var sql = GetBaseQuery(false);

            if (ids.Any())
            {
                sql.Where("umbracoAccess.id IN (@ids)", new { ids = ids });
            }

            var factory = new PublicAccessEntryFactory();
            var dtos = Database.Fetch<AccessDto, AccessRuleDto, AccessDto>(new AccessRulesRelator().Map, sql);
            return dtos.Select(factory.BuildEntity);
        }

        protected override IEnumerable<PublicAccessEntry> PerformGetByQuery(IQuery<PublicAccessEntry> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<PublicAccessEntry>(sqlClause, query);
            var sql = translator.Translate();

            var factory = new PublicAccessEntryFactory();
            var dtos = Database.Fetch<AccessDto, AccessRuleDto, AccessDto>(new AccessRulesRelator().Map, sql);
            return dtos.Select(factory.BuildEntity);
        }

       protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select("*")
                .From<AccessDto>(SqlSyntax)
                .LeftJoin<AccessRuleDto>(SqlSyntax)
                .On<AccessDto, AccessRuleDto>(SqlSyntax, left => left.Id, right => right.AccessId);
                
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoAccess.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
            {
                "DELETE FROM umbracoAccessRule WHERE accessId = @Id",
                "DELETE FROM umbracoAccess WHERE id = @Id"
            };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Returns the repository cache options
        /// </summary>
        protected override RepositoryCacheOptions RepositoryCacheOptions
        {
            get { return _options; }
        }


        protected override void PersistNewItem(PublicAccessEntry entity)
        {
            entity.AddingEntity();
            entity.Rules.ForEach(x => x.AddingEntity());

            var factory = new PublicAccessEntryFactory();
            var dto = factory.BuildDto(entity);

            Database.Insert(dto);
            //update the id so HasEntity is correct
            entity.Id = entity.Key.GetHashCode();

            foreach (var rule in dto.Rules)
            {
                rule.AccessId = entity.Key;
                Database.Insert(rule);
            }

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(PublicAccessEntry entity)
        {
            entity.UpdatingEntity();
            entity.Rules.Where(x => x.HasIdentity).ForEach(x => x.UpdatingEntity());
            entity.Rules.Where(x => x.HasIdentity == false).ForEach(x => x.AddingEntity());

            var factory = new PublicAccessEntryFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);

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
            foreach (var removedRule in entity.RemovedRules)
            {
                Database.Delete<AccessRuleDto>("WHERE id=@Id", new {Id = removedRule});
            }

            entity.ResetDirtyProperties();
        }

        protected override Guid GetEntityId(PublicAccessEntry entity)
        {
            return entity.Key;
        }

    
    }
}