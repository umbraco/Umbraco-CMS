
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
internal sealed class RelationRepository : EntityRepositoryBase<int, IRelation>, IRelationRepository
{
    private readonly IEntityRepositoryExtended _entityRepository;
    private readonly IRelationTypeRepository _relationTypeRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelationRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for transactional operations.</param>
    /// <param name="logger">The logger used for logging repository operations and errors.</param>
    /// <param name="relationTypeRepository">Repository for managing relation types.</param>
    /// <param name="entityRepository">Repository for accessing and managing entities with extended functionality.</param>
    /// <param name="repositoryCacheVersionService">Service for managing cache versioning within the repository.</param>
    /// <param name="cacheSyncService">Service for synchronizing cache across distributed environments.</param>
    public RelationRepository(
        IScopeAccessor scopeAccessor,
        ILogger<RelationRepository> logger,
        IRelationTypeRepository relationTypeRepository,
        IEntityRepositoryExtended entityRepository,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            AppCaches.NoCache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService)
    {
        _relationTypeRepository = relationTypeRepository;
        _entityRepository = entityRepository;
    }

    /// <summary>
    /// Gets a paged collection of parent entities related to the specified child entity ID.
    /// </summary>
    /// <param name="childId">The ID of the child entity to find parents for.</param>
    /// <param name="pageIndex">The zero-based index of the page to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalRecords">Outputs the total number of parent entities related to the child.</param>
    /// <param name="entityTypes">Optional array of entity type GUIDs to filter the parent entities.</param>
    /// <returns>An enumerable collection of parent entities implementing <see cref="IUmbracoEntity"/>.</returns>
    public IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildId(int childId, long pageIndex, int pageSize, out long totalRecords, params Guid[] entityTypes)
        => GetPagedParentEntitiesByChildId(childId, pageIndex, pageSize, out totalRecords, [], entityTypes);

    /// <summary>
    /// Retrieves a paged collection of child entities for the specified parent entity ID.
    /// </summary>
    /// <param name="parentId">The ID of the parent entity.</param>
    /// <param name="pageIndex">The zero-based index of the page to retrieve.</param>
    /// <param name="pageSize">The number of entities per page.</param>
    /// <param name="totalRecords">When this method returns, contains the total number of child entities available.</param>
    /// <param name="entityTypes">Optional. One or more entity type GUIDs to filter the child entities.</param>
    /// <returns>An enumerable collection of child entities that match the specified criteria.</returns>
    public IEnumerable<IUmbracoEntity> GetPagedChildEntitiesByParentId(int parentId, long pageIndex, int pageSize, out long totalRecords, params Guid[] entityTypes)
        => GetPagedChildEntitiesByParentId(parentId, pageIndex, pageSize, out totalRecords, [], entityTypes);

