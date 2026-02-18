using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <inheritdoc cref="ILastSyncedRepository"/>
public class LastSyncedRepository : AsyncRepositoryBase, ILastSyncedRepository
{
    private readonly IMachineInfoFactory _machineInfoFactory;

    public LastSyncedRepository(IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor, AppCaches appCaches, IMachineInfoFactory machineInfoFactory)
        : base(scopeAccessor, appCaches) =>
        _machineInfoFactory = machineInfoFactory;


    /// <inheritdoc />
    public async Task<int?> GetInternalIdAsync()
    {
        string machineId = _machineInfoFactory.GetMachineIdentifier();

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            return await db.LastSynced
                .Where(x => x.MachineId == machineId)
                .Select(x => x.LastSyncedInternalId)
                .FirstOrDefaultAsync();
        });
    }

    /// <inheritdoc />
    public async Task<int?> GetExternalIdAsync()
    {
        string machineId = _machineInfoFactory.GetMachineIdentifier();

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            return await db.LastSynced
                .Where(x => x.MachineId == machineId)
                .Select(x => x.LastSyncedExternalId)
                .FirstOrDefaultAsync();
        });
    }

    /// <inheritdoc />
    public async Task SaveInternalIdAsync(int id)
    {
        var dto = new LastSyncedDto
        {
            MachineId = _machineInfoFactory.GetMachineIdentifier(),
            LastSyncedInternalId = id,
            LastSyncedDate = DateTime.Now,
        };

        await AmbientScope.ExecuteWithContextAsync<LastSyncedDto>(async db =>
        {
            // First we try to update the existing, if there is one.
            int rowsAffected = await db.LastSynced
                .Where(x => x.MachineId == dto.MachineId)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(x => x.LastSyncedInternalId, dto.LastSyncedInternalId)
                    .SetProperty(x => x.LastSyncedDate, dto.LastSyncedDate));

            // If one currently does not exist, we create it.
            if (rowsAffected == 0)
            {
                await db.LastSynced
                    .AddAsync(dto);

                await db.SaveChangesAsync();
            }
        });
    }

    /// <inheritdoc />
    public async Task SaveExternalIdAsync(int id)
    {
        var dto = new LastSyncedDto
        {
            MachineId = _machineInfoFactory.GetMachineIdentifier(),
            LastSyncedExternalId = id,
            LastSyncedDate = DateTime.Now,
        };

        await AmbientScope.ExecuteWithContextAsync<LastSyncedDto>(async db =>
        {
            // First we try to update the existing, if there is one.
            int rowsAffected = await db.LastSynced
                .Where(x => x.MachineId == dto.MachineId)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(x => x.LastSyncedExternalId, dto.LastSyncedExternalId)
                    .SetProperty(x => x.LastSyncedDate, dto.LastSyncedDate));

            // If one currently does not exist, we create it.
            if (rowsAffected == 0)
            {
                await db.LastSynced
                    .AddAsync(dto);

                await db.SaveChangesAsync();
            }
        });
    }

    /// <inheritdoc />
    public async Task DeleteEntriesOlderThanAsync(DateTime pruneDate)
    {
        await AmbientScope.ExecuteWithContextAsync<LastSyncedDto>(async db =>
        {
            int maxId = await db.CacheInstructions
                .Select(x => x.Id)
                .DefaultIfEmpty()
                .MaxAsync();

            await db.LastSynced
                .Where(x =>
                    x.LastSyncedDate < pruneDate ||
                    (x.LastSyncedInternalId > maxId &&
                    x.LastSyncedExternalId > maxId))
                .ExecuteDeleteAsync();
        });
    }
}
