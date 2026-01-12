using System.Linq.Expressions;
using NPoco;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    /// <summary>
    /// Implements <see cref="ITrackedReferencesRepository"/> to provide database access for tracked references."
    /// </summary>
    internal sealed class TrackedReferencesRepository : ITrackedReferencesRepository
    {
        private readonly IScopeAccessor _scopeAccessor;
        private readonly IUmbracoMapper _umbracoMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedReferencesRepository"/> class.
        /// </summary>
        public TrackedReferencesRepository(IScopeAccessor scopeAccessor, IUmbracoMapper umbracoMapper)
        {
            _scopeAccessor = scopeAccessor;
            _umbracoMapper = umbracoMapper;
        }

        /// <inheritdoc/>
        public IEnumerable<RelationItemModel> GetPagedRelationsForItem(
            Guid key,
            long skip,
            long take,
            bool filterMustBeIsDependency,
            out long totalRecords)
            => GetPagedRelations(
                x => x.Key == key,
                skip,
                take,
                filterMustBeIsDependency,
                out totalRecords);

        /// <inheritdoc/>
        public IEnumerable<RelationItemModel> GetPagedRelationsForRecycleBin(
            Guid objectTypeKey,
            long skip,
            long take,
            bool filterMustBeIsDependency,
            out long totalRecords)
            => GetPagedRelations(
                x => x.NodeObjectType == objectTypeKey && x.Trashed == true,
                skip,
                take,
                filterMustBeIsDependency,
                out totalRecords);

        private IEnumerable<RelationItemModel> GetPagedRelations(
            Expression<Func<UnionHelperDto, bool>> itemsFilter,
            long skip,
            long take,
            bool filterMustBeIsDependency,
            out long totalRecords)
        {
            if (_scopeAccessor.AmbientScope is null)
            {
                totalRecords = 0;
                return [];
            }

            ISqlSyntaxProvider sx = _scopeAccessor.AmbientScope.Database.SqlContext.SqlSyntax;
            string[] columns = [
                    sx.ColumnWithAlias("x", "otherId", "nodeId"),
                    sx.ColumnWithAlias("n", "uniqueId", "nodeKey"),
                    sx.ColumnWithAlias("n", "nodeObjectType", "nodeObjectType"),
                    sx.ColumnWithAlias("d", "published", "nodePublished"),
                    sx.ColumnWithAlias("ctn", "uniqueId", "contentTypeKey"),
                    sx.ColumnWithAlias("ct", "icon", "contentTypeIcon"),
                    sx.ColumnWithAlias("ct", "alias", "contentTypeAlias"),
                    sx.ColumnWithAlias("ctn", "text", "contentTypeName"),
                    sx.ColumnWithAlias("x", "alias", "relationTypeAlias"),
                    sx.ColumnWithAlias("x", "name", "relationTypeName"),
                    sx.ColumnWithAlias("x", "isDependency", "relationTypeIsDependency"),
                    sx.ColumnWithAlias("x", "dual", "relationTypeIsBidirectional")
                ];
            Sql<ISqlContext> innerUnionSql = GetInnerUnionSql();
            Sql<ISqlContext> sql = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
                .SelectDistinct(columns)
                .From<NodeDto>("n")
                .InnerJoinNested(innerUnionSql, "x")
                .On<NodeDto, UnionHelperDto>((n, x) => n.NodeId == x.OtherId, "n", "x")
                .LeftJoin<ContentDto>("c")
                .On<NodeDto, ContentDto>(
                    (left, right) => left.NodeId == right.NodeId,
                    aliasLeft: "n",
                    aliasRight: "c")
                .LeftJoin<ContentTypeDto>("ct")
                .On<ContentDto, ContentTypeDto>(
                    (left, right) => left.ContentTypeId == right.NodeId,
                    aliasLeft: "c",
                    aliasRight: "ct")
                .LeftJoin<NodeDto>("ctn")
                .On<ContentTypeDto, NodeDto>(
                    (left, right) => left.NodeId == right.NodeId,
                    aliasLeft: "ct",
                    aliasRight: "ctn")
                .LeftJoin<DocumentDto>("d")
                .On<NodeDto, DocumentDto>(
                    (left, right) => left.NodeId == right.NodeId,
                    aliasLeft: "n",
                    aliasRight: "d")
                .Where(itemsFilter, "x");


            if (filterMustBeIsDependency)
            {
                sql = sql.Where<RelationTypeDto>(rt => rt.IsDependency, "x");
            }

            // find the count before ordering
            totalRecords = _scopeAccessor.AmbientScope?.Database.Count(sql!) ?? 0;

            RelationItemDto[] pagedResult;

            //Only to all this, if there is items
            if (totalRecords > 0)
            {
                // Ordering is required for paging
                sql = sql.OrderBy<RelationTypeDto>(x => x.Alias, "x");

                pagedResult =
                    _scopeAccessor.AmbientScope?.Database.SkipTake<RelationItemDto>(skip, take, sql).ToArray() ??
                    Array.Empty<RelationItemDto>();
            }
            else
            {
                pagedResult = Array.Empty<RelationItemDto>();
            }

            return _umbracoMapper.MapEnumerable<RelationItemDto, RelationItemModel>(pagedResult);
        }

        /// <inheritdoc/>
        public IEnumerable<RelationItemModel> GetPagedItemsWithRelations(
           ISet<Guid> keys,
           long skip,
           long take,
           bool filterMustBeIsDependency,
           out long totalRecords)
        {
            if (_scopeAccessor.AmbientScope is null)
            {
                totalRecords = 0;
                return [];
            }

            ISqlSyntaxProvider sx = _scopeAccessor.AmbientScope.Database.SqlContext.SqlSyntax;
            string[] columns = [
                    sx.ColumnWithAlias("x", "id", "nodeId"),
                    sx.ColumnWithAlias("n", "uniqueId", "nodeKey"),
                    sx.ColumnWithAlias("n", "text", "nodeName"),
                    sx.ColumnWithAlias("n", "nodeObjectType", "nodeObjectType"),
                    sx.ColumnWithAlias("ctn", "uniqueId", "contentTypeKey"),
                    sx.ColumnWithAlias("ct", "icon", "contentTypeIcon"),
                    sx.ColumnWithAlias("ct", "alias", "contentTypeAlias"),
                    sx.ColumnWithAlias("ctn", "text", "contentTypeName"),
                    sx.ColumnWithAlias("x", "alias", "relationTypeAlias"),
                    sx.ColumnWithAlias("x", "name", "relationTypeName"),
                    sx.ColumnWithAlias("x", "isDependency", "relationTypeIsDependency"),
                    sx.ColumnWithAlias("x", "dual", "relationTypeIsBidirectional")
                ];
            Sql<ISqlContext> innerUnionSql = GetInnerUnionSql();
            Sql<ISqlContext>? sql = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
                .SelectDistinct(columns)
                .From<NodeDto>("n")
                .InnerJoinNested(innerUnionSql, "x")
                .On<NodeDto, UnionHelperDto>((n, x) => n.NodeId == x.Id, "n", "x")
                .LeftJoin<ContentDto>("c")
                .On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId, aliasLeft: "n", aliasRight: "c")
                .LeftJoin<ContentTypeDto>("ct")
                .On<ContentDto, ContentTypeDto>((left, right) => left.ContentTypeId == right.NodeId, aliasLeft: "c", aliasRight: "ct")
                .LeftJoin<NodeDto>("ctn")
                .On<ContentTypeDto, NodeDto>((left, right) => left.NodeId == right.NodeId, aliasLeft: "ct", aliasRight: "ctn");
            if (keys.Any())
            {
                sql = sql.Where<NodeDto>(x => keys.Contains(x.UniqueId), "n");
            }

            if (filterMustBeIsDependency)
            {
                sql = sql.Where<RelationTypeDto>(rt => rt.IsDependency, "x");
            }

            // find the count before ordering
            totalRecords = _scopeAccessor.AmbientScope?.Database.Count(sql!) ?? 0;

            RelationItemDto[] pagedResult;

            //Only to all this, if there is items
            if (totalRecords > 0)
            {
                // Ordering is required for paging
                sql = sql.OrderBy<RelationTypeDto>(x => x.Alias, "x");

                pagedResult =
                    _scopeAccessor.AmbientScope?.Database.SkipTake<RelationItemDto>(skip, take, sql).ToArray() ??
                    Array.Empty<RelationItemDto>();
            }
            else
            {
                pagedResult = Array.Empty<RelationItemDto>();
            }

            return _umbracoMapper.MapEnumerable<RelationItemDto, RelationItemModel>(pagedResult);
        }

        /// <inheritdoc/>
        public async Task<PagedModel<Guid>> GetPagedNodeKeysWithDependantReferencesAsync(
            ISet<Guid> keys,
            Guid nodeObjectTypeId,
            long skip,
            long take)
        {
            if (_scopeAccessor.AmbientScope is null)
            {
                throw new InvalidOperationException("Cannot execute without a valid AmbientScope");
            }

            Sql<ISqlContext> sql = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
                .SelectDistinct<NodeDto>(node => node.UniqueId)
                .From<NodeDto>()
                .InnerJoin<RelationDto>()
                .On<NodeDto, RelationDto>((node, relation) =>
                    node.NodeId == relation.ParentId || node.NodeId == relation.ParentId || node.NodeId == relation.ChildId)
                .InnerJoin<RelationTypeDto>()
                .On<RelationDto, RelationTypeDto>((relation, relationType) => relation.RelationType == relationType.Id && relationType.IsDependency)
                .Where<NodeDto, RelationDto, RelationTypeDto>(
                    (node, relation, relationType)
                    => node.NodeObjectType == nodeObjectTypeId
                       && keys.Contains(node.UniqueId)
                       && (node.NodeId == relation.ChildId
                           || (relationType.Dual && relation.ParentId == node.NodeId)));

            var totalRecords = _scopeAccessor.AmbientScope.Database.Count(sql);

            // no need to process further if no records are found
            if (totalRecords < 1)
            {
                return new PagedModel<Guid>(totalRecords, Enumerable.Empty<Guid>());
            }

            // Ordering is required for paging
            sql = sql.OrderBy<NodeDto>(node => node.UniqueId);

            IEnumerable<Guid> result = await _scopeAccessor.AmbientScope.Database.SkipTakeAsync<Guid>(skip, take, sql);

            return new PagedModel<Guid>(totalRecords, result);
        }

        /// <inheritdoc/>
        public IEnumerable<RelationItemModel> GetPagedDescendantsInReferences(
            Guid parentKey,
            long skip,
            long take,
            bool filterMustBeIsDependency,
            out long totalRecords)
        {
            if (_scopeAccessor.AmbientScope is null)
            {
                totalRecords = 0;
                return [];
            }

            IUmbracoDatabase database = _scopeAccessor.AmbientScope.Database;
            ISqlContext sqlContext = database.SqlContext;

            SqlSyntax.ISqlSyntaxProvider syntax = _scopeAccessor.AmbientScope.Database.SqlContext.SqlSyntax;

            // Gets the path of the parent with ",%" added
            Sql<ISqlContext> subsubQuery = sqlContext.Sql()
                .Select<NodeDto>(c => c.Path)
                .From<NodeDto>()
                .Where<NodeDto>(x => x.UniqueId == parentKey);

            // Gets the descendants of the parent node (using ",%" to exclude the parent itself)
            Sql<ISqlContext> subQuery = sqlContext.Sql()
                .Select<NodeDto>(x => x.NodeId)
                .From<NodeDto>()
                .WhereLike<NodeDto>(x => x.Path, subsubQuery, ",%");

            ISqlSyntaxProvider sx = sqlContext.SqlSyntax;
            string[] columns = [
                    sx.ColumnWithAlias("x", "id", "nodeId"),
                    sx.ColumnWithAlias("n", "uniqueId", "nodeKey"),
                    sx.ColumnWithAlias("n", "text", "nodeName"),
                    sx.ColumnWithAlias("n", "nodeObjectType", "nodeObjectType"),
                    sx.ColumnWithAlias("d", "published", "nodePublished"),
                    sx.ColumnWithAlias("ctn", "uniqueId", "contentTypeKey"),
                    sx.ColumnWithAlias("ct", "icon", "contentTypeIcon"),
                    sx.ColumnWithAlias("ct", "alias", "contentTypeAlias"),
                    sx.ColumnWithAlias("ctn", "text", "contentTypeName"),
                    sx.ColumnWithAlias("x", "alias", "relationTypeAlias"),
                    sx.ColumnWithAlias("x", "name", "relationTypeName"),
                    sx.ColumnWithAlias("x", "isDependency", "relationTypeIsDependency"),
                    sx.ColumnWithAlias("x", "dual", "relationTypeIsBidirectional")
                ];
            Sql<ISqlContext> innerUnionSql = GetInnerUnionSql();
            Sql<ISqlContext> sql = sqlContext.Sql()
                .SelectDistinct(columns)
                .From<NodeDto>("n")
                .InnerJoinNested(innerUnionSql, "x")
                .On<NodeDto, UnionHelperDto>((n, x) => n.NodeId == x.Id, "n", "x")
                .LeftJoin<ContentDto>("c")
                .On<NodeDto, ContentDto>(
                (left, right) => left.NodeId == right.NodeId,
                    aliasLeft: "n",
                    aliasRight: "c")
                .LeftJoin<ContentTypeDto>("ct")
                .On<ContentDto, ContentTypeDto>(
                (left, right) => left.ContentTypeId == right.NodeId,
                    aliasLeft: "c",
                    aliasRight: "ct")
                .LeftJoin<NodeDto>("ctn")
                .On<ContentTypeDto, NodeDto>(
                (left, right) => left.NodeId == right.NodeId,
                    aliasLeft: "ct",
                    aliasRight: "ctn")
                .LeftJoin<DocumentDto>("d")
                .On<NodeDto, DocumentDto>(
                    (left, right) => left.NodeId == right.NodeId,
                    aliasLeft: "n",
                    aliasRight: "d")
                .WhereIn((Expression<Func<NodeDto, object?>>)(x => x.NodeId), subQuery, "n");

            if (filterMustBeIsDependency)
            {
                sql.Where<RelationTypeDto>(rt => rt.IsDependency, "x");
            }

            totalRecords = database?.Count(sql!) ?? 0;

            RelationItemDto[] pagedResult;

            //Only to all this, if there is items
            if (totalRecords > 0)
            {
                // Ordering is required for paging
                sql.OrderBy<RelationTypeDto>(x => x.Alias, "x");

                pagedResult =
                    _scopeAccessor.AmbientScope?.Database.SkipTake<RelationItemDto>(skip, take, sql).ToArray() ??
                    Array.Empty<RelationItemDto>();
            }
            else
            {
                pagedResult = Array.Empty<RelationItemDto>();
            }

            return _umbracoMapper.MapEnumerable<RelationItemDto, RelationItemModel>(pagedResult);
        }

        private Sql<ISqlContext> GetInnerUnionSql()
        {
            if (_scopeAccessor.AmbientScope is null)
            {
                throw new InvalidOperationException("No Ambient Scope available");
            }

            ISqlSyntaxProvider? sx = _scopeAccessor.AmbientScope.Database.SqlContext.SqlSyntax;
            string[] columns = sx == null
                ? []
                : [
                    sx.ColumnWithAlias("cn", "uniqueId", "key"),
                    sx.ColumnWithAlias("cn", "trashed", "trashed"),
                    sx.ColumnWithAlias("cn", "nodeObjectType", "nodeObjectType"),
                    sx.ColumnWithAlias("pn", "uniqueId", "otherKey"),
                    sx.ColumnWithAlias("cr", "childId", "id"),
                    sx.ColumnWithAlias("cr", "parentId", "otherId"),
                    sx.ColumnWithAlias("rt", "alias", string.Empty),
                    sx.ColumnWithAlias("rt", "name", string.Empty),
                    sx.ColumnWithAlias("rt", "isDependency", string.Empty),
                    sx.ColumnWithAlias("rt", "dual", string.Empty)
                ];
            Sql<ISqlContext> innerUnionSqlChild = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
                .Select(columns)
                .From<RelationDto>("cr")
                .InnerJoin<RelationTypeDto>("rt")
                .On<RelationDto, RelationTypeDto>((cr, rt) => rt.Dual == false && rt.Id == cr.RelationType, "cr", "rt")
                .InnerJoin<NodeDto>("cn")
                .On<RelationDto, NodeDto>((cr, cn) => cr.ChildId == cn.NodeId, "cr", "cn")
                .InnerJoin<NodeDto>("pn")
                .On<RelationDto, NodeDto>((cr, pn) => cr.ParentId == pn.NodeId, "cr", "pn");

            columns = sx == null
                ? []
                : [
                    sx.ColumnWithAlias("pn", "uniqueId", "key"),
                    sx.ColumnWithAlias("pn", "trashed", "trashed"),
                    sx.ColumnWithAlias("pn", "nodeObjectType", "nodeObjectType"),
                    sx.ColumnWithAlias("cn", "uniqueId", "otherKey"),
                    sx.ColumnWithAlias("dpr", "parentId", "id"),
                    sx.ColumnWithAlias("dpr", "childId", "otherId"),
                    sx.ColumnWithAlias("dprt", "alias", string.Empty),
                    sx.ColumnWithAlias("dprt", "name", string.Empty),
                    sx.ColumnWithAlias("dprt", "isDependency", string.Empty),
                    sx.ColumnWithAlias("dprt", "dual", string.Empty)
                ];
            Sql<ISqlContext> innerUnionSqlDualParent = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
                .Select(columns)
                .From<RelationDto>("dpr")
                .InnerJoin<RelationTypeDto>("dprt")
                .On<RelationDto, RelationTypeDto>(
                    (dpr, dprt) => dprt.Dual == true && dprt.Id == dpr.RelationType, "dpr", "dprt")
                .InnerJoin<NodeDto>("cn")
                .On<RelationDto, NodeDto>((dpr, cn) => dpr.ChildId == cn.NodeId, "dpr", "cn")
                .InnerJoin<NodeDto>("pn")
                .On<RelationDto, NodeDto>((dpr, pn) => dpr.ParentId == pn.NodeId, "dpr", "pn");
            columns = sx == null
                ? []
                : [
                    sx.ColumnWithAlias("cn", "uniqueId", "key"),
                    sx.ColumnWithAlias("cn", "trashed", "trashed"),
                    sx.ColumnWithAlias("cn", "nodeObjectType", "nodeObjectType"),
                    sx.ColumnWithAlias("pn", "uniqueId", "otherKey"),
                    sx.ColumnWithAlias("dcr", "childId", "id"),
                    sx.ColumnWithAlias("dcr", "parentId", "otherId"),
                    sx.ColumnWithAlias("dcrt", "alias", string.Empty),
                    sx.ColumnWithAlias("dcrt", "name", string.Empty),
                    sx.ColumnWithAlias("dcrt", "isDependency", string.Empty),
                    sx.ColumnWithAlias("dcrt", "dual", string.Empty)
                ];
            Sql<ISqlContext> innerUnionSql3 = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
                .Select(columns)
                .From<RelationDto>("dcr")
                .InnerJoin<RelationTypeDto>("dcrt")
                .On<RelationDto, RelationTypeDto>(
                    (dcr, dcrt) => dcrt.Dual == true && dcrt.Id == dcr.RelationType, "dcr", "dcrt")
                .InnerJoin<NodeDto>("cn")
                .On<RelationDto, NodeDto>((dcr, cn) => dcr.ChildId == cn.NodeId, "dcr", "cn")
                .InnerJoin<NodeDto>("pn")
                .On<RelationDto, NodeDto>((dcr, pn) => dcr.ParentId == pn.NodeId, "dcr", "pn");

            Sql<ISqlContext> innerUnionSql = innerUnionSqlChild.Union(innerUnionSqlDualParent).Union(innerUnionSql3);

            return innerUnionSql;
        }

        private sealed class UnionHelperDto
        {
            [Column("id")]
            public int Id { get; set; }

            [Column("otherId")]
            public int OtherId { get; set; }

            [Column("key")]
            public Guid Key { get; set; }

            [Column("trashed")]
            public bool Trashed { get; set; }

            [Column("nodeObjectType")]
            public Guid NodeObjectType { get; set; }

            [Column("otherKey")]
            public Guid OtherKey { get; set; }

            [Column("alias")]
            public string? Alias { get; set; }

            [Column("name")]
            public string? Name { get; set; }

            [Column("isDependency")]
            public bool IsDependency { get; set; }

            [Column("dual")]
            public bool Dual { get; set; }
        }

        private RelationItem MapDtoToEntity(RelationItemDto dto) =>
            new()
            {
                NodeId = dto.ChildNodeId,
                NodeKey = dto.ChildNodeKey,
                NodeType = ObjectTypes.GetUdiType(dto.ChildNodeObjectType),
                NodeName = dto.ChildNodeName,
                NodePublished = dto.ChildNodePublished,
                RelationTypeName = dto.RelationTypeName,
                RelationTypeIsBidirectional = dto.RelationTypeIsBidirectional,
                RelationTypeIsDependency = dto.RelationTypeIsDependency,
                ContentTypeKey = dto.ChildContentTypeKey,
                ContentTypeAlias = dto.ChildContentTypeAlias,
                ContentTypeIcon = dto.ChildContentTypeIcon,
                ContentTypeName = dto.ChildContentTypeName,
            };
    }
}
