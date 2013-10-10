using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Relators;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IMember"/>
    /// </summary>
    internal class MemberRepository : VersionableRepositoryBase<int, IMember>, IMemberRepository
    {
        private readonly IMemberTypeRepository _memberTypeRepository;
        private readonly ITagsRepository _tagRepository;

        public MemberRepository(IDatabaseUnitOfWork work, IMemberTypeRepository memberTypeRepository, ITagsRepository tagRepository)
            : base(work)
        {
            if (memberTypeRepository == null) throw new ArgumentNullException("memberTypeRepository");
            if (tagRepository == null) throw new ArgumentNullException("tagRepository");
            _memberTypeRepository = memberTypeRepository;
            _tagRepository = tagRepository;
        }

        public MemberRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache, IMemberTypeRepository memberTypeRepository, ITagsRepository tagRepository)
            : base(work, cache)
        {
            if (memberTypeRepository == null) throw new ArgumentNullException("memberTypeRepository");
            if (tagRepository == null) throw new ArgumentNullException("tagRepository");
            _memberTypeRepository = memberTypeRepository;
            _tagRepository = tagRepository;
        }

        #region Overrides of RepositoryBase<int, IMembershipUser>

        protected override IMember PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dtos =
                Database.Fetch<MemberReadOnlyDto, PropertyDataReadOnlyDto, MemberReadOnlyDto>(
                    new PropertyDataRelator().Map, sql);

            return BuildFromDto(dtos);
        }

        protected override IEnumerable<IMember> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);
            if (ids.Any())
            {
                var statement = string.Join(" OR ", ids.Select(x => string.Format("umbracoNode.id='{0}'", x)));
                sql.Where(statement);
            }
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dtos =
                Database.Fetch<MemberReadOnlyDto, PropertyDataReadOnlyDto, MemberReadOnlyDto>(
                    new PropertyDataRelator().Map, sql);

            return BuildFromDtos(dtos);
        }

        protected override IEnumerable<IMember> PerformGetByQuery(IQuery<IMember> query)
        {
            var sqlSubquery = GetSubquery();
            var translator = new SqlTranslator<IMember>(sqlSubquery, query);
            var subquery = translator.Translate();
            var sql = GetBaseQuery(false)
                .Append(new Sql("WHERE umbracoNode.id IN (" + subquery.SQL + ")", subquery.Arguments))
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate)
                .OrderBy<NodeDto>(x => x.SortOrder);

            var dtos =
                Database.Fetch<MemberReadOnlyDto, PropertyDataReadOnlyDto, MemberReadOnlyDto>(
                    new PropertyDataRelator().Map, sql);

            return BuildFromDtos(dtos);
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,IMembershipUser>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();

            if (isCount)
            {
                sql.Select("COUNT(*)")
                    .From<NodeDto>()
                    .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                    .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
                return sql;
            }

            sql.Select("umbracoNode.*", "cmsContent.contentType", "cmsContentType.alias AS ContentTypeAlias", "cmsContentVersion.VersionId",
                "cmsContentVersion.VersionDate", "cmsContentVersion.LanguageLocale", "cmsMember.Email",
                "cmsMember.LoginName", "cmsMember.Password", "cmsPropertyData.id AS PropertyDataId", "cmsPropertyData.propertytypeid", 
                "cmsPropertyData.dataDate", "cmsPropertyData.dataInt", "cmsPropertyData.dataNtext", "cmsPropertyData.dataNvarchar",
                "cmsPropertyType.id", "cmsPropertyType.Alias", "cmsPropertyType.Description",
                "cmsPropertyType.Name", "cmsPropertyType.mandatory", "cmsPropertyType.validationRegExp",
                "cmsPropertyType.helpText", "cmsPropertyType.sortOrder AS PropertyTypeSortOrder", "cmsPropertyType.propertyTypeGroupId",
                "cmsPropertyType.dataTypeId", "cmsDataType.propertyEditorAlias", "cmsDataType.dbType")
                .From<NodeDto>()
                .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .LeftJoin<PropertyTypeDto>().On<PropertyTypeDto, ContentDto>(left => left.ContentTypeId, right => right.ContentTypeId)
                .LeftJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
                .LeftJoin<PropertyDataDto>().On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .Append("AND cmsPropertyData.versionId = cmsContentVersion.VersionId")
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sql;
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoNode.id = @Id";
        }

        protected Sql GetSubquery()
        {
            var sql = new Sql();
            sql.Select("DISTINCT(umbracoNode.id)")
                .From<NodeDto>()
                .InnerJoin<ContentDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentTypeDto>().On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                .InnerJoin<ContentVersionDto>().On<ContentVersionDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<MemberDto>().On<MemberDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .LeftJoin<PropertyTypeDto>().On<PropertyTypeDto, ContentDto>(left => left.ContentTypeId, right => right.ContentTypeId)
                .LeftJoin<DataTypeDto>().On<DataTypeDto, PropertyTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
                .LeftJoin<PropertyDataDto>().On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .Append("AND cmsPropertyData.versionId = cmsContentVersion.VersionId")
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);
            return sql;
        }

        protected Sql GetMemberGroupSubquery()
        {
            var sql = new Sql();
            sql.Select("DISTINCT(cmsMember2MemberGroup.Member)")
                .From<Member2MemberGroupDto>()
                .InnerJoin<NodeDto>().On<NodeDto, Member2MemberGroupDto>(left => left.NodeId, right => right.MemberGroup)
                .Where<NodeDto>(x => x.NodeObjectType == new Guid(Constants.ObjectTypes.MemberGroup));
            return sql;
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
                               "DELETE FROM cmsPropertyData WHERE contentNodeId = @Id",
                               "DELETE FROM cmsMember WHERE nodeId = @Id",
                               "DELETE FROM cmsContentVersion WHERE ContentId = @Id",
                               "DELETE FROM cmsContentXml WHERE nodeID = @Id",
                               "DELETE FROM cmsContent WHERE NodeId = @Id",
                               "DELETE FROM umbracoNode WHERE id = @Id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid(Constants.ObjectTypes.Member); }
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IMember entity)
        {
            ((Member)entity).AddingEntity();

            var factory = new MemberFactory(NodeObjectTypeId, entity.Id);
            var dto = factory.BuildDto(entity);

            //NOTE Should the logic below have some kind of fallback for empty parent ids ?
            //Logic for setting Path, Level and SortOrder
            var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = ((IUmbracoEntity)entity).ParentId });
            int level = parent.Level + 1;
            int sortOrder =
                Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE parentID = @ParentId AND nodeObjectType = @NodeObjectType",
                                                      new { ParentId = ((IUmbracoEntity)entity).ParentId, NodeObjectType = NodeObjectTypeId });

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
            ((IUmbracoEntity)entity).Path = nodeDto.Path;
            ((IUmbracoEntity)entity).SortOrder = sortOrder;
            ((IUmbracoEntity)entity).Level = level;

            //Create the Content specific data - cmsContent
            var contentDto = dto.ContentVersionDto.ContentDto;
            contentDto.NodeId = nodeDto.NodeId;
            Database.Insert(contentDto);

            //Create the first version - cmsContentVersion
            //Assumes a new Version guid and Version date (modified date) has been set
            dto.ContentVersionDto.NodeId = nodeDto.NodeId;
            Database.Insert(dto.ContentVersionDto);

            //Create the first entry in cmsMember
            dto.NodeId = nodeDto.NodeId;
            Database.Insert(dto);

            //Create the PropertyData for this version - cmsPropertyData
            var propertyFactory = new PropertyFactory(entity.ContentType, entity.Version, entity.Id);
            var propertyDataDtos = propertyFactory.BuildDto(((Member)entity).Properties);
            var keyDictionary = new Dictionary<int, int>();

            //Add Properties
            foreach (var propertyDataDto in propertyDataDtos)
            {
                var primaryKey = Convert.ToInt32(Database.Insert(propertyDataDto));
                keyDictionary.Add(propertyDataDto.PropertyTypeId, primaryKey);
            }

            //Update Properties with its newly set Id
            foreach (var property in ((Member)entity).Properties)
            {
                property.Id = keyDictionary[property.PropertyTypeId];
            }

            UpdatePropertyTags(entity, _tagRepository);

            ((Member)entity).ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IMember entity)
        {
            //Updates Modified date
            ((Member)entity).UpdatingEntity();

            var dirtyEntity = (ICanBeDirty) entity;

            //Look up parent to get and set the correct Path and update SortOrder if ParentId has changed
            if (dirtyEntity.IsPropertyDirty("ParentId"))
            {
                var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = ((IUmbracoEntity)entity).ParentId });
                ((IUmbracoEntity)entity).Path = string.Concat(parent.Path, ",", entity.Id);
                ((IUmbracoEntity)entity).Level = parent.Level + 1;
                var maxSortOrder =
                    Database.ExecuteScalar<int>(
                        "SELECT coalesce(max(sortOrder),0) FROM umbracoNode WHERE parentid = @ParentId AND nodeObjectType = @NodeObjectType",
                        new { ParentId = ((IUmbracoEntity)entity).ParentId, NodeObjectType = NodeObjectTypeId });
                ((IUmbracoEntity)entity).SortOrder = maxSortOrder + 1;
            }

            var factory = new MemberFactory(NodeObjectTypeId, entity.Id);
            //Look up Content entry to get Primary for updating the DTO
            var contentDto = Database.SingleOrDefault<ContentDto>("WHERE nodeId = @Id", new { Id = entity.Id });
            factory.SetPrimaryKey(contentDto.PrimaryKey);
            var dto = factory.BuildDto(entity);

            //Updates the (base) node data - umbracoNode
            var nodeDto = dto.ContentVersionDto.ContentDto.NodeDto;
            var o = Database.Update(nodeDto);

            //Only update this DTO if the contentType has actually changed
            if (contentDto.ContentTypeId != ((Member)entity).ContentTypeId)
            {
                //Create the Content specific data - cmsContent
                var newContentDto = dto.ContentVersionDto.ContentDto;
                Database.Update(newContentDto);
            }

            //In order to update the ContentVersion we need to retreive its primary key id
            var contentVerDto = Database.SingleOrDefault<ContentVersionDto>("WHERE VersionId = @Version", new { Version = entity.Version });
            dto.ContentVersionDto.Id = contentVerDto.Id;
            //Updates the current version - cmsContentVersion
            //Assumes a Version guid exists and Version date (modified date) has been set/updated
            Database.Update(dto.ContentVersionDto);
            
            //Updates the cmsMember entry if it has changed
            var changedCols = new List<string>();
            if (dirtyEntity.IsPropertyDirty("Email"))
            {
                changedCols.Add("Email");
            }
            if (dirtyEntity.IsPropertyDirty("Username"))
            {
                changedCols.Add("LoginName");
            }
            // DO NOT update the password if it is null or empty
            if (dirtyEntity.IsPropertyDirty("Password") && entity.Password.IsNullOrWhiteSpace() == false)
            {
                changedCols.Add("Password");
            }
            //only update the changed cols
            if (changedCols.Count > 0)
            {
                Database.Update(dto, changedCols);    
            }

            //TODO ContentType for the Member entity

            //Create the PropertyData for this version - cmsPropertyData
            var propertyFactory = new PropertyFactory(entity.ContentType, entity.Version, entity.Id);            
            var keyDictionary = new Dictionary<int, int>();

            //Add Properties
            // - don't try to save the property if it doesn't exist (or doesn't have an ID) on the content type
            // - this can occur if the member type doesn't contain the built-in properties that the
            // - member object contains.
            var existingProperties = entity.Properties
                .Where(property => entity.ContentType.PropertyTypes.Any(x => x.Alias == property.Alias && x.HasIdentity))
                .ToList();

            var propertyDataDtos = propertyFactory.BuildDto(existingProperties);

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
                foreach (var property in ((Member)entity).Properties)
                {
                    property.Id = keyDictionary[property.PropertyTypeId];
                }
            }

            UpdatePropertyTags(entity, _tagRepository);

            dirtyEntity.ResetDirtyProperties();
        }

        protected override void PersistDeletedItem(IMember entity)
        {
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            var uploadFieldAlias = Constants.PropertyEditors.UploadFieldAlias;
            //Loop through properties to check if the media item contains images/file that should be deleted
            foreach (var property in ((Member)entity).Properties)
            {
                if (property.PropertyType.PropertyEditorAlias == uploadFieldAlias &&
                    string.IsNullOrEmpty(property.Value.ToString()) == false
                    && fs.FileExists(IOHelper.MapPath(property.Value.ToString())))
                {
                    var relativeFilePath = fs.GetRelativePath(property.Value.ToString());
                    var parentDirectory = System.IO.Path.GetDirectoryName(relativeFilePath);

                    // don't want to delete the media folder if not using directories.
                    if (UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories && parentDirectory != fs.GetRelativePath("/"))
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

        #region Overrides of VersionableRepositoryBase<IMembershipUser>

        public override IMember GetByVersion(Guid versionId)
        {
            var sql = GetBaseQuery(false);
            sql.Where<ContentVersionDto>(x => x.VersionId == versionId);
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dtos =
                Database.Fetch<MemberReadOnlyDto, PropertyDataReadOnlyDto, MemberReadOnlyDto>(
                    new PropertyDataRelator().Map, sql);

            return BuildFromDto(dtos);
        }

        protected override void PerformDeleteVersion(int id, Guid versionId)
        {
            Database.Delete<PreviewXmlDto>("WHERE nodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<PropertyDataDto>("WHERE contentNodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<ContentVersionDto>("WHERE ContentId = @Id AND VersionId = @VersionId", new { Id = id, VersionId = versionId });
        }

        #endregion

        /// <summary>
        /// Get all members in a specific group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public IEnumerable<IMember> GetByMemberGroup(string groupName)
        {
            var subquery = GetSubquery().Where<NodeDto>(x => x.Text == groupName);
            var sql = GetBaseQuery(false)
                .Append(new Sql("WHERE umbracoNode.id IN (" + subquery.SQL + ")", subquery.Arguments))
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate)
                .OrderBy<NodeDto>(x => x.SortOrder);

            var dtos =
                Database.Fetch<MemberReadOnlyDto, PropertyDataReadOnlyDto, MemberReadOnlyDto>(
                    new PropertyDataRelator().Map, sql);

            return BuildFromDtos(dtos);
        }

        private IMember BuildFromDto(List<MemberReadOnlyDto> dtos)
        {
            if (dtos == null || dtos.Any() == false)
                return null;
            var dto = dtos.First();

            var memberTypes = new Dictionary<string, IMemberType>
                              {
                                  {
                                      dto.ContentTypeAlias,
                                      _memberTypeRepository.Get(dto.ContentTypeId)
                                  }
                              };

            var factory = new MemberReadOnlyFactory(memberTypes);
            var member = factory.BuildEntity(dto);

            return member;
        }

        private IEnumerable<IMember> BuildFromDtos(List<MemberReadOnlyDto> dtos)
        {
            if (dtos == null || dtos.Any() == false)
                return Enumerable.Empty<IMember>();

            //We assume that there won't exist a lot of MemberTypes, so the following should be fairly fast
            var memberTypes = new Dictionary<string, IMemberType>();
            var memberTypeList = _memberTypeRepository.GetAll();
            memberTypeList.ForEach(x => memberTypes.Add(x.Alias, x));

            var factory = new MemberReadOnlyFactory(memberTypes);
            return dtos.Select(factory.BuildEntity);
        }
    }
}