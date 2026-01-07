using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <inheritdoc cref="ILastSyncedRepository"/>
public class LastSyncedRepository : RepositoryBase, ILastSyncedRepository
{
    private readonly IMachineInfoFactory  _machineInfoFactory;

    public LastSyncedRepository(IScopeAccessor scopeAccessor, AppCaches appCaches, IMachineInfoFactory machineInfoFactory)
        : base(scopeAccessor, appCaches)
    {
        _machineInfoFactory = machineInfoFactory;
    }


    /// <inheritdoc />
    public async Task<int?> GetInternalIdAsync()
    {
        string machineName = _machineInfoFactory.GetMachineIdentifier();

        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<LastSyncedDto>(x => x.LastSyncedInternalId)
            .From<LastSyncedDto>()
            .Where<LastSyncedDto>(x => x.MachineId == machineName);

        return await Database.ExecuteScalarAsync<int?>(sql);
    }

    /// <inheritdoc />
    public async Task<int?> GetExternalIdAsync()
    {
        string machineName = _machineInfoFactory.GetMachineIdentifier();

        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select<LastSyncedDto>(x => x.LastSyncedExternalId)
            .From<LastSyncedDto>()
            .Where<LastSyncedDto>(x => x.MachineId == machineName);

        return await Database.ExecuteScalarAsync<int?>(sql);
    }

    /// <inheritdoc />
    public async Task SaveInternalIdAsync(int id)
    {
        LastSyncedDto dto = new LastSyncedDto()
        {
            MachineId = _machineInfoFactory.GetMachineIdentifier(),
            LastSyncedInternalId = id,
            LastSyncedDate = DateTime.Now,
        };

        await Database.InsertOrUpdateAsync(
            dto,
            $"SET {QuoteColumnName("lastSyncedInternalId")}=@LastSyncedInternalId, {QuoteColumnName("lastSyncedDate")}=@LastSyncedDate WHERE {QuoteColumnName("machineId")}=@MachineId",
            new
            {
                dto.LastSyncedInternalId,
                dto.LastSyncedDate,
                dto.MachineId,
            });
    }

    /// <inheritdoc />
    public async Task SaveExternalIdAsync(int id)
    {
        LastSyncedDto dto = new LastSyncedDto()
        {
            MachineId = _machineInfoFactory.GetMachineIdentifier(),
            LastSyncedExternalId = id,
            LastSyncedDate = DateTime.Now,
        };

        await Database.InsertOrUpdateAsync(
            dto,
            $"SET {QuoteColumnName("lastSyncedExternalId")}=@LastSyncedExternalId, {QuoteColumnName("lastSyncedDate")}=@LastSyncedDate WHERE {QuoteColumnName("machineId")}=@MachineId",
            new
            {
                dto.LastSyncedExternalId,
                dto.LastSyncedDate,
                dto.MachineId,
            });
    }

    /// <inheritdoc />
    public async Task DeleteEntriesOlderThanAsync(DateTime pruneDate)
    {
        var maxId = Database.ExecuteScalar<int>($"SELECT MAX(Id) FROM {QuoteTableName("umbracoCacheInstruction")};");

        Sql sql =
            new Sql().Append(
                @$"DELETE FROM {QuoteTableName("umbracoLastSynced")} WHERE {QuoteColumnName("lastSyncedDate")} < @pruneDate OR {QuoteColumnName("lastSyncedInternalId")} > @maxId AND {QuoteColumnName("lastSyncedExternalId")} > @maxId;",
                new { pruneDate, maxId });

        await Database.ExecuteAsync(sql);
    }
}
