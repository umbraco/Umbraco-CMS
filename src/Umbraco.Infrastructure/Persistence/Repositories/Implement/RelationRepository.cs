using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="IRelation"/>.
/// </summary>
internal sealed class RelationRepository : AsyncEntityRepositoryBase<int, IRelation>, IRelationRepository
{
    private readonly IRelationTypeRepository _relationTypeRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current EF Core database scope.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="relationTypeRepository">Repository for managing relation types.</param>
    /// <param name="repositoryCacheVersionService">Service responsible for managing cache versioning for repository data.</param>
    /// <param name="cacheSyncService">Service used to synchronize cache state across distributed environments.</param>
    public RelationRepository(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        ILogger<RelationRepository> logger,
        IRelationTypeRepository relationTypeRepository,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            AppCaches.NoCache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService) =>
        _relationTypeRepository = relationTypeRepository;

    /// <inheritdoc />
    public Task<IEnumerable<IRelation>> GetByParentIdAsync(
        int parentId,
        int? relationTypeId = null,
        CancellationToken cancellationToken = default)
        => GetByFilterAsync(
            q => relationTypeId.HasValue
                ? q.Where(x => x.ParentId == parentId && x.RelationType == relationTypeId.Value).OrderBy(x => x.RelationType)
                : q.Where(x => x.ParentId == parentId).OrderBy(x => x.RelationType),
            cancellationToken);

    /// <inheritdoc />
    public Task<IEnumerable<IRelation>> GetByChildIdAsync(
        int childId,
        int? relationTypeId = null,
        CancellationToken cancellationToken = default)
        => GetByFilterAsync(
            q => relationTypeId.HasValue
                ? q.Where(x => x.ChildId == childId && x.RelationType == relationTypeId.Value).OrderBy(x => x.RelationType)
                : q.Where(x => x.ChildId == childId).OrderBy(x => x.RelationType),
            cancellationToken);

    /// <inheritdoc />
    public Task<IEnumerable<IRelation>> GetByParentOrChildIdAsync(
        int id,
        int? relationTypeId = null,
        CancellationToken cancellationToken = default)
        => GetByFilterAsync(
            q => relationTypeId.HasValue
                ? q.Where(x => (x.ParentId == id || x.ChildId == id) && x.RelationType == relationTypeId.Value).OrderBy(x => x.RelationType)
                : q.Where(x => x.ParentId == id || x.ChildId == id).OrderBy(x => x.RelationType),
            cancellationToken);

    /// <inheritdoc />
    public async Task<IRelation?> GetByParentAndChildIdAsync(
        int parentId,
        int childId,
        int relationTypeId,
        CancellationToken cancellationToken = default)
        => await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            RelationDto? dto = await BaseQuery(db)
                .Where(x => x.ParentId == parentId && x.ChildId == childId && x.RelationType == relationTypeId)
                .FirstOrDefaultAsync(cancellationToken);

