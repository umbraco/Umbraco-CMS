using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dynamics;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;

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

        public MediaRepository(IDatabaseUnitOfWork work, CacheHelper cache, ILogger logger, ISqlSyntaxProvider sqlSyntax, IMediaTypeRepository mediaTypeRepository, ITagRepository tagRepository)
            : base(work, cache, logger, sqlSyntax)
        {
            if (mediaTypeRepository == null) throw new ArgumentNullException("mediaTypeRepository");
            if (tagRepository == null) throw new ArgumentNullException("tagRepository");
            _mediaTypeRepository = mediaTypeRepository;
            _tagRepository = tagRepository;
            _contentXmlRepository = new ContentXmlRepository<IMedia>(work, CacheHelper.CreateDisabledCacheHelper(), logger, sqlSyntax);
            _contentPreviewRepository = new ContentPreviewRepository<IMedia>(work, CacheHelper.CreateDisabledCacheHelper(), logger, sqlSyntax);
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

            var content = CreateMediaFromDto(dto, dto.VersionId, sql);

            return content;
        }

        protected override IEnumerable<IMedia> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(false);
            if (ids.Any())
            {
                sql.Where("umbracoNode.id in (@ids)", new { ids = ids });
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

            var properties = GetPropertyCollection(sql, new[] { new DocumentDefinition(dto.NodeId, dto.VersionId, media.UpdateDate, media.CreateDate, mediaType) });

            media.Properties = properties[dto.NodeId];

            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            ((Entity)media).ResetDirtyProperties(false);
            return media;
        }

        public void RebuildXmlStructures(Func<IMedia, XElement> serializer, int groupSize = 5000, IEnumerable<int> contentTypeIds = null)
        {

            //Ok, now we need to remove the data and re-insert it, we'll do this all in one transaction too.
            using (var tr = Database.GetTransaction())
            {
                //Remove all the data first, if anything fails after this it's no problem the transaction will be reverted
                if (contentTypeIds == null)
                {
                    var mediaObjectType = Guid.Parse(Constants.ObjectTypes.Media);
                    var subQuery = new Sql()
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
                        var subQuery = new Sql()
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
                    var query = Query<IMedia>.Builder;
                    RebuildXmlStructuresProcessQuery(serializer, query, tr, groupSize);
                }
                else
                {
                    foreach (var contentTypeId in contentTypeIds)
                    {
                        //copy local
                        var id = contentTypeId;
                        var query = Query<IMedia>.Builder.Where(x => x.ContentTypeId == id && x.Trashed == false);
                        RebuildXmlStructuresProcessQuery(serializer, query, tr, groupSize);
                    }
                }

                tr.Complete();
            }
        }

        private void RebuildXmlStructuresProcessQuery(Func<IMedia, XElement> serializer, IQuery<IMedia> query, Transaction tr, int pageSize)
        {
            var pageIndex = 0;
            var total = int.MinValue;
            var processed = 0;
            do
            {
                var descendants = GetPagedResultsByQuery(query, pageIndex, pageSize, out total, "Path", Direction.Ascending);

                var xmlItems = (from descendant in descendants
                                let xml = serializer(descendant)
                                select new ContentXmlDto { NodeId = descendant.Id, Xml = xml.ToString(SaveOptions.None) }).ToArray();

                //bulk insert it into the database
                Database.BulkInsertRecords(xmlItems, tr);

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

            //In order to update the ContentVersion we need to retrieve its primary key id
            var contentVerDto = Database.SingleOrDefault<ContentVersionDto>("WHERE VersionId = @Version", new { Version = entity.Version });
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

        /// <summary>
        /// Gets paged media results
        /// </summary>
        /// <param name="query">Query to excute</param>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="filter">Search text filter</param>
        /// <returns>An Enumerable list of <see cref="IMedia"/> objects</returns>
        public IEnumerable<IMedia> GetPagedResultsByQuery(IQuery<IMedia> query, int pageIndex, int pageSize, out int totalRecords,
            string orderBy, Direction orderDirection, string filter = "")
        {
            var args = new List<object>();
            var sbWhere = new StringBuilder();
            Func<Tuple<string, object[]>> filterCallback = null;
            if (filter.IsNullOrWhiteSpace() == false)
            {
                sbWhere.Append("AND (umbracoNode." + SqlSyntax.GetQuotedColumnName("text") + " LIKE @" + args.Count + ")");
                args.Add("%" + filter + "%");
                filterCallback = () => new Tuple<string, object[]>(sbWhere.ToString().Trim(), args.ToArray());
            }

            return GetPagedResultsByQuery<ContentVersionDto, Models.Media>(query, pageIndex, pageSize, out totalRecords,
                new Tuple<string, string>("cmsContentVersion", "contentId"),
                ProcessQuery, orderBy, orderDirection,
                filterCallback);

        }

        private IEnumerable<IMedia> ProcessQuery(Sql sql)
        {
            //NOTE: This doesn't allow properties to be part of the query
            var dtos = Database.Fetch<ContentVersionDto, ContentDto, NodeDto>(sql);

            //content types
            var contentTypes = _mediaTypeRepository.GetAll(dtos.Select(x => x.ContentDto.ContentTypeId).ToArray())
                .ToArray();

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

            var propertyData = GetPropertyCollection(sql, docDefs);

            return dtosWithContentTypes.Select(d => CreateMediaFromDto(
                d.dto,
                contentTypes.First(ct => ct.Id == d.dto.ContentDto.ContentTypeId),
                propertyData[d.dto.NodeId]));
        }

        /// <summary>
        /// Private method to create a media object from a ContentDto
        /// </summary>
        /// <param name="d"></param>
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
        /// <param name="d"></param>
        /// <param name="versionId"></param>
        /// <param name="docSql"></param>
        /// <returns></returns>
        private IMedia CreateMediaFromDto(ContentVersionDto dto, Guid versionId, Sql docSql)
        {
            var contentType = _mediaTypeRepository.Get(dto.ContentDto.ContentTypeId);

            var factory = new MediaFactory(contentType, NodeObjectTypeId, dto.NodeId);
            var media = factory.BuildEntity(dto);

            var docDef = new DocumentDefinition(dto.NodeId, versionId, media.UpdateDate, media.CreateDate, contentType);

            var properties = GetPropertyCollection(docSql, new[] { docDef });

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
