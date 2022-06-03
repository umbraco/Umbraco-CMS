using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    internal class KeyValueRepository : EntityRepositoryBase<string, IKeyValue>, IKeyValueRepository
    {
        public KeyValueRepository(IScopeAccessor scopeAccessor, ILogger<KeyValueRepository> logger)
            : base(scopeAccessor, AppCaches.NoCache, logger)
        { }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, string?>? FindByKeyPrefix(string keyPrefix)
            => Get(Query<IKeyValue>().Where(entity => entity.Identifier!.StartsWith(keyPrefix)))?
                .ToDictionary(x => x.Identifier!, x => x.Value);

        #region Overrides of IReadWriteQueryRepository<string, IKeyValue>

        public override void Save(IKeyValue entity)
        {
            if (Get(entity.Identifier) == null)
                PersistNewItem(entity);
            else
                PersistUpdatedItem(entity);
        }

        #endregion

        #region Overrides of EntityRepositoryBase<string, IKeyValue>

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var sql = SqlContext.Sql();

            sql = isCount
                ? sql.SelectCount()
                : sql.Select<KeyValueDto>();

            sql
                .From<KeyValueDto>();

            return sql;
        }

        protected override string GetBaseWhereClause() => Core.Constants.DatabaseSchema.Tables.KeyValue + ".key = @id";

        protected override IEnumerable<string> GetDeleteClauses() => Enumerable.Empty<string>();

        protected override IKeyValue? PerformGet(string? id)
        {
            var sql = GetBaseQuery(false).Where<KeyValueDto>(x => x.Key == id);
            var dto = Database.Fetch<KeyValueDto>(sql).FirstOrDefault();
            return dto == null ? null : Map(dto);
        }

        protected override IEnumerable<IKeyValue> PerformGetAll(params string[]? ids)
        {
            Sql<ISqlContext> sql = GetAllSql(ids);
            var dtos = Database.Fetch<KeyValueDto>(sql);
            return dtos?.WhereNotNull().Select(Map)!;
        }
        protected override async Task<IEnumerable<IKeyValue>> PerformGetAllAsync(params string[]? ids)
        {
            Sql<ISqlContext> sql = GetAllSql(ids);
            var dtos = await Database.FetchAsync<KeyValueDto>(sql);
            return dtos?.WhereNotNull().Select(Map)!;
        }

        private Sql<ISqlContext> GetAllSql(string[]? ids) => GetBaseQuery(false).WhereIn<KeyValueDto>(x => x.Key, ids);

        protected override IEnumerable<IKeyValue> PerformGetByQuery(IQuery<IKeyValue> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IKeyValue>(sqlClause, query);
            var sql = translator.Translate();
            return Database.Fetch<KeyValueDto>(sql).Select(Map).WhereNotNull();
        }

        protected override void PersistNewItem(IKeyValue entity)
        {
            var dto = Map(entity);
            Database.Insert(dto);
        }

        protected override void PersistUpdatedItem(IKeyValue entity)
        {
            var dto = Map(entity);
            if (dto is not null)
            {
                Database.Update(dto);
            }
        }

        private static KeyValueDto? Map(IKeyValue keyValue)
        {
            if (keyValue == null) return null;

            return new KeyValueDto
            {
                Key = keyValue.Identifier,
                Value = keyValue.Value,
                UpdateDate = keyValue.UpdateDate,
            };
        }

        private static IKeyValue? Map(KeyValueDto dto)
        {
            if (dto == null) return null;

            return new KeyValue
            {
                Identifier = dto.Key,
                Value = dto.Value,
                UpdateDate = dto.UpdateDate,
            };
        }

        #endregion
    }
}
