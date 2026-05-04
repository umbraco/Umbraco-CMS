using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Extensions;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents the EFCore implementation of <see cref="IAuditEntryRepository" />.
/// </summary>
internal sealed class AuditEntryRepository : AsyncEntityRepositoryBase<int, IAuditEntry>, IAuditEntryRepository
{
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuditEntryRepository" /> class.
    /// </summary>
    public AuditEntryRepository(
        IRuntimeState runtimeState,
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches cache,
        ILogger<AuditEntryRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            cache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService)
    {
        _runtimeState = runtimeState;
    }

    /// <inheritdoc />
    public Task<PagedModel<IAuditEntry>> GetPageAsync(long pageIndex, int pageCount)
        => AmbientScope.ExecuteWithContextAsync(async db =>
        {
            long total = await db.AuditEntries.LongCountAsync();

            List<AuditEntryDto> dtos = await db.AuditEntries
                .OrderByDescending(x => x.EventDate)
                .BigSkip(pageIndex * pageCount)
                .Take(pageCount)
                .ToListAsync();

            return new PagedModel<IAuditEntry>(total, dtos.Select(AuditEntryFactory.BuildEntity));
        });

    /// <inheritdoc />
    protected override async Task<IAuditEntry?> PerformGetAsync(int id) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            AuditEntryDto? entry = await db.AuditEntries
                .FirstOrDefaultAsync(x => x.Id == id);

            return entry == null ? null : AuditEntryFactory.BuildEntity(entry);
        });

    /// <inheritdoc/>
    protected override async Task<IEnumerable<IAuditEntry>?> PerformGetAllAsync() =>
        await AmbientScope.ExecuteWithContextAsync<IEnumerable<IAuditEntry>>(async db =>
        {
            List<AuditEntryDto> entries = await db.AuditEntries
                .ToListAsync();

            return AuditEntryFactory.BuildEntities(entries);
        });

    /// <inheritdoc />
    protected override async Task<IEnumerable<IAuditEntry>?> PerformGetManyAsync(params int[]? ids)
    {
        var entries = new List<IAuditEntry>();

        foreach (IEnumerable<int> group in ids.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            await AmbientScope.ExecuteWithContextAsync<AuditEntryDto>(async db =>
            {
                List<AuditEntryDto> fetchedEntries = await db.AuditEntries
                    .Where(x => group.Contains(x.Id))
                    .ToListAsync();

                entries.AddRange(fetchedEntries.Select(AuditEntryFactory.BuildEntity));
            });
        }

        return entries;
    }

    /// <inheritdoc />
    protected override async Task PersistNewItemAsync(IAuditEntry entity)
    {
        entity.AddingEntity();
        AuditEntryDto dto = AuditEntryFactory.BuildDto(entity);

        try
        {
            await AmbientScope.ExecuteWithContextAsync<AuditEntryDto>(async db =>
            {
                db.AuditEntries.Add(dto);
                await db.SaveChangesAsync();
            });
        }
        catch (DbUpdateException) when (_runtimeState.Level is RuntimeLevel.Upgrade or RuntimeLevel.Upgrading)
        {
            await InsertWithoutUserKeysAsync(dto);
        }

        entity.Id = dto.Id;
        entity.ResetDirtyProperties();
    }

    /// <inheritdoc />
    protected override Task PersistUpdatedItemAsync(IAuditEntry entity) =>
        throw new NotSupportedException("Audit entries cannot be updated.");

    /// <summary>
    ///     Pre-migration the *UserKey columns don't exist on the table, so audit entries written
    ///     during the upgrade window must omit them.
    ///
    ///     It's not pretty. But it preserves the logic that was there before EF Core migration.
    /// </summary>
    // TODO (V22): Remove this method when 'V_17_0_0.AddGuidsToAuditEntries' is removed.
    private Task InsertWithoutUserKeysAsync(AuditEntryDto dto) =>
        AmbientScope.ExecuteWithContextAsync<AuditEntryDto>(async db =>
        {
            // Detach so EF Core doesn't retry the failed insert on a subsequent SaveChanges.
            db.Entry(dto).State = EntityState.Detached;

            const string sql = $$"""
                INSERT INTO {{AuditEntryDto.TableName}} (
                    {{AuditEntryDto.PerformingUserIdColumnName}},
                    {{AuditEntryDto.PerformingDetailsColumnName}},
                    {{AuditEntryDto.PerformingIpColumnName}},
                    {{AuditEntryDto.EventDateColumnName}},
                    {{AuditEntryDto.AffectedUserIdColumnName}},
                    {{AuditEntryDto.AffectedDetailsColumnName}},
                    {{AuditEntryDto.EventTypeColumnName}},
                    {{AuditEntryDto.EventDetailsColumnName}})
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})
                """;

            await db.Database.ExecuteSqlRawAsync(
                sql,
                dto.PerformingUserId,
                (object?)dto.PerformingDetails ?? DBNull.Value,
                (object?)dto.PerformingIp ?? DBNull.Value,
                dto.EventDate,
                dto.AffectedUserId,
                (object?)dto.AffectedDetails ?? DBNull.Value,
                (object?)dto.EventType ?? DBNull.Value,
                (object?)dto.EventDetails ?? DBNull.Value);
        });
}
