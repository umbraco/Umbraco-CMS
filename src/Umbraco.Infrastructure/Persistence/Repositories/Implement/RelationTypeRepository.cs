using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Cache;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Extensions;
using EFCoreRelationTypeDto = Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.RelationTypeDto;
using EFCoreRelationDto = Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.RelationDto;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="RelationType" />
/// </summary>
internal sealed class RelationTypeRepository : AsyncEntityRepositoryBase<int, IRelationType>, IRelationTypeRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelationTypeRepository"/> class.
    /// </summary>
    public RelationTypeRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches cache,
        ILogger<RelationTypeRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            cache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }

    protected override IAsyncRepositoryCachePolicy<IRelationType, int> CreateCachePolicy() =>
        new AsyncFullDataSetRepositoryCachePolicy<IRelationType, int>(
            GlobalIsolatedCache,
            ScopeAccessor,
            RepositoryCacheVersionService,
            CacheSyncService,
            entity => entity.Id,
            /*expires:*/ true);

    /// <inheritdoc />
    public Task<IRelationType?> GetAsync(Guid key, CancellationToken cancellationToken = default)
        => AmbientScope.ExecuteWithContextAsync(async db =>
        {
            EFCoreRelationTypeDto? dto = await db.RelationTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UniqueId == key, cancellationToken);
            return dto is null ? null : RelationTypeFactory.BuildEntity(dto);
        });

    /// <inheritdoc />
    public Task<IRelationType?> GetByAliasAsync(string alias, CancellationToken cancellationToken = default)
        => AmbientScope.ExecuteWithContextAsync(async db =>
        {
            EFCoreRelationTypeDto? dto = await db.RelationTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Alias == alias, cancellationToken);
            return dto is null ? null : RelationTypeFactory.BuildEntity(dto);
        });

    /// <inheritdoc />
    protected override Task<IRelationType?> PerformGetAsync(int key)
        => AmbientScope.ExecuteWithContextAsync(async db =>
        {
            EFCoreRelationTypeDto? dto = await db.RelationTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == key);
            return dto is null ? null : RelationTypeFactory.BuildEntity(dto);
        });

    /// <inheritdoc />
    protected override Task<IEnumerable<IRelationType>?> PerformGetAllAsync()
        => AmbientScope.ExecuteWithContextAsync<IEnumerable<IRelationType>?>(async db =>
        {
            List<EFCoreRelationTypeDto> dtos = await db.RelationTypes
                .AsNoTracking()
                .ToListAsync();
            return dtos.Select(RelationTypeFactory.BuildEntity).ToList();
        });

    /// <inheritdoc />
    protected override Task<IEnumerable<IRelationType>?> PerformGetManyAsync(int[]? keys)
        => AmbientScope.ExecuteWithContextAsync<IEnumerable<IRelationType>?>(async db =>
        {
            if (keys is null || keys.Length == 0)
            {
                List<EFCoreRelationTypeDto> all = await db.RelationTypes.AsNoTracking().ToListAsync();
                return all.Select(RelationTypeFactory.BuildEntity).ToList();
            }

            List<EFCoreRelationTypeDto> dtos = await db.RelationTypes
                .AsNoTracking()
                .Where(x => keys.Contains(x.Id))
                .ToListAsync();
            return dtos.Select(RelationTypeFactory.BuildEntity).ToList();
        });

    /// <inheritdoc />
    protected override Task<bool> PerformExistsAsync(int key)
        => AmbientScope.ExecuteWithContextAsync(async db =>
            await db.RelationTypes.AnyAsync(x => x.Id == key));

    /// <inheritdoc />
    protected override async Task PersistNewItemAsync(IRelationType item) =>
        await AmbientScope.ExecuteWithContextAsync<EFCoreRelationTypeDto>(async db =>
        {
            item.AddingEntity();
            CheckNullObjectTypeValues(item);

            EFCoreRelationTypeDto dto = RelationTypeFactory.BuildEFCoreDto(item);
            await db.RelationTypes.AddAsync(dto);
            await db.SaveChangesAsync();

            item.Id = dto.Id;
            item.ResetDirtyProperties();
        });

    /// <inheritdoc />
    protected override async Task PersistUpdatedItemAsync(IRelationType item) =>
        await AmbientScope.ExecuteWithContextAsync<EFCoreRelationTypeDto>(async db =>
        {
            item.UpdatingEntity();
            CheckNullObjectTypeValues(item);

            EFCoreRelationTypeDto dto = RelationTypeFactory.BuildEFCoreDto(item);
            db.RelationTypes.Update(dto);
            await db.SaveChangesAsync();

            item.ResetDirtyProperties();
        });

    /// <inheritdoc />
    protected override async Task PersistDeletedItemAsync(IRelationType entity) =>
        await AmbientScope.ExecuteWithContextAsync<EFCoreRelationTypeDto>(async db =>
        {
            // Cascade-delete relations that reference this relation type, mirroring the previous NPoco behavior.
            await db.Set<EFCoreRelationDto>()
                .Where(x => x.RelationType == entity.Id)
                .ExecuteDeleteAsync();

            await db.RelationTypes
                .Where(x => x.Id == entity.Id)
                .ExecuteDeleteAsync();

            entity.DeleteDate = DateTime.UtcNow;
        });

    private static void CheckNullObjectTypeValues(IRelationType entity)
    {
        if (entity.ParentObjectType.HasValue && entity.ParentObjectType == Guid.Empty)
        {
            entity.ParentObjectType = null;
        }

        if (entity.ChildObjectType.HasValue && entity.ChildObjectType == Guid.Empty)
        {
            entity.ChildObjectType = null;
        }
    }
}
