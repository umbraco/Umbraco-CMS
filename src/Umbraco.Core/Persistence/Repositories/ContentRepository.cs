using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="Content"/>
    /// </summary>
    internal class ContentRepository : Repository<IContent>, IContentRepository
    {
        private const string NodeObjectType = "C66BA18E-EAF3-4CFF-8A22-41B16D66A972";
        private readonly IContentTypeRepository _contentTypeRepository;

        public ContentRepository(IUnitOfWork work, IContentTypeRepository contentTypeRepository)
            : base(work)
        {
            _contentTypeRepository = contentTypeRepository;
        }

        internal ContentRepository(IUnitOfWork work, IContentTypeRepository contentTypeRepository, IRepositoryCacheProvider registry)
            : base(work, registry)
        {
            _contentTypeRepository = contentTypeRepository;
        }

        protected override void PerformAdd(IContent entity)
        {
            ((Content)entity).AddingEntity();

            //NOTE Should the logic below have some kind of fallback for empty parent ids ?
            //Logic for setting Path, Level and SortOrder
            var parent = UnitOfWork.Storage.First<NodeDto>("WHERE id = @ParentId", new { ParentId = entity.ParentId });
            int level = parent.Level + 1;
            int sortOrder =
                UnitOfWork.Storage.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE parentID = @ParentId AND nodeObjectType = @NodeObjectType",
                                                      new { ParentId = entity.ParentId, NodeObjectType = NodeObjectType });

            //Create the (base) node data - umbracoNode
            var nodeDto = ContentFactory.CreateNodeDto(entity, NodeObjectType, parent.Path, level, sortOrder);
            var o = UnitOfWork.Storage.IsNew(nodeDto) ? Convert.ToInt32(UnitOfWork.Storage.Insert(nodeDto)) : UnitOfWork.Storage.Update(nodeDto);

            //Update with new correct path
            nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
            UnitOfWork.Storage.Update(nodeDto);

            //Update entity with correct values
            entity.Id = nodeDto.NodeId; //Set Id on entity to ensure an Id is set
            entity.Path = nodeDto.Path;
            entity.SortOrder = sortOrder;
            entity.Level = level;

            //Create the Content specific data - cmsContent
            var contentDto = ContentFactory.CreateContentDto(entity);
            UnitOfWork.Storage.Insert(contentDto);

            //Create the first version - cmsContentVersion
            //Assumes a new Version guid and Version date (modified date) has been set
            var contentVersionDto = ContentFactory.CreateContentVersionDto(entity);
            UnitOfWork.Storage.Insert(contentVersionDto);

            //Create the Document specific data for this version - cmsDocument
            //Assumes a new Version guid has been generated
            var documentDto = ContentFactory.CreateDocumentDto(entity);
            UnitOfWork.Storage.Insert(documentDto);

            //Create the PropertyData for this version - cmsPropertyData
            var propertyDataDtos = ContentFactory.CreateProperties(entity.Id, entity.Version, entity.Properties);
            //Add Properties
            foreach (var propertyDataDto in propertyDataDtos)
            {
                UnitOfWork.Storage.Insert(propertyDataDto);
            }

            ((Content)entity).ResetDirtyProperties();
        }

        protected override void PerformUpdate(IContent entity)
        {
            //Updates Modified date and Version Guid
            ((Content)entity).UpdatingEntity();

            //Updates the (base) node data - umbracoNode
            var nodeDto = ContentFactory.CreateNodeDto(entity, NodeObjectType);
            var o = UnitOfWork.Storage.Update(nodeDto);

            //Look up Content entry to get Primary for updating the DTO
            var contentDto = UnitOfWork.Storage.SingleOrDefault<ContentDto>("WHERE nodeId = @Id", new { Id = entity.Id });
            //Only update this DTO if the contentType has actually changed
            if (contentDto.ContentType != entity.ContentTypeId)
            {
                //Create the Content specific data - cmsContent
                var newContentDto = ContentFactory.CreateContentDto(entity, contentDto.PrimaryKey);
                UnitOfWork.Storage.Update(newContentDto);
            }

            //Look up entries in cmsDocument table to set newest = false
            var documentDtos = UnitOfWork.Storage.Fetch<DocumentDto>("WHERE nodeId = @Id AND newest = '1'", new { Id = entity.Id });
            foreach (var docDto in documentDtos)
            {
                var dto = docDto;
                dto.Newest = false;
                UnitOfWork.Storage.Update(dto);
            }

            //Create a new version - cmsContentVersion
            //Assumes a new Version guid and Version date (modified date) has been set
            var contentVersionDto = ContentFactory.CreateContentVersionDto(entity);
            UnitOfWork.Storage.Insert(contentVersionDto);

            //Create the Document specific data for this version - cmsDocument
            //Assumes a new Version guid has been generated
            var documentDto = ContentFactory.CreateDocumentDto(entity);
            UnitOfWork.Storage.Insert(documentDto);

            //Create the PropertyData for this version - cmsPropertyData
            var propertyDataDtos = ContentFactory.CreateProperties(entity.Id, entity.Version, entity.Properties);
            //Add Properties
            foreach (var propertyDataDto in propertyDataDtos)
            {
                UnitOfWork.Storage.Insert(propertyDataDto);
            }

            ((Content)entity).ResetDirtyProperties();
        }

        protected override void PerformDelete(IContent entity)
        {
            //Remove Notifications
            UnitOfWork.Storage.Delete<User2NodeNotifyDto>("WHERE nodeId = @Id", new { Id = entity.Id });

            //Remove Permissions
            UnitOfWork.Storage.Delete<User2NodePermissionDto>("WHERE nodeId = @Id", new { Id = entity.Id });

            //Remove associated tags
            UnitOfWork.Storage.Delete<TagRelationshipDto>("WHERE nodeId = @Id", new { Id = entity.Id });

            //Delete entry in Document table
            UnitOfWork.Storage.Delete<DocumentDto>("WHERE NodeId = @Id", new { Id = entity.Id });

            //Delete Properties
            UnitOfWork.Storage.Delete<PropertyDataDto>("WHERE contentNodeId = @Id", new { Id = entity.Id });

            //Delete Preview Xml
            UnitOfWork.Storage.Delete<PreviewXmlDto>("WHERE nodeId = @Id", new { Id = entity.Id });

            //Delete Version history
            UnitOfWork.Storage.Delete<ContentVersionDto>("WHERE ContentId = @Id", new { Id = entity.Id });

            //Delete Content Xml
            UnitOfWork.Storage.Delete<ContentXmlDto>("WHERE nodeID = @Id", new { Id = entity.Id });

            //Delete Content specific data
            UnitOfWork.Storage.Delete<ContentDto>("WHERE NodeId = @Id", new { Id = entity.Id });

            //Delete (base) node data
            UnitOfWork.Storage.Delete<NodeDto>("WHERE uniqueID = @Id", new { Id = entity.Key });
        }

        protected override IContent PerformGet(int id)
        {
            var contentSql = BaseSqlClause(false);
            contentSql.Where("[cmsDocument].[nodeId] = @Id", new { Id = id });
            contentSql.OrderBy("[cmsContentVersion].[VersionDate] DESC");

            var documentDto = UnitOfWork.Storage.Query<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(contentSql).FirstOrDefault();

            if (documentDto == null)
                return null;

            var propertySql = new Sql();
            propertySql.Select("*");
            propertySql.From("cmsPropertyData");
            propertySql.InnerJoin("cmsPropertyType ON ([cmsPropertyData].[propertytypeid] = [cmsPropertyType].[id])");
            propertySql.Where("[cmsPropertyData].[contentNodeId] = @Id", new { Id = id });
            propertySql.Where("[cmsPropertyData].[versionId] = @VersionId", new { VersionId = documentDto.ContentVersionDto.VersionId });

            var propertyDataDtos = UnitOfWork.Storage.Fetch<PropertyDataDto, PropertyTypeDto>(propertySql);

            var contentType = _contentTypeRepository.Get(documentDto.ContentVersionDto.ContentDto.ContentType);
            var content = ContentFactory.CreateContent(id, contentType, documentDto, propertyDataDtos);
            content.ResetDirtyProperties();
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
                var nodeDtos = UnitOfWork.Storage.Fetch<NodeDto>("WHERE nodeObjectType = @NodeObjectType", new { NodeObjectType = NodeObjectType });
                foreach (var nodeDto in nodeDtos)
                {
                    yield return Get(nodeDto.NodeId);
                }
            }
        }

        protected override IEnumerable<IContent> PerformGetByQuery(IQuery<IContent> query)
        {
            var sqlClause = BaseSqlClause(false);
            var translator = new SqlTranslator<IContent>(sqlClause, query);
            var sql = translator.Translate();

            var documentDtos = UnitOfWork.Storage.Fetch<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(sql);

            foreach (var documentDto in documentDtos)
            {
                yield return Get(documentDto.NodeId);
            }
        }

        protected override bool PerformExists(int id)
        {
            return UnitOfWork.Storage.Exists<NodeDto>(id);
        }

        protected override int PerformCount(IQuery<IContent> query)
        {
            var sqlClause = BaseSqlClause(true);
            var translator = new SqlTranslator<IContent>(sqlClause, query);
            var sql = translator.Translate();

            return UnitOfWork.Storage.ExecuteScalar<int>(sql);
        }

        public IEnumerable<IContent> GetAllVersions(int id)
        {
            var contentSql = BaseSqlClause(false);
            contentSql.Where("[cmsDocument].[nodeId] = @Id", new { Id = id });
            contentSql.OrderBy("[cmsContentVersion].[VersionDate] DESC");

            var documentDtos = UnitOfWork.Storage.Fetch<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(contentSql);
            foreach (var dto in documentDtos)
            {
                yield return GetByVersion(id, dto.ContentVersionDto.VersionId);
            }
        }

        public IContent GetByVersion(int id, Guid versionId)
        {
            var contentSql = BaseSqlClause(false);
            contentSql.Where("[cmsDocument].[nodeId] = @Id", new { Id = id });
            contentSql.Where("[cmsContentVersion].[VersionId] = @VersionId", new { VersionId = versionId });
            contentSql.OrderBy("[cmsContentVersion].[VersionDate] DESC");

            var documentDto = UnitOfWork.Storage.Query<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(contentSql).FirstOrDefault();

            if (documentDto == null)
                return null;

            var propertySql = new Sql();
            propertySql.Select("*");
            propertySql.From("cmsPropertyData");
            propertySql.InnerJoin("cmsPropertyType ON [cmsPropertyData].[propertytypeid] = [cmsPropertyType].[id]");
            propertySql.Where("[cmsPropertyData].[contentNodeId] = @Id", new { Id = id });
            propertySql.Where("[cmsPropertyData].[versionId] = @VersionId", new { VersionId = versionId });

            var propertyDataDtos = UnitOfWork.Storage.Query<PropertyDataDto, PropertyTypeDto>(propertySql);

            var contentType = _contentTypeRepository.Get(documentDto.ContentVersionDto.ContentDto.ContentType);
            var content = ContentFactory.CreateContent(id, contentType, documentDto, propertyDataDtos);
            content.ResetDirtyProperties();
            return content;
        }

        private Sql BaseSqlClause(bool doCount)
        {
            var sql = new Sql();

            sql.Select(doCount ? "COUNT(*)" : "*");
            sql.From("cmsDocument");
            sql.InnerJoin("cmsContentVersion ON ([cmsDocument].[versionId]=[cmsContentVersion].[VersionId])");
            sql.InnerJoin("cmsContent ON ([cmsContentVersion].[ContentId]=[cmsContent].[nodeId])");
            sql.InnerJoin("umbracoNode ON ([cmsContent].[nodeId]=[umbracoNode].[id])");
            sql.Where("[umbracoNode].[nodeObjectType]=@NodeObjectType", new { NodeObjectType = NodeObjectType });

            return sql;
        }
    }
}