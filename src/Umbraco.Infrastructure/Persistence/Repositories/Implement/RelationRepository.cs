using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Persistence.SqlExtensionsStatics;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="Relation" />
/// </summary>
internal class RelationRepository : EntityRepositoryBase<int, IRelation>, IRelationRepository
{
    private readonly IEntityRepositoryExtended _entityRepository;
    private readonly IRelationTypeRepository _relationTypeRepository;

    public RelationRepository(IScopeAccessor scopeAccessor, ILogger<RelationRepository> logger, IRelationTypeRepository relationTypeRepository, IEntityRepositoryExtended entityRepository)
        : base(scopeAccessor, AppCaches.NoCache, logger)
    {
        _relationTypeRepository = relationTypeRepository;
        _entityRepository = entityRepository;
    }

    public IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildId(int childId, long pageIndex, int pageSize, out long totalRecords, params Guid[] entityTypes)
        => GetPagedParentEntitiesByChildId(childId, pageIndex, pageSize, out totalRecords, new int[0], entityTypes);

    public IEnumerable<IUmbracoEntity> GetPagedChildEntitiesByParentId(int parentId, long pageIndex, int pageSize, out long totalRecords, params Guid[] entityTypes)
        => GetPagedChildEntitiesByParentId(parentId, pageIndex, pageSize, out totalRecords, new int[0], entityTypes);

    public void Save(IEnumerable<IRelation> relations)
    {
        foreach (IGrouping<bool, IRelation> hasIdentityGroup in relations.GroupBy(r => r.HasIdentity))
        {
            if (hasIdentityGroup.Key)
            {
                // Do updates, we can't really do a bulk update so this is still a 1 by 1 operation
                // however we can bulk populate the object types. It might be possible to bulk update
                // with SQL but would be pretty ugly and we're not really too worried about that for perf,
                // it's the bulk inserts we care about.
                IRelation[] asArray = hasIdentityGroup.ToArray();
                foreach (IRelation relation in hasIdentityGroup)
                {
                    relation.UpdatingEntity();
                    RelationDto dto = RelationFactory.BuildDto(relation);
                    Database.Update(dto);
                }

                PopulateObjectTypes(asArray);
            }
            else
            {
                // Do bulk inserts
                var entitiesAndDtos = hasIdentityGroup.ToDictionary(
                    r => // key = entity
                    {
                        r.AddingEntity();
                        return r;
                    },
                    RelationFactory.BuildDto); // value = DTO

                foreach (RelationDto dto in entitiesAndDtos.Values)
                {
                    Database.Insert(dto);
                }

                // All dtos now have IDs assigned
                foreach (KeyValuePair<IRelation, RelationDto> de in entitiesAndDtos)
                {
                    // re-assign ID to the entity
                    de.Key.Id = de.Value.Id;
                }

                PopulateObjectTypes(entitiesAndDtos.Keys.ToArray());
            }
        }
    }

    public void SaveBulk(IEnumerable<ReadOnlyRelation> relations)
    {
        foreach (IGrouping<bool, ReadOnlyRelation> hasIdentityGroup in relations.GroupBy(r => r.HasIdentity))
        {
            if (hasIdentityGroup.Key)
            {
                // Do updates, we can't really do a bulk update so this is still a 1 by 1 operation
                // however we can bulk populate the object types. It might be possible to bulk update
                // with SQL but would be pretty ugly and we're not really too worried about that for perf,
                // it's the bulk inserts we care about.
                foreach (ReadOnlyRelation relation in hasIdentityGroup)
                {
                    RelationDto dto = RelationFactory.BuildDto(relation);
                    Database.Update(dto);
                }
            }
            else
            {
                // Do bulk inserts
                IEnumerable<RelationDto> dtos = hasIdentityGroup.Select(RelationFactory.BuildDto);

                Database.InsertBulk(dtos);
            }
        }
    }

