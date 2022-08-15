using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents the NPoco implementation of <see cref="ICacheInstructionRepository" />.
/// </summary>
internal class CacheInstructionRepository : ICacheInstructionRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public CacheInstructionRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    private IScope? AmbientScope => _scopeAccessor.AmbientScope;

    /// <inheritdoc />
    public int CountAll()
    {
        Sql<ISqlContext>? sql = AmbientScope?.SqlContext.Sql().Select("COUNT(*)")
            .From<CacheInstructionDto>();

        return AmbientScope?.Database.ExecuteScalar<int>(sql) ?? 0;
    }

    /// <inheritdoc />
    public int CountPendingInstructions(int lastId) =>
        AmbientScope?.Database.ExecuteScalar<int>(
            "SELECT SUM(instructionCount) FROM umbracoCacheInstruction WHERE id > @lastId", new { lastId }) ?? 0;

    /// <inheritdoc />
    public int GetMaxId() =>
        AmbientScope?.Database.ExecuteScalar<int>("SELECT MAX(id) FROM umbracoCacheInstruction") ?? 0;

    /// <inheritdoc />
    public bool Exists(int id) => AmbientScope?.Database.Exists<CacheInstructionDto>(id) ?? false;

    /// <inheritdoc />
    public void Add(CacheInstruction cacheInstruction)
    {
        CacheInstructionDto dto = CacheInstructionFactory.BuildDto(cacheInstruction);
        AmbientScope?.Database.Insert(dto);
    }

    /// <inheritdoc />
    public IEnumerable<CacheInstruction> GetPendingInstructions(int lastId, int maxNumberToRetrieve)
    {
        Sql<ISqlContext>? sql = AmbientScope?.SqlContext.Sql().SelectAll()
            .From<CacheInstructionDto>()
            .Where<CacheInstructionDto>(dto => dto.Id > lastId)
            .OrderBy<CacheInstructionDto>(dto => dto.Id);
        Sql<ISqlContext>? topSql = sql?.SelectTop(maxNumberToRetrieve);
        return AmbientScope?.Database.Fetch<CacheInstructionDto>(topSql).Select(CacheInstructionFactory.BuildEntity) ??
               Array.Empty<CacheInstruction>();
    }

    /// <inheritdoc />
    public void DeleteInstructionsOlderThan(DateTime pruneDate)
    {
        // Using 2 queries is faster than convoluted joins.
        var maxId = AmbientScope?.Database.ExecuteScalar<int>("SELECT MAX(id) FROM umbracoCacheInstruction;");
        Sql deleteSql =
            new Sql().Append(
                @"DELETE FROM umbracoCacheInstruction WHERE utcStamp < @pruneDate AND id < @maxId",
                new { pruneDate, maxId });
        AmbientScope?.Database.Execute(deleteSql);
    }
}
