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
internal class RelationTypeRepository : EntityRepositoryBase<int, IRelationType>, IRelationTypeRepository
{
    public RelationTypeRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<RelationTypeRepository> logger)
        : base(scopeAccessor, cache, logger)
    {
    }

    protected override IRepositoryCachePolicy<IRelationType, int> CreateCachePolicy() =>
        new FullDataSetRepositoryCachePolicy<IRelationType, int>(GlobalIsolatedCache, ScopeAccessor, GetEntityId, /*expires:*/ true);

    private void CheckNullObjectTypeValues(IRelationType entity)
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

    protected override IRelationType? PerformGet(int id) =>

        // use the underlying GetAll which will force cache all content types
        GetMany()?.FirstOrDefault(x => x.Id == id);

    public IRelationType? Get(Guid id) =>

        // use the underlying GetAll which will force cache all content types
        GetMany()?.FirstOrDefault(x => x.Key == id);

    public bool Exists(Guid id) => Get(id) != null;

    protected override IEnumerable<IRelationType> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);

        List<RelationTypeDto>? dtos = Database.Fetch<RelationTypeDto>(sql);

        return dtos.Select(x => DtoToEntity(x));
    }

    public IEnumerable<IRelationType> GetMany(params Guid[]? ids)
    {
        // should not happen due to the cache policy
        if (ids?.Any() ?? false)
        {
            throw new NotImplementedException();
        }

        return GetMany(new int[0]);
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

    protected override string GetBaseWhereClause() => $"{Constants.DatabaseSchema.Tables.RelationType}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string>
        {
            "DELETE FROM umbracoRelation WHERE relType = @id", "DELETE FROM umbracoRelationType WHERE id = @id",
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
