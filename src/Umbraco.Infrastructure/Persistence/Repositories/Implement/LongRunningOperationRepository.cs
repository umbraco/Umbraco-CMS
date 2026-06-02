using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// Repository for managing long-running operations.
/// </summary>
internal class LongRunningOperationRepository : AsyncRepositoryBase, ILongRunningOperationRepository
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="LongRunningOperationRepository"/> class.
    /// </summary>
    public LongRunningOperationRepository(
        IJsonSerializer jsonSerializer,
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches appCaches,
        TimeProvider timeProvider)
        : base(scopeAccessor, appCaches)
    {
        _jsonSerializer = jsonSerializer;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public async Task CreateAsync(LongRunningOperation operation, DateTimeOffset expirationDate)
    {
        LongRunningOperationDto dto = MapEntityToDto(operation, expirationDate);

        await AmbientScope.ExecuteWithContextAsync<LongRunningOperationDto>(async db =>
        {
            EntityEntry<LongRunningOperationDto> entry = db.LongRunningOperations.Add(dto);
            await db.SaveChangesAsync();

            // Detaching the entity from the change tracker here to get consistent exceptions in case of duplicates.
            // If not detached, the same scope that created it will throw a InvalidOperationException and a different
            // scope will throw DbUpdateException.
            entry.State = EntityState.Detached;
        });
    }

    /// <inheritdoc />
    public async Task<LongRunningOperation?> GetAsync(Guid id) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            LongRunningOperationDto? dto = await db.LongRunningOperations
                .FirstOrDefaultAsync(x => x.Id == id);

            return dto is null ? null : MapDtoToEntity(dto);
        });

    /// <inheritdoc />
    public async Task<LongRunningOperation<T>?> GetAsync<T>(Guid id) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            LongRunningOperationDto? dto = await db.LongRunningOperations
                .FirstOrDefaultAsync(x => x.Id == id);

            return dto is null ? null : MapDtoToEntity<T>(dto);
        });

    /// <inheritdoc/>
    public async Task<PagedModel<LongRunningOperation>> GetByTypeAsync(
        string type,
        LongRunningOperationStatus[] statuses,
        int skip,
        int take)
    {
        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            IQueryable<LongRunningOperationDto> query = db.LongRunningOperations
                .Where(x => x.Type == type);

            if (statuses.Length > 0)
            {
                var includeStale = statuses.Contains(LongRunningOperationStatus.Stale);
                var possibleStaleStatuses = new List<string>
                {
                    nameof(LongRunningOperationStatus.Enqueued),
                    nameof(LongRunningOperationStatus.Running)
                };
                var statusList = statuses.Except([LongRunningOperationStatus.Stale]).Select(s => s.ToString()).ToList();

                DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
                query = query.Where(x =>
                    (statusList.Contains(x.Status) && (!possibleStaleStatuses.Contains(x.Status) || x.ExpirationDate >= now))
                    || (includeStale && possibleStaleStatuses.Contains(x.Status) && x.ExpirationDate < now));
            }

            int total = await query.CountAsync();

            List<LongRunningOperationDto> dtos = await query
                .OrderBy(x => x.CreateDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return new PagedModel<LongRunningOperation>(total, dtos.Select(MapDtoToEntity));
        });
    }

    /// <inheritdoc/>
    public async Task<LongRunningOperationStatus?> GetStatusAsync(Guid id) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            var status = await db.LongRunningOperations
                .Where(x => x.Id == id)
                .Select(x => x.Status)
                .FirstOrDefaultAsync();

            return status?.EnumParse<LongRunningOperationStatus>(false);
        });

    /// <inheritdoc/>
    public async Task UpdateStatusAsync(Guid id, LongRunningOperationStatus status, DateTimeOffset expirationTime) =>
        await AmbientScope.ExecuteWithContextAsync<LongRunningOperationDto>(async db =>
        {
            await db.LongRunningOperations
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(setter => setter
                        .SetProperty(y => y.Status, status.ToString())
                        .SetProperty(y => y.UpdateDate, DateTime.UtcNow)
                        .SetProperty(y => y.ExpirationDate, expirationTime.DateTime));
        });

    /// <inheritdoc/>
    public async Task SetResultAsync<T>(Guid id, T result) =>
        await AmbientScope.ExecuteWithContextAsync<LongRunningOperationDto>(async db =>
        {
            await db.LongRunningOperations
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(y => y.Result, _jsonSerializer.Serialize(result))
                    .SetProperty(y => y.UpdateDate, DateTime.UtcNow));
        });

    /// <inheritdoc/>
    public async Task CleanOperationsAsync(DateTimeOffset olderThan) =>
        await AmbientScope.ExecuteWithContextAsync<LongRunningOperationDto>(async db =>
        {
            await db.LongRunningOperations
                .Where(x => x.UpdateDate < olderThan.UtcDateTime)
                .ExecuteDeleteAsync();
        });

    private LongRunningOperation MapDtoToEntity(LongRunningOperationDto dto) =>
        new()
        {
            Id = dto.Id,
            Type = dto.Type,
            Status = DetermineStatus(dto),
        };

    private LongRunningOperation<T> MapDtoToEntity<T>(LongRunningOperationDto dto) =>
        new()
        {
            Id = dto.Id,
            Type = dto.Type,
            Status = DetermineStatus(dto),
            Result = dto.Result == null ? default : _jsonSerializer.Deserialize<T>(dto.Result),
        };

    private static LongRunningOperationDto MapEntityToDto(LongRunningOperation entity, DateTimeOffset expirationTime) =>
        new()
        {
            Id = entity.Id,
            Type = entity.Type,
            Status = entity.Status.ToString(),
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            ExpirationDate = expirationTime.UtcDateTime,
        };

    private LongRunningOperationStatus DetermineStatus(LongRunningOperationDto dto)
    {
        LongRunningOperationStatus status = dto.Status.EnumParse<LongRunningOperationStatus>(false);
        DateTimeOffset now = _timeProvider.GetUtcNow();
        if (status is LongRunningOperationStatus.Enqueued or LongRunningOperationStatus.Running
            && now.UtcDateTime >= dto.ExpirationDate)
        {
            status = LongRunningOperationStatus.Stale;
        }

        return status;
    }
}
