﻿using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IMediaType"/>
    /// </summary>
    internal class MediaTypeRepository : ContentTypeBaseRepository<IMediaType>, IMediaTypeRepository
    {

        public MediaTypeRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
            : base(work, cache, logger, sqlSyntax)
        {
        }

        #region Overrides of RepositoryBase<int,IMedia>

        protected override IMediaType PerformGet(int id)
        {
            var contentTypes = ContentTypeQueryMapper.GetMediaTypes(
                new[] { id }, Database, SqlSyntax, this);

            var contentType = contentTypes.SingleOrDefault();
            return contentType;
        }

        protected override IEnumerable<IMediaType> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                return ContentTypeQueryMapper.GetMediaTypes(ids, Database, SqlSyntax, this);
            }
            else
            {
                var sql = new Sql().Select("id").From<NodeDto>().Where<NodeDto>(dto => dto.NodeObjectType == NodeObjectTypeId);
                var allIds = Database.Fetch<int>(sql).ToArray();
                return ContentTypeQueryMapper.GetMediaTypes(allIds, Database, SqlSyntax, this);
            }
        }

        protected override IEnumerable<IMediaType> PerformGetByQuery(IQuery<IMediaType> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IMediaType>(sqlClause, query);
            var sql = translator.Translate()
                .OrderBy<NodeDto>(x => x.Text);

            var dtos = Database.Fetch<ContentTypeDto, NodeDto>(sql);
            return dtos.Any()
                ? GetAll(dtos.DistinctBy(x => x.NodeId).Select(x => x.NodeId).ToArray())
                : Enumerable.Empty<IMediaType>();
        }

        #endregion


        /// <summary>
        /// Gets all entities of the specified <see cref="PropertyType"/> query
        /// </summary>
        /// <param name="query"></param>
        /// <returns>An enumerable list of <see cref="IContentType"/> objects</returns>
        public IEnumerable<IMediaType> GetByQuery(IQuery<PropertyType> query)
        {
            var ints = PerformGetByQuery(query).ToArray();
            return ints.Any()
                ? GetAll(ints)
                : Enumerable.Empty<IMediaType>();
        }

        #region Overrides of PetaPocoRepositoryBase<int,IMedia>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
                .From<ContentTypeDto>()
                .InnerJoin<NodeDto>()
                .On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoNode.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                           {
                               "DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @Id",
                               "DELETE FROM umbracoUser2NodePermission WHERE nodeId = @Id",
                               "DELETE FROM cmsTagRelationship WHERE nodeId = @Id",
                               "DELETE FROM cmsContentTypeAllowedContentType WHERE Id = @Id",
                               "DELETE FROM cmsContentTypeAllowedContentType WHERE AllowedId = @Id",
                               "DELETE FROM cmsContentType2ContentType WHERE parentContentTypeId = @Id",
                               "DELETE FROM cmsContentType2ContentType WHERE childContentTypeId = @Id",
                               "DELETE FROM cmsPropertyType WHERE contentTypeId = @Id",
                               "DELETE FROM cmsPropertyTypeGroup WHERE contenttypeNodeId = @Id",
                               "DELETE FROM cmsContentType WHERE nodeId = @Id",
                               "DELETE FROM umbracoNode WHERE id = @Id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid(Constants.ObjectTypes.MediaType); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IMediaType entity)
        {
            ((MediaType)entity).AddingEntity();

            var factory = new MediaTypeFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(entity);

            PersistNewBaseContentType(dto, entity);

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IMediaType entity)
        {
            ValidateAlias(entity);

            //Updates Modified date
            ((MediaType)entity).UpdatingEntity();

            //Look up parent to get and set the correct Path if ParentId has changed
            if (entity.IsPropertyDirty("ParentId"))
            {
                var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = entity.ParentId });
                entity.Path = string.Concat(parent.Path, ",", entity.Id);
                entity.Level = parent.Level + 1;
                var maxSortOrder =
                    Database.ExecuteScalar<int>(
                        "SELECT coalesce(max(sortOrder),0) FROM umbracoNode WHERE parentid = @ParentId AND nodeObjectType = @NodeObjectType",
                        new { ParentId = entity.ParentId, NodeObjectType = NodeObjectTypeId });
                entity.SortOrder = maxSortOrder + 1;
            }

            var factory = new MediaTypeFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(entity);
            
            PersistUpdatedBaseContentType(dto, entity);

            entity.ResetDirtyProperties();
        }

        #endregion

        protected override IMediaType PerformGet(Guid id)
        {
            var contentTypes = ContentTypeQueryMapper.GetMediaTypes(
                new[] { id }, Database, SqlSyntax, this);

            var contentType = contentTypes.SingleOrDefault();
            return contentType;
        }

        protected override IEnumerable<IMediaType> PerformGetAll(params Guid[] ids)
        {
            if (ids.Any())
            {
                return ContentTypeQueryMapper.GetMediaTypes(ids, Database, SqlSyntax, this);
            }
            else
            {
                var sql = new Sql().Select("id").From<NodeDto>(SqlSyntax).Where<NodeDto>(dto => dto.NodeObjectType == NodeObjectTypeId);
                var allIds = Database.Fetch<int>(sql).ToArray();
                return ContentTypeQueryMapper.GetMediaTypes(allIds, Database, SqlSyntax, this);
            }
        }
    }
}