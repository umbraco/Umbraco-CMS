using NPoco;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using Umbraco.New.Cms.Core.Models.TrackedReferences;
using Umbraco.New.Cms.Core.Persistence.Repositories;
using Umbraco.New.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.New.Cms.Infrastructure.Persistence.Repositories.Implement;

public class TrackedReferencesSkipTakeRepository : ITrackedReferencesSkipTakeRepository
{
    private readonly IScopeAccessor _scopeAccessor;
    private readonly IUmbracoMapper _umbracoMapper;

    public TrackedReferencesSkipTakeRepository(IScopeAccessor scopeAccessor, IUmbracoMapper umbracoMapper)
    {
        _scopeAccessor = scopeAccessor;
        _umbracoMapper = umbracoMapper;
    }

    public IEnumerable<RelationModel> GetPagedRelationsForItem(
        int id,
        long skip,
        long take,
        bool filterMustBeIsDependency,
        out long totalRecords)
    {
        Sql<ISqlContext> innerUnionSql = GetInnerUnionSql();
            var sql = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql().SelectDistinct(
                                                                            "[x].[otherId] as nodeId",
                                                                            "[n].[uniqueId] as nodeKey",
                                                                            "[n].[text] as nodeName",
                                                                            "[n].[nodeObjectType] as nodeObjectType",
                                                                            "[ct].[icon] as contentTypeIcon",
                                                                            "[ct].[alias] as contentTypeAlias",
                                                                            "[ctn].[text] as contentTypeName",
                                                                            "[x].[alias] as relationTypeAlias",
                                                                            "[x].[name] as relationTypeName",
                                                                            "[x].[isDependency] as relationTypeIsDependency",
                                                                            "[x].[dual] as relationTypeIsBidirectional")
                                                                        .From<NodeDto>("n")
                                                                        .InnerJoinNested(innerUnionSql, "x")
                                                                        .On<NodeDto, UnionHelperDto>((n, x) => n.NodeId == x.OtherId, "n", "x")
                                                                        .LeftJoin<ContentDto>("c").On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId, aliasLeft: "n",
                                                                            aliasRight: "c")
                                                                        .LeftJoin<ContentTypeDto>("ct")
                                                                        .On<ContentDto, ContentTypeDto>((left, right) => left.ContentTypeId == right.NodeId, aliasLeft: "c",
                                                                            aliasRight: "ct")
                                                                        .LeftJoin<NodeDto>("ctn").On<ContentTypeDto, NodeDto>((left, right) => left.NodeId == right.NodeId,
                                                                            aliasLeft: "ct", aliasRight: "ctn")
                                                                        .Where<UnionHelperDto>(x => x.Id == id, "x");

            if (filterMustBeIsDependency)
            {
                sql = sql?.Where<RelationTypeDto>(rt => rt.IsDependency, "x");
            }

            // Ordering is required for paging
            sql = sql?.OrderBy<RelationTypeDto>(x => x.Alias, "x");

            RelationItemDto[] pagedResult = _scopeAccessor.AmbientScope?.Database.SkipTake<RelationItemDto>(skip, take, sql).ToArray() ?? Array.Empty<RelationItemDto>();
            totalRecords = pagedResult.Length;

