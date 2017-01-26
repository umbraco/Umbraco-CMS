﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IContent"/>
    /// </summary>
    internal class ContentRepository : RecycleBinRepository<int, IContent>, IContentRepository
    {
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly ITagRepository _tagRepository;
        private readonly CacheHelper _cacheHelper;
        private readonly ContentPreviewRepository<IContent> _contentPreviewRepository;
        private readonly ContentXmlRepository<IContent> _contentXmlRepository;

        public ContentRepository(IDatabaseUnitOfWork work, CacheHelper cacheHelper, ILogger logger, ISqlSyntaxProvider syntaxProvider, IContentTypeRepository contentTypeRepository, ITemplateRepository templateRepository, ITagRepository tagRepository, IContentSection contentSection)
            : base(work, cacheHelper, logger, syntaxProvider, contentSection)
        {
            if (contentTypeRepository == null) throw new ArgumentNullException("contentTypeRepository");
            if (templateRepository == null) throw new ArgumentNullException("templateRepository");
            if (tagRepository == null) throw new ArgumentNullException("tagRepository");
            _contentTypeRepository = contentTypeRepository;
            _templateRepository = templateRepository;
            _tagRepository = tagRepository;
            _cacheHelper = cacheHelper;
            _contentPreviewRepository = new ContentPreviewRepository<IContent>(work, CacheHelper.CreateDisabledCacheHelper(), logger, syntaxProvider);
            _contentXmlRepository = new ContentXmlRepository<IContent>(work, CacheHelper.CreateDisabledCacheHelper(), logger, syntaxProvider);

            EnsureUniqueNaming = true;
        }

        public bool EnsureUniqueNaming { get; set; }

        #region Overrides of RepositoryBase<IContent>

        protected override IContent PerformGet(int id)
        {
            var sql = GetBaseQuery(false)
                .Where(GetBaseWhereClause(), new { Id = id })
                .Where<DocumentDto>(x => x.Newest, SqlSyntax)
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate, SqlSyntax);

            var dto = Database.Fetch<DocumentDto, ContentVersionDto, ContentDto, NodeDto, DocumentPublishedReadOnlyDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();

            if (dto == null)
                return null;

            var content = CreateContentFromDto(dto, dto.ContentVersionDto.VersionId, sql);

            return content;
        }

        protected override IEnumerable<IContent> PerformGetAll(params int[] ids)
        {
            Func<Sql, Sql> translate = s =>
            {
                if (ids.Any())
                {
                    s.Where("umbracoNode.id in (@ids)", new { ids });
                }
                //we only want the newest ones with this method
                s.Where<DocumentDto>(x => x.Newest, SqlSyntax);
                return s;
            };
            
            var sqlBaseFull = GetBaseQuery(BaseQueryType.Full);
            var sqlBaseIds = GetBaseQuery(BaseQueryType.Ids);

            return ProcessQuery(translate(sqlBaseFull), translate(sqlBaseIds));
        }

        protected override IEnumerable<IContent> PerformGetByQuery(IQuery<IContent> query)
        {
            var sqlBaseFull = GetBaseQuery(BaseQueryType.Full);
            var sqlBaseIds = GetBaseQuery(BaseQueryType.Ids);

            Func<SqlTranslator<IContent>, Sql> translate = (translator) =>
            {
                return translator.Translate()
                    .Where<DocumentDto>(x => x.Newest, SqlSyntax)
                    .OrderByDescending<ContentVersionDto>(x => x.VersionDate, SqlSyntax)
                    .OrderBy<NodeDto>(x => x.SortOrder, SqlSyntax);
            };

            var translatorFull = new SqlTranslator<IContent>(sqlBaseFull, query);
            var translatorIds = new SqlTranslator<IContent>(sqlBaseIds, query);

            return ProcessQuery(translate(translatorFull), translate(translatorIds));
        }

        #endregion        

        #region Overrides of PetaPocoRepositoryBase<IContent>

        protected override Sql GetBaseQuery(BaseQueryType queryType)
        {
            var sql = new Sql();
            sql.Select(queryType == BaseQueryType.Count ? "COUNT(*)" : (queryType == BaseQueryType.Ids ? "cmsDocument.nodeId" : "*"))
                .From<DocumentDto>(SqlSyntax)
                .InnerJoin<ContentVersionDto>(SqlSyntax)
                .On<DocumentDto, ContentVersionDto>(SqlSyntax, left => left.VersionId, right => right.VersionId)
                .InnerJoin<ContentDto>(SqlSyntax)
                .On<ContentVersionDto, ContentDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>(SqlSyntax)
                .On<ContentDto, NodeDto>(SqlSyntax, left => left.NodeId, right => right.NodeId);

            if (queryType == BaseQueryType.Full)
            {
                //The only reason we apply this left outer join is to be able to pull back the DocumentPublishedReadOnlyDto
                //information with the entire data set, so basically this will get both the latest document and also it's published
                //version if it has one. When performing a count or when just retrieving Ids like in paging, this is unecessary 
                //and causes huge performance overhead for the SQL server, especially when sorting the result. 
                //To fix this perf overhead we'd need another index on :
                // CREATE NON CLUSTERED INDEX ON cmsDocument.node + cmsDocument.published

                var sqlx = string.Format("LEFT OUTER JOIN {0} {1} ON ({1}.{2}={0}.{2} AND {1}.{3}=1)",
                SqlSyntax.GetQuotedTableName("cmsDocument"),
                SqlSyntax.GetQuotedTableName("cmsDocument2"),
                SqlSyntax.GetQuotedColumnName("nodeId"),
                SqlSyntax.GetQuotedColumnName("published"));

                // cannot do this because PetaPoco does not know how to alias the table
                //.LeftOuterJoin<DocumentPublishedReadOnlyDto>()
                //.On<DocumentDto, DocumentPublishedReadOnlyDto>(left => left.NodeId, right => right.NodeId)
                // so have to rely on writing our own SQL
                sql.Append(sqlx /*, new { @published = true }*/);
            }

            sql.Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId, SqlSyntax);

            return sql;
        }

        protected override Sql GetBaseQuery(bool isCount)
        {
            return GetBaseQuery(isCount ? BaseQueryType.Count : BaseQueryType.Full);
        }

        protected override string GetBaseWhereClause()
        {
            return "umbracoNode.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
                           {
                               "DELETE FROM umbracoRedirectUrl WHERE contentKey IN (SELECT uniqueID FROM umbracoNode WHERE id = @Id)",
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
                               "DELETE FROM umbracoAccess WHERE nodeId = @Id",
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

        public void RebuildXmlStructures(Func<IContent, XElement> serializer, int groupSize = 200, IEnumerable<int> contentTypeIds = null)
        {
            // the previous way of doing this was to run it all in one big transaction,
            // and to bulk-insert groups of xml rows - which works, until the transaction
            // times out - and besides, because v7 transactions are ReadCommited, it does
            // not bring much safety - so this reverts to updating each record individually,
            // and it may be slower in the end, but should be more resilient.

            var contentTypeIdsA = contentTypeIds == null ? new int[0] : contentTypeIds.ToArray();

            Func<int, Sql, Sql> translate = (bId, sql) =>
            {
                if (contentTypeIdsA.Length > 0)
                {
                    sql.WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIdsA, SqlSyntax);
                }

                sql
                    .Where<NodeDto>(x => x.NodeId > bId && x.Trashed == false, SqlSyntax)
                    .Where<DocumentDto>(x => x.Published, SqlSyntax)
                    .OrderBy<NodeDto>(x => x.NodeId, SqlSyntax);

                return sql;
            };

            var baseId = 0;
            
            while (true)
            {
                // get the next group of nodes
                var sqlFull = translate(baseId, GetBaseQuery(BaseQueryType.Full));
                var sqlIds = translate(baseId, GetBaseQuery(BaseQueryType.Ids));

                var xmlItems = ProcessQuery(SqlSyntax.SelectTop(sqlFull, groupSize), SqlSyntax.SelectTop(sqlIds, groupSize))
                    .Select(x => new ContentXmlDto { NodeId = x.Id, Xml = serializer(x).ToString() })
                    .ToList();

                // no more nodes, break
                if (xmlItems.Count == 0) break;

                foreach (var xmlItem in xmlItems)
                {
                    try
                    {
                        // should happen in most cases, then it tries to insert, and it should work
                        // unless the node has been deleted, and we just report the exception
                        Database.InsertOrUpdate(xmlItem);
                    }
                    catch (Exception e)
                    {
                        Logger.Error<MediaRepository>("Could not rebuild XML for nodeId=" + xmlItem.NodeId, e);
                    }
                }
                baseId = xmlItems.Last().NodeId;
            }
        }

        public override IEnumerable<IContent> GetAllVersions(int id)
        {
            Func<Sql, Sql> translate = s =>
            {
                return s.Where(GetBaseWhereClause(), new {Id = id})
                    .OrderByDescending<ContentVersionDto>(x => x.VersionDate, SqlSyntax);
            };

            var sqlFull = translate(GetBaseQuery(BaseQueryType.Full));
            var sqlIds = translate(GetBaseQuery(BaseQueryType.Ids));
            
            return ProcessQuery(sqlFull, sqlIds, true);
        }

        public override IContent GetByVersion(Guid versionId)
        {
            var sql = GetBaseQuery(false);
            sql.Where("cmsContentVersion.VersionId = @VersionId", new { VersionId = versionId });
            sql.OrderByDescending<ContentVersionDto>(x => x.VersionDate, SqlSyntax);

            var dto = Database.Fetch<DocumentDto, ContentVersionDto, ContentDto, NodeDto, DocumentPublishedReadOnlyDto>(sql).FirstOrDefault();

            if (dto == null)
                return null;

            var content = CreateContentFromDto(dto, versionId, sql);

            return content;
        }

        public override void DeleteVersion(Guid versionId)
        {
            var sql = new Sql()
                .Select("*")
                .From<DocumentDto>(SqlSyntax)
                .InnerJoin<ContentVersionDto>(SqlSyntax).On<ContentVersionDto, DocumentDto>(SqlSyntax, left => left.VersionId, right => right.VersionId)
                .Where<ContentVersionDto>(x => x.VersionId == versionId, SqlSyntax)
                .Where<DocumentDto>(x => x.Newest != true, SqlSyntax);
            var dto = Database.Fetch<DocumentDto, ContentVersionDto>(sql).FirstOrDefault();

            if (dto == null) return;

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

        protected override void PersistDeletedItem(IContent entity)
        {
            //We need to clear out all access rules but we need to do this in a manual way since 
            // nothing in that table is joined to a content id
            var subQuery = new Sql()
                .Select("umbracoAccessRule.accessId")
                .From<AccessRuleDto>(SqlSyntax)
                .InnerJoin<AccessDto>(SqlSyntax)
                .On<AccessRuleDto, AccessDto>(SqlSyntax, left => left.AccessId, right => right.Id)
                .Where<AccessDto>(dto => dto.NodeId == entity.Id);
            Database.Execute(SqlSyntax.GetDeleteSubquery("umbracoAccessRule", "accessId", subQuery));

            //now let the normal delete clauses take care of everything else
            base.PersistDeletedItem(entity);
        }

        protected override void PersistNewItem(IContent entity)
        {
            ((Content)entity).AddingEntity();

            //ensure the default template is assigned
            if (entity.Template == null)
            {
                entity.Template = entity.ContentType.DefaultTemplate;
            }

            //Ensure unique name on the same level
            entity.Name = EnsureUniqueNodeName(entity.ParentId, entity.Name);

            //Ensure that strings don't contain characters that are invalid in XML
            entity.SanitizeEntityPropertiesForXmlStorage();

            var factory = new ContentFactory(NodeObjectTypeId, entity.Id);
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
            var o = Database.IsNew(nodeDto) ? Convert.ToInt32(Database.Insert(nodeDto)) : Database.Update(nodeDto);

            //Update with new correct path
            nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
            nodeDto.ValidatePathWithException();
            Database.Update(nodeDto);

            //Update entity with correct values
            entity.Id = nodeDto.NodeId; //Set Id on entity to ensure an Id is set
            entity.Path = nodeDto.Path;
            entity.SortOrder = sortOrder;
            entity.Level = level;

            //Assign the same permissions to it as the parent node
            // http://issues.umbraco.org/issue/U4-2161     
            var permissionsRepo = new PermissionRepository<IContent>(UnitOfWork, _cacheHelper, SqlSyntax);
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
                ((Content)entity).PermissionsChanged = true;
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

            //lastly, check if we are a creating a published version , then update the tags table
            if (entity.Published)
            {
                UpdatePropertyTags(entity, _tagRepository);
            }

            // published => update published version infos, else leave it blank
            if (entity.Published)
            {
                dto.DocumentPublishedReadOnlyDto = new DocumentPublishedReadOnlyDto
                {
                    VersionId = dto.VersionId,
                    Newest = true,
                    NodeId = dto.NodeId,
                    Published = true
                };
                ((Content)entity).PublishedVersionGuid = dto.VersionId;
            }

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IContent entity)
        {
            var publishedState = ((Content)entity).PublishedState;

            //check if we need to make any database changes at all
            if (entity.RequiresSaving(publishedState) == false)
            {
                entity.ResetDirtyProperties();
                return;
            }

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
            nodeDto.ValidatePathWithException();
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
            var propertyFactory = new PropertyFactory(entity.ContentType.CompositionPropertyTypes.ToArray(), entity.Version, entity.Id);
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
                    if (keyDictionary.ContainsKey(property.PropertyTypeId) == false) continue;

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

            // published => update published version infos,
            // else if unpublished then clear published version infos
            if (entity.Published)
            {
                dto.DocumentPublishedReadOnlyDto = new DocumentPublishedReadOnlyDto
                {
                    VersionId = dto.VersionId,
                    Newest = true,
                    NodeId = dto.NodeId,
                    Published = true
                };
                ((Content)entity).PublishedVersionGuid = dto.VersionId;
            }
            else if (publishedStateChanged)
            {
                dto.DocumentPublishedReadOnlyDto = new DocumentPublishedReadOnlyDto
                {
                    VersionId = default(Guid),
                    Newest = false,
                    NodeId = dto.NodeId,
                    Published = false
                };
                ((Content)entity).PublishedVersionGuid = default(Guid);
            }

            entity.ResetDirtyProperties();
        }


        #endregion

        #region Implementation of IContentRepository

        public IEnumerable<IContent> GetByPublishedVersion(IQuery<IContent> query)
        {
            Func<SqlTranslator<IContent>, Sql> translate = t =>
            {
                return t.Translate()
                    .Where<DocumentDto>(x => x.Published, SqlSyntax)
                    .OrderBy<NodeDto>(x => x.Level, SqlSyntax)
                    .OrderBy<NodeDto>(x => x.SortOrder, SqlSyntax);
            };

            // we WANT to return contents in top-down order, ie parents should come before children
            // ideal would be pure xml "document order" which can be achieved with:
            // ORDER BY substring(path, 1, len(path) - charindex(',', reverse(path))), sortOrder
            // but that's probably an overkill - sorting by level,sortOrder should be enough

            var sqlFull = GetBaseQuery(BaseQueryType.Full);
            var translatorFull = new SqlTranslator<IContent>(sqlFull, query);
            var sqlIds = GetBaseQuery(BaseQueryType.Ids);
            var translatorIds = new SqlTranslator<IContent>(sqlIds, query);

            return ProcessQuery(translate(translatorFull), translate(translatorIds), true);
        }
        
        /// <summary>
        /// This builds the Xml document used for the XML cache
        /// </summary>
        /// <returns></returns>
        public XmlDocument BuildXmlCache()
        {
            //TODO: This is what we should do , but converting to use XDocument would be breaking unless we convert
            // to XmlDocument at the end of this, but again, this would be bad for memory... though still not nearly as
            // bad as what is happening before!
            // We'll keep using XmlDocument for now though, but XDocument xml generation is much faster:
            // https://blogs.msdn.microsoft.com/codejunkie/2008/10/08/xmldocument-vs-xelement-performance/
            // I think we already have code in here to convert XDocument to XmlDocument but in case we don't here
            // it is: https://blogs.msdn.microsoft.com/marcelolr/2009/03/13/fast-way-to-convert-xmldocument-into-xdocument/

            //// Prepare an XmlDocument with an appropriate inline DTD to match
            //// the expected content
            //var parent = new XElement("root", new XAttribute("id", "-1"));
            //var xmlDoc = new XDocument(
            //    new XDocumentType("root", null, null, DocumentType.GenerateDtd()),
            //    parent);

            var xmlDoc = new XmlDocument();
            var doctype = xmlDoc.CreateDocumentType("root", null, null,
                ApplicationContext.Current.Services.ContentTypeService.GetContentTypesDtd());
            xmlDoc.AppendChild(doctype);
            var parent = xmlDoc.CreateElement("root");
            var pIdAtt = xmlDoc.CreateAttribute("id");
            pIdAtt.Value = "-1";
            parent.Attributes.Append(pIdAtt);
            xmlDoc.AppendChild(parent);

            //Ensure that only nodes that have published versions are selected
            var sql = string.Format(@"select umbracoNode.id, umbracoNode.parentID, umbracoNode.sortOrder, cmsContentXml.{0}, umbracoNode.{1} from umbracoNode
inner join cmsContentXml on cmsContentXml.nodeId = umbracoNode.id and umbracoNode.nodeObjectType = @type
where umbracoNode.id in (select cmsDocument.nodeId from cmsDocument where cmsDocument.published = 1)
order by umbracoNode.{2}, umbracoNode.parentID, umbracoNode.sortOrder",
                SqlSyntax.GetQuotedColumnName("xml"),
                SqlSyntax.GetQuotedColumnName("level"),
                SqlSyntax.GetQuotedColumnName("level"));

            XmlElement last = null;

            //NOTE: Query creates a reader - does not load all into memory
            foreach (var row in Database.Query<dynamic>(sql, new { type = new Guid(Constants.ObjectTypes.Document) }))
            {
                string parentId = ((int)row.parentID).ToInvariantString();
                string xml = row.xml;
                int sortOrder = row.sortOrder;

                //if the parentid is changing
                if (last != null && last.GetAttribute("parentID") != parentId)
                {
                    parent = xmlDoc.GetElementById(parentId);
                    if (parent == null)
                    {
                        //Need to short circuit here, if the parent is not there it means that the parent is unpublished
                        // and therefore the child is not published either so cannot be included in the xml cache
                        continue;
                    }
                }

                var xmlDocFragment = xmlDoc.CreateDocumentFragment();
                xmlDocFragment.InnerXml = xml;

                last = (XmlElement)parent.AppendChild(xmlDocFragment);

                // fix sortOrder - see notes in UpdateSortOrder
                last.Attributes["sortOrder"].Value = sortOrder.ToInvariantString();
            }

            return xmlDoc;

        }

        public int CountPublished()
        {
            var sql = GetBaseQuery(true).Where<NodeDto>(x => x.Trashed == false)
                .Where<DocumentDto>(x => x.Published == true);
            return Database.ExecuteScalar<int>(sql);
        }

        public void ReplaceContentPermissions(EntityPermissionSet permissionSet)
        {
            var repo = new PermissionRepository<IContent>(UnitOfWork, _cacheHelper, SqlSyntax);
            repo.ReplaceEntityPermissions(permissionSet);
        }

        public void ClearPublished(IContent content)
        {
            // race cond!
            var documentDtos = Database.Fetch<DocumentDto>("WHERE nodeId=@id AND published=@published", new { id = content.Id, published = true });
            foreach (var documentDto in documentDtos)
            {
                documentDto.Published = false;
                Database.Update(documentDto);
            }
        }

        /// <summary>
        /// Assigns a single permission to the current content item for the specified user ids
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permission"></param>
        /// <param name="userIds"></param>        
        public void AssignEntityPermission(IContent entity, char permission, IEnumerable<int> userIds)
        {
            var repo = new PermissionRepository<IContent>(UnitOfWork, _cacheHelper, SqlSyntax);
            repo.AssignEntityPermission(entity, permission, userIds);
        }

        public IEnumerable<EntityPermission> GetPermissionsForEntity(int entityId)
        {
            var repo = new PermissionRepository<IContent>(UnitOfWork, _cacheHelper, SqlSyntax);
            return repo.GetPermissionsForEntity(entityId);
        }

        /// <summary>
        /// Adds/updates content/published xml
        /// </summary>
        /// <param name="content"></param>
        /// <param name="xml"></param>
        public void AddOrUpdateContentXml(IContent content, Func<IContent, XElement> xml)
        {
            _contentXmlRepository.AddOrUpdate(new ContentXmlEntity<IContent>(content, xml));
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
            _contentPreviewRepository.AddOrUpdate(new ContentPreviewEntity<IContent>(content, xml));
        }

        /// <summary>
        /// Gets paged content results
        /// </summary>
        /// <param name="query">Query to excute</param>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="orderBySystemField">Flag to indicate when ordering by system field</param>
        /// <param name="filter">Search text filter</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetPagedResultsByQuery(IQuery<IContent> query, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IContent> filter = null)
        {

            //NOTE: This uses the GetBaseQuery method but that does not take into account the required 'newest' field which is 
            // what we always require for a paged result, so we'll ensure it's included in the filter
            
            var filterSql = new Sql().Append("AND (cmsDocument.newest = 1)");
            if (filter != null)
            {
                foreach (var filterClaus in filter.GetWhereClauses())
                {
                    filterSql.Append(string.Format("AND ({0})", filterClaus.Item1), filterClaus.Item2);
                }
            }
            
            Func<Tuple<string, object[]>> filterCallback = () => new Tuple<string, object[]>(filterSql.SQL, filterSql.Arguments);

            return GetPagedResultsByQuery<DocumentDto>(query, pageIndex, pageSize, out totalRecords,
                new Tuple<string, string>("cmsDocument", "nodeId"),
                (sqlFull, sqlIds) => ProcessQuery(sqlFull, sqlIds), orderBy, orderDirection, orderBySystemField,
                filterCallback);

        }

        #endregion

        #region IRecycleBinRepository members

        protected override int RecycleBinId
        {
            get { return Constants.System.RecycleBinContent; }
        }

        #endregion

        protected override string GetDatabaseFieldNameForOrderBy(string orderBy)
        {
            //Some custom ones
            switch (orderBy.ToUpperInvariant())
            {
                case "NAME":
                    return "cmsDocument.text";
                case "UPDATER":
                    //TODO: This isn't going to work very nicely because it's going to order by ID, not by letter
                    return "cmsDocument.documentUser";
            }

            return base.GetDatabaseFieldNameForOrderBy(orderBy);
        }

        /// <summary>
        /// This is the underlying method that processes most queries for this repository
        /// </summary>
        /// <param name="sqlFull">
        /// The full SQL with the outer join to return all data required to create an IContent
        /// </param>
        /// <param name="sqlIds">
        /// The Id SQL without the outer join to just return all document ids - used to process the properties for the content item
        /// </param>
        /// <param name="withCache"></param>
        /// <returns></returns>
        private IEnumerable<IContent> ProcessQuery(Sql sqlFull, Sql sqlIds, bool withCache = false)
        {
            // fetch returns a list so it's ok to iterate it in this method
            var dtos = Database.Fetch<DocumentDto, ContentVersionDto, ContentDto, NodeDto, DocumentPublishedReadOnlyDto>(sqlFull);
            if (dtos.Count == 0) return Enumerable.Empty<IContent>();

            var content = new IContent[dtos.Count];
            var defs = new List<DocumentDefinition>();
            var templateIds = new List<int>();
            
            //track the looked up content types, even though the content types are cached
            // they still need to be deep cloned out of the cache and we don't want to add
            // the overhead of deep cloning them on every item in this loop
            var contentTypes = new Dictionary<int, IContentType>();
            
            for (var i = 0; i < dtos.Count; i++)
            {
                var dto = dtos[i];

                // if the cache contains the published version, use it
                if (withCache)
                {
                    var cached = RuntimeCache.GetCacheItem<IContent>(GetCacheIdKey<IContent>(dto.NodeId));
                    //only use this cached version if the dto returned is also the publish version, they must match
                    if (cached != null && cached.Published && dto.Published)
                    {
                        content[i] = cached;
                        continue;
                    }
                }

                // else, need to fetch from the database
                // content type repository is full-cache so OK to get each one independently

                IContentType contentType;
                if (contentTypes.ContainsKey(dto.ContentVersionDto.ContentDto.ContentTypeId))
                {
                    contentType = contentTypes[dto.ContentVersionDto.ContentDto.ContentTypeId];
                }
                else
                {
                    contentType = _contentTypeRepository.Get(dto.ContentVersionDto.ContentDto.ContentTypeId);
                    contentTypes[dto.ContentVersionDto.ContentDto.ContentTypeId] = contentType;
                }
                
                content[i] = ContentFactory.BuildEntity(dto, contentType);

                // need template
                if (dto.TemplateId.HasValue && dto.TemplateId.Value > 0)
                    templateIds.Add(dto.TemplateId.Value);

                // need properties
                defs.Add(new DocumentDefinition(
                    dto.NodeId,
                    dto.VersionId,
                    dto.ContentVersionDto.VersionDate,
                    dto.ContentVersionDto.ContentDto.NodeDto.CreateDate,
                    contentType
                ));
            }

            // load all required templates in 1 query
            var templates = _templateRepository.GetAll(templateIds.ToArray())
                .ToDictionary(x => x.Id, x => x);

            // load all properties for all documents from database in 1 query
            var propertyData = GetPropertyCollection(sqlIds, defs);

            // assign
            var dtoIndex = 0;
            foreach (var def in defs)
            {
                // move to corresponding item (which has to exist)
                while (dtos[dtoIndex].NodeId != def.Id) dtoIndex++;

                // complete the item
                var cc = content[dtoIndex];
                var dto = dtos[dtoIndex];
                ITemplate template = null;
                if (dto.TemplateId.HasValue)
                    templates.TryGetValue(dto.TemplateId.Value, out template); // else null
                cc.Template = template;
                cc.Properties = propertyData[cc.Id];

                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                cc.ResetDirtyProperties(false);
            }

            return content;
        }

        /// <summary>
        /// Private method to create a content object from a DocumentDto, which is used by Get and GetByVersion.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="versionId"></param>
        /// <param name="docSql"></param>
        /// <returns></returns>
        private IContent CreateContentFromDto(DocumentDto dto, Guid versionId, Sql docSql)
        {
            var contentType = _contentTypeRepository.Get(dto.ContentVersionDto.ContentDto.ContentTypeId);

            var content = ContentFactory.BuildEntity(dto, contentType);

            //Check if template id is set on DocumentDto, and get ITemplate if it is.
            if (dto.TemplateId.HasValue && dto.TemplateId.Value > 0)
            {
                content.Template = _templateRepository.Get(dto.TemplateId.Value);
            }

            var docDef = new DocumentDefinition(dto.NodeId, versionId, content.UpdateDate, content.CreateDate, contentType);

            var properties = GetPropertyCollection(docSql, new[] { docDef });

            content.Properties = properties[dto.NodeId];

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
            _contentTypeRepository.Dispose();
            _templateRepository.Dispose();
            _tagRepository.Dispose();
            _contentPreviewRepository.Dispose();
            _contentXmlRepository.Dispose();
        }
    }
}