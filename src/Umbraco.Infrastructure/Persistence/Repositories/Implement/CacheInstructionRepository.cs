using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents the NPoco implementation of <see cref="ICacheInstructionRepository"/>.
    /// </summary>
    internal class CacheInstructionRepository : IAsyncCacheInstructionRepository
    {
        private readonly IScopeAccessor _scopeAccessor;

        public CacheInstructionRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

        /// <inheritdoc/>
        private Scoping.IScope? AmbientScope => _scopeAccessor.AmbientScope;

        /// <inheritdoc/>
        public int CountAll()
        {
            Sql<ISqlContext>? sql = CountAllSql();

            return AmbientScope?.Database.ExecuteScalar<int>(sql) ?? 0;
        }

        /// <inheritdoc/>
        public async Task<int> CountAllAsync()
        {
            Sql<ISqlContext>? sql = CountAllSql();
            if(AmbientScope == null)
            {
                return 0;
            }
            return await AmbientScope.Database.ExecuteScalarAsync<int>(sql);
        }

        private Sql<ISqlContext>? CountAllSql() => AmbientScope?.SqlContext.Sql().Select("COUNT(*)")
                        .From<CacheInstructionDto>();

        /// <inheritdoc/>
        public int CountPendingInstructions(int lastId)
        {
            string sql = CountPendingInstructionsSql();
            return AmbientScope?.Database.ExecuteScalar<int>(sql, new { lastId }) ?? 0;
        }

        /// <inheritdoc/>
        public async Task<int> CountPendingInstructionsAsync(int lastId)
        {
            string sql = CountPendingInstructionsSql();
            if (AmbientScope == null)
            {
                return 0;
            }
            return await AmbientScope.Database.ExecuteScalarAsync<int>(sql, new { lastId });
        }

        private static string CountPendingInstructionsSql() => "SELECT SUM(instructionCount) FROM umbracoCacheInstruction WHERE id > @lastId";

        /// <inheritdoc/>
        public int GetMaxId()
        {
            string sql = GetMaxIdSql();
            return AmbientScope?.Database.ExecuteScalar<int>(sql) ?? 0;
        }

        /// <inheritdoc/>
        public async Task<int> GetMaxIdAsync()
        {
            string sql = GetMaxIdSql();
            if (AmbientScope == null)
            {
                return 0;
            }
            return await AmbientScope.Database.ExecuteScalarAsync<int>(sql);
        }

        private static string GetMaxIdSql() => "SELECT MAX(id) FROM umbracoCacheInstruction";

        /// <inheritdoc/>
        public bool Exists(int id) => AmbientScope?.Database.Exists<CacheInstructionDto>(id) ?? false;

        /// <inheritdoc/>
        public Task<bool> ExistsAsync(int id)
        {
            return Task.FromResult(Exists(id));
        }

        /// <inheritdoc/>
        public void Add(CacheInstruction cacheInstruction)
        {
            CacheInstructionDto dto = CacheInstructionFactory.BuildDto(cacheInstruction);
            AmbientScope?.Database.Insert(dto);
        }

        /// <inheritdoc/>
        public async Task AddAsync(CacheInstruction cacheInstruction)
        {
            CacheInstructionDto dto = CacheInstructionFactory.BuildDto(cacheInstruction);
            if (AmbientScope == null)
            {
                return;
            }
            await AmbientScope.Database.InsertAsync(dto);
        }

        /// <inheritdoc/>
        public IEnumerable<CacheInstruction> GetPendingInstructions(int lastId, int maxNumberToRetrieve)
        {
            Sql<ISqlContext>? topSql = GetTopPendingInstructionsSql(lastId, maxNumberToRetrieve);
            return AmbientScope?.Database.Fetch<CacheInstructionDto>(topSql).Select(CacheInstructionFactory.BuildEntity) ?? Array.Empty<CacheInstruction>();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<CacheInstruction>> GetPendingInstructionsAsync(int lastId, int maxNumberToRetrieve)
        {
            Sql<ISqlContext>? topSql = GetTopPendingInstructionsSql(lastId, maxNumberToRetrieve);
            if (AmbientScope == null)
            {
                return Array.Empty<CacheInstruction>();
            }
            return (await AmbientScope.Database.FetchAsync<CacheInstructionDto>(topSql)).Select(CacheInstructionFactory.BuildEntity);
        }

        private Sql<ISqlContext>? GetTopPendingInstructionsSql(int lastId, int maxNumberToRetrieve)
        {
            Sql<ISqlContext>? sql = AmbientScope?.SqlContext.Sql().SelectAll()
                .From<CacheInstructionDto>()
                .Where<CacheInstructionDto>(dto => dto.Id > lastId)
                .OrderBy<CacheInstructionDto>(dto => dto.Id);
            Sql<ISqlContext>? topSql = sql?.SelectTop(maxNumberToRetrieve);
            return topSql;
        }

        /// <inheritdoc/>
        public void DeleteInstructionsOlderThan(DateTime pruneDate)
        {
            // Using 2 queries is faster than convoluted joins.
            var maxIdSql = GetMaxIdSql();
            var maxId = AmbientScope?.Database.ExecuteScalar<int>(maxIdSql);
            Sql deleteSql = DeleteInstructionsSql(pruneDate, maxId);
            AmbientScope?.Database.Execute(deleteSql);
        }

        /// <inheritdoc/>
        public async Task DeleteInstructionsOlderThanAsync(DateTime pruneDate)
        {
            // Using 2 queries is faster than convoluted joins.
            var maxIdSql = GetMaxIdSql();
            var maxId = AmbientScope?.Database.ExecuteScalar<int>(maxIdSql);
            Sql deleteSql = DeleteInstructionsSql(pruneDate, maxId);
            if (AmbientScope == null)
            {
                return;
            }
            await AmbientScope.Database.ExecuteAsync(deleteSql);
        }

        private static Sql DeleteInstructionsSql(DateTime pruneDate, int? maxId) => new Sql().Append(@"DELETE FROM umbracoCacheInstruction WHERE utcStamp < @pruneDate AND id < @maxId", new { pruneDate, maxId });
    }
}