    public IEnumerable<IRelation> GetPagedRelationsByQuery(IQuery<IRelation>? query, long pageIndex, int pageSize, out long totalRecords, Ordering? ordering)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);

        if (ordering == null || ordering.IsEmpty)
        {
            ordering = Ordering.By(SqlSyntax.GetQuotedColumn(Constants.DatabaseSchema.Tables.Relation, "id"));
        }

        var translator = new SqlTranslator<IRelation>(sql, query);
        sql = translator.Translate();

        // apply ordering
        ApplyOrdering(ref sql, ordering);

        var pageIndexToFetch = pageIndex + 1;
        Page<RelationDto>? page = Database.Page<RelationDto>(pageIndexToFetch, pageSize, sql);
        List<RelationDto>? dtos = page.Items;
        totalRecords = page.TotalItems;

        var relTypes = _relationTypeRepository.GetMany(dtos.Select(x => x.RelationType).Distinct().ToArray())?
            .ToDictionary(x => x.Id, x => x);

        var result = dtos.Select(r =>
        {
            if (relTypes is null || !relTypes.TryGetValue(r.RelationType, out IRelationType? relType))
            {
                throw new InvalidOperationException(string.Format("RelationType with Id: {0} doesn't exist", r.RelationType));
            }

            return DtoToEntity(r, relType);
        }).WhereNotNull().ToList();

        return result;
    }

    public void DeleteByParent(int parentId, params string[] relationTypeAliases)
    {
        // HACK: SQLite - hard to replace this without provider specific repositories/another ORM.
        if (Database.DatabaseType.IsSqlite())
        {
            Sql<ISqlContext>? query = Sql().Append(@"delete from umbracoRelation");

            Sql<ISqlContext> subQuery = Sql().Select<RelationDto>(x => x.Id)
                .From<RelationDto>()
                .InnerJoin<RelationTypeDto>().On<RelationDto, RelationTypeDto>(x => x.RelationType, x => x.Id)
                .Where<RelationDto>(x => x.ParentId == parentId);

            if (relationTypeAliases.Length > 0)
            {
                subQuery.WhereIn<RelationTypeDto>(x => x.Alias, relationTypeAliases);
            }

            Sql<ISqlContext> fullQuery = query.WhereIn<RelationDto>(x => x.Id, subQuery);

            Database.Execute(fullQuery);
        }
        else
        {
            if (relationTypeAliases.Length > 0)
            {
                SqlTemplate template = SqlContext.Templates.Get(
                    Constants.SqlTemplates.RelationRepository.DeleteByParentIn,
                    tsql => Sql().Delete<RelationDto>()
                        .From<RelationDto>()
                        .InnerJoin<RelationTypeDto>().On<RelationDto, RelationTypeDto>(x => x.RelationType, x => x.Id)
                        .Where<RelationDto>(x => x.ParentId == SqlTemplate.Arg<int>("parentId"))
                        .WhereIn<RelationTypeDto>(x => x.Alias, SqlTemplate.ArgIn<string>("relationTypeAliases")));

                Sql<ISqlContext> sql = template.Sql(parentId, relationTypeAliases);

                Database.Execute(sql);
            }
            else
            {
                SqlTemplate template = SqlContext.Templates.Get(
                    Constants.SqlTemplates.RelationRepository.DeleteByParentAll,
                    tsql => Sql().Delete<RelationDto>()
                        .From<RelationDto>()
                        .InnerJoin<RelationTypeDto>().On<RelationDto, RelationTypeDto>(x => x.RelationType, x => x.Id)
                        .Where<RelationDto>(x => x.ParentId == SqlTemplate.Arg<int>("parentId")));

                Sql<ISqlContext> sql = template.Sql(parentId);

                Database.Execute(sql);
            }
        }
    }

    /// <summary>
    ///     Used for joining the entity query with relations for the paging methods
    /// </summary>
    /// <param name="sql"></param>
    private void SqlJoinRelations(Sql<ISqlContext> sql)
    {
        // add left joins for relation tables (this joins on both child or parent, so beware that this will normally return entities for
        // both sides of the relation type unless the IUmbracoEntity query passed in filters one side out).
        sql.LeftJoin<RelationDto>()
            .On<NodeDto, RelationDto>((left, right) => left.NodeId == right.ChildId || left.NodeId == right.ParentId);
        sql.LeftJoin<RelationTypeDto>()
            .On<RelationDto, RelationTypeDto>((left, right) => left.RelationType == right.Id);
    }

    public IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildId(int childId, long pageIndex, int pageSize, out long totalRecords, int[] relationTypes, params Guid[] entityTypes) =>

        // var contentObjectTypes = new[] { Constants.ObjectTypes.Document, Constants.ObjectTypes.Media, Constants.ObjectTypes.Member }
        // we could pass in the contentObjectTypes so that the entity repository sql is configured to do full entity lookups so that we get the full data
        // required to populate content, media or members, else we get the bare minimum data needed to populate an entity. BUT if we do this it
        // means that the SQL is less efficient and returns data that is probably not needed for what we need this lookup for. For the time being we
        // will just return the bare minimum entity data.
        _entityRepository.GetPagedResultsByQuery(Query<IUmbracoEntity>(), entityTypes, pageIndex, pageSize, out totalRecords, null, null, sql =>
            {
                SqlJoinRelations(sql);

                sql.Where<RelationDto>(rel => rel.ChildId == childId);
                sql.Where<RelationDto, NodeDto>((rel, node) => rel.ParentId == childId || node.NodeId != childId);

                if (relationTypes != null && relationTypes.Any())
                {
                    sql.WhereIn<RelationDto>(rel => rel.RelationType, relationTypes);
                }
            });

    public IEnumerable<IUmbracoEntity> GetPagedChildEntitiesByParentId(int parentId, long pageIndex, int pageSize, out long totalRecords, int[] relationTypes, params Guid[] entityTypes) =>

        // var contentObjectTypes = new[] { Constants.ObjectTypes.Document, Constants.ObjectTypes.Media, Constants.ObjectTypes.Member }
        // we could pass in the contentObjectTypes so that the entity repository sql is configured to do full entity lookups so that we get the full data
        // required to populate content, media or members, else we get the bare minimum data needed to populate an entity. BUT if we do this it
        // means that the SQL is less efficient and returns data that is probably not needed for what we need this lookup for. For the time being we
        // will just return the bare minimum entity data.
        _entityRepository.GetPagedResultsByQuery(Query<IUmbracoEntity>(), entityTypes, pageIndex, pageSize, out totalRecords, null, null, sql =>
            {
                SqlJoinRelations(sql);

                sql.Where<RelationDto>(rel => rel.ParentId == parentId);
                sql.Where<RelationDto, NodeDto>((rel, node) => rel.ChildId == parentId || node.NodeId != parentId);

                if (relationTypes != null && relationTypes.Any())
                {
                    sql.WhereIn<RelationDto>(rel => rel.RelationType, relationTypes);
                }
            });

    /// <summary>
    ///     Used to populate the object types after insert/update
    /// </summary>
    /// <param name="entities"></param>
    private void PopulateObjectTypes(params IRelation[] entities)
    {
        IEnumerable<int> entityIds =
            entities.Select(x => x.ParentId).Concat(entities.Select(y => y.ChildId)).Distinct();

        var nodes = Database.Fetch<NodeDto>(Sql().Select<NodeDto>().From<NodeDto>()
                .WhereIn<NodeDto>(x => x.NodeId, entityIds))
            .ToDictionary(x => x.NodeId, x => x.NodeObjectType);

        foreach (IRelation e in entities)
        {
            if (nodes.TryGetValue(e.ParentId, out Guid? parentObjectType))
            {
                e.ParentObjectType = parentObjectType.GetValueOrDefault();
            }

            if (nodes.TryGetValue(e.ChildId, out Guid? childObjectType))
            {
                e.ChildObjectType = childObjectType.GetValueOrDefault();
            }
        }
    }

    private void ApplyOrdering(ref Sql<ISqlContext> sql, Ordering ordering)
    {
        if (sql == null)
        {
            throw new ArgumentNullException(nameof(sql));
        }

        if (ordering == null)
        {
            throw new ArgumentNullException(nameof(ordering));
        }

        // TODO: although this works for name, it probably doesn't work for others without an alias of some sort
        var orderBy = ordering.OrderBy;

        if (ordering.Direction == Direction.Ascending)
        {
            sql.OrderBy(orderBy);
        }
        else
        {
            sql.OrderByDescending(orderBy);
        }
    }

    #region Overrides of RepositoryBase<int,Relation>

    protected override IRelation? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);
        sql.Where(GetBaseWhereClause(), new { id });

        RelationDto? dto = Database.Fetch<RelationDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();
        if (dto == null)
        {
            return null;
        }

        IRelationType? relationType = _relationTypeRepository.Get(dto.RelationType);
        if (relationType == null)
        {
            throw new InvalidOperationException(string.Format("RelationType with Id: {0} doesn't exist", dto.RelationType));
        }

        return DtoToEntity(dto, relationType);
    }

    protected override IEnumerable<IRelation> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);
        if (ids?.Length > 0)
        {
            sql.WhereIn<RelationDto>(x => x.Id, ids);
        }

        sql.OrderBy<RelationDto>(x => x.RelationType);
        List<RelationDto>? dtos = Database.Fetch<RelationDto>(sql);
        return DtosToEntities(dtos);
    }

    protected override IEnumerable<IRelation> PerformGetByQuery(IQuery<IRelation> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<IRelation>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();
        sql.OrderBy<RelationDto>(x => x.RelationType);
        List<RelationDto>? dtos = Database.Fetch<RelationDto>(sql);
        return DtosToEntities(dtos);
    }

    private IEnumerable<IRelation> DtosToEntities(IEnumerable<RelationDto> dtos) =>

        // NOTE: This is N+1, BUT ALL relation types are cached so shouldn't matter
        dtos.Select(x => DtoToEntity(x, _relationTypeRepository.Get(x.RelationType))).WhereNotNull().ToList();

    private static IRelation? DtoToEntity(RelationDto dto, IRelationType? relationType)
    {
        if (relationType is null)
        {
            return null;
        }

        IRelation entity = RelationFactory.BuildEntity(dto, relationType);

        // reset dirty initial properties (U4-1946)
        entity.ResetDirtyProperties(false);

        return entity;
    }

    #endregion

    #region Overrides of EntityRepositoryBase<int,Relation>

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        if (isCount)
        {
            return Sql().SelectCount().From<RelationDto>();
        }

        Sql<ISqlContext> sql = Sql().Select<RelationDto>()
            .AndSelect<NodeDto>("uchild", x => Alias(x.NodeObjectType, "childObjectType"))
            .AndSelect<NodeDto>("uparent", x => Alias(x.NodeObjectType, "parentObjectType"))
            .From<RelationDto>()
            .InnerJoin<NodeDto>("uchild")
            .On<RelationDto, NodeDto>((rel, node) => rel.ChildId == node.NodeId, aliasRight: "uchild")
            .InnerJoin<NodeDto>("uparent")
            .On<RelationDto, NodeDto>((rel, node) => rel.ParentId == node.NodeId, aliasRight: "uparent");

        return sql;
    }

    protected override string GetBaseWhereClause() => $"{Constants.DatabaseSchema.Tables.Relation}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string> { "DELETE FROM umbracoRelation WHERE id = @id" };
        return list;
    }

    #endregion

    #region Unit of Work Implementation

    protected override void PersistNewItem(IRelation entity)
    {
        entity.AddingEntity();

        RelationDto dto = RelationFactory.BuildDto(entity);

        var id = Convert.ToInt32(Database.Insert(dto));

        entity.Id = id;
        PopulateObjectTypes(entity);

        entity.ResetDirtyProperties();
    }

    protected override void PersistUpdatedItem(IRelation entity)
    {
        entity.UpdatingEntity();

        RelationDto dto = RelationFactory.BuildDto(entity);
        Database.Update(dto);

        PopulateObjectTypes(entity);

        entity.ResetDirtyProperties();
    }

    #endregion
}

internal class RelationItemDto
{
    [Column(Name = "nodeId")]
    public int ChildNodeId { get; set; }

    [Column(Name = "nodeKey")]
    public Guid ChildNodeKey { get; set; }

    [Column(Name = "nodeName")]
    public string? ChildNodeName { get; set; }

    [Column(Name = "nodeObjectType")]
    public Guid ChildNodeObjectType { get; set; }

    [Column(Name = "contentTypeIcon")]
    public string? ChildContentTypeIcon { get; set; }

    [Column(Name = "contentTypeAlias")]
    public string? ChildContentTypeAlias { get; set; }

    [Column(Name = "contentTypeName")]
    public string? ChildContentTypeName { get; set; }

    [Column(Name = "relationTypeName")]
    public string? RelationTypeName { get; set; }

    [Column(Name = "relationTypeAlias")]
    public string? RelationTypeAlias { get; set; }

    [Column(Name = "relationTypeIsDependency")]
    public bool RelationTypeIsDependency { get; set; }

    [Column(Name = "relationTypeIsBidirectional")]
    public bool RelationTypeIsBidirectional { get; set; }
}
