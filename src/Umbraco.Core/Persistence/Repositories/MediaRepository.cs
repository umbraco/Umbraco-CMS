using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Cache;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IMedia"/>
    /// </summary>
    internal class MediaRepository : RecycleBinRepository<int, IMedia>, IMediaRepository
    {
        private readonly IMediaTypeRepository _mediaTypeRepository;
        private readonly ITagRepository _tagRepository;
        private readonly ContentXmlRepository<IMedia> _contentXmlRepository;
        private readonly ContentPreviewRepository<IMedia> _contentPreviewRepository;
        private readonly MediaByGuidReadRepository _mediaByGuidReadRepository;

        public MediaRepository(IScopeUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax, IMediaTypeRepository mediaTypeRepository, ITagRepository tagRepository, IContentSection contentSection)
            : base(work, cache, logger, sqlSyntax, contentSection)
        {
            if (mediaTypeRepository == null) throw new ArgumentNullException("mediaTypeRepository");
            if (tagRepository == null) throw new ArgumentNullException("tagRepository");
            _mediaTypeRepository = mediaTypeRepository;
            _tagRepository = tagRepository;
            _contentXmlRepository = new ContentXmlRepository<IMedia>(work, CacheHelper.NoCache, logger, sqlSyntax);
            _contentPreviewRepository = new ContentPreviewRepository<IMedia>(work, CacheHelper.NoCache, logger, sqlSyntax);
            _mediaByGuidReadRepository = new MediaByGuidReadRepository(this, work, cache, logger, sqlSyntax);
            EnsureUniqueNaming = contentSection.EnsureUniqueNaming;
        }

        public bool EnsureUniqueNaming { get; private set; }

        #region Overrides of RepositoryBase<int,IMedia>

        protected override IMedia PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate, SqlSyntax);

            var dto = Database.Fetch<ContentVersionDto, ContentDto, NodeDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();

            if (dto == null)
                return null;

            var content = CreateMediaFromDto(dto, sql);

            return content;
        }

        protected override IEnumerable<IMedia> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);
            if (ids.Any())
            {
                sql.Where("umbracoNode.id in (@ids)", new { ids = ids });
            }

            return ProcessQuery(sql, new PagingSqlQuery(sql));
        }

        protected override IEnumerable<IMedia> PerformGetByQuery(IQuery<IMedia> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IMedia>(sqlClause, query);
            var sql = translator.Translate()
                                .OrderBy<NodeDto>(x => x.SortOrder, SqlSyntax);

            return ProcessQuery(sql, new PagingSqlQuery(sql));
        }

        #endregion

        #region Overrides of PetaPocoRepositoryBase<int,IMedia>

        private Sql GetBaseQuery(BaseQueryType queryType, bool includeFilePaths)
        {
            var sql = new Sql();
            sql.Select(queryType == BaseQueryType.Count ? "COUNT(*)" : (queryType == BaseQueryType.Ids ? "cmsContentVersion.contentId" : "*"))
                .From<ContentVersionDto>(SqlSyntax)
                .InnerJoin<ContentDto>(SqlSyntax)
                .On<ContentVersionDto, ContentDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>(SqlSyntax)
                .On<ContentDto, NodeDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentTypeDto>(SqlSyntax)
                .On<ContentTypeDto, ContentDto>(SqlSyntax, left => left.NodeId, right => right.ContentTypeId);

            if (includeFilePaths)
            {
                sql.InnerJoin<MediaDto>(SqlSyntax)
                    .On<MediaDto, ContentVersionDto>(SqlSyntax, left => left.VersionId, right => right.VersionId);
            }
            
            //TODO: IF we want to enable querying on content type information this will need to be joined
            //.InnerJoin<ContentTypeDto>(SqlSyntax)
            //.On<ContentDto, ContentTypeDto>(SqlSyntax, left => left.ContentTypeId, right => right.NodeId, SqlSyntax);
            sql.Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId, SqlSyntax);
            return sql;
        }

        protected override Sql GetBaseQuery(BaseQueryType queryType)
        {
            return GetBaseQuery(queryType, false);
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            return GetBaseQuery(isCount ? BaseQueryType.Count : BaseQueryType.FullSingle);
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
                               "DELETE FROM umbracoUserGroup2NodePermission WHERE nodeId = @Id",
                               "DELETE FROM umbracoUserStartNode WHERE startNode = @Id",
                               "UPDATE umbracoUserGroup SET startMediaId = NULL WHERE startMediaId = @Id",
                               "DELETE FROM umbracoRelation WHERE parentId = @Id",
                               "DELETE FROM umbracoRelation WHERE childId = @Id",
                               "DELETE FROM cmsTagRelationship WHERE nodeId = @Id",
                               "DELETE FROM cmsDocument WHERE nodeId = @Id",
                               "DELETE FROM cmsPropertyData WHERE contentNodeId = @Id",
                               "DELETE FROM cmsPreviewXml WHERE nodeId = @Id",
                               "DELETE FROM cmsMedia WHERE nodeId = @Id",
                               "DELETE FROM cmsContentVersion WHERE ContentId = @Id",
                               "DELETE FROM cmsContentXml WHERE nodeId = @Id",
                               "DELETE FROM cmsContent WHERE nodeId = @Id",
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

        public override IEnumerable<IMedia> GetAllVersions(int id)
        {
            var sql = GetBaseQuery(false)
                .Where(GetBaseWhereClause(), new { Id = id })
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate, SqlSyntax);
            return ProcessQuery(sql, new PagingSqlQuery(sql), true);
        }

        /// <summary>
        /// This is the underlying method that processes most queries for this repository
        /// </summary>
        /// <param name="sqlFull">
        /// The full SQL to select all media data
        /// </param>
        /// <param name="pagingSqlQuery">
        /// The Id SQL to just return all media ids - used to process the properties for the media item
        /// </param>
        /// <param name="withCache"></param>
        /// <returns></returns>
        private IEnumerable<IMedia> ProcessQuery(Sql sqlFull, PagingSqlQuery pagingSqlQuery, bool withCache = false)
        {
            // fetch returns a list so it's ok to iterate it in this method
            var dtos = Database.Fetch<ContentVersionDto, ContentDto, NodeDto>(sqlFull);

            //This is a tuple list identifying if the content item came from the cache or not
            var content = new List<Tuple<IMedia, bool>>();
            var defs = new DocumentDefinitionCollection();

            //track the looked up content types, even though the content types are cached
            // they still need to be deep cloned out of the cache and we don't want to add
            // the overhead of deep cloning them on every item in this loop
            var contentTypes = new Dictionary<int, IMediaType>();

            foreach (var dto in dtos)
            {
                // if the cache contains the item, use it
                if (withCache)
                {
                    var cached = IsolatedCache.GetCacheItem<IMedia>(GetCacheIdKey<IMedia>(dto.NodeId));
                    //only use this cached version if the dto returned is the same version - this is just a safety check, media doesn't
                    //store different versions, but just in case someone corrupts some data we'll double check to be sure.
                    if (cached != null && cached.Version == dto.VersionId)
                    {
                        content.Add(new Tuple<IMedia, bool>(cached, true));
                        continue;
                    }
                }

                // else, need to fetch from the database
                // content type repository is full-cache so OK to get each one independently

                IMediaType contentType;
                if (contentTypes.ContainsKey(dto.ContentDto.ContentTypeId))
                {
                    contentType = contentTypes[dto.ContentDto.ContentTypeId];
                }
                else
                {
                    contentType = _mediaTypeRepository.Get(dto.ContentDto.ContentTypeId);
                    contentTypes[dto.ContentDto.ContentTypeId] = contentType;
                }

                // track the definition and if it's successfully added or updated then processed
                if (defs.AddOrUpdate(new DocumentDefinition(dto, contentType)))
                {
                    content.Add(new Tuple<IMedia, bool>(MediaFactory.BuildEntity(dto, contentType), false));
                }
            }

            // load all properties for all documents from database in 1 query
            var propertyData = GetPropertyCollection(pagingSqlQuery, defs);

            // assign property data
            foreach (var contentItem in content)
            {
                var cc = contentItem.Item1;
                var fromCache = contentItem.Item2;

                //if this has come from cache, we do not need to build up it's structure
                if (fromCache) continue;

                cc.Properties = propertyData[cc.Version];

                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                cc.ResetDirtyProperties(false);
            }

            return content.Select(x => x.Item1).ToArray();
        }

        public override IMedia GetByVersion(Guid versionId)
        {
            var sql = GetBaseQuery(false);
            sql.Where("cmsContentVersion.VersionId = @VersionId", new { VersionId = versionId });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate, SqlSyntax);

            var dto = Database.Fetch<ContentVersionDto, ContentDto, NodeDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            var content = CreateMediaFromDto(dto, sql);

            return content;
        }

        public void RebuildXmlStructures(Func<IMedia, XElement> serializer, int groupSize = 200, IEnumerable<int> contentTypeIds = null)
        {
            // the previous way of doing this was to run it all in one big transaction,
            // and to bulk-insert groups of xml rows - which works, until the transaction
            // times out - and besides, because v7 transactions are ReadCommited, it does
            // not bring much safety - so this reverts to updating each record individually,
            // and it may be slower in the end, but should be more resilient.

            var baseId = 0;
            var contentTypeIdsA = contentTypeIds == null ? new int[0] : contentTypeIds.ToArray();
            while (true)
            {
                // get the next group of nodes
                var query = GetBaseQuery(false);
                if (contentTypeIdsA.Length > 0)
                {
                    query = query.WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIdsA, SqlSyntax);
                }
                query = query
                    .Where<NodeDto>(x => x.NodeId > baseId, SqlSyntax)
                    .Where<NodeDto>(x => x.Trashed == false, SqlSyntax)
                    .OrderBy<NodeDto>(x => x.NodeId, SqlSyntax);
                var sql = SqlSyntax.SelectTop(query, groupSize);
                var xmlItems = ProcessQuery(sql, new PagingSqlQuery(sql))
                    .Select(x => new ContentXmlDto { NodeId = x.Id, Xml = serializer(x).ToString() })
                    .ToList();

                // no more nodes, break
                if (xmlItems.Count == 0) break;

                foreach (var xmlItem in xmlItems)
                {
                    try
                    {
                        // InsertOrUpdate tries to update first, which is good since it is what
                        // should happen in most cases, then it tries to insert, and it should work
                        // unless the node has been deleted, and we just report the exception
                        Database.InsertOrUpdate(xmlItem);
                    }
                    catch (Exception e)
                    {
                        Logger.Error<MediaRepository>("Could not rebuild XML for nodeId=" + xmlItem.NodeId, e);
                    }
                }
                baseId = xmlItems[xmlItems.Count - 1].NodeId;
            }

            //now delete the items that shouldn't be there
            var allMediaIds = Database.Fetch<int>(GetBaseQuery(BaseQueryType.Ids).Where<NodeDto>(x => x.Trashed == false, SqlSyntax));
            var mediaObjectType = Guid.Parse(Constants.ObjectTypes.Media);
            var xmlIdsQuery = new Sql()
                .Select("DISTINCT cmsContentXml.nodeId")
                .From<ContentXmlDto>(SqlSyntax)
                .InnerJoin<NodeDto>(SqlSyntax)
                .On<ContentXmlDto, NodeDto>(SqlSyntax, left => left.NodeId, right => right.NodeId);

            if (contentTypeIdsA.Length > 0)
            {
                xmlIdsQuery.InnerJoin<ContentDto>(SqlSyntax)
                    .On<ContentDto, NodeDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
                    .InnerJoin<ContentTypeDto>(SqlSyntax)
                    .On<ContentTypeDto, ContentDto>(SqlSyntax, left => left.NodeId, right => right.ContentTypeId)
                    .WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIdsA, SqlSyntax);
            }

            xmlIdsQuery.Where<NodeDto>(dto => dto.NodeObjectType == mediaObjectType, SqlSyntax);

            var allXmlIds = Database.Fetch<int>(xmlIdsQuery);

            var toRemove = allXmlIds.Except(allMediaIds).ToArray();
            if (toRemove.Length > 0)
            {
                foreach (var idGroup in toRemove.InGroupsOf(2000))
                {
                    Database.Execute("DELETE FROM cmsContentXml WHERE nodeId IN (@ids)", new { ids = idGroup });
                }
            }
        }

        public void AddOrUpdateContentXml(IMedia content, Func<IMedia, XElement> xml)
        {
            _contentXmlRepository.AddOrUpdate(new ContentXmlEntity<IMedia>(content, xml));
        }

        public void DeleteContentXml(IMedia content)
        {
            _contentXmlRepository.Delete(new ContentXmlEntity<IMedia>(content));
        }

        public void AddOrUpdatePreviewXml(IMedia content, Func<IMedia, XElement> xml)
        {
            _contentPreviewRepository.AddOrUpdate(new ContentPreviewEntity<IMedia>(content, xml));
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

            //Ensure that strings don't contain characters that are invalid in XML
            entity.SanitizeEntityPropertiesForXmlStorage();

            var factory = new MediaFactory(NodeObjectTypeId);
            var dto = factory.BuildDto(entity);

            //NOTE Should the logic below have some kind of fallback for empty parent ids ?
            //Logic for setting Path, Level and SortOrder
            var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { ParentId = entity.ParentId });
            var level = parent.Level + 1;
            var maxSortOrder = Database.ExecuteScalar<int>(
                "SELECT coalesce(max(sortOrder),-1) FROM umbracoNode WHERE parentid = @ParentId AND nodeObjectType = @NodeObjectType",
                new { /*ParentId =*/ entity.ParentId, NodeObjectType = NodeObjectTypeId });
            var sortOrder = maxSortOrder + 1;

            //Create the (base) node data - umbracoNode
            var nodeDto = dto.ContentVersionDto.ContentDto.NodeDto;
            nodeDto.Path = parent.Path;
            nodeDto.Level = short.Parse(level.ToString(CultureInfo.InvariantCulture));
            nodeDto.SortOrder = sortOrder;

            // note:
            // there used to be a check on Database.IsNew(nodeDto) here to either Insert or Update,
            // but I cannot figure out what was the point, as the node should obviously be new if
            // we reach that point - removed.

            // see if there's a reserved identifier for this unique id
            var sql = new Sql("SELECT id FROM umbracoNode WHERE uniqueID=@0 AND nodeObjectType=@1", nodeDto.UniqueId, Constants.ObjectTypes.IdReservationGuid);
            var id = Database.ExecuteScalar<int>(sql);
            if (id > 0)
            {
                nodeDto.NodeId = id;
                Database.Update(nodeDto);
            }
            else
            {
                Database.Insert(nodeDto);
            }

            //Update with new correct path
            nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
            nodeDto.ValidatePathWithException();
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

            //Create the Media specific data for this version - cmsMedia
            //Assumes a new Version guid has been generated
            dto.NodeId = nodeDto.NodeId;
            Database.Insert(dto);
            
            //Create the PropertyData for this version - cmsPropertyData
            var propertyFactory = new PropertyFactory(entity.ContentType.CompositionPropertyTypes.ToArray(), entity.Version, entity.Id);
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

            UpdatePropertyTags(entity, _tagRepository);

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IMedia entity)
        {
            //Updates Modified date
            ((Models.Media)entity).UpdatingEntity();

            //Ensure unique name on the same level
            entity.Name = EnsureUniqueNodeName(entity.ParentId, entity.Name, entity.Id);

            //Ensure that strings don't contain characters that are invalid in XML
            entity.SanitizeEntityPropertiesForXmlStorage();

            //Look up parent to get and set the correct Path and update SortOrder if ParentId has changed
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

            var factory = new MediaFactory(NodeObjectTypeId);
            //Look up Content entry to get Primary for updating the DTO
            var contentDto = Database.First<ContentDto>("WHERE nodeId = @Id", new { Id = entity.Id });
            factory.SetPrimaryKey(contentDto.PrimaryKey);
            var dto = factory.BuildDto(entity);

            //Updates the (base) node data - umbracoNode
            var nodeDto = dto.ContentVersionDto.ContentDto.NodeDto;
            nodeDto.ValidatePathWithException();
            var o = Database.Update(nodeDto);

            //Only update this DTO if the contentType has actually changed
            if (contentDto.ContentTypeId != entity.ContentTypeId)
            {
                //Create the Content specific data - cmsContent
                var newContentDto = dto.ContentVersionDto.ContentDto;
                Database.Update(newContentDto);
            }

            //In order to update the ContentVersion we need to retrieve its primary key id
            var contentVerDto = Database.First<ContentVersionDto>("WHERE VersionId = @Version", new { Version = entity.Version });
            dto.ContentVersionDto.Id = contentVerDto.Id;
            //Updates the current version - cmsContentVersion
            //Assumes a Version guid exists and Version date (modified date) has been set/updated
            Database.Update(dto.ContentVersionDto);

            //now update the media entry
            Database.Update(dto);

            //Create the PropertyData for this version - cmsPropertyData
            var propertyFactory = new PropertyFactory(entity.ContentType.CompositionPropertyTypes.ToArray(), entity.Version, entity.Id);
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
                    if (keyDictionary.ContainsKey(property.PropertyTypeId) == false) continue;

                    property.Id = keyDictionary[property.PropertyTypeId];
                }
            }

            UpdatePropertyTags(entity, _tagRepository);

            entity.ResetDirtyProperties();
        }

        #endregion

        #region IRecycleBinRepository members

        protected override int RecycleBinId
        {
            get { return Constants.System.RecycleBinMedia; }
        }

        #endregion

        #region Read Repository implementation for GUID keys
        public IMedia Get(Guid id)
        {
            return _mediaByGuidReadRepository.Get(id);
        }

        IEnumerable<IMedia> IReadRepository<Guid, IMedia>.GetAll(params Guid[] ids)
        {
            return _mediaByGuidReadRepository.GetAll(ids);
        }

        public bool Exists(Guid id)
        {
            return _mediaByGuidReadRepository.Exists(id);
        }

        /// <summary>
        /// A reading repository purely for looking up by GUID
        /// </summary>
        /// <remarks>
        /// TODO: This is ugly and to fix we need to decouple the IRepositoryQueryable -> IRepository -> IReadRepository which should all be separate things!
        /// Then we can do the same thing with repository instances and we wouldn't need to leave all these methods as not implemented because we wouldn't need to implement them
        /// </remarks>
        private class MediaByGuidReadRepository : PetaPocoRepositoryBase<Guid, IMedia>
        {
            private readonly MediaRepository _outerRepo;

            public MediaByGuidReadRepository(MediaRepository outerRepo,
                IScopeUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax)
                : base(work, cache, logger, sqlSyntax)
            {
                _outerRepo = outerRepo;
            }

            protected override IMedia PerformGet(Guid id)
            {
                var sql = GetBaseQuery(false);
                sql.Where(GetBaseWhereClause(), new { Id = id });
                sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate, SqlSyntax);

                var dto = Database.Fetch<ContentVersionDto, ContentDto, NodeDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();

                if (dto == null)
                    return null;

                var content = _outerRepo.CreateMediaFromDto(dto, sql);

                return content;
            }

            protected override IEnumerable<IMedia> PerformGetAll(params Guid[] ids)
            {
                var sql = GetBaseQuery(false);
                if (ids.Any())
                {
                    sql.Where("umbracoNode.uniqueID in (@ids)", new { ids = ids });
                }

                return _outerRepo.ProcessQuery(sql, new PagingSqlQuery(sql));
            }

            protected override Sql GetBaseQuery(bool isCount)
            {
                return _outerRepo.GetBaseQuery(isCount);
            }

            protected override string GetBaseWhereClause()
            {
                return "umbracoNode.uniqueID = @Id";
            }

            protected override Guid NodeObjectTypeId
            {
                get { return _outerRepo.NodeObjectTypeId; }
            }

            #region Not needed to implement

            protected override IEnumerable<IMedia> PerformGetByQuery(IQuery<IMedia> query)
            {
                throw new NotImplementedException();
            }
            protected override IEnumerable<string> GetDeleteClauses()
            {
                throw new NotImplementedException();
            }
            protected override void PersistNewItem(IMedia entity)
            {
                throw new NotImplementedException();
            }
            protected override void PersistUpdatedItem(IMedia entity)
            {
                throw new NotImplementedException();
            }
            #endregion
        }
        #endregion

        /// <summary>
        /// Gets an <see cref="IMedia"/> object from the path stored in the for the media item.
        /// </summary>
        /// <param name="mediaPath">Path of the media item to retrieve (for example: /media/1024/koala_403x328.jpg)</param>
        /// <returns><see cref="IMedia"/></returns>
        public IMedia GetMediaByPath(string mediaPath)
        {
            var umbracoFileValue = mediaPath;

            const string pattern = ".*[_][0-9]+[x][0-9]+[.].*";
            var isResized = Regex.IsMatch(mediaPath, pattern);

            // If the image has been resized we strip the "_403x328" of the original "/media/1024/koala_403x328.jpg" url.
            if (isResized)
            {
                var underscoreIndex = mediaPath.LastIndexOf('_');
                var dotIndex = mediaPath.LastIndexOf('.');
                umbracoFileValue = string.Concat(mediaPath.Substring(0, underscoreIndex), mediaPath.Substring(dotIndex));
            }

            var sql = GetBaseQuery(BaseQueryType.FullSingle, true);
            sql.Where<MediaDto>(mediaDto => mediaDto.MediaPath == umbracoFileValue, SqlSyntax);
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate, SqlSyntax);
            var dto = Database.Fetch<ContentVersionDto, ContentDto, NodeDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();
            if (dto == null)
                return null;

            var content = CreateMediaFromDto(dto, sql);

            return content;
        }

        /// <summary>
        /// Gets paged media results
        /// </summary>
        /// <param name="query">Query to excute</param>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="orderBySystemField">Flag to indicate when ordering by system field</param>
        /// <param name="filter">Search text filter</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetPagedResultsByQuery(IQuery<IMedia> query, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IMedia> filter = null)
        {
            var filterSql = new Sql();
            if (filter != null)
            {
                foreach (var filterClause in filter.GetWhereClauses())
                {
                    filterSql.Append(string.Format("AND ({0})", filterClause.Item1), filterClause.Item2);
                }
            }

            Func<Tuple<string, object[]>> filterCallback = () => new Tuple<string, object[]>(filterSql.SQL, filterSql.Arguments);

            return GetPagedResultsByQuery<ContentVersionDto>(query, pageIndex, pageSize, out totalRecords,
                new Tuple<string, string>("cmsContentVersion", "contentId"),
                (sqlFull, pagingSqlQuery) => ProcessQuery(sqlFull, pagingSqlQuery), orderBy, orderDirection, orderBySystemField,
                filterCallback);

        }

        /// <summary>
        /// Private method to create a media object from a ContentDto
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="docSql"></param>
        /// <returns></returns>
        private IMedia CreateMediaFromDto(ContentVersionDto dto, Sql docSql)
        {
            var contentType = _mediaTypeRepository.Get(dto.ContentDto.ContentTypeId);

            var media = MediaFactory.BuildEntity(dto, contentType);

            var docDef = new DocumentDefinition(dto, contentType);

            var properties = GetPropertyCollection(new PagingSqlQuery(docSql), new[] { docDef });

            media.Properties = properties[dto.VersionId];

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)media).ResetDirtyProperties(false);
            return media;
        }

        private string EnsureUniqueNodeName(int parentId, string nodeName, int id = 0)
        {
            if (EnsureUniqueNaming == false)
                return nodeName;

            var names = Database.Fetch<SimilarNodeName>("SELECT id, text AS name FROM umbracoNode WHERE nodeObjectType=@objectType AND parentId=@parentId",
                new { objectType = NodeObjectTypeId, parentId });

            return SimilarNodeName.GetUniqueName(names, id, nodeName);
        }

        /// <summary>
        /// Dispose disposable properties
        /// </summary>
        /// <remarks>
        /// Ensure the unit of work is disposed
        /// </remarks>
        protected override void DisposeResources()
        {
            _mediaTypeRepository.Dispose();
            _tagRepository.Dispose();
            _contentXmlRepository.Dispose();
            _contentPreviewRepository.Dispose();
        }
    }
}
