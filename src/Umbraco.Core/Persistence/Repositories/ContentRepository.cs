using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dynamics;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
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
        private readonly ITagRepository _tagRepository;
        private readonly CacheHelper _cacheHelper;
        private readonly ContentPreviewRepository<IContent> _contentPreviewRepository;
        private readonly ContentXmlRepository<IContent> _contentXmlRepository;
        
        public ContentRepository(IDatabaseUnitOfWork work, IContentTypeRepository contentTypeRepository, ITemplateRepository templateRepository, ITagRepository tagRepository, CacheHelper cacheHelper)
            : base(work)
        {
            if (contentTypeRepository == null) throw new ArgumentNullException("contentTypeRepository");
            if (templateRepository == null) throw new ArgumentNullException("templateRepository");
            if (tagRepository == null) throw new ArgumentNullException("tagRepository");
            _contentTypeRepository = contentTypeRepository;
            _templateRepository = templateRepository;
		    _tagRepository = tagRepository;
            _cacheHelper = cacheHelper;
            _contentPreviewRepository = new ContentPreviewRepository<IContent>(work, NullCacheProvider.Current);
            _contentXmlRepository = new ContentXmlRepository<IContent>(work, NullCacheProvider.Current);

		    EnsureUniqueNaming = true;
        }

        public ContentRepository(IDatabaseUnitOfWork work, IRepositoryCacheProvider cache, IContentTypeRepository contentTypeRepository, ITemplateRepository templateRepository, ITagRepository tagRepository, CacheHelper cacheHelper)
            : base(work, cache)
        {
            if (contentTypeRepository == null) throw new ArgumentNullException("contentTypeRepository");
            if (templateRepository == null) throw new ArgumentNullException("templateRepository");
            if (tagRepository == null) throw new ArgumentNullException("tagRepository");
            _contentTypeRepository = contentTypeRepository;
            _templateRepository = templateRepository;
            _tagRepository = tagRepository;
            _cacheHelper = cacheHelper;
            _contentPreviewRepository = new ContentPreviewRepository<IContent>(work, NullCacheProvider.Current);
            _contentXmlRepository = new ContentXmlRepository<IContent>(work, NullCacheProvider.Current);

            EnsureUniqueNaming = true;
        }

        public bool EnsureUniqueNaming { get; set; }

        #region Overrides of RepositoryBase<IContent>

        protected override IContent PerformGet(int id)
        {
            var sql = GetBaseQuery(false)
                .Where(GetBaseWhereClause(), new { Id = id })
                .Where<DocumentDto>(x => x.Newest)
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto = Database.Fetch<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            var content = CreateContentFromDto(dto, dto.ContentVersionDto.VersionId);

            return content;
        }

        protected override IEnumerable<IContent> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);
            if (ids.Any())
            {
                sql.Where("umbracoNode.id in (@ids)", new {ids = ids});
            }
            else
            {
                sql.Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);                
            }

            return ProcessQuery(sql);
        }

        protected override IEnumerable<IContent> PerformGetByQuery(IQuery<IContent> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IContent>(sqlClause, query);
            var sql = translator.Translate()
                                .Where<DocumentDto>(x => x.Newest)
                                .OrderByDescending<ContentVersionDto>(x => x.VersionDate)
                                .OrderBy<NodeDto>(x => x.SortOrder);

            return ProcessQuery(sql);
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<IContent>

        protected override Sql GetBaseQuery(bool isCount)
        {
            var sql = new Sql();
            sql.Select(isCount ? "COUNT(*)" : "*")
                .From<DocumentDto>()
                .InnerJoin<ContentVersionDto>()
                .On<DocumentDto, ContentVersionDto>(left => left.VersionId, right => right.VersionId)
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId );
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
                               "DELETE FROM cmsTask WHERE nodeId = @Id",
                               "DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @Id",
                               "DELETE FROM umbracoUser2NodePermission WHERE nodeId = @Id",
                               "DELETE FROM umbracoRelation WHERE parentId = @Id",
                               "DELETE FROM umbracoRelation WHERE childId = @Id",
                               "DELETE FROM cmsTagRelationship WHERE nodeId = @Id",
                               "DELETE FROM umbracoDomains WHERE domainRootStructureID = @Id",
                               "DELETE FROM cmsDocument WHERE nodeId = @Id",
                               "DELETE FROM cmsPropertyData WHERE contentNodeId = @Id",
                               "DELETE FROM cmsPreviewXml WHERE nodeId = @Id",
                               "DELETE FROM cmsContentVersion WHERE ContentId = @Id",
                               "DELETE FROM cmsContentXml WHERE nodeId = @Id",
                               "DELETE FROM cmsContent WHERE nodeId = @Id",
                               "DELETE FROM umbracoNode WHERE id = @Id"
                           };
            return list;
        }

        protected override Guid NodeObjectTypeId
        {
            get { return new Guid(Constants.ObjectTypes.Document); }
        }

        #endregion

        #region Overrides of VersionableRepositoryBase<IContent>
        
        public override IContent GetByVersion(Guid versionId)
        {
            var sql = GetBaseQuery(false);
            sql.Where("cmsContentVersion.VersionId = @VersionId", new { VersionId = versionId });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto = Database.Fetch<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;
            
            var content = CreateContentFromDto(dto, versionId);

            return content;
        }

        public override void DeleteVersion(Guid versionId)
        {
            var sql = new Sql()
                .Select("*")
                .From<DocumentDto>()
                .InnerJoin<ContentVersionDto>().On<ContentVersionDto, DocumentDto>(left => left.VersionId, right => right.VersionId)
                .Where<ContentVersionDto>(x => x.VersionId == versionId)
                .Where<DocumentDto>(x => x.Newest != true);
            var dto = Database.Fetch<DocumentDto, ContentVersionDto>(sql).FirstOrDefault();

            if(dto == null) return;

            using (var transaction = Database.GetTransaction())
            {
                PerformDeleteVersion(dto.NodeId, versionId);

                transaction.Complete();
            }
        }

        public override void DeleteVersions(int id, DateTime versionDate)
        {
            var sql = new Sql()
                .Select("*")
                .From<DocumentDto>()
                .InnerJoin<ContentVersionDto>().On<ContentVersionDto, DocumentDto>(left => left.VersionId, right => right.VersionId)
                .Where<ContentVersionDto>(x => x.NodeId == id)
                .Where<ContentVersionDto>(x => x.VersionDate < versionDate)
                .Where<DocumentDto>(x => x.Newest != true);
            var list = Database.Fetch<DocumentDto, ContentVersionDto>(sql);
            if (list.Any() == false) return;

            using (var transaction = Database.GetTransaction())
            {
                foreach (var dto in list)
                {
                    PerformDeleteVersion(id, dto.VersionId);
                }

                transaction.Complete();
            }
        }

        protected override void PerformDeleteVersion(int id, Guid versionId)
        {
            Database.Delete<PreviewXmlDto>("WHERE nodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<PropertyDataDto>("WHERE contentNodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<ContentVersionDto>("WHERE ContentId = @Id AND VersionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<DocumentDto>("WHERE nodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
        }

        #endregion

        #region Unit of Work Implementation

        protected override void PersistNewItem(IContent entity)
        {
            ((Content)entity).AddingEntity();

            //Ensure unique name on the same level
            entity.Name = EnsureUniqueNodeName(entity.ParentId, entity.Name);
            
            //Ensure that strings don't contain characters that are invalid in XML
            entity.SanitizeEntityPropertiesForXmlStorage();

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


            //Assign the same permissions to it as the parent node
            // http://issues.umbraco.org/issue/U4-2161     
            var permissionsRepo = new PermissionRepository<IContent>(UnitOfWork, _cacheHelper);
            var parentPermissions = permissionsRepo.GetPermissionsForEntity(entity.ParentId).ToArray();
            //if there are parent permissions then assign them, otherwise leave null and permissions will become the
            // user's default permissions.
            if (parentPermissions.Any())
            {
                var userPermissions = (
                    from perm in parentPermissions 
                    from p in perm.AssignedPermissions 
                    select new EntityPermissionSet.UserPermission(perm.UserId, p)).ToList();

                permissionsRepo.ReplaceEntityPermissions(new EntityPermissionSet(entity.Id, userPermissions));
                //flag the entity's permissions changed flag so we can track those changes.
                //Currently only used for the cache refreshers to detect if we should refresh all user permissions cache.
                ((Content) entity).PermissionsChanged = true;
            }

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

            //lastly, check if we are a creating a published version , then update the tags table
            if (entity.Published)
            {
                UpdatePropertyTags(entity, _tagRepository);
            }

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IContent entity)
        {
            var publishedState = ((Content) entity).PublishedState;
            
            //check if we need to create a new version
            bool shouldCreateNewVersion = entity.ShouldCreateNewVersion(publishedState);
            if (shouldCreateNewVersion)
            {
                //Updates Modified date and Version Guid
                ((Content)entity).UpdatingEntity();
            }
            else
            {
                entity.UpdateDate = DateTime.Now;
            }

            //Ensure unique name on the same level
            entity.Name = EnsureUniqueNodeName(entity.ParentId, entity.Name, entity.Id);

            //Ensure that strings don't contain characters that are invalid in XML
            entity.SanitizeEntityPropertiesForXmlStorage();

            //Look up parent to get and set the correct Path and update SortOrder if ParentId has changed
            if (((ICanBeDirty)entity).IsPropertyDirty("ParentId"))
            {
                var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = entity.ParentId });
                entity.Path = string.Concat(parent.Path, ",", entity.Id);
                entity.Level = parent.Level + 1;
                var maxSortOrder =
                    Database.ExecuteScalar<int>(
                        "SELECT coalesce(max(sortOrder),0) FROM umbracoNode WHERE parentid = @ParentId AND nodeObjectType = @NodeObjectType",
                        new {ParentId = entity.ParentId, NodeObjectType = NodeObjectTypeId});
                entity.SortOrder = maxSortOrder + 1;

                //Question: If we move a node, should we update permissions to inherit from the new parent if the parent has permissions assigned?
                // if we do that, then we'd need to propogate permissions all the way downward which might not be ideal for many people.
                // Gonna just leave it as is for now, and not re-propogate permissions.
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

            //a flag that we'll use later to create the tags in the tag db table
            var publishedStateChanged = false;

            //If Published state has changed then previous versions should have their publish state reset.
            //If state has been changed to unpublished the previous versions publish state should also be reset.
            //if (((ICanBeDirty)entity).IsPropertyDirty("Published") && (entity.Published || publishedState == PublishedState.Unpublished))
            if (entity.ShouldClearPublishedFlagForPreviousVersions(publishedState, shouldCreateNewVersion))            
            {
                var publishedDocs = Database.Fetch<DocumentDto>("WHERE nodeId = @Id AND published = @IsPublished", new { Id = entity.Id, IsPublished = true });
                foreach (var doc in publishedDocs)
                {
                    var docDto = doc;
                    docDto.Published = false;
                    Database.Update(docDto);
                }

                //this is a newly published version so we'll update the tags table too (end of this method)
                publishedStateChanged = true;
            }

            //Look up (newest) entries by id in cmsDocument table to set newest = false
            var documentDtos = Database.Fetch<DocumentDto>("WHERE nodeId = @Id AND newest = @IsNewest", new { Id = entity.Id, IsNewest = true });
            foreach (var documentDto in documentDtos)
            {
                var docDto = documentDto;
                docDto.Newest = false;
                Database.Update(docDto);
            }

            var contentVersionDto = dto.ContentVersionDto;
            if (shouldCreateNewVersion)
            {
                //Create a new version - cmsContentVersion
                //Assumes a new Version guid and Version date (modified date) has been set
                Database.Insert(contentVersionDto);
                //Create the Document specific data for this version - cmsDocument
                //Assumes a new Version guid has been generated
                Database.Insert(dto);
            }
            else
            {
                //In order to update the ContentVersion we need to retrieve its primary key id
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
                    if(keyDictionary.ContainsKey(property.PropertyTypeId) == false) continue;

                    property.Id = keyDictionary[property.PropertyTypeId];
                }
            }

            //lastly, check if we are a newly published version and then update the tags table
            if (publishedStateChanged && entity.Published)
            {
                UpdatePropertyTags(entity, _tagRepository);
            }
            else if (publishedStateChanged && (entity.Trashed || entity.Published == false))
            {
                //it's in the trash or not published remove all entity tags
                ClearEntityTags(entity, _tagRepository);
            }

            ((ICanBeDirty)entity).ResetDirtyProperties();
        }

        protected override void PersistDeletedItem(IContent entity)
        {
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

            //Loop through properties to check if the content contains images/files that should be deleted
            foreach (var property in entity.Properties)
            {
                if (property.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias && property.Value != null &&
                    string.IsNullOrEmpty(property.Value.ToString()) == false
                    && fs.FileExists(fs.GetRelativePath(property.Value.ToString())))
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

        #region Implementation of IContentRepository

        public IEnumerable<IContent> GetByPublishedVersion(IQuery<IContent> query)
        {
            // we WANT to return contents in top-down order, ie parents should come before children
            // ideal would be pure xml "document order" which can be achieved with:
            // ORDER BY substring(path, 1, len(path) - charindex(',', reverse(path))), sortOrder
            // but that's probably an overkill - sorting by level,sortOrder should be enough

            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IContent>(sqlClause, query);
            var sql = translator.Translate()
                                .Where<DocumentDto>(x => x.Published)
                                .OrderBy<NodeDto>(x => x.Level)
                                .OrderBy<NodeDto>(x => x.SortOrder);

            //NOTE: This doesn't allow properties to be part of the query
            var dtos = Database.Fetch<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(sql);

            foreach (var dto in dtos)
            {
                //Check in the cache first. If it exists there AND it is published
                // then we can use that entity. Otherwise if it is not published (which can be the case
                // because we only store the 'latest' entries in the cache which might not be the published
                // version)
                var fromCache = TryGetFromCache(dto.NodeId);
                if (fromCache.Success && fromCache.Result.Published)
                {
                    yield return fromCache.Result;
                }
                else
                {
                    yield return CreateContentFromDto(dto, dto.VersionId);    
                }
            }
        }

        public void ReplaceContentPermissions(EntityPermissionSet permissionSet)
        {
            var repo = new PermissionRepository<IContent>(UnitOfWork, _cacheHelper);
            repo.ReplaceEntityPermissions(permissionSet);
        }

        public IContent GetByLanguage(int id, string language)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });
            sql.Where<ContentVersionDto>(x => x.Language == language);
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto = Database.Fetch<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            return GetByVersion(dto.ContentVersionDto.VersionId);
        }
        
        /// <summary>
        /// Assigns a single permission to the current content item for the specified user ids
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permission"></param>
        /// <param name="userIds"></param>        
        public void AssignEntityPermission(IContent entity, char permission, IEnumerable<int> userIds)
        {
            var repo = new PermissionRepository<IContent>(UnitOfWork, _cacheHelper);
            repo.AssignEntityPermission(entity, permission, userIds);
        }

        public IEnumerable<EntityPermission> GetPermissionsForEntity(int entityId)
        {
            var repo = new PermissionRepository<IContent>(UnitOfWork, _cacheHelper);
            return repo.GetPermissionsForEntity(entityId);
        }

        /// <summary>
        /// Adds/updates content/published xml
        /// </summary>
        /// <param name="content"></param>
        /// <param name="xml"></param>
        public void AddOrUpdateContentXml(IContent content, Func<IContent, XElement> xml)
        {
            var contentExists = Database.ExecuteScalar<int>("SELECT COUNT(nodeId) FROM cmsContentXml WHERE nodeId = @Id", new { Id = content.Id }) != 0;

            _contentXmlRepository.AddOrUpdate(new ContentXmlEntity<IContent>(contentExists, content, xml));
        }

        /// <summary>
        /// Used to remove the content xml for a content item
        /// </summary>
        /// <param name="content"></param>
        public void DeleteContentXml(IContent content)
        {
            _contentXmlRepository.Delete(new ContentXmlEntity<IContent>(content));
        }

        /// <summary>
        /// Adds/updates preview xml
        /// </summary>
        /// <param name="content"></param>
        /// <param name="xml"></param>
        public void AddOrUpdatePreviewXml(IContent content, Func<IContent, XElement> xml)
        {
            var previewExists =
                    Database.ExecuteScalar<int>("SELECT COUNT(nodeId) FROM cmsPreviewXml WHERE nodeId = @Id AND versionId = @Version",
                                                    new { Id = content.Id, Version = content.Version }) != 0;

            _contentPreviewRepository.AddOrUpdate(new ContentPreviewEntity<IContent>(previewExists, content, xml));
        }

        /// <summary>
        /// Gets paged content results
        /// </summary>
        /// <param name="query">Query to excute</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="filter">Search text filter</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetPagedResultsByQuery(IQuery<IContent> query, int pageNumber, int pageSize, out int totalRecords,
            string orderBy, Direction orderDirection, string filter = "")
        {
            // Get base query
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IContent>(sqlClause, query);
            var sql = translator.Translate()
                                .Where<DocumentDto>(x => x.Newest);

            // Apply filter
            if (!string.IsNullOrEmpty(filter))
            {
                sql = sql.Where("cmsDocument.text LIKE @0", "%" + filter + "%");
            }

            // Apply order according to parameters
            if (!string.IsNullOrEmpty(orderBy))
            {
                var orderByParams = new[] { GetDatabaseFieldNameForOrderBy(orderBy) };
                if (orderDirection == Direction.Ascending)
                {
                    sql = sql.OrderBy(orderByParams);
                }
                else
                {
                    sql = sql.OrderByDescending(orderByParams);
                }
            }

            // Note we can't do multi-page for several DTOs like we can multi-fetch and are doing in PerformGetByQuery, 
            // but actually given we are doing a Get on each one (again as in PerformGetByQuery), we only need the node Id.
            // So we'll modify the SQL.
            var modifiedSQL = sql.SQL.Replace("SELECT *", "SELECT cmsDocument.nodeId");

            // Get page of results and total count
            IEnumerable<IContent> result;
            var pagedResult = Database.Page<DocumentDto>(pageNumber, pageSize, modifiedSQL, sql.Arguments);
            totalRecords = Convert.ToInt32(pagedResult.TotalItems);
            if (totalRecords > 0)
            {
                // Parse out node Ids and load content (we need the cast here in order to be able to call the IQueryable extension
                // methods OrderBy or OrderByDescending)
                var content = GetAll(pagedResult.Items
                    .DistinctBy(x => x.NodeId)
                    .Select(x => x.NodeId).ToArray())
                    .Cast<Content>()
                    .AsQueryable();

                // Now we need to ensure this result is also ordered by the same order by clause
                var orderByProperty = GetIContentPropertyNameForOrderBy(orderBy);
                if (orderDirection == Direction.Ascending)
                {
                    result = content.OrderBy(orderByProperty);
                }
                else
                {
                    result = content.OrderByDescending(orderByProperty);
                }
            }
            else
            {
                result = Enumerable.Empty<IContent>();
            }

            return result;
        }

        private string GetDatabaseFieldNameForOrderBy(string orderBy)
        {
            // Translate the passed order by field (which were originally defined for in-memory object sorting
            // of ContentItemBasic instances) to the database field names.
            switch (orderBy)
            {
                case "Name":
                    return "cmsDocument.text";
                case "Owner":
                    return "umbracoNode.nodeUser";
                case "Updator":
                    return "cmsDocument.documentUser";
                default:
                    return orderBy;
            }
        }

        private string GetIContentPropertyNameForOrderBy(string orderBy)
        {
            // Translate the passed order by field (which were originally defined for in-memory object sorting
            // of ContentItemBasic instances) to the IContent property names.
            switch (orderBy)
            {
                case "Owner":
                    return "CreatorId";
                case "Updator":
                    return "WriterId";
                default:
                    return orderBy;
            }
        }

        #endregion

        private IEnumerable<IContent> ProcessQuery(Sql sql)
        {
            //NOTE: This doesn't allow properties to be part of the query
            var dtos = Database.Fetch<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(sql);

            //content types
            var contentTypes = _contentTypeRepository.GetAll(dtos.Select(x => x.ContentVersionDto.ContentDto.ContentTypeId).ToArray())
                .ToArray();

            var templates = _templateRepository.GetAll(
                dtos
                    .Where(dto => dto.TemplateId.HasValue && dto.TemplateId.Value > 0)
                    .Select(x => x.TemplateId.Value).ToArray())
                .ToArray();

            //Go get the property data for each document
            var docDefs = dtos.Select(dto => new Tuple<int, Guid, IContentTypeComposition, DateTime, DateTime>(
                dto.NodeId,
                dto.VersionId,
                contentTypes.First(ct => ct.Id == dto.ContentVersionDto.ContentDto.ContentTypeId),
                dto.ContentVersionDto.ContentDto.NodeDto.CreateDate,
                dto.ContentVersionDto.VersionDate))
                .ToArray();

            var propertyData = GetPropertyCollection(docDefs);

            return dtos.Select(dto => CreateContentFromDto(
                dto,
                dto.ContentVersionDto.VersionId,
                contentTypes.First(ct => ct.Id == dto.ContentVersionDto.ContentDto.ContentTypeId),
                templates.FirstOrDefault(tem => tem.Id == (dto.TemplateId.HasValue ? dto.TemplateId.Value : -1)),
                propertyData[dto.NodeId]));
        }

        /// <summary>
        /// Private method to create a content object from a DocumentDto, which is used by Get and GetByVersion.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="versionId"></param>
        /// <param name="contentType"></param>
        /// <param name="template"></param>
        /// <param name="propCollection"></param>
        /// <returns></returns>
        private IContent CreateContentFromDto(DocumentDto dto, Guid versionId, 
            IContentType contentType = null,
            ITemplate template = null,
            Models.PropertyCollection propCollection = null)
        {
            contentType = contentType ?? _contentTypeRepository.Get(dto.ContentVersionDto.ContentDto.ContentTypeId);

            var factory = new ContentFactory(contentType, NodeObjectTypeId, dto.NodeId);
            var content = factory.BuildEntity(dto);

            //Check if template id is set on DocumentDto, and get ITemplate if it is.
            if (dto.TemplateId.HasValue && dto.TemplateId.Value > 0)
            {
                content.Template = template ?? _templateRepository.Get(dto.TemplateId.Value);
            }

            content.Properties = propCollection ?? 
                GetPropertyCollection(dto.NodeId, versionId, contentType, content.CreateDate, content.UpdateDate);

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)content).ResetDirtyProperties(false);
            return content;
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
                    if(id != 0 && id == dto.NodeId) continue;

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
