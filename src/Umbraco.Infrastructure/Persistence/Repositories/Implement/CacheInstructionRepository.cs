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
internal sealed class CacheInstructionRepository : ICacheInstructionRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.CacheInstructionRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for repository operations.</param>
    public CacheInstructionRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    private IScope? AmbientScope => _scopeAccessor.AmbientScope;

    /// <inheritdoc />
    public int CountAll()
    {
        if (AmbientScope is null)
        {
            return 0;
        }

        Sql<ISqlContext>? sql = AmbientScope.SqlContext.Sql().SelectCount()
            .From<CacheInstructionDto>();

        return AmbientScope.Database.ExecuteScalar<int>(sql);
    }

    /// <inheritdoc />
    public int CountPendingInstructions(int lastId)
    {
        if (AmbientScope is null)
        {
            return 0;
        }

        Sql<ISqlContext>? sql = AmbientScope.SqlContext.Sql()
            .SelectSum<CacheInstructionDto>(c => c.InstructionCount)
            .From<CacheInstructionDto>()
            .Where<CacheInstructionDto>(dto => dto.Id > lastId);

        return AmbientScope.Database.ExecuteScalar<int>(sql);
    }

    /// <inheritdoc />
    public int GetMaxId()
    {
        if (AmbientScope is null)
        {
            return 0;
        }

        Sql<ISqlContext> sql = AmbientScope.SqlContext.Sql()
            .SelectMax<CacheInstructionDto>(c => c.Id)
            .From<CacheInstructionDto>();

        return AmbientScope.Database.ExecuteScalar<int>(sql);
    }

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
        if (AmbientScope is null)
        {
            return [];
        }

        Sql<ISqlContext> sql = AmbientScope.SqlContext.Sql().SelectAll()
            .From<CacheInstructionDto>()
            .Where<CacheInstructionDto>(dto => dto.Id > lastId)
            .OrderBy<CacheInstructionDto>(dto => dto.Id);
        Sql<ISqlContext> topSql = sql.SelectTop(maxNumberToRetrieve);
        return AmbientScope.Database.Fetch<CacheInstructionDto>(topSql).Select(CacheInstructionFactory.BuildEntity);
    }

    /// <inheritdoc />
    public void DeleteInstructionsOlderThan(DateTime pruneDate)
    {
        if (AmbientScope is null)
        {
            return;
        }

        // Using 2 queries is faster than convoluted joins.
        Sql<ISqlContext> sql = AmbientScope.SqlContext.Sql()
            .SelectMax<CacheInstructionDto>(c => c.Id)
            .From<CacheInstructionDto>();
        var maxId = AmbientScope.Database.ExecuteScalar<int>(sql);
        if (maxId == 0)
        {
            return; // No instructions to delete.
        }

        Sql<ISqlContext>? deleteSql = AmbientScope.SqlContext.Sql()
            .Delete<CacheInstructionDto>()
            .Where<CacheInstructionDto>(dto => dto.UtcStamp < pruneDate && dto.Id < maxId);

        AmbientScope.Database.Execute(deleteSql);
    }
}
