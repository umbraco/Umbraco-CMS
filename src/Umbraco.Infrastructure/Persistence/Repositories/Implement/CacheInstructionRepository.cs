using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents the NPoco implementation of <see cref="ICacheInstructionRepository"/>.
    /// </summary>
    internal class CacheInstructionRepository : ICacheInstructionRepository
    {
        /// <inheritdoc/>
        public int CountAll(IScope scope)
        {
            Sql<ISqlContext> sql = scope.SqlContext.Sql().Select("COUNT(*)")
                .From<CacheInstructionDto>();

            return scope.Database.ExecuteScalar<int>(sql);
        }

        /// <inheritdoc/>
        public int CountPendingInstructions(IScope scope, int lastId) =>
            scope.Database.ExecuteScalar<int>("SELECT SUM(instructionCount) FROM umbracoCacheInstruction WHERE id > @lastId", new { lastId });

        /// <inheritdoc/>
        public int GetMaxId(IScope scope) =>
            scope.Database.ExecuteScalar<int>("SELECT MAX(id) FROM umbracoCacheInstruction");

        /// <inheritdoc/>
        public bool Exists(IScope scope, int id) => scope.Database.Exists<CacheInstructionDto>(id);

        /// <inheritdoc/>
        public void Add(IScope scope, CacheInstruction cacheInstruction)
        {
            CacheInstructionDto dto = CacheInstructionFactory.BuildDto(cacheInstruction);
            scope.Database.Insert(dto);
        }

        /// <inheritdoc/>
        public IEnumerable<CacheInstruction> GetPendingInstructions(IScope scope, int lastId, int maxNumberToRetrieve)
        {
            Sql<ISqlContext> sql = scope.SqlContext.Sql().SelectAll()
                .From<CacheInstructionDto>()
                .Where<CacheInstructionDto>(dto => dto.Id > lastId)
                .OrderBy<CacheInstructionDto>(dto => dto.Id);
            Sql<ISqlContext> topSql = sql.SelectTop(maxNumberToRetrieve);
            return scope.Database.Fetch<CacheInstructionDto>(topSql).Select(CacheInstructionFactory.BuildEntity);
        }

        /// <inheritdoc/>
        public void DeleteInstructionsOlderThan(IScope scope, DateTime pruneDate)
        {
            // Using 2 queries is faster than convoluted joins.
            var maxId = scope.Database.ExecuteScalar<int>("SELECT MAX(id) FROM umbracoCacheInstruction;");
            Sql deleteSql = new Sql().Append(@"DELETE FROM umbracoCacheInstruction WHERE utcStamp < @pruneDate AND id < @maxId", new { pruneDate, maxId });
            scope.Database.Execute(deleteSql);
        }
    }
}
