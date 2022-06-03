using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    internal class LogViewerQueryRepository : EntityRepositoryBase<int, ILogViewerQuery>, ILogViewerQueryRepository
    {
        public LogViewerQueryRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<LogViewerQueryRepository> logger)
            : base(scopeAccessor, cache, logger)
        { }

        protected override IRepositoryCachePolicy<ILogViewerQuery, int> CreateCachePolicy()
        {
            return new FullDataSetRepositoryCachePolicy<ILogViewerQuery, int>(GlobalIsolatedCache, ScopeAccessor, GetEntityId, /*expires:*/ false);
        }

        protected override IEnumerable<ILogViewerQuery> PerformGetAll(params int[]? ids)
        {
            Sql<ISqlContext> sql = GetAllSql(ids);

            return Database.Fetch<LogViewerQueryDto>(sql).Select(ConvertFromDto);
        }
        protected override async Task<IEnumerable<ILogViewerQuery>> PerformGetAllAsync(params int[]? ids)
        {
            Sql<ISqlContext> sql = GetAllSql(ids);

            return (await Database.FetchAsync<LogViewerQueryDto>(sql)).Select(ConvertFromDto);
        }

        private Sql<ISqlContext> GetAllSql(int[]? ids)
        {
            var sql = GetBaseQuery(false).Where($"{Cms.Core.Constants.DatabaseSchema.Tables.LogViewerQuery}.id > 0");
            if (ids?.Any() ?? false)
            {
                sql.Where($"{Cms.Core.Constants.DatabaseSchema.Tables.LogViewerQuery}.id in (@ids)", new { ids = ids });
            }

            return sql;
        }

        protected override IEnumerable<ILogViewerQuery> PerformGetByQuery(IQuery<ILogViewerQuery> query)
        {
            throw new NotSupportedException("This repository does not support this method");
        }

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            var sql = Sql();
            sql = isCount ? sql.SelectCount() : sql.Select<LogViewerQueryDto>();
            sql = sql.From<LogViewerQueryDto>();
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return $"{Cms.Core.Constants.DatabaseSchema.Tables.LogViewerQuery}.id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                {
                    $"DELETE FROM {Cms.Core.Constants.DatabaseSchema.Tables.LogViewerQuery} WHERE id = @id"
                };
            return list;
        }

        protected override void PersistNewItem(ILogViewerQuery entity)
        {
            var exists = Database.ExecuteScalar<int>($"SELECT COUNT(*) FROM {Core.Constants.DatabaseSchema.Tables.LogViewerQuery} WHERE name = @name",
                new { name = entity.Name });
            if (exists > 0) throw new DuplicateNameException($"The log query name '{entity.Name}' is already used");

            entity.AddingEntity();

            var factory = new LogViewerQueryModelFactory();
            var dto = factory.BuildDto(entity);

            var id = Convert.ToInt32(Database.Insert(dto));
            entity.Id = id;
        }

        protected override void PersistUpdatedItem(ILogViewerQuery entity)
        {
            entity.UpdatingEntity();

            var exists = Database.ExecuteScalar<int>($"SELECT COUNT(*) FROM {Core.Constants.DatabaseSchema.Tables.LogViewerQuery} WHERE name = @name AND id <> @id",
                new { name = entity.Name, id = entity.Id });
            //ensure there is no other log query with the same name on another entity
            if (exists > 0) throw new DuplicateNameException($"The log query name '{entity.Name}' is already used");


            var factory = new LogViewerQueryModelFactory();
            var dto = factory.BuildDto(entity);

            Database.Update(dto);
        }

        private ILogViewerQuery ConvertFromDto(LogViewerQueryDto dto)
        {
            var factory = new LogViewerQueryModelFactory();
            var entity = factory.BuildEntity(dto);
            return entity;
        }

        internal class LogViewerQueryModelFactory
        {

            public ILogViewerQuery BuildEntity(LogViewerQueryDto dto)
            {
                var logViewerQuery = new LogViewerQuery(dto.Name, dto.Query)
                {
                    Id = dto.Id,
                };
                return logViewerQuery;
            }

            public LogViewerQueryDto BuildDto(ILogViewerQuery entity)
            {
                var dto = new LogViewerQueryDto { Name = entity.Name, Query = entity.Query, Id = entity.Id };
                return dto;
            }
        }

        protected override ILogViewerQuery? PerformGet(int id)
        {
            //use the underlying GetAll which will force cache all log queries
            return GetMany()?.FirstOrDefault(x => x.Id == id);
        }

        public ILogViewerQuery? GetByName(string name)
        {
            //use the underlying GetAll which will force cache all log queries
            return GetMany()?.FirstOrDefault(x => x.Name == name);
        }
    }
}
