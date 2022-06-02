using System.Linq.Expressions;
using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal class TrackedReferencesRepository : ITrackedReferencesRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public TrackedReferencesRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    /// <summary>
    ///     Gets a page of items used in any kind of relation from selected integer ids.
    /// </summary>
    public IEnumerable<RelationItem> GetPagedItemsWithRelations(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency, out long totalRecords)
    {
        Sql<ISqlContext>? sql = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql().SelectDistinct(
                "[pn].[id] as nodeId",
                "[pn].[uniqueId] as nodeKey",
                "[pn].[text] as nodeName",
                "[pn].[nodeObjectType] as nodeObjectType",
                "[ct].[icon] as contentTypeIcon",
                "[ct].[alias] as contentTypeAlias",
                "[ctn].[text] as contentTypeName",
                "[umbracoRelationType].[alias] as relationTypeAlias",
                "[umbracoRelationType].[name] as relationTypeName",
                "[umbracoRelationType].[isDependency] as relationTypeIsDependency",
                "[umbracoRelationType].[dual] as relationTypeIsBidirectional")
            .From<RelationDto>("r")
            .InnerJoin<RelationTypeDto>("umbracoRelationType")
            .On<RelationDto, RelationTypeDto>((left, right) => left.RelationType == right.Id, "r", "umbracoRelationType")
            .InnerJoin<NodeDto>("cn").On<RelationDto, NodeDto, RelationTypeDto>(
                (r, cn, rt) => (!rt.Dual && r.ParentId == cn.NodeId) || (rt.Dual && (r.ChildId == cn.NodeId || r.ParentId == cn.NodeId)), "r", "cn", "umbracoRelationType")
            .InnerJoin<NodeDto>("pn").On<RelationDto, NodeDto, NodeDto>(
                (r, pn, cn) => (pn.NodeId == r.ChildId && cn.NodeId == r.ParentId) || (pn.NodeId == r.ParentId && cn.NodeId == r.ChildId), "r", "pn", "cn")
            .LeftJoin<ContentDto>("c").On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId, "pn", "c")
            .LeftJoin<ContentTypeDto>("ct")
            .On<ContentDto, ContentTypeDto>((left, right) => left.ContentTypeId == right.NodeId, "c", "ct")
            .LeftJoin<NodeDto>("ctn")
            .On<ContentTypeDto, NodeDto>((left, right) => left.NodeId == right.NodeId, "ct", "ctn");

        if (ids.Any())
        {
            sql = sql?.Where<NodeDto>(x => ids.Contains(x.NodeId), "pn");
        }

        if (filterMustBeIsDependency)
        {
            sql = sql?.Where<RelationTypeDto>(rt => rt.IsDependency, "umbracoRelationType");
        }

        // Ordering is required for paging
        sql = sql?.OrderBy<RelationTypeDto>(x => x.Alias);

        Page<RelationItemDto>? pagedResult =
            _scopeAccessor.AmbientScope?.Database.Page<RelationItemDto>(pageIndex + 1, pageSize, sql);
        totalRecords = Convert.ToInt32(pagedResult?.TotalItems);

        return pagedResult?.Items.Select(MapDtoToEntity) ?? Enumerable.Empty<RelationItem>();
    }

    /// <summary>
    ///     Gets a page of the descending items that have any references, given a parent id.
    /// </summary>
    public IEnumerable<RelationItem> GetPagedDescendantsInReferences(int parentId, long pageIndex, int pageSize, bool filterMustBeIsDependency, out long totalRecords)
    {
        ISqlSyntaxProvider? syntax = _scopeAccessor.AmbientScope?.Database.SqlContext.SqlSyntax;

        // Gets the path of the parent with ",%" added
        Sql<ISqlContext>? subsubQuery = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql()
            .Select(syntax?.GetConcat("[node].[path]", "',%'"))
            .From<NodeDto>("node")
            .Where<NodeDto>(x => x.NodeId == parentId, "node");

        // Gets the descendants of the parent node
        Sql<ISqlContext>? subQuery = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql()
            .Select<NodeDto>(x => x.NodeId)
            .From<NodeDto>()
            .WhereLike<NodeDto>(x => x.Path, subsubQuery);

        // Get all relations where parent is in the sub query
        Sql<ISqlContext>? sql = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql().SelectDistinct(
                "[pn].[id] as nodeId",
                "[pn].[uniqueId] as nodeKey",
                "[pn].[text] as nodeName",
                "[pn].[nodeObjectType] as nodeObjectType",
                "[ct].[icon] as contentTypeIcon",
                "[ct].[alias] as contentTypeAlias",
                "[ctn].[text] as contentTypeName",
                "[umbracoRelationType].[alias] as relationTypeAlias",
                "[umbracoRelationType].[name] as relationTypeName",
                "[umbracoRelationType].[isDependency] as relationTypeIsDependency",
                "[umbracoRelationType].[dual] as relationTypeIsBidirectional")
            .From<RelationDto>("r")
            .InnerJoin<RelationTypeDto>("umbracoRelationType")
            .On<RelationDto, RelationTypeDto>((left, right) => left.RelationType == right.Id, "r", "umbracoRelationType")
            .InnerJoin<NodeDto>("cn").On<RelationDto, NodeDto, RelationTypeDto>(
                (r, cn, rt) => (!rt.Dual && r.ParentId == cn.NodeId) || (rt.Dual && (r.ChildId == cn.NodeId || r.ParentId == cn.NodeId)), "r", "cn", "umbracoRelationType")
            .InnerJoin<NodeDto>("pn").On<RelationDto, NodeDto, NodeDto>(
                (r, pn, cn) => (pn.NodeId == r.ChildId && cn.NodeId == r.ParentId) || (pn.NodeId == r.ParentId && cn.NodeId == r.ChildId), "r", "pn", "cn")
            .LeftJoin<ContentDto>("c").On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId, "pn", "c")
            .LeftJoin<ContentTypeDto>("ct")
            .On<ContentDto, ContentTypeDto>((left, right) => left.ContentTypeId == right.NodeId, "c", "ct")
            .LeftJoin<NodeDto>("ctn")
            .On<ContentTypeDto, NodeDto>((left, right) => left.NodeId == right.NodeId, "ct", "ctn")
            .WhereIn((Expression<Func<NodeDto, object?>>)(x => x.NodeId), subQuery, "pn");

        if (filterMustBeIsDependency)
        {
            sql = sql?.Where<RelationTypeDto>(rt => rt.IsDependency, "umbracoRelationType");
        }

        // Ordering is required for paging
        sql = sql?.OrderBy<RelationTypeDto>(x => x.Alias);

        Page<RelationItemDto>? pagedResult =
            _scopeAccessor.AmbientScope?.Database.Page<RelationItemDto>(pageIndex + 1, pageSize, sql);
        totalRecords = Convert.ToInt32(pagedResult?.TotalItems);

        return pagedResult?.Items.Select(MapDtoToEntity) ?? Enumerable.Empty<RelationItem>();
    }

    /// <summary>
    ///     Gets a page of items which are in relation with the current item.
    ///     Basically, shows the items which depend on the current item.
    /// </summary>
    public IEnumerable<RelationItem> GetPagedRelationsForItem(int id, long pageIndex, int pageSize, bool filterMustBeIsDependency, out long totalRecords)
    {
        Sql<ISqlContext>? sql = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql().SelectDistinct(
                "[cn].[id] as nodeId",
                "[cn].[uniqueId] as nodeKey",
                "[cn].[text] as nodeName",
                "[cn].[nodeObjectType] as nodeObjectType",
                "[ct].[icon] as contentTypeIcon",
                "[ct].[alias] as contentTypeAlias",
                "[ctn].[text] as contentTypeName",
                "[umbracoRelationType].[alias] as relationTypeAlias",
                "[umbracoRelationType].[name] as relationTypeName",
                "[umbracoRelationType].[isDependency] as relationTypeIsDependency",
                "[umbracoRelationType].[dual] as relationTypeIsBidirectional")
            .From<RelationDto>("r")
            .InnerJoin<RelationTypeDto>("umbracoRelationType")
            .On<RelationDto, RelationTypeDto>((left, right) => left.RelationType == right.Id, "r", "umbracoRelationType")
            .InnerJoin<NodeDto>("cn").On<RelationDto, NodeDto, RelationTypeDto>(
                (r, cn, rt) => (!rt.Dual && r.ParentId == cn.NodeId) || (rt.Dual && (r.ChildId == cn.NodeId || r.ParentId == cn.NodeId)), "r", "cn", "umbracoRelationType")
            .InnerJoin<NodeDto>("pn").On<RelationDto, NodeDto, NodeDto>(
                (r, pn, cn) => (pn.NodeId == r.ChildId && cn.NodeId == r.ParentId) || (pn.NodeId == r.ParentId && cn.NodeId == r.ChildId), "r", "pn", "cn")
            .LeftJoin<ContentDto>("c").On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId, "cn", "c")
            .LeftJoin<ContentTypeDto>("ct")
            .On<ContentDto, ContentTypeDto>((left, right) => left.ContentTypeId == right.NodeId, "c", "ct")
            .LeftJoin<NodeDto>("ctn")
            .On<ContentTypeDto, NodeDto>((left, right) => left.NodeId == right.NodeId, "ct", "ctn")
            .Where<NodeDto>(x => x.NodeId == id, "pn")
            .Where<RelationDto>(
                x => x.ChildId == id || x.ParentId == id,
                "r"); // This last Where is purely to help SqlServer make a smarter query plan. More info https://github.com/umbraco/Umbraco-CMS/issues/12190

        if (filterMustBeIsDependency)
        {
            sql = sql?.Where<RelationTypeDto>(rt => rt.IsDependency, "umbracoRelationType");
        }

        // Ordering is required for paging
        sql = sql?.OrderBy<RelationTypeDto>(x => x.Alias);

        Page<RelationItemDto>? pagedResult =
            _scopeAccessor.AmbientScope?.Database.Page<RelationItemDto>(pageIndex + 1, pageSize, sql);
        totalRecords = Convert.ToInt32(pagedResult?.TotalItems);

        return pagedResult?.Items.Select(MapDtoToEntity) ?? Enumerable.Empty<RelationItem>();
    }

    private RelationItem MapDtoToEntity(RelationItemDto dto) =>
        new RelationItem
        {
            NodeId = dto.ChildNodeId,
            NodeKey = dto.ChildNodeKey,
            NodeType = ObjectTypes.GetUdiType(dto.ChildNodeObjectType),
            NodeName = dto.ChildNodeName,
            RelationTypeName = dto.RelationTypeName,
            RelationTypeIsBidirectional = dto.RelationTypeIsBidirectional,
            RelationTypeIsDependency = dto.RelationTypeIsDependency,
            ContentTypeAlias = dto.ChildContentTypeAlias,
            ContentTypeIcon = dto.ChildContentTypeIcon,
            ContentTypeName = dto.ChildContentTypeName,
        };
}
