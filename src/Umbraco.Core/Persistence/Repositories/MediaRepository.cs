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
    /// Represents a repository for doing CRUD operations for <see cref="IMedia"/>
    /// </summary>
    internal class MediaRepository : VersionableRepositoryBase<int, IMedia>, IMediaRepository
    {
        private readonly IMediaTypeRepository _mediaTypeRepository;

		public MediaRepository(IDatabaseUnitOfWork work, IMediaTypeRepository mediaTypeRepository)
            : base(work)
        {
            _mediaTypeRepository = mediaTypeRepository;
        }

		public MediaRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache, IMediaTypeRepository mediaTypeRepository)
            : base(work, cache)
        {
            _mediaTypeRepository = mediaTypeRepository;
        }

        #region Overrides of RepositoryBase<int,IMedia>

        protected override IMedia PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto = Database.Query<ContentVersionDto, ContentDto, NodeDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            var mediaType = _mediaTypeRepository.Get(dto.ContentDto.ContentTypeId);

            var factory = new MediaFactory(mediaType, NodeObjectTypeId, id);
            var media = factory.BuildEntity(dto);

            media.Properties = GetPropertyCollection(id, dto.VersionId, mediaType);

            ((ICanBeDirty)media).ResetDirtyProperties();
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
            sql.Select(isCount ? "COUNT(*)" : "*");
            sql.From("cmsContentVersion");
            sql.InnerJoin("cmsContent ON ([cmsContentVersion].[ContentId] = [cmsContent].[nodeId])");
            sql.InnerJoin("umbracoNode ON ([cmsContent].[nodeId] = [umbracoNode].[id])");
            sql.Where("[umbracoNode].[nodeObjectType] = @NodeObjectType", new { NodeObjectType = NodeObjectTypeId });
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
                               string.Format("DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @Id"),
                               string.Format("DELETE FROM umbracoUser2NodePermission WHERE nodeId = @Id"),
                               string.Format("DELETE FROM cmsTagRelationship WHERE nodeId = @Id"),
                               string.Format("DELETE FROM cmsDocument WHERE NodeId = @Id"),
                               string.Format("DELETE FROM cmsPropertyData WHERE contentNodeId = @Id"),
                               string.Format("DELETE FROM cmsContentVersion WHERE ContentId = @Id"),
                               string.Format("DELETE FROM cmsContent WHERE NodeId = @Id"),
                               string.Format("DELETE FROM umbracoNode WHERE id = @Id")
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid("B796F64C-1F99-4FFB-B886-4BF4BC011A9C"); }
        }

        #endregion

        #region Overrides of VersionableRepositoryBase<IContent>

        public override IMedia GetByVersion(Guid versionId)
        {
            var sql = GetBaseQuery(false);
            sql.Where("cmsContentVersion.VersionId = @VersionId", new { VersionId = versionId });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto = Database.Query<ContentVersionDto, ContentDto, NodeDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            var mediaType = _mediaTypeRepository.Get(dto.ContentDto.ContentTypeId);

            var factory = new MediaFactory(mediaType, NodeObjectTypeId, dto.NodeId);
            var media = factory.BuildEntity(dto);

            media.Properties = GetPropertyCollection(dto.NodeId, dto.VersionId, mediaType);

            ((ICanBeDirty)media).ResetDirtyProperties();
            return media;
        }

        protected override void PerformDeleteVersion(int id, Guid versionId)
        {
            Database.Delete<PreviewXmlDto>("WHERE nodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<PropertyDataDto>("WHERE nodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<ContentVersionDto>("WHERE nodeId = @Id AND VersionId = @VersionId", new { Id = id, VersionId = versionId });
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IMedia entity)
        {
            ((Models.Media)entity).AddingEntity();

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

            //Look up parent to get and set the correct Path if ParentId has changed
            if (((ICanBeDirty)entity).IsPropertyDirty("ParentId"))
            {
                var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = entity.ParentId });
                entity.Path = string.Concat(parent.Path, ",", entity.Id);
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

        #endregion

        private PropertyCollection GetPropertyCollection(int id, Guid versionId, IMediaType contentType)
        {
            var propertySql = new Sql();
            propertySql.Select("*");
            propertySql.From("cmsPropertyData");
            propertySql.InnerJoin("cmsPropertyType ON ([cmsPropertyData].[propertytypeid] = [cmsPropertyType].[id])");
            propertySql.Where("[cmsPropertyData].[contentNodeId] = @Id", new { Id = id });
            propertySql.Where("[cmsPropertyData].[versionId] = @VersionId", new { VersionId = versionId });

            var propertyDataDtos = Database.Fetch<PropertyDataDto, PropertyTypeDto>(propertySql);
            var propertyFactory = new PropertyFactory(contentType, versionId, id);
            var properties = propertyFactory.BuildMediaEntity(propertyDataDtos);
            return new PropertyCollection(properties);
        }
    }
}