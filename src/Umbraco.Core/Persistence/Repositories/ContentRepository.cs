using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IContent"/>
    /// </summary>
    internal class ContentRepository : PetaPocoRepositoryBase<int, IContent>, IContentRepository
    {
        private readonly IContentTypeRepository _contentTypeRepository;

        public ContentRepository(IUnitOfWork work, IContentTypeRepository contentTypeRepository)
            : base(work)
        {
            _contentTypeRepository = contentTypeRepository;
        }

        public ContentRepository(IUnitOfWork work, IRepositoryCacheProvider cache, IContentTypeRepository contentTypeRepository)
            : base(work, cache)
        {
            _contentTypeRepository = contentTypeRepository;
        }

        #region Overrides of RepositoryBase<IContent>

        protected override IContent PerformGet(int id)
        {
            var contentSql = GetBaseQuery(false);
            contentSql.Append(GetBaseWhereClause(id));
            contentSql.OrderBy("[cmsContentVersion].[VersionDate] DESC");

            var dto = Database.Query<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(contentSql).FirstOrDefault();

            if (dto == null)
                return null;

            var contentType = _contentTypeRepository.Get(dto.ContentVersionDto.ContentDto.ContentTypeId);

            var factory = new ContentFactory(contentType, NodeObjectTypeId, id);
            var content = factory.BuildEntity(dto);
            
            content.Properties = GetPropertyCollection(id, dto.ContentVersionDto.VersionId, contentType);

            ((ICanBeDirty)content).ResetDirtyProperties();
            return content;
        }

        protected override IEnumerable<IContent> PerformGetAll(params int[] ids)
        {
            if (ids.Any())
            {
                foreach (var id in ids)
                {
                    yield return Get(id);
                }
            }
            else
            {
                var nodeDtos = Database.Fetch<NodeDto>("WHERE nodeObjectType = @NodeObjectType", new { NodeObjectType = NodeObjectTypeId });
                foreach (var nodeDto in nodeDtos)
                {
                    yield return Get(nodeDto.NodeId);
                }
            }
        }

        protected override IEnumerable<IContent> PerformGetByQuery(IQuery<IContent> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IContent>(sqlClause, query);
            var sql = translator.Translate();

            //NOTE: This doesn't allow properties to be part of the query
            var dtos = Database.Fetch<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(sql);

            foreach (var dto in dtos)
            {
                yield return Get(dto.NodeId);
            }
        }
        
        #endregion

        #region Overrides of PetaPocoRepositoryBase<IContent>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*");
            sql.From("cmsDocument");
            sql.InnerJoin("cmsContentVersion ON ([cmsDocument].[versionId] = [cmsContentVersion].[VersionId])");
            sql.InnerJoin("cmsContent ON ([cmsContentVersion].[ContentId] = [cmsContent].[nodeId])");
            sql.InnerJoin("umbracoNode ON ([cmsContent].[nodeId] = [umbracoNode].[id])");
            sql.Where("[umbracoNode].[nodeObjectType] = @NodeObjectType", new { NodeObjectType = NodeObjectTypeId });
            return sql;
        }

        protected override Sql GetBaseWhereClause(object id)
        {
            var sql = new Sql();
            sql.Where("[umbracoNode].[id] = @Id", new { Id = id });
            return sql;
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                           {
                               string.Format("DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @Id"),
                               string.Format("DELETE FROM umbracoUser2NodePermission WHERE nodeId = @Id"),
                               string.Format("DELETE FROM cmsTagRelationship WHERE nodeId = @Id"),
                               string.Format("DELETE FROM cmsDocument WHERE NodeId = @Id"),
                               string.Format("DELETE FROM cmsPropertyData WHERE contentNodeId = @Id"),
                               string.Format("DELETE FROM cmsPreviewXml WHERE nodeId = @Id"),
                               string.Format("DELETE FROM cmsContentVersion WHERE ContentId = @Id"),
                               string.Format("DELETE FROM cmsContentXml WHERE nodeID = @Id"),
                               string.Format("DELETE FROM cmsContent WHERE NodeId = @Id"),
                               string.Format("DELETE FROM umbracoNode WHERE id = @Id")
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972"); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IContent entity)
        {
            ((Content)entity).AddingEntity();

            var factory = new ContentFactory(null, NodeObjectTypeId, entity.Id);
            var dto = factory.BuildDto(entity);

            //NOTE Should the logic below have some kind of fallback for empty parent ids ?
            //Logic for setting Path, Level and SortOrder
            var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = entity.ParentId });
            int level = parent.Level + 1;
            int sortOrder =
                Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE parentID = @ParentId AND nodeObjectType = @NodeObjectType",
                                                      new { ParentId = entity.ParentId, NodeObjectType = NodeObjectTypeId });

            //Create the (base) node data - umbracoNode
            var nodeDto = dto.ContentVersionDto.ContentDto.NodeDto;
            nodeDto.Path = parent.Path;
            nodeDto.Level = short.Parse(level.ToString(CultureInfo.InvariantCulture));
            nodeDto.SortOrder = sortOrder;
            var o = Database.IsNew(nodeDto) ? Convert.ToInt32(Database.Insert(nodeDto)) : Database.Update(nodeDto);

            //Update with new correct path
            nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
            Database.Update(nodeDto);

            //Update entity with correct values
            entity.Id = nodeDto.NodeId; //Set Id on entity to ensure an Id is set
            entity.Path = nodeDto.Path;
            entity.SortOrder = sortOrder;
            entity.Level = level;

            //Create the Content specific data - cmsContent
            var contentDto = dto.ContentVersionDto.ContentDto;
            contentDto.NodeId = nodeDto.NodeId;
            Database.Insert(contentDto);

            //Create the first version - cmsContentVersion
            //Assumes a new Version guid and Version date (modified date) has been set
            var contentVersionDto = dto.ContentVersionDto;
            contentVersionDto.NodeId = nodeDto.NodeId;
            Database.Insert(contentVersionDto);

            //Create the Document specific data for this version - cmsDocument
            //Assumes a new Version guid has been generated
            dto.NodeId = nodeDto.NodeId;
            Database.Insert(dto);

            //Create the PropertyData for this version - cmsPropertyData
            var propertyFactory = new PropertyFactory(((Content)entity).ContentType, entity.Version, entity.Id);
            var propertyDataDtos = propertyFactory.BuildDto(entity.Properties);
            //Add Properties
            foreach (var propertyDataDto in propertyDataDtos)
            {
                Database.Insert(propertyDataDto);
            }

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IContent entity)
        {
            //Updates Modified date and Version Guid
            ((Content)entity).UpdatingEntity();

            var factory = new ContentFactory(null, NodeObjectTypeId, entity.Id);
            //Look up Content entry to get Primary for updating the DTO
            var contentDto = Database.SingleOrDefault<ContentDto>("WHERE nodeId = @Id", new { Id = entity.Id });
            factory.SetPrimaryKey(contentDto.PrimaryKey);
            var dto = factory.BuildDto(entity);

            //Updates the (base) node data - umbracoNode
            var nodeDto = dto.ContentVersionDto.ContentDto.NodeDto;
            var o = Database.Update(nodeDto);
            
            //Only update this DTO if the contentType has actually changed
            if (contentDto.ContentTypeId != entity.ContentTypeId)
            {
                //Create the Content specific data - cmsContent
                var newContentDto = dto.ContentVersionDto.ContentDto;
                Database.Update(newContentDto);
            }

            //Look up entries in cmsDocument table to set newest = false
            var documentDtos = Database.Fetch<DocumentDto>("WHERE nodeId = @Id AND newest = '1'", new { Id = entity.Id });
            foreach (var documentDto in documentDtos)
            {
                var docDto = documentDto;
                docDto.Newest = false;
                Database.Update(docDto);
            }

            //Create a new version - cmsContentVersion
            //Assumes a new Version guid and Version date (modified date) has been set
            var contentVersionDto = dto.ContentVersionDto;
            Database.Insert(contentVersionDto);

            //Create the Document specific data for this version - cmsDocument
            //Assumes a new Version guid has been generated
            Database.Insert(dto);

            //Create the PropertyData for this version - cmsPropertyData
            var propertyFactory = new PropertyFactory(((Content)entity).ContentType, entity.Version, entity.Id);
            var propertyDataDtos = propertyFactory.BuildDto(entity.Properties);
            //Add Properties
            foreach (var propertyDataDto in propertyDataDtos)
            {
                Database.Insert(propertyDataDto);
            }

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }
        
        #endregion

        #region Implementation of IContentRepository

        public IEnumerable<IContent> GetAllVersions(int id)
        {
            var contentSql = GetBaseQuery(false);
            contentSql.Append(GetBaseWhereClause(id));
            contentSql.OrderBy("[cmsContentVersion].[VersionDate] DESC");

            var documentDtos = Database.Fetch<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(contentSql);
            foreach (var dto in documentDtos)
            {
                yield return GetByVersion(id, dto.ContentVersionDto.VersionId);
            }
        }

        public IContent GetByVersion(int id, Guid versionId)
        {
            var contentSql = GetBaseQuery(false);
            contentSql.Append(GetBaseWhereClause(id));
            contentSql.Where("[cmsContentVersion].[VersionId] = @VersionId", new { VersionId = versionId });
            contentSql.OrderBy("[cmsContentVersion].[VersionDate] DESC");

            var dto = Database.Query<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(contentSql).FirstOrDefault();

            if (dto == null)
                return null;

            var contentType = _contentTypeRepository.Get(dto.ContentVersionDto.ContentDto.ContentTypeId);

            var factory = new ContentFactory(contentType, NodeObjectTypeId, id);
            var content = factory.BuildEntity(dto);

            content.Properties = GetPropertyCollection(id, versionId, contentType);

            ((Content)content).ResetDirtyProperties();
            return content;
        }

        #endregion

        private PropertyCollection GetPropertyCollection(int id, Guid versionId, IContentType contentType)
        {
            var propertySql = new Sql();
            propertySql.Select("*");
            propertySql.From("cmsPropertyData");
            propertySql.InnerJoin("cmsPropertyType ON ([cmsPropertyData].[propertytypeid] = [cmsPropertyType].[id])");
            propertySql.Where("[cmsPropertyData].[contentNodeId] = @Id", new { Id = id });
            propertySql.Where("[cmsPropertyData].[versionId] = @VersionId", new { VersionId = versionId });

            var propertyDataDtos = Database.Fetch<PropertyDataDto, PropertyTypeDto>(propertySql);
            var propertyFactory = new PropertyFactory(contentType, versionId, id);
            var properties = propertyFactory.BuildEntity(propertyDataDtos);
            return new PropertyCollection(properties);
        }
    }
}