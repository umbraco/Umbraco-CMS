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
        Sql<ISqlContext>? sql = Database.SqlContext.Sql()
            .Select<LastSyncedDto>(x => x.LastSyncedInternalId)
            .From<LastSyncedDto>()
            .Where<LastSyncedDto>(x => x.MachineId == _machineInfoFactory.GetMachineName());

        return await Database.ExecuteScalarAsync<int?>(sql);
    }

    public async Task<int?> GetExternal()
    {
        Sql<ISqlContext>? sql = Database.SqlContext.Sql()
            .Select<LastSyncedDto>(x => x.LastSyncedExternalId)
            .From<LastSyncedDto>()
            .Where<LastSyncedDto>(x => x.MachineId == _machineInfoFactory.GetMachineName());

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
                lastSyncedInternalId = dto.LastSyncedInternalId,
                lastSyncedDate = dto.LastSyncedDate,
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
                lastSyncedInternalId = dto.LastSyncedExternalId,
                lastSyncedDate = dto.LastSyncedDate,
            });
    }
}