    /// <summary>
    /// Asynchronously retrieves a paged list of relations where the specified child key matches, optionally filtered by a relation type alias.
    /// </summary>
    /// <param name="childKey">The unique identifier (GUID) of the child entity to filter relations by.</param>
    /// <param name="skip">The number of relations to skip before starting to collect the result set (used for paging).</param>
    /// <param name="take">The maximum number of relations to return (used for paging).</param>
    /// <param name="relationTypeAlias">An optional alias to filter relations by their type; if null or empty, all relation types are included.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains a <see cref="PagedModel{IRelation}"/> with the total number of matching relations and the current page of <see cref="IRelation"/> entities.
    /// </returns>
    public Task<PagedModel<IRelation>> GetPagedByChildKeyAsync(Guid childKey, int skip, int take, string? relationTypeAlias)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);

        if (string.IsNullOrEmpty(relationTypeAlias) is false)
        {

            sql = sql.InnerJoin<RelationTypeDto>().On<RelationDto, RelationTypeDto>(umbracoRelation => umbracoRelation.RelationType, rt => rt.Id)
                .Where<RelationTypeDto>(rt => rt.Alias == relationTypeAlias);
        }
        sql = sql.Where<NodeDto>(n => n.UniqueId == childKey, "uchild"); // "uchild" comes from the base query


        RelationDto[] pagedResult =
            Database.SkipTake<RelationDto>(skip, take, sql).ToArray();
        var totalRecords = Database.Count(sql);

        return Task.FromResult(new PagedModel<IRelation>(totalRecords, DtosToEntities(pagedResult)));

    }

    /// <summary>
    /// Saves a collection of relations by updating existing ones and performing bulk inserts for new relations.
    /// Existing relations (those with an identity) are updated individually, while new relations are inserted in bulk for performance.
    /// </summary>
    /// <param name="relations">The collection of <see cref="IRelation"/> objects to save.</param>
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

    /// <summary>
    /// Saves a collection of relations in bulk, updating existing relations and inserting new ones as needed.
    /// Existing relations (those with an identity) are updated individually, while new relations are inserted in bulk for performance.
    /// </summary>
    /// <param name="relations">The collection of <see cref="ReadOnlyRelation"/> objects to save.</param>
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

    /// <summary>
    /// Retrieves a paged collection of relations matching the specified query, with support for paging and custom ordering.
    /// </summary>
    /// <param name="query">An optional query to filter the relations; if <c>null</c>, all relations are considered.</param>
    /// <param name="pageIndex">The zero-based index of the page to retrieve.</param>
    /// <param name="pageSize">The number of items to include in each page.</param>
    /// <param name="totalRecords">When this method returns, contains the total number of records matching the query.</param>
    /// <param name="ordering">An optional ordering to apply to the results; if <c>null</c> or empty, defaults to ordering by relation ID.</param>
    /// <returns>An enumerable collection of <see cref="IRelation"/> objects for the specified page.</returns>
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

    /// <summary>
    /// Deletes all relations for the specified parent entity, optionally filtered by relation type aliases.
    /// </summary>
    /// <param name="parentId">The ID of the parent entity whose relations will be deleted.</param>
    /// <param name="relationTypeAliases">An optional array of relation type aliases. If specified, only relations of these types will be deleted; if omitted or empty, all relations for the parent will be deleted.</param>
    public void DeleteByParent(int parentId, params string[] relationTypeAliases)
    {
        // HACK: SQLite - hard to replace this without provider specific repositories/another ORM.
        if (Database.DatabaseType.IsSqlServer() is false)
        {
            Sql<ISqlContext>? query = Sql().Append($"DELETE FROM {QuoteTableName("umbracoRelation")}");

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
    private static void SqlJoinRelations(Sql<ISqlContext> sql)
    {
        // add left joins for relation tables (this joins on both child or parent, so beware that this will normally return entities for
        // both sides of the relation type unless the IUmbracoEntity query passed in filters one side out).
        sql.LeftJoin<RelationDto>()
            .On<NodeDto, RelationDto>((left, right) => left.NodeId == right.ChildId || left.NodeId == right.ParentId);
        sql.LeftJoin<RelationTypeDto>()
            .On<RelationDto, RelationTypeDto>((left, right) => left.RelationType == right.Id);
    }

    /// <summary>
    /// Retrieves a paged collection of parent entities related to the specified child entity.
    /// </summary>
    /// <param name="childId">The identifier of the child entity whose parents are to be retrieved.</param>
    /// <param name="pageIndex">The zero-based index of the page to retrieve.</param>
    /// <param name="pageSize">The number of entities to include in a page.</param>
    /// <param name="totalRecords">When this method returns, contains the total number of parent entities found.</param>
    /// <param name="relationTypes">An array of relation type IDs to filter the parent-child relationships. Pass an empty array to include all relation types.</param>
    /// <param name="entityTypes">A set of entity type GUIDs to filter the parent entities. This parameter is optional and supports passing multiple values.</param>
    /// <returns>An enumerable collection of parent entities that match the specified criteria.</returns>
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

    /// <summary>
    /// Retrieves a paged collection of child entities related to the specified parent entity by its ID.
    /// </summary>
    /// <param name="parentId">The identifier of the parent entity.</param>
    /// <param name="pageIndex">The zero-based index of the page to retrieve.</param>
    /// <param name="pageSize">The number of child entities to include per page.</param>
    /// <param name="totalRecords">When this method returns, contains the total number of child entities related to the parent.</param>
    /// <param name="relationTypes">An optional array of relation type IDs to filter the relations. Pass an empty array to include all relation types.</param>
    /// <param name="entityTypes">A set of entity type GUIDs to filter the child entities. This parameter is variadic (params).</param>
    /// <returns>An enumerable collection of child entities that match the specified criteria.</returns>
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

    private static void ApplyOrdering(ref Sql<ISqlContext> sql, Ordering ordering)
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

        RelationDto? dto = Database.FirstOrDefault<RelationDto>(sql);
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

    protected override string GetBaseWhereClause() => $"{QuoteTableName(Constants.DatabaseSchema.Tables.Relation)}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string>
        {
            $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Relation)} WHERE id = @id"
        };
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

internal sealed class RelationItemDto
{
    /// <summary>
    /// Gets or sets the identifier of the child node in the relation.
    /// </summary>
    [Column(Name = "nodeId")]
    public int ChildNodeId { get; set; }

    /// <summary>
    /// Gets or sets the unique key (GUID) of the child node in this relation.
    /// </summary>
    [Column(Name = "nodeKey")]
    public Guid ChildNodeKey { get; set; }

    /// <summary>
    /// Gets or sets the name of the child node associated with this relation item.
    /// This property is mapped to the 'nodeName' column in the database.
    /// </summary>
    [Column(Name = "nodeName")]
    public string? ChildNodeName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the child node is published.
    /// </summary>
    [Column(Name = "nodePublished")]
    public bool? ChildNodePublished { get; set; }

    /// <summary>
    /// Gets or sets the object type identifier of the child node in the relation.
    /// </summary>
    [Column(Name = "nodeObjectType")]
    public Guid ChildNodeObjectType { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (key) of the child content type.
    /// </summary>
    [Column(Name = "contentTypeKey")]
    public Guid ChildContentTypeKey { get; set; }

    /// <summary>
    /// Gets or sets the icon associated with the child content type.
    /// </summary>
    [Column(Name = "contentTypeIcon")]
    public string? ChildContentTypeIcon { get; set; }

    /// <summary>
    /// Gets or sets the alias of the child content type.
    /// </summary>
    [Column(Name = "contentTypeAlias")]
    public string? ChildContentTypeAlias { get; set; }

    /// <summary>
    /// Gets or sets the name of the child content type.
    /// </summary>
    [Column(Name = "contentTypeName")]
    public string? ChildContentTypeName { get; set; }

    /// <summary>
    /// Gets or sets the display name of the relation type associated with this relation item.
    /// </summary>
    [Column(Name = "relationTypeName")]
    public string? RelationTypeName { get; set; }

    /// <summary>
    /// Gets or sets the alias of the relation type.
    /// </summary>
    [Column(Name = "relationTypeAlias")]
    public string? RelationTypeAlias { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the relation type represents a dependency between related items.
    /// </summary>
    [Column(Name = "relationTypeIsDependency")]
    public bool RelationTypeIsDependency { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the relation type is bidirectional.
    /// </summary>
    [Column(Name = "relationTypeIsBidirectional")]
    public bool RelationTypeIsBidirectional { get; set; }
}
