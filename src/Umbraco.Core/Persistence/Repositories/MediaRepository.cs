using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
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
    internal class MediaRepository : PetaPocoRepositoryBase<int, IMedia>, IMediaRepository
    {
        private readonly IMediaTypeRepository _mediaTypeRepository;
        private readonly IUserRepository _userRepository;

        public MediaRepository(IUnitOfWork work, IMediaTypeRepository mediaTypeRepository, IUserRepository userRepository)
            : base(work)
        {
            _mediaTypeRepository = mediaTypeRepository;
            _userRepository = userRepository;
        }

        public MediaRepository(IUnitOfWork work, IRepositoryCacheProvider cache, IMediaTypeRepository mediaTypeRepository, IUserRepository userRepository)
            : base(work, cache)
        {
            _mediaTypeRepository = mediaTypeRepository;
            _userRepository = userRepository;
        }

        #region Overrides of RepositoryBase<int,IMedia>

        protected override IMedia PerformGet(int id)
        {
            var contentSql = GetBaseQuery(false);
            contentSql.Where(GetBaseWhereClause(), new { Id = id });
            contentSql.OrderBy("[cmsContentVersion].[VersionDate] DESC");

            var dto = Database.Query<ContentVersionDto, ContentDto, NodeDto>(contentSql).FirstOrDefault();

            if (dto == null)
                return null;

            var contentType = _mediaTypeRepository.Get(dto.ContentDto.ContentTypeId);

            var factory = new MediaFactory(contentType, NodeObjectTypeId, id);
            var content = factory.BuildEntity(dto);

            content.Properties = GetPropertyCollection(id, dto.VersionId, contentType);

            ((ICanBeDirty)content).ResetDirtyProperties();
            return content;
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
            var propertyFactory = new PropertyFactory(((Models.Media)entity).ContentType, entity.Version, entity.Id);
            var propertyDataDtos = propertyFactory.BuildDto(entity.Properties);
            //Add Properties
            foreach (var propertyDataDto in propertyDataDtos)
            {
                Database.Insert(propertyDataDto);
            }

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IMedia entity)
        {
            //Updates Modified date and Version Guid
            ((Models.Media)entity).UpdatingEntity();

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

            //Create a new version - cmsContentVersion
            //Assumes a new Version guid and Version date (modified date) has been set
            Database.Insert(dto);

            //Create the PropertyData for this version - cmsPropertyData
            var propertyFactory = new PropertyFactory(((Models.Media)entity).ContentType, entity.Version, entity.Id);
            var propertyDataDtos = propertyFactory.BuildDto(entity.Properties);
            //Add Properties
            foreach (var propertyDataDto in propertyDataDtos)
            {
                Database.Insert(propertyDataDto);
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