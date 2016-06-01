using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Mappers;
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

        public MediaRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, IMediaTypeRepository mediaTypeRepository, ITagRepository tagRepository, IContentSection contentSection, IMappingResolver mappingResolver)
            : base(work, cache, logger, contentSection, mappingResolver)
        {
            if (mediaTypeRepository == null) throw new ArgumentNullException(nameof(mediaTypeRepository));
            if (tagRepository == null) throw new ArgumentNullException(nameof(tagRepository));
            _mediaTypeRepository = mediaTypeRepository;
            _tagRepository = tagRepository;
            _contentXmlRepository = new ContentXmlRepository<IMedia>(work, CacheHelper.CreateDisabledCacheHelper(), logger, mappingResolver);
            _contentPreviewRepository = new ContentPreviewRepository<IMedia>(work, CacheHelper.CreateDisabledCacheHelper(), logger, mappingResolver);
            EnsureUniqueNaming = contentSection.EnsureUniqueNaming;
        }

        public bool EnsureUniqueNaming { get; }

        #region Overrides of RepositoryBase<int,IMedia>

        protected override IMedia PerformGet(int id)
        {
            var sql = GetBaseQuery(false);
            sql.Where(GetBaseWhereClause(), new { Id = id });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto = Database.Fetch<ContentVersionDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            var content = CreateMediaFromDto(dto, dto.VersionId);

            return content;
        }

        protected override IEnumerable<IMedia> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);
            if (ids.Any())
            {
                sql.Where("umbracoNode.id in (@ids)", new { /*ids =*/ ids });
            }

            return ProcessQuery(sql);
        }

        protected override IEnumerable<IMedia> PerformGetByQuery(IQuery<IMedia> query)
        {
            var sqlClause = GetBaseQuery(false);
            var translator = new SqlTranslator<IMedia>(sqlClause, query);
            var sql = translator.Translate()
                                .OrderBy<NodeDto>(x => x.SortOrder);

            return ProcessQuery(sql);
        }

        #endregion

        #region Overrides of NPocoRepositoryBase<int,IMedia>

        protected override Sql<SqlContext> GetBaseQuery(bool isCount)
        {
            var sql = Sql();

            sql = isCount
                ? sql.SelectCount()
                : sql.Select<ContentVersionDto>(r =>
                        r.Select<ContentDto>(rr =>
                            rr.Select<NodeDto>()));

            sql
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
                               "DELETE FROM cmsTask WHERE nodeId = @Id",
                               "DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @Id",
                               "DELETE FROM umbracoUser2NodePermission WHERE nodeId = @Id",
                               "DELETE FROM umbracoRelation WHERE parentId = @Id",
                               "DELETE FROM umbracoRelation WHERE childId = @Id",
                               "DELETE FROM cmsTagRelationship WHERE nodeId = @Id",
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

        protected override Guid NodeObjectTypeId => new Guid(Constants.ObjectTypes.Media);

        #endregion

        #region Overrides of VersionableRepositoryBase<IContent>

        public override IMedia GetByVersion(Guid versionId)
        {
            var sql = GetBaseQuery(false);
            sql.Where("cmsContentVersion.VersionId = @VersionId", new { VersionId = versionId });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            var dto = Database.Fetch<ContentVersionDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            var mediaType = _mediaTypeRepository.Get(dto.ContentDto.ContentTypeId);

            var factory = new MediaFactory(mediaType, NodeObjectTypeId, dto.NodeId);
            var media = factory.BuildEntity(dto);

            var properties = GetPropertyCollection(new[] { new DocumentDefinition(dto.NodeId, dto.VersionId, media.UpdateDate, media.CreateDate, mediaType) });

            media.Properties = properties[dto.NodeId];

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)media).ResetDirtyProperties(false);
            return media;
        }

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

            // If the stripped-down url returns null, we try again with the original url.
            // Previously, the function would fail on e.g. "my_x_image.jpg"
            var nodeId = GetMediaNodeIdByPath(umbracoFileValue);
            if (nodeId < 0) nodeId = GetMediaNodeIdByPath(mediaPath);

            return nodeId < 0 ? null : Get(nodeId);
        }

        private int GetMediaNodeIdByPath(string url)
        {
            var sql = Sql().SelectAll()
                .From<PropertyDataDto>()
                .InnerJoin<PropertyTypeDto>()
                .On<PropertyDataDto, PropertyTypeDto>(left => left.PropertyTypeId, right => right.Id)
                .Where<PropertyTypeDto>(x => x.Alias == "umbracoFile")
                .Where<PropertyDataDto>(x => x.VarChar == url);

            var dto = Database.Fetch<PropertyDataDto>(sql).FirstOrDefault();
            return dto?.NodeId ?? -1;
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

            var factory = new MediaFactory(NodeObjectTypeId, entity.Id);
            var dto = factory.BuildDto(entity);

            //NOTE Should the logic below have some kind of fallback for empty parent ids ?
            //Logic for setting Path, Level and SortOrder
            var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { /*ParentId =*/ entity.ParentId });
            var level = parent.Level + 1;
            var maxSortOrder = Database.ExecuteScalar<int>(
                "SELECT coalesce(max(sortOrder),-1) FROM umbracoNode WHERE parentId = @ParentId AND nodeObjectType = @NodeObjectType",
                new { /*ParentId =*/ entity.ParentId, NodeObjectType = NodeObjectTypeId });
            var sortOrder = maxSortOrder + 1;

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

            UpdateEntityTags(entity, _tagRepository);

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
                var parent = Database.First<NodeDto>("WHERE id = @ParentId", new { /*ParentId =*/ entity.ParentId });
                entity.Path = string.Concat(parent.Path, ",", entity.Id);
                entity.Level = parent.Level + 1;
                var maxSortOrder =
                    Database.ExecuteScalar<int>(
                        "SELECT coalesce(max(sortOrder),0) FROM umbracoNode WHERE parentid = @ParentId AND nodeObjectType = @NodeObjectType",
                        new { /*ParentId =*/ entity.ParentId, NodeObjectType = NodeObjectTypeId });
                entity.SortOrder = maxSortOrder + 1;
            }

            var factory = new MediaFactory(NodeObjectTypeId, entity.Id);
            //Look up Content entry to get Primary for updating the DTO
            var contentDto = Database.SingleOrDefault<ContentDto>("WHERE nodeId = @Id", new { /*Id =*/ entity.Id });
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

            //In order to update the ContentVersion we need to retrieve its primary key id
            var contentVerDto = Database.SingleOrDefault<ContentVersionDto>("WHERE VersionId = @Version", new { /*Version =*/ entity.Version });
            dto.Id = contentVerDto.Id;
            //Updates the current version - cmsContentVersion
            //Assumes a Version guid exists and Version date (modified date) has been set/updated
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
                    property.Id = keyDictionary[property.PropertyTypeId];
                }
            }

            UpdateEntityTags(entity, _tagRepository);

            entity.ResetDirtyProperties();
        }

        #endregion

        #region IRecycleBinRepository members

        protected override int RecycleBinId => Constants.System.RecycleBinMedia;

        #endregion

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
        /// <param name="filter"></param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetPagedResultsByQuery(IQuery<IMedia> query, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IMedia> filter = null)
        {
            Sql<SqlContext> filterSql = null;
            if (filter != null)
            {
                foreach (var filterClaus in filter.GetWhereClauses())
                {
                    filterSql = Sql().Append($"AND ({filterClaus.Item1})", filterClaus.Item2);
                }
            }

            return GetPagedResultsByQuery<ContentVersionDto>(query, pageIndex, pageSize, out totalRecords,
                MapQueryDtos, orderBy, orderDirection, orderBySystemField,
                filterSql);
        }

        private IEnumerable<IMedia> ProcessQuery(Sql sql)
        {
            //NOTE: This doesn't allow properties to be part of the query
            var dtos = Database.Fetch<ContentVersionDto>(sql);
            return MapQueryDtos(dtos);
        }

        private IEnumerable<IMedia> MapQueryDtos(List<ContentVersionDto> dtos)
        {
            var ids = dtos.Select(x => x.ContentDto.ContentTypeId).ToArray();

            //content types
            var contentTypes = ids.Length == 0 ? Enumerable.Empty<IMediaType>() : _mediaTypeRepository.GetAll(ids).ToArray();

            var dtosWithContentTypes = dtos
                //This select into and null check are required because we don't have a foreign damn key on the contentType column
                // http://issues.umbraco.org/issue/U4-5503
                .Select(x => new { dto = x, contentType = contentTypes.FirstOrDefault(ct => ct.Id == x.ContentDto.ContentTypeId) })
                .Where(x => x.contentType != null)
                .ToArray();

            //Go get the property data for each document
            var docDefs = dtosWithContentTypes.Select(d => new DocumentDefinition(
                d.dto.NodeId,
                d.dto.VersionId,
                d.dto.VersionDate,
                d.dto.ContentDto.NodeDto.CreateDate,
                d.contentType))
                .ToArray();

            var propertyData = GetPropertyCollection(docDefs);

            return dtosWithContentTypes.Select(d => CreateMediaFromDto(
                d.dto,
                contentTypes.First(ct => ct.Id == d.dto.ContentDto.ContentTypeId),
                propertyData[d.dto.NodeId]));
        }

        /// <summary>
        /// Private method to create a media object from a ContentDto
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="contentType"></param>
        /// <param name="propCollection"></param>
        /// <returns></returns>
        private IMedia CreateMediaFromDto(ContentVersionDto dto,
            IMediaType contentType,
            PropertyCollection propCollection)
        {
            var factory = new MediaFactory(contentType, NodeObjectTypeId, dto.NodeId);
            var media = factory.BuildEntity(dto);

            media.Properties = propCollection;

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)media).ResetDirtyProperties(false);
            return media;
        }

        /// <summary>
        /// Private method to create a media object from a ContentDto
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        private IMedia CreateMediaFromDto(ContentVersionDto dto, Guid versionId)
        {
            var contentType = _mediaTypeRepository.Get(dto.ContentDto.ContentTypeId);

            var factory = new MediaFactory(contentType, NodeObjectTypeId, dto.NodeId);
            var media = factory.BuildEntity(dto);

            var docDef = new DocumentDefinition(dto.NodeId, versionId, media.UpdateDate, media.CreateDate, contentType);

            var properties = GetPropertyCollection(new[] { docDef });

            media.Properties = properties[dto.NodeId];

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)media).ResetDirtyProperties(false);
            return media;
        }

        private string EnsureUniqueNodeName(int parentId, string nodeName, int id = 0)
        {
            if (EnsureUniqueNaming == false)
                return nodeName;

            var sql = Sql()
                .SelectAll()
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
                        currentName = $"{nodeName} ({uniqueNumber})";
                        uniqueNumber++;
                    }
                }
            }

            return currentName;
        }

        #region Xml - Should Move!

        public void RebuildXmlStructures(Func<IMedia, XElement> serializer, int groupSize = 5000, IEnumerable<int> contentTypeIds = null)
        {

            //Ok, now we need to remove the data and re-insert it, we'll do this all in one transaction too.
            using (var tr = Database.GetTransaction())
            {
                //Remove all the data first, if anything fails after this it's no problem the transaction will be reverted
                if (contentTypeIds == null)
                {
                    var mediaObjectType = Guid.Parse(Constants.ObjectTypes.Media);
                    var subQuery = Sql()
                        .Select("DISTINCT cmsContentXml.nodeId")
                        .From<ContentXmlDto>()
                        .InnerJoin<NodeDto>()
                        .On<ContentXmlDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                        .Where<NodeDto>(dto => dto.NodeObjectType == mediaObjectType);

                    var deleteSql = SqlSyntax.GetDeleteSubquery("cmsContentXml", "nodeId", subQuery);
                    Database.Execute(deleteSql);
                }
                else
                {
                    foreach (var id in contentTypeIds)
                    {
                        var id1 = id;
                        var mediaObjectType = Guid.Parse(Constants.ObjectTypes.Media);
                        var subQuery = Sql()
                            .Select("DISTINCT cmsContentXml.nodeId")
                            .From<ContentXmlDto>()
                            .InnerJoin<NodeDto>()
                            .On<ContentXmlDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                            .InnerJoin<ContentDto>()
                            .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                            .Where<NodeDto>(dto => dto.NodeObjectType == mediaObjectType)
                            .Where<ContentDto>(dto => dto.ContentTypeId == id1);

                        var deleteSql = SqlSyntax.GetDeleteSubquery("cmsContentXml", "nodeId", subQuery);
                        Database.Execute(deleteSql);
                    }
                }

                //now insert the data, again if something fails here, the whole transaction is reversed
                if (contentTypeIds == null)
                {
                    RebuildXmlStructuresProcessQuery(serializer, Query, tr, groupSize);
                }
                else
                {
                    foreach (var contentTypeId in contentTypeIds)
                    {
                        //copy local
                        var id = contentTypeId;
                        var query = Query.Where(x => x.ContentTypeId == id && x.Trashed == false);
                        RebuildXmlStructuresProcessQuery(serializer, query, tr, groupSize);
                    }
                }

                tr.Complete();
            }
        }

        private void RebuildXmlStructuresProcessQuery(Func<IMedia, XElement> serializer, IQuery<IMedia> query, ITransaction tr, int pageSize)
        {
            var pageIndex = 0;
            long total;
            var processed = 0;
            do
            {
                var descendants = GetPagedResultsByQuery(query, pageIndex, pageSize, out total, "Path", Direction.Ascending, true);

                var xmlItems = (from descendant in descendants
                                let xml = serializer(descendant)
                                select new ContentXmlDto { NodeId = descendant.Id, Xml = xml.ToDataString() }).ToArray();

                //bulk insert it into the database
                Database.BulkInsertRecords(SqlSyntax, xmlItems, tr);

                processed += xmlItems.Length;

                pageIndex++;
            } while (processed < total);
        }


        public void AddOrUpdateContentXml(IMedia content, Func<IMedia, XElement> xml)
        {
            _contentXmlRepository.AddOrUpdate(new ContentXmlEntity<IMedia>(content, xml));
        }

        public void AddOrUpdatePreviewXml(IMedia content, Func<IMedia, XElement> xml)
        {
            _contentPreviewRepository.AddOrUpdate(new ContentPreviewEntity<IMedia>(content, xml));
        }

        #endregion
    }
}
