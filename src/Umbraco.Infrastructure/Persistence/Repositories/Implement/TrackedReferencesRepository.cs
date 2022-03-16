using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    internal class TrackedReferencesRepository : ITrackedReferencesRepository
    {
        private readonly IScopeAccessor _scopeAccessor;

        public TrackedReferencesRepository(IScopeAccessor scopeAccessor)
        {
            _scopeAccessor = scopeAccessor;
        }

        public IEnumerable<RelationItem> GetPagedItemsWithRelations(int[] ids, long pageIndex, int pageSize,
            bool filterMustBeIsDependency, out long totalRecords)
        {
            var sql = _scopeAccessor.AmbientScope.Database.SqlContext.Sql().Select(
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
                .InnerJoin<RelationTypeDto>("umbracoRelationType").On<RelationDto, RelationTypeDto>((left, right) => left.RelationType == right.Id, aliasLeft: "r", aliasRight:"umbracoRelationType")
                .InnerJoin<NodeDto>("cn").On<RelationDto, NodeDto, RelationTypeDto>((r, cn, rt) => (!rt.Dual && r.ParentId == cn.NodeId) || (rt.Dual && (r.ChildId == cn.NodeId || r.ParentId == cn.NodeId )), aliasLeft: "r", aliasRight:"cn", aliasOther: "umbracoRelationType" )
                .InnerJoin<NodeDto>("pn").On<RelationDto, NodeDto, NodeDto>((r, pn, cn) => (pn.NodeId == r.ChildId && cn.NodeId == r.ParentId) || (pn.NodeId == r.ParentId && cn.NodeId == r.ChildId), aliasLeft: "r", aliasRight:"pn", aliasOther:"cn" )
                .LeftJoin<ContentDto>("c").On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId, aliasLeft:"pn", aliasRight:"c")
                .LeftJoin<ContentTypeDto>("ct").On<ContentDto, ContentTypeDto>((left, right) => left.ContentTypeId == right.NodeId, aliasLeft:"c",  aliasRight:"ct")
                .LeftJoin<NodeDto>("ctn").On<ContentTypeDto, NodeDto>((left, right) => left.NodeId == right.NodeId, aliasLeft:"ct",  aliasRight:"ctn");

            if (ids.Any())
            {
                sql = sql.Where<NodeDto>(x => ids.Contains(x.NodeId), "pn");
            }

            if (filterMustBeIsDependency)
            {
                sql = sql.Where<RelationTypeDto>(rt => rt.IsDependency, "umbracoRelationType");
            }

            // Ordering is required for paging
            sql = sql.OrderBy<RelationTypeDto>(x => x.Alias);

            var pagedResult = _scopeAccessor.AmbientScope.Database.Page<RelationItemDto>(pageIndex + 1, pageSize, sql);
            totalRecords = Convert.ToInt32(pagedResult.TotalItems);

            return pagedResult.Items.Select(MapDtoToEntity);
        }

        public IEnumerable<RelationItem> GetPagedDescendantsInReferences(int parentId, long pageIndex, int pageSize, bool filterMustBeIsDependency,
            out long totalRecords)
        {
            var syntax = _scopeAccessor.AmbientScope.Database.SqlContext.SqlSyntax;

            // Gets the path of the parent with ",%" added
            var subsubQuery = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
                .Select(syntax.GetConcat("[node].[path]", "',%'"))
                .From<NodeDto>("node")
                .Where<NodeDto>(x => x.NodeId == parentId, "node");


            // Gets the descendants of the parent node
            Sql<ISqlContext> subQuery = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
                .Select<NodeDto>(x => x.NodeId)
                .From<NodeDto>()
                .WhereLike<NodeDto>(x => x.Path, subsubQuery);

            // Get all relations where parent is in the sub query
            var sql = _scopeAccessor.AmbientScope.Database.SqlContext.Sql().Select(
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
                .InnerJoin<RelationTypeDto>("umbracoRelationType").On<RelationDto, RelationTypeDto>((left, right) => left.RelationType == right.Id, aliasLeft: "r", aliasRight:"umbracoRelationType")
                .InnerJoin<NodeDto>("cn").On<RelationDto, NodeDto, RelationTypeDto>((r, cn, rt) => (!rt.Dual && r.ParentId == cn.NodeId) || (rt.Dual && (r.ChildId == cn.NodeId || r.ParentId == cn.NodeId )), aliasLeft: "r", aliasRight:"cn", aliasOther: "umbracoRelationType" )
                .InnerJoin<NodeDto>("pn").On<RelationDto, NodeDto, NodeDto>((r, pn, cn) => (pn.NodeId == r.ChildId && cn.NodeId == r.ParentId) || (pn.NodeId == r.ParentId && cn.NodeId == r.ChildId), aliasLeft: "r", aliasRight:"pn", aliasOther:"cn" )
                .LeftJoin<ContentDto>("c").On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId, aliasLeft:"pn", aliasRight:"c")
                .LeftJoin<ContentTypeDto>("ct").On<ContentDto, ContentTypeDto>((left, right) => left.ContentTypeId == right.NodeId, aliasLeft:"c",  aliasRight:"ct")
                .LeftJoin<NodeDto>("ctn").On<ContentTypeDto, NodeDto>((left, right) => left.NodeId == right.NodeId, aliasLeft:"ct",  aliasRight:"ctn")
                .WhereIn((System.Linq.Expressions.Expression<Func<NodeDto, object>>)(x => x.NodeId), subQuery, "pn");
            if (filterMustBeIsDependency)
            {
                sql = sql.Where<RelationTypeDto>(rt => rt.IsDependency, "umbracoRelationType");
            }
            // Ordering is required for paging
            sql = sql.OrderBy<RelationTypeDto>(x => x.Alias);

            var pagedResult = _scopeAccessor.AmbientScope.Database.Page<RelationItemDto>(pageIndex + 1, pageSize, sql);
            totalRecords = Convert.ToInt32(pagedResult.TotalItems);

            return pagedResult.Items.Select(MapDtoToEntity);
        }

        public IEnumerable<RelationItem> GetPagedRelationsForItems(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency, out long totalRecords)
        {
            var sql = _scopeAccessor.AmbientScope.Database.SqlContext.Sql().Select(
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
                .InnerJoin<RelationTypeDto>("umbracoRelationType").On<RelationDto, RelationTypeDto>((left, right) => left.RelationType == right.Id, aliasLeft: "r", aliasRight:"umbracoRelationType")
                .InnerJoin<NodeDto>("cn").On<RelationDto, NodeDto, RelationTypeDto>((r, cn, rt) => (!rt.Dual && r.ParentId == cn.NodeId) || (rt.Dual && (r.ChildId == cn.NodeId || r.ParentId == cn.NodeId )), aliasLeft: "r", aliasRight:"cn", aliasOther: "umbracoRelationType" )
                .InnerJoin<NodeDto>("pn").On<RelationDto, NodeDto, NodeDto>((r, pn, cn) => (pn.NodeId == r.ChildId && cn.NodeId == r.ParentId) || (pn.NodeId == r.ParentId && cn.NodeId == r.ChildId), aliasLeft: "r", aliasRight:"pn", aliasOther:"cn" )
                .LeftJoin<ContentDto>("c").On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId, aliasLeft:"cn", aliasRight:"c")
                .LeftJoin<ContentTypeDto>("ct").On<ContentDto, ContentTypeDto>((left, right) => left.ContentTypeId == right.NodeId, aliasLeft:"c",  aliasRight:"ct")
                .LeftJoin<NodeDto>("ctn").On<ContentTypeDto, NodeDto>((left, right) => left.NodeId == right.NodeId, aliasLeft:"ct",  aliasRight:"ctn");

            if (ids.Any())
            {
                sql = sql.Where<NodeDto>(x => ids.Contains(x.NodeId), "pn");
            }

            if (filterMustBeIsDependency)
            {
                sql = sql.Where<RelationTypeDto>(rt => rt.IsDependency, "umbracoRelationType");
            }

            // Ordering is required for paging
            sql = sql.OrderBy<RelationTypeDto>(x => x.Alias);

            var pagedResult = _scopeAccessor.AmbientScope.Database.Page<RelationItemDto>(pageIndex + 1, pageSize, sql);
            totalRecords = Convert.ToInt32(pagedResult.TotalItems);

            return pagedResult.Items.Select(MapDtoToEntity);
        }

        private RelationItem MapDtoToEntity(RelationItemDto dto)
        {
            var type = ObjectTypes.GetUdiType(dto.ChildNodeObjectType);
            return new RelationItem()
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
    }
}