            return _umbracoMapper.MapEnumerable<RelationItemDto, RelationModel>(pagedResult);
    }

    public IEnumerable<RelationModel> GetPagedItemsWithRelations(
        int[] ids,
        long skip,
        long take,
        bool filterMustBeIsDependency,
        out long totalRecords)
    {
        Sql<ISqlContext> innerUnionSql = GetInnerUnionSql();
        Sql<ISqlContext>? sql = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql().SelectDistinct(
                "[x].[id] as nodeId",
                "[n].[uniqueId] as nodeKey",
                "[n].[text] as nodeName",
                "[n].[nodeObjectType] as nodeObjectType",
                "[ct].[icon] as contentTypeIcon",
                "[ct].[alias] as contentTypeAlias",
                "[ctn].[text] as contentTypeName",
                "[x].[alias] as relationTypeAlias",
                "[x].[name] as relationTypeName",
                "[x].[isDependency] as relationTypeIsDependency",
                "[x].[dual] as relationTypeIsBidirectional")
            .From<NodeDto>("n")
            .InnerJoinNested(innerUnionSql, "x")
            .On<NodeDto, UnionHelperDto>((n, x) => n.NodeId == x.Id, "n", "x")
            .LeftJoin<ContentDto>("c")
            .On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId, aliasLeft: "n", aliasRight: "c")
            .LeftJoin<ContentTypeDto>("ct")
            .On<ContentDto, ContentTypeDto>((left, right) => left.ContentTypeId == right.NodeId, aliasLeft: "c", aliasRight: "ct")
            .LeftJoin<NodeDto>("ctn")
            .On<ContentTypeDto, NodeDto>((left, right) => left.NodeId == right.NodeId, aliasLeft: "ct", aliasRight: "ctn");
        if (ids.Any())
        {
            sql = sql?.Where<NodeDto>(x => ids.Contains(x.NodeId), "n");
        }

        if (filterMustBeIsDependency)
        {
            sql = sql?.Where<RelationTypeDto>(rt => rt.IsDependency, "x");
        }

        // Ordering is required for paging
        sql = sql?.OrderBy<RelationTypeDto>(x => x.Alias, "x");

        RelationItemDto[] pagedResult = _scopeAccessor.AmbientScope?.Database.SkipTake<RelationItemDto>(skip, take, sql).ToArray() ?? Array.Empty<RelationItemDto>();
        totalRecords = pagedResult.Length;

        return _umbracoMapper.MapEnumerable<RelationItemDto, RelationModel>(pagedResult);
    }

    public IEnumerable<RelationModel> GetPagedDescendantsInReferences(
        int parentId,
        long skip,
        long take,
        bool filterMustBeIsDependency,
        out long totalRecords)
    {
        var syntax = _scopeAccessor.AmbientScope?.Database.SqlContext.SqlSyntax;

            // Gets the path of the parent with ",%" added
            var subsubQuery = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql()
                .Select(syntax?.GetConcat("[node].[path]", "',%'"))
                .From<NodeDto>("node")
                .Where<NodeDto>(x => x.NodeId == parentId, "node");

            // Gets the descendants of the parent node
            Sql<ISqlContext>? subQuery = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql()
                .Select<NodeDto>(x => x.NodeId)
                .From<NodeDto>()
                .WhereLike<NodeDto>(x => x.Path, subsubQuery);

            Sql<ISqlContext> innerUnionSql = GetInnerUnionSql();
            var sql = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql().SelectDistinct(
                                                                            "[x].[id] as nodeId",
                                                                            "[n].[uniqueId] as nodeKey",
                                                                            "[n].[text] as nodeName",
                                                                            "[n].[nodeObjectType] as nodeObjectType",
                                                                            "[ct].[icon] as contentTypeIcon",
                                                                            "[ct].[alias] as contentTypeAlias",
                                                                            "[ctn].[text] as contentTypeName",
                                                                            "[x].[alias] as relationTypeAlias",
                                                                            "[x].[name] as relationTypeName",
                                                                            "[x].[isDependency] as relationTypeIsDependency",
                                                                            "[x].[dual] as relationTypeIsBidirectional")
                                                                        .From<NodeDto>("n")
                                                                        .InnerJoinNested(innerUnionSql, "x")
                                                                        .On<NodeDto, UnionHelperDto>((n, x) => n.NodeId == x.Id, "n", "x")
                                                                        .LeftJoin<ContentDto>("c").On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId, aliasLeft: "n",
                                                                            aliasRight: "c")
                                                                        .LeftJoin<ContentTypeDto>("ct")
                                                                        .On<ContentDto, ContentTypeDto>((left, right) => left.ContentTypeId == right.NodeId, aliasLeft: "c",
                                                                            aliasRight: "ct")
                                                                        .LeftJoin<NodeDto>("ctn").On<ContentTypeDto, NodeDto>((left, right) => left.NodeId == right.NodeId,
                                                                            aliasLeft: "ct", aliasRight: "ctn");
            sql = sql?.WhereIn((System.Linq.Expressions.Expression<Func<NodeDto, object?>>)(x => x.NodeId), subQuery, "n");

            if (filterMustBeIsDependency)
            {
                sql = sql?.Where<RelationTypeDto>(rt => rt.IsDependency, "x");
            }

            // Ordering is required for paging
            sql = sql?.OrderBy<RelationTypeDto>(x => x.Alias, "x");

            RelationItemDto[] pagedResult = _scopeAccessor.AmbientScope?.Database.SkipTake<RelationItemDto>(skip, take, sql).ToArray() ?? Array.Empty<RelationItemDto>();
            totalRecords = pagedResult.Length;

            return _umbracoMapper.MapEnumerable<RelationItemDto, RelationModel>(pagedResult);
    }

    private Sql<ISqlContext> GetInnerUnionSql()
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("No Ambient Scope available");
        }

        Sql<ISqlContext> innerUnionSqlChild = _scopeAccessor.AmbientScope.Database.SqlContext.Sql().Select(
                "[cr].childId as id",
                "[cr].parentId as otherId",
                "[rt].[alias]",
                "[rt].[name]",
                "[rt].[isDependency]",
                "[rt].[dual]")
            .From<RelationDto>("cr").InnerJoin<RelationTypeDto>("rt")
            .On<RelationDto, RelationTypeDto>((cr, rt) => rt.Dual == false && rt.Id == cr.RelationType, "cr", "rt");

        Sql<ISqlContext> innerUnionSqlDualParent = _scopeAccessor.AmbientScope.Database.SqlContext.Sql().Select(
                "[dpr].parentId as id",
                "[dpr].childId as otherId",
                "[dprt].[alias]",
                "[dprt].[name]",
                "[dprt].[isDependency]",
                "[dprt].[dual]")
            .From<RelationDto>("dpr").InnerJoin<RelationTypeDto>("dprt")
            .On<RelationDto, RelationTypeDto>((dpr, dprt) => dprt.Dual == true && dprt.Id == dpr.RelationType, "dpr", "dprt");

        Sql<ISqlContext> innerUnionSql3 = _scopeAccessor.AmbientScope.Database.SqlContext.Sql().Select(
                "[dcr].childId as id",
                "[dcr].parentId as otherId",
                "[dcrt].[alias]",
                "[dcrt].[name]",
                "[dcrt].[isDependency]",
                "[dcrt].[dual]")
            .From<RelationDto>("dcr").InnerJoin<RelationTypeDto>("dcrt")
            .On<RelationDto, RelationTypeDto>((dcr, dcrt) => dcrt.Dual == true && dcrt.Id == dcr.RelationType, "dcr", "dcrt");


        Sql<ISqlContext> innerUnionSql = innerUnionSqlChild.Union(innerUnionSqlDualParent).Union(innerUnionSql3);

        return innerUnionSql;
    }
}