            return dto is null ? null : await BuildEntityAsync(dto);
        });

    /// <inheritdoc />
    public Task<IEnumerable<IRelation>> GetByRelationTypeIdAsync(
        int relationTypeId,
        CancellationToken cancellationToken = default)
        => GetByFilterAsync(
            q => q.Where(x => x.RelationType == relationTypeId),
            cancellationToken);

    /// <inheritdoc />
    public async Task<bool> IsRelatedAsync(
        int id,
        RelationDirectionFilter directionFilter,
        int[]? includeRelationTypeIds = null,
        int[]? excludeRelationTypeIds = null,
        CancellationToken cancellationToken = default)
        => await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            IQueryable<RelationDto> query = db.Relations.AsQueryable();

            query = directionFilter switch
            {
                RelationDirectionFilter.Parent => query.Where(x => x.ParentId == id),
                RelationDirectionFilter.Child => query.Where(x => x.ChildId == id),
                RelationDirectionFilter.Any => query.Where(x => x.ParentId == id || x.ChildId == id),
                _ => throw new ArgumentOutOfRangeException(nameof(directionFilter)),
            };

            if (includeRelationTypeIds is { Length: > 0 })
            {
                query = query.Where(x => includeRelationTypeIds.Contains(x.RelationType));
            }

            if (excludeRelationTypeIds is { Length: > 0 })
            {
                query = query.Where(x => excludeRelationTypeIds.Contains(x.RelationType) == false);
            }

            return await query.AnyAsync(cancellationToken);
        });

    /// <inheritdoc />
    public async Task<bool> AreRelatedAsync(
        int parentId,
        int childId,
        int? relationTypeId = null,
        CancellationToken cancellationToken = default)
        => await AmbientScope.ExecuteWithContextAsync(async db =>
            relationTypeId.HasValue
                ? await db.Relations.AnyAsync(
                    x => x.ParentId == parentId && x.ChildId == childId && x.RelationType == relationTypeId.Value,
                    cancellationToken)
                : await db.Relations.AnyAsync(
                    x => x.ParentId == parentId && x.ChildId == childId,
                    cancellationToken));

    /// <inheritdoc />
    public async Task<PagedModel<IRelation>> GetPagedByRelationTypeIdAsync(
        int relationTypeId,
        int skip,
        int take,
        Ordering? ordering = null,
        CancellationToken cancellationToken = default)
        => await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            IQueryable<RelationDto> query = BaseQuery(db).Where(x => x.RelationType == relationTypeId);

            int total = await query.CountAsync(cancellationToken);

            query = ApplyOrdering(query, ordering);

            List<RelationDto> page = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);

            return new PagedModel<IRelation>(total, await BuildEntitiesAsync(page));
        });

    /// <inheritdoc />
    public async Task<PagedModel<IRelation>> GetPagedByChildKeyAsync(
        Guid childKey,
        int skip,
        int take,
        string? relationTypeAlias,
        CancellationToken cancellationToken = default)
        => await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            IQueryable<RelationDto> query = BaseQuery(db)
                .Where(x => x.ChildNode!.UniqueId == childKey);

            if (string.IsNullOrEmpty(relationTypeAlias) is false)
            {
                query = query.Where(x => x.RelationTypeDto!.Alias == relationTypeAlias);
            }

            int total = await query.CountAsync(cancellationToken);

            List<RelationDto> page = await query
                .OrderBy(x => x.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return new PagedModel<IRelation>(total, await BuildEntitiesAsync(page));
        });

    /// <inheritdoc />
    public async Task SaveManyAsync(IEnumerable<IRelation> relations, CancellationToken cancellationToken = default) =>
        await AmbientScope.ExecuteWithContextAsync<RelationDto>(async db =>
        {
            foreach (IGrouping<bool, IRelation> hasIdentityGroup in relations.GroupBy(r => r.HasIdentity))
            {
                IRelation[] entities = hasIdentityGroup.ToArray();

                if (hasIdentityGroup.Key)
                {
                    foreach (IRelation entity in entities)
                    {
                        entity.UpdatingEntity();
                        RelationDto dto = RelationFactory.BuildDto(entity);
                        db.Relations.Update(dto);
                    }

                    await db.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var dtos = new List<RelationDto>(entities.Length);
                    foreach (IRelation entity in entities)
                    {
                        entity.AddingEntity();
                        dtos.Add(RelationFactory.BuildDto(entity));
                    }

                    await db.Relations.AddRangeAsync(dtos, cancellationToken);
                    await db.SaveChangesAsync(cancellationToken);

                    // Re-assign generated identifiers back onto the entities.
                    for (int i = 0; i < entities.Length; i++)
                    {
                        entities[i].Id = dtos[i].Id;
                    }
                }

                await PopulateObjectTypesAsync(db, entities, cancellationToken);

                foreach (IRelation entity in entities)
                {
                    entity.ResetDirtyProperties();
                }
            }
        });

    /// <inheritdoc />
    public async Task SaveBulkAsync(IEnumerable<ReadOnlyRelation> relations, CancellationToken cancellationToken = default) =>
        await AmbientScope.ExecuteWithContextAsync<RelationDto>(async db =>
        {
            foreach (IGrouping<bool, ReadOnlyRelation> hasIdentityGroup in relations.GroupBy(r => r.HasIdentity))
            {
                if (hasIdentityGroup.Key)
                {
                    foreach (ReadOnlyRelation entity in hasIdentityGroup)
                    {
                        RelationDto dto = RelationFactory.BuildDto(entity);
                        db.Relations.Update(dto);
                    }
                }
                else
                {
                    IEnumerable<RelationDto> dtos = hasIdentityGroup.Select(RelationFactory.BuildDto);
                    await db.Relations.AddRangeAsync(dtos, cancellationToken);
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        });

    /// <inheritdoc />
    public async Task DeleteByParentAsync(
        int parentId,
        string[]? relationTypeAliases = null,
        CancellationToken cancellationToken = default) =>
        await AmbientScope.ExecuteWithContextAsync<RelationDto>(async db =>
        {
            if (relationTypeAliases is { Length: > 0 })
            {
                int[] matchingRelationTypeIds = await db.RelationTypes
                    .Where(rt => relationTypeAliases.Contains(rt.Alias))
                    .Select(rt => rt.Id)
                    .ToArrayAsync(cancellationToken);

                if (matchingRelationTypeIds.Length == 0)
                {
                    return;
                }

                await db.Relations
                    .Where(x => x.ParentId == parentId && matchingRelationTypeIds.Contains(x.RelationType))
                    .ExecuteDeleteAsync(cancellationToken);
            }
            else
            {
                await db.Relations
                    .Where(x => x.ParentId == parentId)
                    .ExecuteDeleteAsync(cancellationToken);
            }
        });

    /// <inheritdoc />
    public async Task DeleteRelationsOfTypeAsync(int relationTypeId, CancellationToken cancellationToken = default) =>
        await AmbientScope.ExecuteWithContextAsync<RelationDto>(async db =>
            await db.Relations
                .Where(x => x.RelationType == relationTypeId)
                .ExecuteDeleteAsync(cancellationToken));

    /// <inheritdoc />
    /// <remarks>
    ///     TODO: Implement when <see cref="EntityRepository"/> is migrated to EF Core. The current NPoco implementation
    ///     paged across <c>umbracoNode</c> joined to relation rows; recreating that join over EF Core requires either
    ///     duplicating the entity-slim projection or coordinating with the existing NPoco entity repository.
    /// </remarks>
    public Task<PagedModel<IUmbracoEntity>> GetPagedParentEntitiesByChildIdAsync(
        int childId,
        int skip,
        int take,
        Guid[]? entityTypes = null,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "GetPagedParentEntitiesByChildIdAsync depends on EntityRepository being migrated to EF Core.");

    /// <inheritdoc />
    /// <remarks>
    ///     TODO: Implement when <see cref="EntityRepository"/> is migrated to EF Core. See <see cref="GetPagedParentEntitiesByChildIdAsync"/>.
    /// </remarks>
    public Task<PagedModel<IUmbracoEntity>> GetPagedChildEntitiesByParentIdAsync(
        int parentId,
        int skip,
        int take,
        Guid[]? entityTypes = null,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException(
            "GetPagedChildEntitiesByParentIdAsync depends on EntityRepository being migrated to EF Core.");

    /// <inheritdoc />
    protected override async Task<IRelation?> PerformGetAsync(int key)
        => await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            RelationDto? dto = await BaseQuery(db).FirstOrDefaultAsync(x => x.Id == key);
            return dto is null ? null : await BuildEntityAsync(dto);
        });

    /// <inheritdoc />
    protected override async Task<IEnumerable<IRelation>?> PerformGetAllAsync()
        => await AmbientScope.ExecuteWithContextAsync<IEnumerable<IRelation>?>(async db =>
        {
            List<RelationDto> dtos = await BaseQuery(db).OrderBy(x => x.RelationType).ToListAsync();
            return await BuildEntitiesAsync(dtos);
        });

    /// <inheritdoc />
    protected override async Task<IEnumerable<IRelation>?> PerformGetManyAsync(int[]? keys)
        => await AmbientScope.ExecuteWithContextAsync<IEnumerable<IRelation>?>(async db =>
        {
            if (keys is null || keys.Length == 0)
            {
                return Array.Empty<IRelation>();
            }

            List<RelationDto> dtos = await BaseQuery(db)
                .Where(x => keys.Contains(x.Id))
                .ToListAsync();

            return await BuildEntitiesAsync(dtos);
        });

    /// <inheritdoc />
    protected override async Task<bool> PerformExistsAsync(int key)
        => await AmbientScope.ExecuteWithContextAsync(async db =>
            await db.Relations.AnyAsync(x => x.Id == key));

    /// <inheritdoc />
    protected override async Task PersistNewItemAsync(IRelation item) =>
        await AmbientScope.ExecuteWithContextAsync<RelationDto>(async db =>
        {
            item.AddingEntity();

            RelationDto dto = RelationFactory.BuildDto(item);
            await db.Relations.AddAsync(dto);
            await db.SaveChangesAsync();

            item.Id = dto.Id;
            await PopulateObjectTypesAsync(db, [item], CancellationToken.None);
            item.ResetDirtyProperties();
        });

    /// <inheritdoc />
    protected override async Task PersistUpdatedItemAsync(IRelation item) =>
        await AmbientScope.ExecuteWithContextAsync<RelationDto>(async db =>
        {
            item.UpdatingEntity();

            RelationDto dto = RelationFactory.BuildDto(item);
            db.Relations.Update(dto);
            await db.SaveChangesAsync();

            await PopulateObjectTypesAsync(db, [item], CancellationToken.None);
            item.ResetDirtyProperties();
        });

    /// <inheritdoc />
    protected override async Task PersistDeletedItemAsync(IRelation entity) =>
        await AmbientScope.ExecuteWithContextAsync<RelationDto>(async db =>
        {
            await db.Relations
                .Where(x => x.Id == entity.Id)
                .ExecuteDeleteAsync();

            entity.DeleteDate = DateTime.UtcNow;
        });

    private static IQueryable<RelationDto> BaseQuery(UmbracoDbContext db)
        => db.Relations
            .Include(x => x.ParentNode)
            .Include(x => x.ChildNode);

    private async Task<IEnumerable<IRelation>> GetByFilterAsync(
        Func<IQueryable<RelationDto>, IQueryable<RelationDto>> filter,
        CancellationToken cancellationToken)
        => await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            List<RelationDto> dtos = await filter(BaseQuery(db)).ToListAsync(cancellationToken);
            return await BuildEntitiesAsync(dtos);
        });

    private async Task<IEnumerable<IRelation>> BuildEntitiesAsync(IReadOnlyCollection<RelationDto> dtos)
    {
        if (dtos.Count == 0)
        {
            return Array.Empty<IRelation>();
        }

        int[] relationTypeIds = dtos.Select(x => x.RelationType).Distinct().ToArray();
        IEnumerable<IRelationType> relationTypeEntities = await _relationTypeRepository.GetManyAsync(relationTypeIds, CancellationToken.None);
        Dictionary<int, IRelationType> relationTypes = relationTypeEntities.ToDictionary(x => x.Id, x => x);

        var entities = new List<IRelation>(dtos.Count);
        foreach (RelationDto dto in dtos)
        {
            if (relationTypes.TryGetValue(dto.RelationType, out IRelationType? relationType) is false)
            {
                throw new InvalidOperationException($"RelationType with Id: {dto.RelationType} doesn't exist");
            }

            entities.Add(RelationFactory.BuildEntity(dto, relationType));
        }

        return entities;
    }

    private async Task<IRelation> BuildEntityAsync(RelationDto dto)
    {
        IRelationType? relationType = await _relationTypeRepository.GetAsync(dto.RelationType, CancellationToken.None);
        if (relationType is null)
        {
            throw new InvalidOperationException($"RelationType with Id: {dto.RelationType} doesn't exist");
        }

        return RelationFactory.BuildEntity(dto, relationType);
    }

    /// <summary>
    ///     Populates the parent/child object types on the given relations after a save by querying the corresponding nodes.
    /// </summary>
    private static async Task PopulateObjectTypesAsync(
        UmbracoDbContext db,
        IReadOnlyCollection<IRelation> entities,
        CancellationToken cancellationToken)
    {
        if (entities.Count == 0)
        {
            return;
        }

        int[] nodeIds = entities
            .SelectMany(e => new[] { e.ParentId, e.ChildId })
            .Distinct()
            .ToArray();

        Dictionary<int, Guid?> objectTypes = await db.Nodes
            .Where(n => nodeIds.Contains(n.NodeId))
            .ToDictionaryAsync(n => n.NodeId, n => n.NodeObjectType, cancellationToken);

        foreach (IRelation entity in entities)
        {
            if (objectTypes.TryGetValue(entity.ParentId, out Guid? parentObjectType))
            {
                entity.ParentObjectType = parentObjectType.GetValueOrDefault();
            }

            if (objectTypes.TryGetValue(entity.ChildId, out Guid? childObjectType))
            {
                entity.ChildObjectType = childObjectType.GetValueOrDefault();
            }
        }
    }

    private static IQueryable<RelationDto> ApplyOrdering(IQueryable<RelationDto> query, Ordering? ordering)
    {
        if (ordering is null || ordering.IsEmpty)
        {
            return query.OrderBy(x => x.Id);
        }

        bool descending = ordering.Direction == Direction.Descending;

        return ordering.OrderBy?.ToLowerInvariant() switch
        {
            "id" => descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
            "parentid" => descending ? query.OrderByDescending(x => x.ParentId) : query.OrderBy(x => x.ParentId),
            "childid" => descending ? query.OrderByDescending(x => x.ChildId) : query.OrderBy(x => x.ChildId),
            "reltype" or "relationtype" or "relationtypeid"
                => descending ? query.OrderByDescending(x => x.RelationType) : query.OrderBy(x => x.RelationType),
            "datetime" or "createdate"
                => descending ? query.OrderByDescending(x => x.Datetime) : query.OrderBy(x => x.Datetime),
            "comment"
                => descending ? query.OrderByDescending(x => x.Comment) : query.OrderBy(x => x.Comment),
            _ => query.OrderBy(x => x.Id),
        };
    }
}
