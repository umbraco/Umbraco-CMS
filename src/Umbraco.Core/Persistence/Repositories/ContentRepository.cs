using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IContent"/>.
    /// </summary>
    internal class ContentRepository : RecycleBinRepository<int, IContent, ContentRepository>, IContentRepository
    {
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly ITagRepository _tagRepository;
        private readonly CacheHelper _cacheHelper;
        private PermissionRepository<IContent> _permissionRepository;
        private readonly ContentByGuidReadRepository _contentByGuidReadRepository;

        public ContentRepository(IScopeUnitOfWork work, CacheHelper cacheHelper, ILogger logger, IContentTypeRepository contentTypeRepository, ITemplateRepository templateRepository, ITagRepository tagRepository, IContentSection settings)
            : base(work, cacheHelper, logger)
        {
            _contentTypeRepository = contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
            _cacheHelper = cacheHelper;
            _contentByGuidReadRepository = new ContentByGuidReadRepository(this, work, cacheHelper, logger);
            EnsureUniqueNaming = settings.EnsureUniqueNaming;
        }

        protected override ContentRepository This => this;

        public bool EnsureUniqueNaming { get; set; }

        // note: is ok to 'new' the repo here as it's a sub-repo really
        private PermissionRepository<IContent> PermissionRepository => _permissionRepository
            ?? (_permissionRepository = new PermissionRepository<IContent>(UnitOfWork, _cacheHelper, Logger));

        #region Repository Base

        protected override Guid NodeObjectTypeId => Constants.ObjectTypes.Document;

        protected override IContent PerformGet(int id)
        {
            var sql = GetBaseQuery(QueryType.Single)
                .Where<NodeDto>(x => x.NodeId == id)
                .SelectTop(1);

            var dto = Database.Fetch<DocumentDto>(sql).FirstOrDefault();
            return dto == null
                ? null
                : MapDtoToContent(dto);
        }

        protected override IEnumerable<IContent> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(QueryType.Many);

            if (ids.Any())
                sql.WhereIn<NodeDto>(x => x.NodeId, ids);

            return MapDtosToContent(Database.Fetch<DocumentDto>(sql));
        }

        protected override IEnumerable<IContent> PerformGetByQuery(IQuery<IContent> query)
        {
            var sqlClause = GetBaseQuery(QueryType.Many);

            var translator = new SqlTranslator<IContent>(sqlClause, query);
            var sql = translator.Translate();

            sql // fixme why?
                .OrderBy<NodeDto>(x => x.Level)
                .OrderBy<NodeDto>(x => x.SortOrder);

            return MapDtosToContent(Database.Fetch<DocumentDto>(sql));
        }

        protected override Sql<ISqlContext> GetBaseQuery(QueryType queryType)
        {
            return GetBaseQuery(queryType, true);
        }

        protected virtual Sql<ISqlContext> GetBaseQuery(QueryType queryType, bool current)
        {
            var sql = SqlContext.Sql();

            switch (queryType) // FIXME pretend we still need these queries for now
            {
                case QueryType.Count:
                    sql = sql.SelectCount();
                    break;
                case QueryType.Ids:
                    sql = sql.Select<DocumentDto>(x => x.NodeId);
                    break;
                case QueryType.Single:
                case QueryType.Many:
                    sql = sql.Select<DocumentDto>(r =>
                        r.Select(documentDto => documentDto.ContentDto, r1 =>
                            r1.Select(contentDto => contentDto.NodeDto))
                         .Select(documentDto => documentDto.DocumentVersionDto, r1 =>
                            r1.Select(documentVersionDto => documentVersionDto.ContentVersionDto)));
                    break;
            }

            sql
                .From<DocumentDto>()
                .InnerJoin<ContentDto>().On<DocumentDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentVersionDto>().On<DocumentDto, ContentVersionDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<DocumentVersionDto>().On<ContentVersionDto, DocumentVersionDto>(left => left.Id, right => right.Id);

            sql.Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

            if (current)
                sql.Where<ContentVersionDto>(x => x.Current); // always get the current version

            return sql;
        }

        // fixme - move that one up to Versionable! or better: kill it!
        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            return GetBaseQuery(isCount ? QueryType.Count : QueryType.Single);
        }

        protected override string GetBaseWhereClause() // fixme - can we kill / refactor this?
        {
            return "umbracoNode.id = @Id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string> // fixme table names, escape everything, etc
                           {
                               "DELETE FROM umbracoRedirectUrl WHERE contentKey IN (SELECT uniqueID FROM umbracoNode WHERE id = @Id)",
                               "DELETE FROM cmsTask WHERE nodeId = @Id",
                               "DELETE FROM umbracoUser2NodeNotify WHERE nodeId = @Id",
                               "DELETE FROM umbracoUserGroup2NodePermission WHERE nodeId = @Id",
                               "DELETE FROM umbracoUserStartNode WHERE startNode = @Id",
                               "UPDATE umbracoUserGroup SET startContentId = NULL WHERE startContentId = @Id",
                               "DELETE FROM umbracoRelation WHERE parentId = @Id",
                               "DELETE FROM umbracoRelation WHERE childId = @Id",
                               "DELETE FROM cmsTagRelationship WHERE nodeId = @Id",
                               "DELETE FROM umbracoDomains WHERE domainRootStructureID = @Id",
                               "DELETE FROM " + Constants.DatabaseSchema.Tables.Document + " WHERE nodeId = @Id",
                               "DELETE FROM " + Constants.DatabaseSchema.Tables.DocumentVersion + " WHERE nodeId = @Id",
                               "DELETE FROM " + Constants.DatabaseSchema.Tables.PropertyData + " WHERE nodeId = @Id",
                               "DELETE FROM cmsPreviewXml WHERE nodeId = @Id",
                               "DELETE FROM " + Constants.DatabaseSchema.Tables.ContentVersion + " WHERE ContentId = @Id",
                               "DELETE FROM cmsContentXml WHERE nodeId = @Id",
                               "DELETE FROM " + Constants.DatabaseSchema.Tables.Content + " WHERE nodeId = @Id",
                               "DELETE FROM umbracoAccess WHERE nodeId = @Id",
                               "DELETE FROM umbracoNode WHERE id = @Id"
                           };
            return list;
        }

        #endregion

        #region Versions

        public override IEnumerable<IContent> GetAllVersions(int nodeId)
        {
            var sql = GetBaseQuery(QueryType.Many, false)
                .Where<NodeDto>(x => x.NodeId == nodeId)
                .OrderByDescending<ContentVersionDto>(x => x.Current)
                .AndByDescending<ContentVersionDto>(x => x.VersionDate);

            return MapDtosToContent(Database.Fetch<DocumentDto>(sql), true);
        }

        public override IContent GetByVersion(Guid versionId)
        {
            var sql = GetBaseQuery(QueryType.Single)
                .Where<ContentVersionDto>(x => x.VersionId == versionId);

            var dto = Database.Fetch<DocumentDto>(sql).FirstOrDefault();
            return dto == null ? null : MapDtoToContent(dto);
        }

        protected override void PerformDeleteVersion(int id, Guid versionId)
        {
            // raise event first else potential FK issues
            OnUowRemovingVersion(new UnitOfWorkVersionEventArgs(UnitOfWork, id, versionId));

            // fixme - syntax + ...
            Database.Delete<PropertyDataDto>("WHERE nodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<ContentVersionDto>("WHERE ContentId = @Id AND VersionId = @VersionId", new { Id = id, VersionId = versionId });
            Database.Delete<DocumentDto>("WHERE nodeId = @Id AND versionId = @VersionId", new { Id = id, VersionId = versionId });
        }

        #endregion

        #region Persist

        protected override void PersistNewItem(IContent entity)
        {
            ((Content) entity).AddingEntity();

            // ensure that the default template is assigned
            if (entity.Template == null)
                entity.Template = entity.ContentType.DefaultTemplate;

            // ensure unique name on the same level
            entity.Name = EnsureUniqueNodeName(entity.ParentId, entity.Name);

            // ensure that strings don't contain characters that are invalid in xml
            // fixme - do we really want to keep doing this here?
            entity.SanitizeEntityPropertiesForXmlStorage();

            // create the dto
            var dto = ContentFactory.BuildDto(entity);

            // derive path and level from parent
            var template = SqlContext.Templates.Get("Umbraco.Core.ContentRepository.GetParentNode", tsql =>
                tsql.Select<NodeDto>(x => x.NodeId).Where<NodeDto>(x => x.NodeId == SqlTemplate.Arg<int>("parentId"))
            );
            var parent = Database.Fetch<NodeDto>(template.Sql(entity.ParentId)).First();
            var level = parent.Level + 1;

            // get sort order
            var sortOrder = GetNewChildSortOrder(entity.ParentId, 0);

            // persist the node dto
            var nodeDto = dto.ContentDto.NodeDto;
            nodeDto.Path = parent.Path;
            nodeDto.Level = Convert.ToInt16(level);
            nodeDto.SortOrder = sortOrder;

            // see if there's a reserved identifier for this unique id
            // and then either update or insert the node dto
            template = SqlContext.Templates.Get("Umbraco.Core.ContentRepository.GetReservedId", tsql =>
                tsql.Select<NodeDto>(x => x.NodeId).Where<NodeDto>(x => x.UniqueId == SqlTemplate.Arg<Guid>("uniqueId") && x.NodeObjectType == Constants.ObjectTypes.IdReservation)
            );
            var id = Database.ExecuteScalar<int>(template.Sql(nodeDto.UniqueId)); // fixme can we mix named & non-named?
            if (id > 0)
            {
                nodeDto.NodeId = id;
                nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
                nodeDto.ValidatePathWithException();
                Database.Update(nodeDto);
            }
            else
            {
                Database.Insert(nodeDto);

                // update path, now that we have an id
                nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
                nodeDto.ValidatePathWithException();
                Database.Update(nodeDto);
            }

            // update entity
            entity.Id = nodeDto.NodeId;
            entity.Path = nodeDto.Path;
            entity.SortOrder = sortOrder;
            entity.Level = level;

            // persist the content dto
            var contentDto = dto.ContentDto;
            contentDto.NodeId = nodeDto.NodeId;
            Database.Insert(contentDto);

            // persist the content version dto
            // assumes a new version id and version date (modified date) has been set
            var contentVersionDto = dto.DocumentVersionDto.ContentVersionDto; // fixme version id etc?
            contentVersionDto.NodeId = nodeDto.NodeId;
            contentVersionDto.Current = true;
            Database.Insert(contentVersionDto);

            // persist the document version dto
            var documentVersionDto = dto.DocumentVersionDto;
            documentVersionDto.Id = contentVersionDto.Id; // fixme do we want this?
            Database.Insert(documentVersionDto);

            // persist the document dto
            dto.NodeId = nodeDto.NodeId; // fixme version id etc?
            Database.Insert(dto);

            // persist the property data
            var propertyDataDtos = PropertyFactory.BuildDtos(entity.Id, entity.Version, entity.Properties).ToArray();
            foreach (var propertyDataDto in propertyDataDtos)
                Database.Insert(propertyDataDto);

            // assign ids to properties, using propertyTypeId as a key
            var xids = propertyDataDtos.ToDictionary(x => x.PropertyTypeId, x => x.Id);
            foreach (var property in entity.Properties)
                property.Id = xids[property.PropertyTypeId];

            // if published, set tags accordingly
            if (entity.Published)
                UpdateEntityTags(entity, _tagRepository);

            // published => update published version infos, else leave it blank
            if (entity.Published)
            {
                // fixme anything else we should do?
                ((Content) entity).PublishedDate = dto.UpdateDate;
            }

            OnUowRefreshedEntity(new UnitOfWorkEntityEventArgs(UnitOfWork, entity));

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IContent entity)
        {
            var content = (Content) entity;
            var publishedState = content.PublishedState;
            var publishedStateChanged = publishedState == PublishedState.Publishing || publishedState == PublishedState.Unpublishing;

            // check if we need to make any database changes at all
            if (entity.RequiresSaving(publishedState) == false)
            {
                entity.ResetDirtyProperties();
                return;
            }

            //check if we need to create a new version
            var requiresNewVersion = entity.RequiresNewVersion(publishedState);
            if (requiresNewVersion)
            {
                // drop all draft infos for the current version, won't need it anymore
                Database.Delete<PropertyDataDto>("WHERE nodeId=@nodeId AND versionId=@versionId AND published=@published",
                    new { nodeId = content.Id, versionId = content.Version, published = false });

                // current version is not current anymore
                Database.Execute($"UPDATE {SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.ContentVersion)} SET current=0 WHERE nodeId=@nodeId AND versionId=@versionId",
                    new { nodeId = content.Id, versionId = content.Version });

                // resets identifiers ie get a new version id
                content.UpdatingEntity();
            }
            else
            {
                // just bump current version's update date
                if (entity.IsPropertyDirty("UpdateDate") == false || entity.UpdateDate == default)
                    entity.UpdateDate = DateTime.Now;
            }

            // ensure unique name on the same level
            entity.Name = EnsureUniqueNodeName(entity.ParentId, entity.Name, entity.Id);

            // ensure that strings don't contain characters that are invalid in xml
            // fixme - do we really want to keep doing this here?
            entity.SanitizeEntityPropertiesForXmlStorage();

            // if parent has changed, get path, level and sort order
            if (entity.IsPropertyDirty("ParentId"))
            {
                var template = SqlContext.Templates.Get("Umbraco.Core.ContentRepository.GetParentNode", tsql =>
                    tsql.Select<NodeDto>(x => x.NodeId).Where<NodeDto>(x => x.NodeId == SqlTemplate.Arg<int>("parentId"))
                );
                var parent = Database.Fetch<NodeDto>(template.Sql(entity.ParentId)).First();

                entity.Path = string.Concat(parent.Path, ",", entity.Id);
                entity.Level = parent.Level + 1;
                entity.SortOrder = GetNewChildSortOrder(entity.ParentId, 0);
            }

            // create the dto
            var dto = ContentFactory.BuildDto(entity);

            // update the node dto
            var nodeDto = dto.ContentDto.NodeDto;
            nodeDto.ValidatePathWithException();
            Database.Update(nodeDto);

            // update the content dto - only if the content type has changed
            var origContentDto = Database.Fetch<ContentDto>(SqlContext.Sql().Select<ContentDto>().From<ContentDto>().Where<ContentDto>(x => x.NodeId == entity.Id)).FirstOrDefault();
            if (origContentDto.ContentTypeId != entity.ContentTypeId)
            {
                dto.ContentDto.Id = origContentDto.Id; // fixme - annoying, needed?
                Database.Update(dto.ContentDto);
            }

            //If Published state has changed then previous versions should have their publish state reset.
            //If state has been changed to unpublished the previous versions publish state should also be reset.
            //if (((ICanBeDirty)entity).IsPropertyDirty("Published") && (entity.Published || publishedState == PublishedState.Unpublished))
            if (entity.RequiresClearPublishedFlag(publishedState, requiresNewVersion))
                ClearPublishedFlag(entity); // FIXME OUCH NO

            //Look up (newest) entries by id in cmsDocument table to set newest = false
            ClearNewestFlag(entity); // FIXME OUCH NO

            // insert or update the content & document version dtos
            var contentVersionDto = dto.DocumentVersionDto.ContentVersionDto; // fixme version id etc?
            contentVersionDto.Current = true;
            var documentVersionDto = dto.DocumentVersionDto;
            if (requiresNewVersion)
            {
                // assumes a new version id and date (modified date) have been set
                Database.Insert(contentVersionDto);
                Database.Insert(documentVersionDto);
            }
            else
            {
                // fixme this pk thing is annoying
                var id = Database.ExecuteScalar<int>(SqlContext.Sql().Select<ContentVersionDto>(x => x.Id).From<ContentVersionDto>().Where<ContentVersionDto>(x => x.VersionId == entity.Version));
                contentVersionDto.Id = id;
                Database.Update(contentVersionDto);
                documentVersionDto.Id = id;
                Database.Update(documentVersionDto);
            }

            // update the document dto
            Database.Update(dto);

            // update the property data
            var propertyDataDtos = PropertyFactory.BuildDtos(entity.Id, entity.Version, entity.Properties).ToArray();
            foreach (var propertyDataDto in propertyDataDtos)
            {
                if (requiresNewVersion == false && propertyDataDto.Id > 0)
                    Database.Update(propertyDataDto);
                else
                    Database.Insert(propertyDataDto);
            }

            // assign ids to properties, using propertyTypeId as a key
            var xids = propertyDataDtos.ToDictionary(x => x.PropertyTypeId, x => x.Id);
            foreach (var property in entity.Properties)
                property.Id = xids[property.PropertyTypeId];

            // update tags
            if (HasTagProperty(entity)) // fixme what-if it had and now has not?
            {
                switch (content.PublishedState)
                {
                    case PublishedState.Publishing:
                        // explicitely publishing, must update tags
                        UpdateEntityTags(entity, _tagRepository);
                        break;
                    case PublishedState.Unpublishing:
                        // explicitely unpublishing, must clear tags
                        ClearEntityTags(entity, _tagRepository);
                        break;
                    case PublishedState.Saving:
                        // saving, nothing to do
                        break;
                    case PublishedState.Published:
                    case PublishedState.Unpublished:
                        // no change, depends on path-published
                        // that should take care of trashing and un-trashing
                        if (IsPathPublished(entity)) // slightly expensive ;-(
                            UpdateEntityTags(entity, _tagRepository);
                        else
                            ClearEntityTags(entity, _tagRepository);
                        break;
                }
            }

            content.PublishedDate = dto.UpdateDate;
            OnUowRefreshedEntity(new UnitOfWorkEntityEventArgs(UnitOfWork, entity));

            entity.ResetDirtyProperties();
        }

        protected override void PersistDeletedItem(IContent entity)
        {
            // raise event first else potential FK issues
            OnUowRemovingEntity(new UnitOfWorkEntityEventArgs(UnitOfWork, entity));

            //We need to clear out all access rules but we need to do this in a manual way since
            // nothing in that table is joined to a content id
            var subQuery = SqlContext.Sql()
                .Select<AccessRuleDto>(x => x.AccessId)
                .From<AccessRuleDto>()
                .InnerJoin<AccessDto>()
                .On<AccessRuleDto, AccessDto>(left => left.AccessId, right => right.Id)
                .Where<AccessDto>(dto => dto.NodeId == entity.Id);
            Database.Execute(SqlContext.SqlSyntax.GetDeleteSubquery("umbracoAccessRule", "accessId", subQuery));

            //now let the normal delete clauses take care of everything else
            base.PersistDeletedItem(entity);
        }

        #endregion

        #region Content Repository

        // fixme could we at least document what's that supposed to do?
        public IEnumerable<IContent> GetByPublishedVersion(IQuery<IContent> query)
        {
            // we WANT to return contents in top-down order, ie parents should come before children
            // ideal would be pure xml "document order" - which we cannot achieve at database level

            var sqlClause = GetBaseQuery(QueryType.Many);
            var translator = new SqlTranslator<IContent>(sqlClause, query);
            var sql = translator.Translate()
                                .Where<DocumentDto>(x => x.Published)
                                .OrderBy<NodeDto>(x => x.Level)
                                .OrderBy<NodeDto>(x => x.SortOrder);

            return MapDtosToContent(Database.Fetch<DocumentDto>(sql), true);
        }

        public int CountPublished(string contentTypeAlias = null)
        {
            var sql = SqlContext.Sql();
            if (contentTypeAlias.IsNullOrWhiteSpace())
            {
                sql.SelectCount()
                    .From<NodeDto>()
                    .InnerJoin<DocumentDto>()
                    .On<NodeDto, DocumentDto>(left => left.NodeId, right => right.NodeId)
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId && x.Trashed == false)
                    .Where<DocumentDto>(x => x.Published);
            }
            else
            {
                sql.SelectCount()
                    .From<NodeDto>()
                    .InnerJoin<ContentDto>()
                    .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<DocumentDto>()
                    .On<NodeDto, DocumentDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<ContentTypeDto>()
                    .On<ContentTypeDto, ContentDto>(left => left.NodeId, right => right.ContentTypeId)
                    .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId && x.Trashed == false)
                    .Where<ContentTypeDto>(x => x.Alias == contentTypeAlias)
                    .Where<DocumentDto>(x => x.Published);
            }

            return Database.ExecuteScalar<int>(sql);
        }

        public void ReplaceContentPermissions(EntityPermissionSet permissionSet)
        {
            PermissionRepository.ReplaceEntityPermissions(permissionSet);
        }

        public void ClearPublishedFlag(IContent content)
        {
            var sql = "UPDATE cmsDocument SET published=0 WHERE nodeId=@id AND published=1";
            Database.Execute(sql, new { id = content.Id });
        }

        public void ClearNewestFlag(IContent content)
        {
            var sql = "UPDATE cmsDocument SET newest=0 WHERE nodeId=@id AND newest=1";
            Database.Execute(sql, new { id = content.Id });
        }

        /// <summary>
        /// Assigns a single permission to the current content item for the specified group ids
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="permission"></param>
        /// <param name="groupIds"></param>
        public void AssignEntityPermission(IContent entity, char permission, IEnumerable<int> groupIds)
        {
            PermissionRepository.AssignEntityPermission(entity, permission, groupIds);
        }

        public EntityPermissionCollection GetPermissionsForEntity(int entityId)
        {
            return PermissionRepository.GetPermissionsForEntity(entityId);
        }

        /// <summary>
        /// Used to add/update a permission for a content item
        /// </summary>
        /// <param name="permission"></param>
        public void AddOrUpdatePermissions(ContentPermissionSet permission)
        {
            PermissionRepository.AddOrUpdate(permission);
        }

        // fixme must refactor entirely, will not work!
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
        /// <param name="filter"></param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        public IEnumerable<IContent> GetPagedResultsByQuery(IQuery<IContent> query, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<IContent> filter = null, bool newest = true)
        {
            var filterSql = SqlContext.Sql();
            if (newest)
                filterSql.Append("AND (cmsDocument.newest = 1)");

            if (filter != null)
            {
                foreach (var filterClause in filter.GetWhereClauses())
                    filterSql.Append($"AND ({filterClause.Item1})", filterClause.Item2);
            }

            return GetPagedResultsByQuery<DocumentDto>(query, pageIndex, pageSize, out totalRecords,
                x => MapDtosToContent(x),
                orderBy, orderDirection, orderBySystemField, "cmsDocument",
                filterSql);
        }

        public bool IsPathPublished(IContent content)
        {
            // fail fast
            if (content.Path.StartsWith("-1,-20,"))
                return false;

            // succeed fast
            if (content.ParentId == -1)
                return content.HasPublishedVersion;

            var ids = content.Path.Split(',').Skip(1).Select(int.Parse);

            var sql = SqlContext.Sql()
                .SelectCount<NodeDto>(x => x.NodeId)
                .From<NodeDto>()
                .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((n, d) => n.NodeId == d.NodeId && d.Published)
                .WhereIn<NodeDto>(x => x.NodeId, ids);

            var count = Database.ExecuteScalar<int>(sql);
            return count == content.Level;
        }

        #endregion

        #region Recycle Bin

        protected override int RecycleBinId => Constants.System.RecycleBinContent;

        #endregion

        #region Read Repository implementation for Guid keys

        public IContent Get(Guid id)
        {
            return _contentByGuidReadRepository.Get(id);
        }

        IEnumerable<IContent> IReadRepository<Guid, IContent>.GetAll(params Guid[] ids)
        {
            return _contentByGuidReadRepository.GetAll(ids);
        }

        public bool Exists(Guid id)
        {
            return _contentByGuidReadRepository.Exists(id);
        }

        /// <summary>
        /// A reading repository purely for looking up by GUID
        /// </summary>
        /// <remarks>
        /// TODO: This is ugly and to fix we need to decouple the IRepositoryQueryable -> IRepository -> IReadRepository which should all be separate things!
        /// Then we can do the same thing with repository instances and we wouldn't need to leave all these methods as not implemented because we wouldn't need to implement them
        /// </remarks>
        private class ContentByGuidReadRepository : NPocoRepositoryBase<Guid, IContent>
        {
            private readonly ContentRepository _outerRepo;

            public ContentByGuidReadRepository(ContentRepository outerRepo, IScopeUnitOfWork work, CacheHelper cache, ILogger logger)
                : base(work, cache, logger)
            {
                _outerRepo = outerRepo;
            }

            protected override Guid NodeObjectTypeId => _outerRepo.NodeObjectTypeId;

            protected override IContent PerformGet(Guid id)
            {
                var sql = _outerRepo.GetBaseQuery(QueryType.Single)
                    .Where<NodeDto>(x => x.UniqueId == id);

                var dto = Database.Fetch<DocumentDto>(sql.SelectTop(1)).FirstOrDefault();

                if (dto == null)
                    return null;

                var content = _outerRepo.MapDtoToContent(dto);

                return content;
            }

            protected override IEnumerable<IContent> PerformGetAll(params Guid[] ids)
            {
                var sql = _outerRepo.GetBaseQuery(QueryType.Many);
                if (ids.Length > 0)
                    sql.WhereIn<NodeDto>(x => x.UniqueId, ids);

                return _outerRepo.MapDtosToContent(Database.Fetch<DocumentDto>(sql));
            }

            protected override IEnumerable<IContent> PerformGetByQuery(IQuery<IContent> query)
            {
                throw new WontImplementException();
            }

            protected override IEnumerable<string> GetDeleteClauses()
            {
                throw new WontImplementException();
            }

            protected override void PersistNewItem(IContent entity)
            {
                throw new WontImplementException();
            }

            protected override void PersistUpdatedItem(IContent entity)
            {
                throw new WontImplementException();
            }

            protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
            {
                throw new WontImplementException();
            }

            protected override string GetBaseWhereClause()
            {
                throw new WontImplementException();
            }
        }

        #endregion

        protected override string GetDatabaseFieldNameForOrderBy(string orderBy)
        {
            // NOTE see sortby.prevalues.controller.js for possible values
            // that need to be handled here or in VersionableRepositoryBase

            //Some custom ones
            switch (orderBy.ToUpperInvariant())
            {
                case "UPDATER":
                    // fixme This isn't going to work very nicely because it's going to order by ID, not by letter
                    return GetDatabaseFieldNameForOrderBy(Constants.DatabaseSchema.Tables.Document, "writerUserId");
                case "PUBLISHED":
                    // fixme is this still relevant?
                    return GetDatabaseFieldNameForOrderBy(Constants.DatabaseSchema.Tables.Document, "published");
                case "CONTENTTYPEALIAS":
                    throw new NotSupportedException("Don't know how to support ContentTypeAlias.");
            }

            return base.GetDatabaseFieldNameForOrderBy(orderBy);
        }

        // fixme - we are not implementing the double-query thing for pagination from 7.6, need to do it differently!

        private IEnumerable<IContent> MapDtosToContent(List<DocumentDto> dtos, bool withCache = false)
        {
            var temps = new List<TempContent<Content>>();
            var contentTypes = new Dictionary<int, IContentType>();
            var templateIds = new List<int>();

            var content = new Content[dtos.Count];

            for (var i = 0; i < dtos.Count; i++)
            {
                var dto = dtos[i];
                var versionId = dto.DocumentVersionDto.ContentVersionDto.VersionId;

                if (withCache)
                {
                    // if the cache contains the (proper version of the) item, use it
                    var cached = IsolatedCache.GetCacheItem<IContent>(GetCacheIdKey<IContent>(dto.NodeId));
                    if (cached != null && cached.Version == versionId)
                    {
                        content[i] = (Content) cached; // fixme should we just cache Content not IContent?
                        continue;
                    }
                }

                // else, need to build it

                // get the content type - the repository is full cache *but* still deep-clones
                // whatever comes out of it, so use our own local index here to avoid this
                var contentTypeId = dto.ContentDto.ContentTypeId;
                if (contentTypes.TryGetValue(contentTypeId, out var contentType) == false)
                    contentTypes[contentTypeId] = contentType = _contentTypeRepository.Get(contentTypeId);

                var c = content[i] = ContentFactory.BuildEntity(dto, contentType);

                // need template
                var templateId = dto.DocumentVersionDto.TemplateId;
                if (templateId.HasValue && templateId.Value > 0)
                    templateIds.Add(templateId.Value);

                // need properties
                temps.Add(new TempContent<Content>(dto.NodeId, versionId, contentType, c)
                {
                    TemplateId = dto.DocumentVersionDto.TemplateId
                });
            }

            // load all required templates in 1 query, and index
            var templates = _templateRepository.GetAll(templateIds.ToArray())
                .ToDictionary(x => x.Id, x => x);

            // load all properties for all documents from database in 1 query - indexed by version id
            var properties = GetPropertyCollections(temps);

            // assign templates and properties
            foreach (var temp in temps)
            {
                // complete the item
                if (temp.TemplateId.HasValue && templates.TryGetValue(temp.TemplateId.Value, out var template))
                    temp.Content.Template = template;
                temp.Content.Properties = properties[temp.VersionId];

                // reset dirty initial properties (U4-1946)
                temp.Content.ResetDirtyProperties(false);
            }

            return content;
        }

        private IContent MapDtoToContent(DocumentDto dto)
        {
            var contentType = _contentTypeRepository.Get(dto.ContentDto.ContentTypeId);
            var content = ContentFactory.BuildEntity(dto, contentType);

            // get template
            if (dto.DocumentVersionDto.TemplateId.HasValue && dto.DocumentVersionDto.TemplateId.Value > 0)
                content.Template = _templateRepository.Get(dto.DocumentVersionDto.TemplateId.Value);

            // get properties - indexed by version id
            var temp = new TempContent<Content>(dto.NodeId, dto.DocumentVersionDto.ContentVersionDto.VersionId, contentType);
            var properties = GetPropertyCollections(new List<TempContent<Content>> { temp });
            content.Properties = properties[dto.DocumentVersionDto.ContentVersionDto.VersionId];

            // clear dirty props on init - U4-1943
            content.ResetDirtyProperties(false);
            return content;
        }

        private string EnsureUniqueNodeName(int parentId, string nodeName, int id = 0)
        {
            if (EnsureUniqueNaming == false)
                return nodeName;

            var template = SqlContext.Templates.Get("Umbraco.Core.ContentRepository.EnsureUniqueNodeName", tsql => tsql
                .Select<NodeDto>(x => x.NodeId, x => x.Text)
                .From<NodeDto>()
                .Where<NodeDto>(x => x.NodeObjectType == SqlTemplate.Arg<Guid>("nodeObjectType") && x.ParentId == SqlTemplate.Arg<int>("parentId")));

            var sql = template.Sql(NodeObjectTypeId, parentId);
            var names = Database.Fetch<SimilarNodeName>(sql);

            return SimilarNodeName.GetUniqueName(names, id, nodeName);
        }

        private int GetNewChildSortOrder(int parentId, int first)
        {
            var template = SqlContext.Templates.Get("Umbraco.Core.ContentRepository.GetSortOrder", tsql =>
                tsql.Select($"COALESCE(MAX(sortOrder),{first - 1})").From<NodeDto>().Where<NodeDto>(x => x.NodeId == SqlTemplate.Arg<int>("parentId") && x.NodeObjectType == NodeObjectTypeId)
            );
            return Database.ExecuteScalar<int>(template.Sql(parentId)) + 1; // fixme can we mix named & non-named?
        }
    }
}
