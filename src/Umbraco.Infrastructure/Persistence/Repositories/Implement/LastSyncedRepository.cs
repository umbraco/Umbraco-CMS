using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class LastSyncedRepository : RepositoryBase, ILastSyncedRepository
{
    private readonly IMachineInfoFactory  _machineInfoFactory;

    public LastSyncedRepository(IScopeAccessor scopeAccessor, AppCaches appCaches, IMachineInfoFactory machineInfoFactory)
        : base(scopeAccessor, appCaches)
    {
        _machineInfoFactory = machineInfoFactory;
    }


    public async Task<int?> GetInternal()
    {
        string machineName = _machineInfoFactory.GetMachineName();

        Sql<ISqlContext>? sql = Database.SqlContext.Sql()
            .Select<LastSyncedDto>(x => x.LastSyncedInternalId)
            .From<LastSyncedDto>()
            .Where<LastSyncedDto>(x => x.MachineId == machineName);

        return await Database.ExecuteScalarAsync<int?>(sql);
    }

    public async Task<int?> GetExternal()
    {
        string machineName = _machineInfoFactory.GetMachineName();

        Sql<ISqlContext>? sql = Database.SqlContext.Sql()
            .Select<LastSyncedDto>(x => x.LastSyncedExternalId)
            .From<LastSyncedDto>()
            .Where<LastSyncedDto>(x => x.MachineId == machineName);

        return await Database.ExecuteScalarAsync<int?>(sql);
    }

    public async Task SaveInternal(int id)
    {
        LastSyncedDto dto = new LastSyncedDto()
        {
            MachineId = _machineInfoFactory.GetMachineName(),
            LastSyncedInternalId = id,
            LastSyncedDate = DateTime.Now,
        };

        await Database.InsertOrUpdateAsync(
            dto,
            "SET LastSyncedInternalId=@LastSyncedInternalId, LastSyncedDate=@LastSyncedDate WHERE MachineId=@MachineId",
            new
            {
                dto.LastSyncedInternalId,
                dto.LastSyncedDate,
                dto.MachineId,
            });
    }

    public async Task SaveExternal(int id)
    {
        LastSyncedDto dto = new LastSyncedDto()
        {
            MachineId = _machineInfoFactory.GetMachineName(),
            LastSyncedExternalId = id,
            LastSyncedDate = DateTime.Now,
        };

        await Database.InsertOrUpdateAsync(
            dto,
            "SET LastSyncedExternalId=@LastSyncedExternalId, LastSyncedDate=@LastSyncedDate WHERE MachineId=@MachineId",
            new
            {
                dto.LastSyncedExternalId,
                dto.LastSyncedDate,
                dto.MachineId,
            });
    }

    public async Task DeleteEntriesOlderThan(DateTime pruneDate)
    {
        var maxId = Database.ExecuteScalar<int>($"SELECT MAX(Id) FROM umbracoCacheInstruction;");

        Sql sql =
            new Sql().Append(
                @"DELETE FROM umbracoLastSynced WHERE LastSyncedDate < @pruneDate OR LastSyncedInternalId > @maxId AND LastSyncedExternalId > @maxId;",
                new { pruneDate, maxId });

        await Database.ExecuteAsync(sql);
    }
}
