using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class KeyValueRepository : AsyncEntityRepositoryBase<string, IKeyValue>, IKeyValueRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValueRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for repository operations.</param>
    /// <param name="logger">The logger used to record diagnostic and operational information for this repository.</param>
    /// <param name="repositoryCacheVersionService">Service used to manage cache versioning for repository data.</param>
    /// <param name="cacheSyncService">Service responsible for synchronizing cache across distributed environments.</param>
    public KeyValueRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        ILogger<KeyValueRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            AppCaches.NoCache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, string?>?> FindByKeyPrefixAsync(string keyPrefix)
    {
        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            Dictionary<string, string?> result = await db.KeyValue
                .Where(x => x.Key.StartsWith(keyPrefix))
                .ToDictionaryAsync(x => x.Key, x => x.Value);

            return result;
        });
    }

    #region Overrides of AsyncEntityRepositoryBase<string, IKeyValue>

    /// <inheritdoc/>
    protected override async Task<IKeyValue?> PerformGetAsync(string? id) =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            KeyValueDto? dto = await db.KeyValue
                .Where(x => x.Key == id)
                .FirstOrDefaultAsync();

            return dto is null ? null : Map(dto);
        });

    /// <inheritdoc/>
    protected override async Task<IEnumerable<IKeyValue>?> PerformGetAllAsync() =>
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<KeyValueDto> dtos = await db.KeyValue
                .ToListAsync();

            return dtos
                .Select(Map)
                .WhereNotNull()
                .AsEnumerable();
        });

    /// <inheritdoc/>
    protected override async Task<IEnumerable<IKeyValue>?> PerformGetManyAsync(string[]? ids)
    {
        if (ids is null)
        {
            return null;
        }

        return await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<KeyValueDto> dtos = await db.KeyValue
                .Where(x => ids.Any(id => id == x.Key))
                .ToListAsync();

            return dtos
                .Select(Map)
                .WhereNotNull()
                .AsEnumerable();
        });
    }

    /// <inheritdoc/>
    protected override async Task PersistNewItemAsync(IKeyValue entity) =>
        await AmbientScope.ExecuteWithContextAsync<KeyValueDto>(async db =>
        {
            KeyValueDto? dto = Map(entity);
            if (dto is not null)
            {
                await db.KeyValue
                    .AddAsync(dto);

                await db.SaveChangesAsync();
            }
        });

    /// <inheritdoc/>
    protected override async Task PersistUpdatedItemAsync(IKeyValue entity) =>
        await AmbientScope.ExecuteWithContextAsync<KeyValueDto>(async db =>
        {
            KeyValueDto? dto = Map(entity);
            if (dto is not null)
            {
                await db.UpsertAsync(dto, () =>
                    db.KeyValue
                        .Where(x => x.Key == dto.Key)
                        .ExecuteUpdateAsync(setter => setter
                            .SetProperty(x => x.Value, dto.Value)
                            .SetProperty(x => x.UpdateDate, dto.UpdateDate)));
            }
        });

    #endregion

    #region Mapping

    private static KeyValueDto? Map(IKeyValue? keyValue)
    {
        if (keyValue == null)
        {
            return null;
        }

        return new KeyValueDto { Key = keyValue.Identifier, Value = keyValue.Value, UpdateDate = keyValue.UpdateDate };
    }

    private static IKeyValue? Map(KeyValueDto? dto)
    {
        if (dto == null)
        {
            return null;
        }

        return new KeyValue { Identifier = dto.Key, Value = dto.Value, UpdateDate = dto.UpdateDate };
    }

    #endregion
}
