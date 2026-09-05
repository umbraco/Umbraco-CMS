using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="RelationType" />
/// </summary>
internal sealed class RelationTypeRepository : EntityRepositoryBase<int, IRelationType>, IRelationTypeRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelationTypeRepository"/> class with the specified dependencies.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope.</param>
    /// <param name="cache">The application-level cache manager.</param>
    /// <param name="logger">The logger used for logging repository operations.</param>
    /// <param name="repositoryCacheVersionService">Service for managing repository cache versions.</param>
    /// <param name="cacheSyncService">Service for synchronizing cache across distributed environments.</param>
    public RelationTypeRepository(
        IScopeAccessor scopeAccessor,
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

    protected override IRepositoryCachePolicy<IRelationType, int> CreateCachePolicy() =>
        new FullDataSetRepositoryCachePolicy<IRelationType, int>(GlobalIsolatedCache, ScopeAccessor,  RepositoryCacheVersionService, CacheSyncService, GetEntityId, /*expires:*/ true);

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

    #region Overrides of RepositoryBase<int,RelationType>

    /// <summary>
    /// Gets the cache policy as <see cref="FullDataSetRepositoryCachePolicy{TEntity, TId}"/> for predicate-based lookups.
    /// Returns null when caching is disabled (e.g. <see cref="AppCaches.NoCache"/>).
    /// </summary>
    private FullDataSetRepositoryCachePolicy<IRelationType, int>? TypedCachePolicy
        => CachePolicy as FullDataSetRepositoryCachePolicy<IRelationType, int>;

    // Note: PerformGet(int) is passed as a callback to the cache policy's Get(TId) method,
    // but FullDataSetRepositoryCachePolicy.Get() never invokes it — it uses GetAllCached()
    // internally and clones only the matched entity. This override exists only as a required
    // implementation of the abstract base and as a fallback for non-FullDataSet policies.
    protected override IRelationType? PerformGet(int id) =>
        GetMany()?.FirstOrDefault(x => x.Id == id);

    /// <summary>
    /// Gets a relation type by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the relation type.</param>
    /// <returns>The relation type matching the specified identifier, or null if not found.</returns>
    public IRelationType? Get(Guid id) =>
        TypedCachePolicy?.FindCached(x => x.Key == id, PerformGetAll)
        ?? GetMany()?.FirstOrDefault(x => x.Key == id);

    /// <summary>
    /// Determines whether a relation type with the specified identifier exists.
    /// </summary>
    /// <param name="id">The unique identifier of the relation type.</param>
    /// <returns>True if the relation type exists; otherwise, false.</returns>
    public bool Exists(Guid id) =>
        TypedCachePolicy?.ExistsCached(x => x.Key == id, PerformGetAll)
        ?? GetMany()?.Any(x => x.Key == id) == true;

    protected override IEnumerable<IRelationType> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);

        List<RelationTypeDto>? dtos = Database.Fetch<RelationTypeDto>(sql);

        return dtos.Select(x => DtoToEntity(x));
    }

    /// <summary>
    /// Retrieves multiple relation types by their unique identifiers.
    /// </summary>
    /// <param name="ids">An optional array of unique identifiers for the relation types to retrieve. If no identifiers are provided, all relation types are returned.</param>
    /// <returns>An enumerable collection of relation types matching the specified identifiers, or all relation types if no identifiers are specified.</returns>
    public IEnumerable<IRelationType> GetMany(params Guid[]? ids)
    {
        // should not happen due to the cache policy
        if (ids is { Length: not 0 })
        {
            throw new NotImplementedException();
        }

        return GetMany(Array.Empty<int>());
    }

    protected override IEnumerable<IRelationType> PerformGetByQuery(IQuery<IRelationType> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<IRelationType>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        List<RelationTypeDto>? dtos = Database.Fetch<RelationTypeDto>(sql);

        return dtos.Select(x => DtoToEntity(x));
    }

    private static IRelationType DtoToEntity(RelationTypeDto dto)
    {
        IRelationType entity = RelationTypeFactory.BuildEntity(dto);

        // reset dirty initial properties (U4-1946)
        ((BeingDirtyBase)entity).ResetDirtyProperties(false);

        return entity;
    }

    #endregion

    #region Overrides of EntityRepositoryBase<int,RelationType>

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = Sql();

        sql = isCount
            ? sql.SelectCount()
            : sql.Select<RelationTypeDto>();

        sql
            .From<RelationTypeDto>();

        return sql;
    }

    protected override string GetBaseWhereClause() => $"{QuoteTableName(Constants.DatabaseSchema.Tables.RelationType)}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string>
        {
            $"DELETE FROM {QuoteTableName("umbracoRelation")} WHERE {QuoteColumnName("relType")} = @id",
            $"DELETE FROM {QuoteTableName("umbracoRelationType")} WHERE id = @id",
        };
        return list;
    }

    #endregion

    #region Unit of Work Implementation

    protected override void PersistNewItem(IRelationType entity)
    {
        entity.AddingEntity();

        CheckNullObjectTypeValues(entity);

        RelationTypeDto dto = RelationTypeFactory.BuildDto(entity);

        var id = Convert.ToInt32(Database.Insert(dto));
        entity.Id = id;

        entity.ResetDirtyProperties();
    }

    protected override void PersistUpdatedItem(IRelationType entity)
    {
        entity.UpdatingEntity();

        CheckNullObjectTypeValues(entity);

        RelationTypeDto dto = RelationTypeFactory.BuildDto(entity);
        Database.Update(dto);

        entity.ResetDirtyProperties();
    }

    #endregion
}
