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
    /// Represents a repository for doing CRUD operations for <see cref="IMedia"/>
    /// </summary>
    internal class MediaRepository : VersionableRepositoryBase<int, IMedia>, IMediaRepository
    {
        private readonly IMediaTypeRepository _mediaTypeRepository;

		public MediaRepository(IDatabaseUnitOfWork work, IMediaTypeRepository mediaTypeRepository)
            : base(work)
        {
            _mediaTypeRepository = mediaTypeRepository;

            EnsureUniqueNaming = true;
        }

		public MediaRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache, IMediaTypeRepository mediaTypeRepository)
            : base(work, cache)
        {
            _mediaTypeRepository = mediaTypeRepository;

            EnsureUniqueNaming = true;
        }

        public bool EnsureUniqueNaming { get; set; }

        #region Overrides of RepositoryBase<int,IMedia>

        protected override IMedia PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto = Database.Fetch<ContentVersionDto, ContentDto, NodeDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            var mediaType = _mediaTypeRepository.Get(dto.ContentDto.ContentTypeId);

            var factory = new MediaFactory(mediaType, NodeObjectTypeId, id);
            var media = factory.BuildEntity(dto);

            media.Properties = GetPropertyCollection(id, dto.VersionId, mediaType, media.CreateDate, media.UpdateDate);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)media).ResetDirtyProperties(false);
            return media;
        }

        protected override IEnumerable<IMedia> PerformGetAll(params int[] ids)
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

        protected override IEnumerable<IMedia> PerformGetByQuery(IQuery<IMedia> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IMedia>(sqlClause, query);
            var sql = translator.Translate();

            var dtos = Database.Fetch<ContentVersionDto, ContentDto, NodeDto>(sql);

            foreach (var dto in dtos)
            {
                yield return Get(dto.ContentDto.NodeDto.NodeId);
            }
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,IMedia>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
                .From<ContentVersionDto>()
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
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
                               "DELETE FROM umbracoRelation WHERE parentId = @Id",
                               "DELETE FROM umbracoRelation WHERE childId = @Id",
                               "DELETE FROM cmsTagRelationship WHERE nodeId = @Id",
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
            get { return new Guid(Constants.ObjectTypes.Media); }
        }

        #endregion

        #region Overrides of VersionableRepositoryBase<IContent>

        public override IMedia GetByVersion(Guid versionId)
        {
            var sql = GetBaseQuery(false);
            sql.Where("cmsContentVersion.VersionId = @VersionId", new { VersionId = versionId });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto = Database.Fetch<ContentVersionDto, ContentDto, NodeDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            var mediaType = _mediaTypeRepository.Get(dto.ContentDto.ContentTypeId);

            var factory = new MediaFactory(mediaType, NodeObjectTypeId, dto.NodeId);
            var media = factory.BuildEntity(dto);

            media.Properties = GetPropertyCollection(dto.NodeId, dto.VersionId, mediaType, media.CreateDate, media.UpdateDate);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)media).ResetDirtyProperties(false);
            return media;
        }

        protected override void PerformDeleteVersion(int id, Guid versionId)
        {
            Database.Delete<PreviewXmlDto>("WHERE nodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<PropertyDataDto>("WHERE contentNodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<ContentVersionDto>("WHERE ContentId = @Id AND VersionId = @VersionId", new { Id = id, VersionId = versionId });
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IMedia entity)
        {
            ((Models.Media)entity).AddingEntity();

            //Ensure unique name on the same level
            entity.Name = EnsureUniqueNodeName(entity.ParentId, entity.Name);

            var factory = new MediaFactory(NodeObjectTypeId, entity.Id);
            var dto = factory.BuildDto(entity);

            //NOTE Should the logic below have some kind of fallback for empty parent ids ?
            //Logic for setting Path, Level and SortOrder
            var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = entity.ParentId });
            int level = parent.Level + 1;
            int sortOrder =
                Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE parentID = @ParentId AND nodeObjectType = @NodeObjectType",
                                                      new { ParentId = entity.ParentId, NodeObjectType = NodeObjectTypeId });

            //Create the (base) node data - umbracoNode
            var nodeDto = dto.ContentDto.NodeDto;
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
            var contentDto = dto.ContentDto;
            contentDto.NodeId = nodeDto.NodeId;
            Database.Insert(contentDto);

            //Create the first version - cmsContentVersion
            //Assumes a new Version guid and Version date (modified date) has been set
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

        protected override void PersistUpdatedItem(IMedia entity)
        {
            //Updates Modified date
            ((Models.Media)entity).UpdatingEntity();

            //Ensure unique name on the same level
            entity.Name = EnsureUniqueNodeName(entity.ParentId, entity.Name, entity.Id);

            //Look up parent to get and set the correct Path and update SortOrder if ParentId has changed
            if (((ICanBeDirty)entity).IsPropertyDirty("ParentId"))
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

            var factory = new MediaFactory(NodeObjectTypeId, entity.Id);
            //Look up Content entry to get Primary for updating the DTO
            var contentDto = Database.SingleOrDefault<ContentDto>("WHERE nodeId = @Id", new { Id = entity.Id });
            factory.SetPrimaryKey(contentDto.PrimaryKey);
            var dto = factory.BuildDto(entity);

            //Updates the (base) node data - umbracoNode
            var nodeDto = dto.ContentDto.NodeDto;
            var o = Database.Update(nodeDto);

            //Only update this DTO if the contentType has actually changed
            if (contentDto.ContentTypeId != entity.ContentTypeId)
            {
                //Create the Content specific data - cmsContent
                var newContentDto = dto.ContentDto;
                Database.Update(newContentDto);
            }

            //Updates the current version - cmsContentVersion
            //Assumes a Version guid exists and Version date (modified date) has been set/updated
            Database.Update(dto);

            //Create the PropertyData for this version - cmsPropertyData
            var propertyFactory = new PropertyFactory(entity.ContentType, entity.Version, entity.Id);
            var propertyDataDtos = propertyFactory.BuildDto(entity.Properties);
            var keyDictionary = new Dictionary<int, int>();

            //Add Properties
            foreach (var propertyDataDto in propertyDataDtos)
            {
                if (propertyDataDto.Id > 0)
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

        protected override void PersistDeletedItem(IMedia entity)
        {
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            var uploadFieldId = new Guid(Constants.PropertyEditors.UploadField);
            //Loop through properties to check if the media item contains images/file that should be deleted
            foreach (var property in entity.Properties)
            {
                if (property.PropertyType.DataTypeId == uploadFieldId &&
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

        private PropertyCollection GetPropertyCollection(int id, Guid versionId, IMediaType contentType, DateTime createDate, DateTime updateDate)
        {
            var sql = new Sql();
            sql.Select("*")
                .From<PropertyDataDto>()
                .InnerJoin<PropertyTypeDto>()
                .On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .Where<PropertyDataDto>(x => x.NodeId == id)
                .Where<PropertyDataDto>(x => x.VersionId == versionId);

            var propertyDataDtos = Database.Fetch<PropertyDataDto, PropertyTypeDto>(sql);
            var propertyFactory = new PropertyFactory(contentType, versionId, id, createDate, updateDate);
            var properties = propertyFactory.BuildEntity(propertyDataDtos);

            var newProperties = properties.Where(x => x.HasIdentity == false);
            foreach (var property in newProperties)
            {
                var propertyDataDto = new PropertyDataDto { NodeId = id, PropertyTypeId = property.PropertyTypeId, VersionId = versionId };
                int primaryKey = Convert.ToInt32(Database.Insert(propertyDataDto));

                property.Version = versionId;
                property.Id = primaryKey;
            }

            return new PropertyCollection(properties);
        }

        private string EnsureUniqueNodeName(int parentId, string nodeName, int id = 0)
        {
            if (EnsureUniqueNaming == false)
                return nodeName;

            var sql = new Sql();
            sql.Select("*")
               .From<NodeDto>()
               .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId && x.ParentId == parentId && x.Text.StartsWith(nodeName));

            int uniqueNumber = 1;
            var currentName = nodeName;

            var dtos = Database.Fetch<NodeDto>(sql);
            if (dtos.Any())
            {
                var results = dtos.OrderBy(x => x.Text, new SimilarNodeNameComparer());
                foreach (var dto in results)
                {
                    if (id != 0 && id == dto.NodeId) continue;

                    if (dto.Text.ToLowerInvariant().Equals(currentName.ToLowerInvariant()))
                    {
                        currentName = nodeName + string.Format(" ({0})", uniqueNumber);
                        uniqueNumber++;
                    }
                }
            }

            return currentName;
        }
    }
}