using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
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
    internal class ContentRepository : VersionableRepositoryBase<int, IContent>, IContentRepository
    {
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly ITemplateRepository _templateRepository;

		public ContentRepository(IDatabaseUnitOfWork work, IContentTypeRepository contentTypeRepository, ITemplateRepository templateRepository)
            : base(work)
        {
            _contentTypeRepository = contentTypeRepository;
            _templateRepository = templateRepository;
        }

		public ContentRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache, IContentTypeRepository contentTypeRepository, ITemplateRepository templateRepository)
            : base(work, cache)
        {
            _contentTypeRepository = contentTypeRepository;
            _templateRepository = templateRepository;
        }

        #region Overrides of RepositoryBase<IContent>

        protected override IContent PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto = Database.Query<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            //Get the ContentType that this Content is based on
            var contentType = _contentTypeRepository.Get(dto.ContentVersionDto.ContentDto.ContentTypeId);

            var factory = new ContentFactory(contentType, NodeObjectTypeId, id);
            var content = factory.BuildEntity(dto);

            //Check if template id is set on DocumentDto, and get ITemplate if it is.
            if (dto.TemplateId.HasValue)
            {
                content.Template = _templateRepository.Get(dto.TemplateId.Value);
            }

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
            //sql.OrderBy("[cmsContentVersion].[VersionDate] DESC");

            //NOTE: This doesn't allow properties to be part of the query
            var dtos = Database.Fetch<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(sql);

            //NOTE: Won't work with language related queries because the language version isn't passed to the Get() method.
            //A solution could be to look at the sql for the LanguageLocale column and choose the foreach-loop based on that.
            foreach (var dto in dtos.DistinctBy(x => x.NodeId))
            {
                yield return Get(dto.NodeId);
            }
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<IContent>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
                .From("cmsDocument")
                .InnerJoin("cmsContentVersion").On("[cmsDocument].[versionId] = [cmsContentVersion].[VersionId]")
                .InnerJoin("cmsContent").On("[cmsContentVersion].[ContentId] = [cmsContent].[nodeId]")
                .InnerJoin("umbracoNode").On("[cmsContent].[nodeId] = [umbracoNode].[id]")
                .Where("[umbracoNode].[nodeObjectType] = @NodeObjectType", new { NodeObjectType = NodeObjectTypeId });
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "[umbracoNode].[id] = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                           {
                               "DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @Id",
                               "DELETE FROM umbracoUser2NodePermission WHERE nodeId = @Id",
                               "DELETE FROM umbracoRelation WHERE parentId = @Id",
                               "DELETE FROM umbracoRelation WHERE childId = @Id",
                               "DELETE FROM cmsTagRelationship WHERE nodeId = @Id",
                               "DELETE FROM umbracoDomains WHERE domainRootStructureID = @Id",
                               "DELETE FROM cmsDocument WHERE NodeId = @Id",
                               "DELETE FROM cmsPropertyData WHERE contentNodeId = @Id",
                               "DELETE FROM cmsPreviewXml WHERE nodeId = @Id",
                               "DELETE FROM cmsContentVersion WHERE ContentId = @Id",
                               "DELETE FROM cmsContentXml WHERE nodeID = @Id",
                               "DELETE FROM cmsContent WHERE NodeId = @Id",
                               "DELETE FROM umbracoNode WHERE id = @Id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972"); }
        }

        #endregion

        #region Overrides of VersionableRepositoryBase<IContent>
        
        public override IContent GetByVersion(Guid versionId)
        {
            var sql = GetBaseQuery(false);
            sql.Where("cmsContentVersion.VersionId = @VersionId", new { VersionId = versionId });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto = Database.Query<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            var contentType = _contentTypeRepository.Get(dto.ContentVersionDto.ContentDto.ContentTypeId);

            var factory = new ContentFactory(contentType, NodeObjectTypeId, dto.NodeId);
            var content = factory.BuildEntity(dto);

            content.Properties = GetPropertyCollection(dto.NodeId, versionId, contentType);

            ((ICanBeDirty)content).ResetDirtyProperties();
            return content;
        }

        protected override void PerformDeleteVersion(int id, Guid versionId)
        {
            Database.Delete<PreviewXmlDto>("WHERE nodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<PropertyDataDto>("WHERE nodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<ContentVersionDto>("WHERE nodeId = @Id AND VersionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<DocumentDto>("WHERE nodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IContent entity)
        {
            ((Content)entity).AddingEntity();

            var factory = new ContentFactory(NodeObjectTypeId, entity.Id);
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
            var propertyFactory = new PropertyFactory(entity.ContentType, entity.Version, entity.Id);
            var propertyDataDtos = propertyFactory.BuildDto(entity.Properties);
            var keyDictionary = new Dictionary<int, int>();

            //Add Properties
            foreach (var propertyDataDto in propertyDataDtos)
            {
                var primaryKey = Convert.ToInt32(Database.Insert(propertyDataDto));
                keyDictionary.Add(propertyDataDto.PropertyTypeId, primaryKey);
            }

            //Update Properties with its newly set Id
            foreach (var property in entity.Properties)
            {
                property.Id = keyDictionary[property.PropertyTypeId];
            }

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IContent entity)
        {
            //A new version should only be created if published state (or language) has changed
            bool shouldCreateNewVersion = ((ICanBeDirty)entity).IsPropertyDirty("Published") || ((ICanBeDirty)entity).IsPropertyDirty("Language");
            if (shouldCreateNewVersion)
            {
                //Updates Modified date and Version Guid
                ((Content)entity).UpdatingEntity();
            }
            else
            {
                entity.UpdateDate = DateTime.UtcNow;
            }

            //Look up parent to get and set the correct Path if ParentId has changed
            if (((ICanBeDirty)entity).IsPropertyDirty("ParentId"))
            {
                var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = entity.ParentId });
                entity.Path = string.Concat(parent.Path, ",", entity.Id);
            }

            var factory = new ContentFactory(NodeObjectTypeId, entity.Id);
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

            //If Published state has changed then previous versions should have their publish state reset
            if (((ICanBeDirty)entity).IsPropertyDirty("Published") && entity.Published)
            {
                var publishedDocs = Database.Fetch<DocumentDto>("WHERE nodeId = @Id AND published = @IsPublished", new { Id = entity.Id, IsPublished = true });
                foreach (var doc in publishedDocs)
                {
                    var docDto = doc;
                    docDto.Published = false;
                    Database.Update(docDto);
                }
            }

            var contentVersionDto = dto.ContentVersionDto;
            if (shouldCreateNewVersion)
            {
                //Look up (newest) entries by id in cmsDocument table to set newest = false
                //NOTE: This is only relevant when a new version is created, which is why its done inside this if-statement.
                var documentDtos = Database.Fetch<DocumentDto>("WHERE nodeId = @Id AND newest = @IsNewest", new { Id = entity.Id, IsNewest = true });
                foreach (var documentDto in documentDtos)
                {
                    var docDto = documentDto;
                    docDto.Newest = false;
                    Database.Update(docDto);
                }

                //Create a new version - cmsContentVersion
                //Assumes a new Version guid and Version date (modified date) has been set
                Database.Insert(contentVersionDto);
                //Create the Document specific data for this version - cmsDocument
                //Assumes a new Version guid has been generated
                Database.Insert(dto);
            }
            else
            {
                //In order to update the ContentVersion we need to retreive its primary key id
                var contentVerDto = Database.SingleOrDefault<ContentVersionDto>("WHERE VersionId = @Version", new { Version = entity.Version });
                contentVersionDto.Id = contentVerDto.Id;

                Database.Update(contentVersionDto);
                Database.Update(dto);
            }

            //Create the PropertyData for this version - cmsPropertyData
            var propertyFactory = new PropertyFactory(((Content)entity).ContentType, entity.Version, entity.Id);
            var propertyDataDtos = propertyFactory.BuildDto(entity.Properties);
            var keyDictionary = new Dictionary<int, int>();

            //Add Properties
            foreach (var propertyDataDto in propertyDataDtos)
            {
                if (shouldCreateNewVersion == false && propertyDataDto.Id > 0)
                {
                    Database.Update(propertyDataDto);
                }
                else
                {
                    int primaryKey = Convert.ToInt32(Database.Insert(propertyDataDto));
                    keyDictionary.Add(propertyDataDto.PropertyTypeId, primaryKey);
                }
            }

            //Update Properties with its newly set Id
            if (keyDictionary.Any())
            {
                foreach (var property in entity.Properties)
                {
                    property.Id = keyDictionary[property.PropertyTypeId];
                }
            }

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        protected override void PersistDeletedItem(IContent entity)
        {
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            var uploadFieldId = new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c");
            //Loop through properties to check if the content contains images/files that should be deleted
            foreach (var property in entity.Properties)
            {
                if (property.PropertyType.DataTypeControlId == uploadFieldId &&
                    string.IsNullOrEmpty(property.Value.ToString()) == false
                    && fs.FileExists(IOHelper.MapPath(property.Value.ToString())))
                {
                    var relativeFilePath = fs.GetRelativePath(property.Value.ToString());
                    var parentDirectory = System.IO.Path.GetDirectoryName(relativeFilePath);

                    // don't want to delete the media folder if not using directories.
                    if (UmbracoSettings.UploadAllowDirectories && parentDirectory != fs.GetRelativePath("/"))
                    {
                        //issue U4-771: if there is a parent directory the recursive parameter should be true
                        fs.DeleteDirectory(parentDirectory, String.IsNullOrEmpty(parentDirectory) == false);
                    }
                    else
                    {
                        fs.DeleteFile(relativeFilePath, true);
                    }
                }
            }

            base.PersistDeletedItem(entity);
        }

        #endregion

        #region Implementation of IContentRepository

        public IContent GetByLanguage(int id, string language)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });
            sql.Where("[cmsContentVersion].[LanguageLocale] = @Language", new { Language = language });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto = Database.Query<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            return GetByVersion(dto.ContentVersionDto.VersionId);
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