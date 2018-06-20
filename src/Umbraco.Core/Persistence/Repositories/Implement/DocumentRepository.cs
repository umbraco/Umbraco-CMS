using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;
using static Umbraco.Core.Persistence.NPocoSqlExtensions.Statics;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IContent"/>.
    /// </summary>
    internal class DocumentRepository : ContentRepositoryBase<int, IContent, DocumentRepository>, IDocumentRepository
    {
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly ITagRepository _tagRepository;
        private readonly CacheHelper _cacheHelper;
        private PermissionRepository<IContent> _permissionRepository;
        private readonly ContentByGuidReadRepository _contentByGuidReadRepository;
        private readonly IScopeAccessor _scopeAccessor;

        public DocumentRepository(IScopeAccessor scopeAccessor, CacheHelper cacheHelper, ILogger logger, IContentTypeRepository contentTypeRepository, ITemplateRepository templateRepository, ITagRepository tagRepository, ILanguageRepository languageRepository, IContentSection settings)
            : base(scopeAccessor, cacheHelper, languageRepository, logger)
        {
            _contentTypeRepository = contentTypeRepository ?? throw new ArgumentNullException(nameof(contentTypeRepository));
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
            _cacheHelper = cacheHelper;
            _scopeAccessor = scopeAccessor;
            _contentByGuidReadRepository = new ContentByGuidReadRepository(this, scopeAccessor, cacheHelper, logger);
            EnsureUniqueNaming = settings.EnsureUniqueNaming;
        }

        protected override DocumentRepository This => this;

        public bool EnsureUniqueNaming { get; set; }

        // note: is ok to 'new' the repo here as it's a sub-repo really
        private PermissionRepository<IContent> PermissionRepository => _permissionRepository
            ?? (_permissionRepository = new PermissionRepository<IContent>(_scopeAccessor, _cacheHelper, Logger));

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

            switch (queryType)
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
                            r1.Select(documentVersionDto => documentVersionDto.ContentVersionDto))
                         .Select(documentDto => documentDto.PublishedVersionDto, "pdv", r1 =>
                            r1.Select(documentVersionDto => documentVersionDto.ContentVersionDto, "pcv")));
                    break;
            }

            sql
                .From<DocumentDto>()
                .InnerJoin<ContentDto>().On<DocumentDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)

                // inner join on mandatory edited version
                .InnerJoin<ContentVersionDto>().On<DocumentDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId)
                .InnerJoin<DocumentVersionDto>().On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id)

                // left join on optional published version
                .LeftJoin<ContentVersionDto>(nested =>
                        nested.InnerJoin<DocumentVersionDto>("pdv").On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id && right.Published, "pcv", "pdv"), "pcv")
                    .On<DocumentDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId, aliasRight: "pcv");

            sql
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

            // this would ensure we don't get the published version - keep for reference
            //sql
            //    .WhereAny(
            //        x => x.Where<ContentVersionDto, ContentVersionDto>((x1, x2) => x1.Id != x2.Id, alias2: "pcv"),
            //        x => x.WhereNull<ContentVersionDto>(x1 => x1.Id, "pcv")
            //    );

            if (current)
                sql.Where<ContentVersionDto>(x => x.Current); // always get the current version

            return sql;
        }

        // fixme - kill, eventually
        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            return GetBaseQuery(isCount ? QueryType.Count : QueryType.Single);
        }

        // fixme - kill, eventually
        // ah maybe not, that what's used for eg Exists in base repo
        protected override string GetBaseWhereClause()
        {
            return $"{Constants.DatabaseSchema.Tables.Node}.id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
            {
                "DELETE FROM " + Constants.DatabaseSchema.Tables.RedirectUrl + " WHERE contentKey IN (SELECT uniqueId FROM " + Constants.DatabaseSchema.Tables.Node + " WHERE id = @id)",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.Task + " WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.User2NodeNotify + " WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.UserGroup2NodePermission + " WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.UserStartNode + " WHERE startNode = @id",
                "UPDATE " + Constants.DatabaseSchema.Tables.UserGroup + " SET startContentId = NULL WHERE startContentId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.Relation + " WHERE parentId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.Relation + " WHERE childId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.TagRelationship + " WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.Domain + " WHERE domainRootStructureID = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.Document + " WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.DocumentCultureVariation + " WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.DocumentVersion + " WHERE id IN (SELECT id FROM " + Constants.DatabaseSchema.Tables.ContentVersion + " WHERE nodeId = @id)",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.PropertyData + " WHERE versionId IN (SELECT id FROM " + Constants.DatabaseSchema.Tables.ContentVersion + " WHERE nodeId = @id)",
                "DELETE FROM cmsPreviewXml WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.ContentVersionCultureVariation + " WHERE versionId IN (SELECT id FROM " + Constants.DatabaseSchema.Tables.ContentVersion + " WHERE nodeId = @id)",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.ContentVersion + " WHERE nodeId = @id",
                "DELETE FROM cmsContentXml WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.Content + " WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.Access + " WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.Node + " WHERE id = @id"
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

        public override IContent GetVersion(int versionId)
        {
            var sql = GetBaseQuery(QueryType.Single, false)
                .Where<ContentVersionDto>(x => x.Id == versionId);

            var dto = Database.Fetch<DocumentDto>(sql).FirstOrDefault();
            return dto == null ? null : MapDtoToContent(dto);
        }

        protected override void PerformDeleteVersion(int id, int versionId)
        {
            // raise event first else potential FK issues
            OnUowRemovingVersion(new ScopedVersionEventArgs(AmbientScope, id, versionId));

            // fixme - syntax + ...
            Database.Delete<PropertyDataDto>("WHERE versionId = @versionId", new { versionId });
            Database.Delete<ContentVersionDto>("WHERE id = @versionId", new { versionId });
            Database.Delete<DocumentVersionDto>("WHERE id = @versionId", new { versionId });
        }

        #endregion

        #region Persist

        protected override void PersistNewItem(IContent entity)
        {
            // fixme - stop doing this - sort out IContent vs Content
            // however, it's not just so we have access to AddingEntity
            // there are tons of things at the end of the methods, that can only work with a true Content
            // and basically, the repository requires a Content, not an IContent
            var content = (Content) entity;

            content.AddingEntity();
            var publishing = content.PublishedState == PublishedState.Publishing;

            // ensure that the default template is assigned
            if (entity.Template == null)
                entity.Template = entity.ContentType.DefaultTemplate;

            // sanitize names
            SanitizeNames(content, publishing);

            // ensure that strings don't contain characters that are invalid in xml
            // fixme - do we really want to keep doing this here?
            entity.SanitizeEntityPropertiesForXmlStorage();

            // create the dto
            var dto = ContentBaseFactory.BuildDto(entity, NodeObjectTypeId);

            // derive path and level from parent
            var parent = GetParentNodeDto(entity.ParentId);
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
            var id = GetReservedId(nodeDto.UniqueId);
            if (id > 0)
                nodeDto.NodeId = id;
            else
                Database.Insert(nodeDto);

            nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
            nodeDto.ValidatePathWithException();
            Database.Update(nodeDto);

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
            var contentVersionDto = dto.DocumentVersionDto.ContentVersionDto;
            contentVersionDto.NodeId = nodeDto.NodeId;
            contentVersionDto.Current = !publishing;
            contentVersionDto.Text = content.Name;
            Database.Insert(contentVersionDto);
            content.VersionId = contentVersionDto.Id;

            // persist the document version dto
            var documentVersionDto = dto.DocumentVersionDto;
            documentVersionDto.Id = content.VersionId;
            if (publishing)
                documentVersionDto.Published = true;
            Database.Insert(documentVersionDto);

            // and again in case we're publishing immediately
            if (publishing)
            {
                content.PublishedVersionId = content.VersionId;
                contentVersionDto.Id = 0;
                contentVersionDto.Current = true;
                contentVersionDto.Text = content.Name;
                Database.Insert(contentVersionDto);
                content.VersionId = contentVersionDto.Id;

                documentVersionDto.Id = content.VersionId;
                documentVersionDto.Published = false;
                Database.Insert(documentVersionDto);
            }

            // persist the property data
            var propertyDataDtos = PropertyFactory.BuildDtos(content.VersionId, content.PublishedVersionId, entity.Properties, LanguageRepository, out var edited, out var editedCultures);
            foreach (var propertyDataDto in propertyDataDtos)
                Database.Insert(propertyDataDto);

            // if !publishing, we may have a new name != current publish name,
            // also impacts 'edited'
            if (content.PublishName != content.Name)
                edited = true;

            // persist the document dto
            // at that point, when publishing, the entity still has its old Published value
            // so we need to explicitely update the dto to persist the correct value
            if (content.PublishedState == PublishedState.Publishing)
                dto.Published = true;
            dto.NodeId = nodeDto.NodeId;
            content.Edited = dto.Edited = !dto.Published || edited; // if not published, always edited
            Database.Insert(dto);

            // persist the variations
            if (content.ContentType.VariesByCulture())
            {
                // names also impact 'edited'
                foreach (var (culture, name) in content.CultureNames)
                    if (name != content.GetPublishName(culture))
                        (editedCultures ?? (editedCultures = new HashSet<string>(StringComparer.OrdinalIgnoreCase))).Add(culture);

                // insert content variations
                Database.BulkInsertRecords(GetContentVariationDtos(content, publishing));

                // insert document variations
                Database.BulkInsertRecords(GetDocumentVariationDtos(content, publishing, editedCultures));
            }

            // refresh content
            content.SetCultureEdited(editedCultures);

            // trigger here, before we reset Published etc
            OnUowRefreshedEntity(new ScopedEntityEventArgs(AmbientScope, entity));

            // flip the entity's published property
            // this also flips its published state
            // note: what depends on variations (eg PublishNames) is managed directly by the content
            if (content.PublishedState == PublishedState.Publishing)
            {
                content.Published = true;
                content.PublishTemplate = content.Template;
                content.PublisherId = content.WriterId;
                content.PublishName = content.Name;
                content.PublishDate = content.UpdateDate;

                SetEntityTags(entity, _tagRepository);
            }
            else if (content.PublishedState == PublishedState.Unpublishing)
            {
                content.Published = false;
                content.PublishTemplate = null;
                content.PublisherId = null;
                content.PublishName = null;
                content.PublishDate = null;

                ClearEntityTags(entity, _tagRepository);
            }

            entity.ResetDirtyProperties();

            // troubleshooting
            //if (Database.ExecuteScalar<int>($"SELECT COUNT(*) FROM {Constants.DatabaseSchema.Tables.DocumentVersion} JOIN {Constants.DatabaseSchema.Tables.ContentVersion} ON {Constants.DatabaseSchema.Tables.DocumentVersion}.id={Constants.DatabaseSchema.Tables.ContentVersion}.id WHERE published=1 AND nodeId=" + content.Id) > 1)
            //{
            //    Debugger.Break();
            //    throw new Exception("oops");
            //}
            //if (Database.ExecuteScalar<int>($"SELECT COUNT(*) FROM {Constants.DatabaseSchema.Tables.DocumentVersion} JOIN {Constants.DatabaseSchema.Tables.ContentVersion} ON {Constants.DatabaseSchema.Tables.DocumentVersion}.id={Constants.DatabaseSchema.Tables.ContentVersion}.id WHERE [current]=1 AND nodeId=" + content.Id) > 1)
            //{
            //    Debugger.Break();
            //    throw new Exception("oops");
            //}
        }

        protected override void PersistUpdatedItem(IContent entity)
        {
            // fixme - stop doing this - sort out IContent vs Content
            // however, it's not just so we have access to AddingEntity
            // there are tons of things at the end of the methods, that can only work with a true Content
            // and basically, the repository requires a Content, not an IContent
            var content = (Content) entity;

            // check if we need to make any database changes at all
            if ((content.PublishedState == PublishedState.Published || content.PublishedState == PublishedState.Unpublished) && !content.IsEntityDirty() && !content.IsAnyUserPropertyDirty())
                return; // no change to save, do nothing, don't even update dates

            // whatever we do, we must check that we are saving the current version
            // fixme maybe we can just fetch Current (bool)
            var version = Database.Fetch<ContentVersionDto>(SqlContext.Sql().Select<ContentVersionDto>().From<ContentVersionDto>().Where<ContentVersionDto>(x => x.Id == content.VersionId)).FirstOrDefault();
            if (version == null || !version.Current )
                throw new InvalidOperationException("Cannot save a non-current version.");

            // update
            content.UpdatingEntity();
            var publishing = content.PublishedState == PublishedState.Publishing;

            // check if we need to create a new version
            if (publishing && content.PublishedVersionId > 0)
            {
                // published version is not published anymore
                Database.Execute(Sql().Update<DocumentVersionDto>(u => u.Set(x => x.Published, false)).Where<DocumentVersionDto>(x => x.Id == content.PublishedVersionId));
            }

            // sanitize names
            SanitizeNames(content, publishing);

            // ensure that strings don't contain characters that are invalid in xml
            // fixme - do we really want to keep doing this here?
            entity.SanitizeEntityPropertiesForXmlStorage();

            // if parent has changed, get path, level and sort order
            if (entity.IsPropertyDirty("ParentId"))
            {
                var parent = GetParentNodeDto(entity.ParentId);
                entity.Path = string.Concat(parent.Path, ",", entity.Id);
                entity.Level = parent.Level + 1;
                entity.SortOrder = GetNewChildSortOrder(entity.ParentId, 0);
            }

            // create the dto
            var dto = ContentBaseFactory.BuildDto(entity, NodeObjectTypeId);

            // update the node dto
            var nodeDto = dto.ContentDto.NodeDto;
            nodeDto.ValidatePathWithException();
            Database.Update(nodeDto);

            // update the content dto
            Database.Update(dto.ContentDto);

            // update the content & document version dtos
            var contentVersionDto = dto.DocumentVersionDto.ContentVersionDto;
            var documentVersionDto = dto.DocumentVersionDto;
            if (publishing)
            {
                documentVersionDto.Published = true; // now published
                contentVersionDto.Current = false; // no more current
                contentVersionDto.Text = content.Name;
            }
            Database.Update(contentVersionDto);
            Database.Update(documentVersionDto);

            // and, if publishing, insert new content & document version dtos
            if (publishing)
            {
                content.PublishedVersionId = content.VersionId;

                contentVersionDto.Id = 0; // want a new id
                contentVersionDto.Current = true; // current version
                contentVersionDto.Text = content.Name;
                Database.Insert(contentVersionDto);
                content.VersionId = documentVersionDto.Id = contentVersionDto.Id; // get the new id

                documentVersionDto.Published = false; // non-published version
                Database.Insert(documentVersionDto);
            }

            // replace the property data (rather than updating)
            // only need to delete for the version that existed, the new version (if any) has no property data yet
            var versionToDelete = publishing ? content.PublishedVersionId : content.VersionId;
            var deletePropertyDataSql = Sql().Delete<PropertyDataDto>().Where<PropertyDataDto>(x => x.VersionId == versionToDelete);
            Database.Execute(deletePropertyDataSql);

            // insert property data
            var propertyDataDtos = PropertyFactory.BuildDtos(content.VersionId, publishing ? content.PublishedVersionId : 0, entity.Properties, LanguageRepository, out var edited, out var editedCultures);
            foreach (var propertyDataDto in propertyDataDtos)
                Database.Insert(propertyDataDto);

            // if !publishing, we may have a new name != current publish name,
            // also impacts 'edited'
            if (!publishing && content.PublishName != content.Name)
                edited = true;

            if (content.ContentType.VariesByCulture())
            {
                // names also impact 'edited'
                foreach (var (culture, name) in content.CultureNames)
                    if (name != content.GetPublishName(culture))
                        (editedCultures ?? (editedCultures = new HashSet<string>(StringComparer.OrdinalIgnoreCase))).Add(culture);

                // replace the content version variations (rather than updating)
                // only need to delete for the version that existed, the new version (if any) has no property data yet
                var deleteContentVariations = Sql().Delete<ContentVersionCultureVariationDto>().Where<ContentVersionCultureVariationDto>(x => x.VersionId == versionToDelete);
                Database.Execute(deleteContentVariations);

                // replace the document version variations (rather than updating)
                var deleteDocumentVariations = Sql().Delete<DocumentCultureVariationDto>().Where<DocumentCultureVariationDto>(x => x.NodeId == content.Id);
                Database.Execute(deleteDocumentVariations);

                // fixme is we'd like to use the native NPoco InsertBulk here but it causes problems (not sure exaclty all scenarios)
                // but by using SQL Server and updating a variants name will cause: Unable to cast object of type
                // 'Umbraco.Core.Persistence.FaultHandling.RetryDbConnection' to type 'System.Data.SqlClient.SqlConnection'.
                // (same in PersistNewItem above)

                // insert content variations
                Database.BulkInsertRecords(GetContentVariationDtos(content, publishing));

                // insert document variations
                Database.BulkInsertRecords(GetDocumentVariationDtos(content, publishing, editedCultures));
            }

            // refresh content
            content.SetCultureEdited(editedCultures);

            // update the document dto
            // at that point, when un/publishing, the entity still has its old Published value
            // so we need to explicitely update the dto to persist the correct value
            if (content.PublishedState == PublishedState.Publishing)
                dto.Published = true;
            else if (content.PublishedState == PublishedState.Unpublishing)
                dto.Published = false;
            content.Edited = dto.Edited = !dto.Published || edited; // if not published, always edited
            Database.Update(dto);

            // if entity is publishing, update tags, else leave tags there
            // means that implicitely unpublished, or trashed, entities *still* have tags in db
            if (content.PublishedState == PublishedState.Publishing)
                SetEntityTags(entity, _tagRepository);

            // trigger here, before we reset Published etc
            OnUowRefreshedEntity(new ScopedEntityEventArgs(AmbientScope, entity));

            // flip the entity's published property
            // this also flips its published state
            if (content.PublishedState == PublishedState.Publishing)
            {
                content.Published = true;
                content.PublishTemplate = content.Template;
                content.PublisherId = content.WriterId;
                content.PublishName = content.Name;
                content.PublishDate = content.UpdateDate;

                SetEntityTags(entity, _tagRepository);
            }
            else if (content.PublishedState == PublishedState.Unpublishing)
            {
                content.Published = false;
                content.PublishTemplate = null;
                content.PublisherId = null;
                content.PublishName = null;
                content.PublishDate = null;

                ClearEntityTags(entity, _tagRepository);
            }

            // note re. tags: explicitely unpublished entities have cleared tags,
            // but masked or trashed entitites *still* have tags in the db fixme so what?

            entity.ResetDirtyProperties();

            // troubleshooting
            //if (Database.ExecuteScalar<int>($"SELECT COUNT(*) FROM {Constants.DatabaseSchema.Tables.DocumentVersion} JOIN {Constants.DatabaseSchema.Tables.ContentVersion} ON {Constants.DatabaseSchema.Tables.DocumentVersion}.id={Constants.DatabaseSchema.Tables.ContentVersion}.id WHERE published=1 AND nodeId=" + content.Id) > 1)
            //{
            //    Debugger.Break();
            //    throw new Exception("oops");
            //}
            //if (Database.ExecuteScalar<int>($"SELECT COUNT(*) FROM {Constants.DatabaseSchema.Tables.DocumentVersion} JOIN {Constants.DatabaseSchema.Tables.ContentVersion} ON {Constants.DatabaseSchema.Tables.DocumentVersion}.id={Constants.DatabaseSchema.Tables.ContentVersion}.id WHERE [current]=1 AND nodeId=" + content.Id) > 1)
            //{
            //    Debugger.Break();
            //    throw new Exception("oops");
            //}
        }

        protected override void PersistDeletedItem(IContent entity)
        {
            // raise event first else potential FK issues
            OnUowRemovingEntity(new ScopedEntityEventArgs(AmbientScope, entity));

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
            PermissionRepository.Save(permission);
        }

        /// <summary>
        /// Gets paged content results.
        /// </summary>
        public override IEnumerable<IContent> GetPage(IQuery<IContent> query,
            long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, bool orderBySystemField,
            IQuery<IContent> filter = null)
        {
            Sql<ISqlContext> filterSql = null;

            if (filter != null)
            {
                filterSql = Sql();
                foreach (var filterClause in filter.GetWhereClauses())
                    filterSql.Append($"AND ({filterClause.Item1})", filterClause.Item2);
            }

            return GetPage<DocumentDto>(query, pageIndex, pageSize, out totalRecords,
                x => MapDtosToContent(x),
                orderBy, orderDirection, orderBySystemField,
                filterSql);
        }

        public bool IsPathPublished(IContent content)
        {
            // fail fast
            if (content.Path.StartsWith("-1,-20,"))
                return false;

            // succeed fast
            if (content.ParentId == -1)
                return content.Published;

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

        public override int RecycleBinId => Constants.System.RecycleBinContent;

        #endregion

        #region Read Repository implementation for Guid keys

        public IContent Get(Guid id)
        {
            return _contentByGuidReadRepository.Get(id);
        }

        IEnumerable<IContent> IReadRepository<Guid, IContent>.GetMany(params Guid[] ids)
        {
            return _contentByGuidReadRepository.GetMany(ids);
        }

        public bool Exists(Guid id)
        {
            return _contentByGuidReadRepository.Exists(id);
        }

        // reading repository purely for looking up by GUID
        // fixme - ugly and to fix we need to decouple the IRepositoryQueryable -> IRepository -> IReadRepository which should all be separate things!
        private class ContentByGuidReadRepository : NPocoRepositoryBase<Guid, IContent>
        {
            private readonly DocumentRepository _outerRepo;

            public ContentByGuidReadRepository(DocumentRepository outerRepo, IScopeAccessor scopeAccessor, CacheHelper cache, ILogger logger)
                : base(scopeAccessor, cache, logger)
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
                    // fixme orders by id not letter = bad
                    return GetDatabaseFieldNameForOrderBy(Constants.DatabaseSchema.Tables.ContentVersion, "userId");
                case "PUBLISHED":
                    // fixme kill
                    return GetDatabaseFieldNameForOrderBy(Constants.DatabaseSchema.Tables.Document, "published");
                case "CONTENTTYPEALIAS":
                    throw new NotSupportedException("Don't know how to support ContentTypeAlias.");
            }

            return base.GetDatabaseFieldNameForOrderBy(orderBy);
        }

        private IEnumerable<IContent> MapDtosToContent(List<DocumentDto> dtos, bool withCache = false)
        {
            var temps = new List<TempContent<Content>>();
            var contentTypes = new Dictionary<int, IContentType>();
            var templateIds = new List<int>();

            var content = new Content[dtos.Count];

            for (var i = 0; i < dtos.Count; i++)
            {
                var dto = dtos[i];

                if (withCache)
                {
                    // if the cache contains the (proper version of the) item, use it
                    var cached = IsolatedCache.GetCacheItem<IContent>(RepositoryCacheKeys.GetKey<IContent>(dto.NodeId));
                    if (cached != null && cached.VersionId == dto.DocumentVersionDto.ContentVersionDto.Id)
                    {
                        content[i] = (Content) cached;
                        continue;
                    }
                }

                // else, need to build it

                // get the content type - the repository is full cache *but* still deep-clones
                // whatever comes out of it, so use our own local index here to avoid this
                var contentTypeId = dto.ContentDto.ContentTypeId;
                if (contentTypes.TryGetValue(contentTypeId, out var contentType) == false)
                    contentTypes[contentTypeId] = contentType = _contentTypeRepository.Get(contentTypeId);

                var c = content[i] = ContentBaseFactory.BuildEntity(dto, contentType);

                // need templates
                var templateId = dto.DocumentVersionDto.TemplateId;
                if (templateId.HasValue && templateId.Value > 0)
                    templateIds.Add(templateId.Value);
                if (dto.Published)
                {
                    templateId = dto.PublishedVersionDto.TemplateId;
                    if (templateId.HasValue && templateId.Value > 0)
                        templateIds.Add(templateId.Value);
                }

                // need properties
                var versionId = dto.DocumentVersionDto.Id;
                var publishedVersionId = dto.Published ? dto.PublishedVersionDto.Id : 0;
                var temp = new TempContent<Content>(dto.NodeId, versionId, publishedVersionId, contentType, c)
                {
                    Template1Id = dto.DocumentVersionDto.TemplateId
                };
                if (dto.Published) temp.Template2Id = dto.PublishedVersionDto.TemplateId;
                temps.Add(temp);
            }

            // load all required templates in 1 query, and index
            var templates = _templateRepository.GetMany(templateIds.ToArray())
                .ToDictionary(x => x.Id, x => x);

            // load all properties for all documents from database in 1 query - indexed by version id
            var properties = GetPropertyCollections(temps);

            // assign templates and properties
            foreach (var temp in temps)
            {
                // complete the item
                if (temp.Template1Id.HasValue && templates.TryGetValue(temp.Template1Id.Value, out var template))
                    temp.Content.Template = template;
                if (temp.Template2Id.HasValue && templates.TryGetValue(temp.Template2Id.Value, out template))
                    temp.Content.PublishTemplate = template;
                temp.Content.Properties = properties[temp.VersionId];

                // reset dirty initial properties (U4-1946)
                temp.Content.ResetDirtyProperties(false);
            }

            // set variations, if varying
            temps = temps.Where(x => x.ContentType.VariesByCulture()).ToList();
            if (temps.Count > 0)
            {
                // load all variations for all documents from database, in one query
                var contentVariations = GetContentVariations(temps);
                var documentVariations = GetDocumentVariations(temps);
                foreach (var temp in temps)
                    SetVariations(temp.Content, contentVariations, documentVariations);
            }

            return content;
        }

        private IContent MapDtoToContent(DocumentDto dto)
        {
            var contentType = _contentTypeRepository.Get(dto.ContentDto.ContentTypeId);
            var content = ContentBaseFactory.BuildEntity(dto, contentType);

            // get template
            if (dto.DocumentVersionDto.TemplateId.HasValue && dto.DocumentVersionDto.TemplateId.Value > 0)
                content.Template = _templateRepository.Get(dto.DocumentVersionDto.TemplateId.Value);

            // get properties - indexed by version id
            var versionId = dto.DocumentVersionDto.Id;

            // fixme - shall we get published properties or not?
            //var publishedVersionId = dto.Published ? dto.PublishedVersionDto.Id : 0;
            var publishedVersionId = dto.PublishedVersionDto != null ? dto.PublishedVersionDto.Id : 0;

            var temp = new TempContent<Content>(dto.NodeId, versionId, publishedVersionId, contentType);
            var ltemp = new List<TempContent<Content>> { temp };
            var properties = GetPropertyCollections(ltemp);
            content.Properties = properties[dto.DocumentVersionDto.Id];

            // set variations, if varying
            if (contentType.VariesByCulture())
            {
                var contentVariations = GetContentVariations(ltemp);
                var documentVariations = GetDocumentVariations(ltemp);
                SetVariations(content, contentVariations, documentVariations);
            }

            // reset dirty initial properties (U4-1946)
            content.ResetDirtyProperties(false);
            return content;
        }

        private void SetVariations(Content content, IDictionary<int, List<ContentVariation>> contentVariations, IDictionary<int, List<DocumentVariation>> documentVariations)
        {
            if (contentVariations.TryGetValue(content.VersionId, out var contentVariation))
                foreach (var v in contentVariation)
                    content.SetCultureInfo(v.Culture, v.Name, v.Date);
            if (content.PublishedVersionId > 0 && contentVariations.TryGetValue(content.PublishedVersionId, out contentVariation))
                foreach (var v in contentVariation)
                    content.SetPublishInfo(v.Culture, v.Name, v.Date);
            if (documentVariations.TryGetValue(content.Id, out var documentVariation))
                foreach (var v in documentVariation.Where(x => x.Edited))
                    content.SetCultureEdited(v.Culture);
        }

        private IDictionary<int, List<ContentVariation>> GetContentVariations<T>(List<TempContent<T>> temps)
            where T : class, IContentBase
        {
            var versions = new List<int>();
            foreach (var temp in temps)
            {
                versions.Add(temp.VersionId);
                if (temp.PublishedVersionId > 0)
                    versions.Add(temp.PublishedVersionId);
            }
            if (versions.Count == 0) return new Dictionary<int, List<ContentVariation>>();

            var dtos = Database.FetchByGroups<ContentVersionCultureVariationDto, int>(versions, 2000, batch
                => Sql()
                    .Select<ContentVersionCultureVariationDto>()
                    .From<ContentVersionCultureVariationDto>()
                    .WhereIn<ContentVersionCultureVariationDto>(x => x.VersionId, batch));

            var variations = new Dictionary<int, List<ContentVariation>>();

            foreach (var dto in dtos)
            {
                if (!variations.TryGetValue(dto.VersionId, out var variation))
                    variations[dto.VersionId] = variation = new List<ContentVariation>();

                variation.Add(new ContentVariation
                {
                    Culture = LanguageRepository.GetIsoCodeById(dto.LanguageId),
                    Name = dto.Name,
                    Date = dto.Date
                });
            }

            return variations;
        }

        private IDictionary<int, List<DocumentVariation>> GetDocumentVariations<T>(List<TempContent<T>> temps)
            where T : class, IContentBase
        {
            var ids = temps.Select(x => x.Id);

            var dtos = Database.FetchByGroups<DocumentCultureVariationDto, int>(ids, 2000, batch =>
                Sql()
                    .Select<DocumentCultureVariationDto>()
                    .From<DocumentCultureVariationDto>()
                    .WhereIn<DocumentCultureVariationDto>(x => x.NodeId, batch));

            var variations = new Dictionary<int, List<DocumentVariation>>();

            foreach (var dto in dtos)
            {
                if (!variations.TryGetValue(dto.NodeId, out var variation))
                    variations[dto.NodeId] = variation = new List<DocumentVariation>();

                variation.Add(new DocumentVariation
                {
                    Culture = LanguageRepository.GetIsoCodeById(dto.LanguageId),
                    Edited = dto.Edited
                });
            }

            return variations;
        }

        private IEnumerable<ContentVersionCultureVariationDto> GetContentVariationDtos(IContent content, bool publishing)
        {
            // create dtos for the 'current' (non-published) version, all cultures
            foreach (var (culture, name) in content.CultureNames)
                yield return new ContentVersionCultureVariationDto
                {
                    VersionId = content.VersionId,
                    LanguageId = LanguageRepository.GetIdByIsoCode(culture) ?? throw new InvalidOperationException("Not a valid culture."),
                    Culture = culture,
                    Name = name,
                    Date = content.GetCultureDate(culture) ?? DateTime.MinValue // we *know* there is a value
                };

            // if not publishing, we're just updating the 'current' (non-published) version,
            // so there are no DTOs to create for the 'published' version which remains unchanged
            if (!publishing) yield break;

            // create dtos for the 'published' version, for published cultures (those having a name)
            foreach (var (culture, name) in content.PublishNames)
                yield return new ContentVersionCultureVariationDto
                {
                    VersionId = content.PublishedVersionId,
                    LanguageId = LanguageRepository.GetIdByIsoCode(culture) ?? throw new InvalidOperationException("Not a valid culture."),
                    Culture = culture,
                    Name = name,
                    Date = content.GetPublishDate(culture) ?? DateTime.MinValue // we *know* there is a value
                };
        }

        private IEnumerable<DocumentCultureVariationDto> GetDocumentVariationDtos(IContent content, bool publishing, HashSet<string> editedCultures)
        {
            foreach (var (culture, name) in content.CultureNames)
                yield return new DocumentCultureVariationDto
                {
                    NodeId = content.Id,
                    LanguageId = LanguageRepository.GetIdByIsoCode(culture) ?? throw new InvalidOperationException("Not a valid culture."),
                    Culture = culture,
                    Edited = !content.IsCulturePublished(culture) || (editedCultures != null && editedCultures.Contains(culture)) // if not published, always edited
                };
        }

        private class ContentVariation
        {
            public string Culture { get; set; }
            public string Name { get; set; }
            public DateTime Date { get; set; }
        }

        private class DocumentVariation
        {
            public string Culture { get; set; }
            public bool Edited { get; set; }
        }

        #region Utilities

        private void SanitizeNames(Content content, bool publishing)
        {
            // a content item *must* have an invariant name, and invariant published name
            // else we just cannot write the invariant rows (node, content version...) to the database

            // ensure that we have an invariant name
            // invariant content = must be there already, else throw
            // variant content = update with default culture or anything really
            EnsureInvariantNameExists(content);

            // ensure that that invariant name is unique
            EnsureInvariantNameIsUnique(content);

            // now that we have an invariant name, which is unique,
            // if publishing, ensure that we have an invariant publish name
            // invariant content = must be there, else throw - then must follow the invariant name
            // variant content = update with invariant name
            // fixme wtf is this we never needed it, PublishName derives from Name when publishing!
            //if (publishing) EnsureInvariantPublishName(content);

            // and finally,
            // ensure that each culture has a unique node name
            // no published name = not published
            // else, it needs to be unique
            EnsureVariantNamesAreUnique(content, publishing);
        }

        private void EnsureInvariantNameExists(Content content)
        {
            if (content.ContentType.VariesByCulture())
            {
                // content varies by culture
                // then it must have at least a variant name, else it makes no sense
                if (content.CultureNames.Count == 0)
                    throw new InvalidOperationException("Cannot save content with an empty name.");

                // and then, we need to set the invariant name implicitely,
                // using the default culture if it has a name, otherwise anything we can
                var defaultCulture = LanguageRepository.GetDefaultIsoCode();
                content.Name = defaultCulture != null && content.CultureNames.TryGetValue(defaultCulture, out var cultureName)
                    ? cultureName
                    : content.CultureNames.First().Value;
            }
            else
            {
                // content is invariant, and invariant content must have an explicit invariant name
                if (string.IsNullOrWhiteSpace(content.Name))
                    throw new InvalidOperationException("Cannot save content with an empty name.");
            }
        }

        private void EnsureInvariantNameIsUnique(Content content)
        {
            content.Name = EnsureUniqueNodeName(content.ParentId, content.Name, content.Id);
        }

        //private void EnsureInvariantPublishName(Content content)
        //{
        //    if (content.ContentType.VariesByCulture())
        //    {
        //        // content varies by culture, reuse name as publish name
        //        content.UpdatePublishName(null, content.Name);
        //    }
        //    else
        //    {
        //        // content is invariant, and invariant content must have an explicit invariant name
        //        if (string.IsNullOrWhiteSpace(content.PublishName))
        //            throw new InvalidOperationException("Cannot save content with an empty name.");
        //        // and then, must follow the name itself
        //        if (content.PublishName != content.Name)
        //            content.UpdatePublishName(null, content.Name);
        //    }
        //}

        protected override string EnsureUniqueNodeName(int parentId, string nodeName, int id = 0)
        {
            return EnsureUniqueNaming == false ? nodeName : base.EnsureUniqueNodeName(parentId, nodeName, id);
        }

        private SqlTemplate SqlEnsureVariantNamesAreUnique => SqlContext.Templates.Get("Umbraco.Core.DomainRepository.EnsureVariantNamesAreUnique", tsql => tsql
            .Select<ContentVersionCultureVariationDto>(x => x.Id, x => x.Name, x => x.LanguageId)
            .From<ContentVersionCultureVariationDto>()
            .InnerJoin<ContentVersionDto>().On<ContentVersionDto, ContentVersionCultureVariationDto>(x => x.Id, x => x.VersionId)
            .InnerJoin<NodeDto>().On<NodeDto, ContentVersionDto>(x => x.NodeId, x => x.NodeId)
            .Where<ContentVersionDto>(x => x.Current == SqlTemplate.Arg<bool>("current"))
            .Where<NodeDto>(x => x.NodeObjectType == SqlTemplate.Arg<Guid>("nodeObjectType") &&
                                 x.ParentId == SqlTemplate.Arg<int>("parentId") &&
                                 x.NodeId != SqlTemplate.Arg<int>("id"))
            .OrderBy<ContentVersionCultureVariationDto>(x => x.LanguageId));

        private void EnsureVariantNamesAreUnique(Content content, bool publishing)
        {
            if (!EnsureUniqueNaming || !content.ContentType.VariesByCulture() || content.CultureNames.Count == 0) return;

            // get names per culture, at same level (ie all siblings)
            var sql = SqlEnsureVariantNamesAreUnique.Sql(true, NodeObjectTypeId, content.ParentId, content.Id);
            var names = Database.Fetch<CultureNodeName>(sql)
                .GroupBy(x => x.LanguageId)
                .ToDictionary(x => x.Key, x => x);

            if (names.Count == 0) return;

            foreach(var (culture, name) in content.CultureNames)
            {
                var langId = LanguageRepository.GetIdByIsoCode(culture);
                if (!langId.HasValue) continue;
                if (!names.TryGetValue(langId.Value, out var cultureNames)) continue;

                // get a unique name
                var otherNames = cultureNames.Select(x => new SimilarNodeName { Id = x.Id, Name = x.Name });
                var uniqueName = SimilarNodeName.GetUniqueName(otherNames, 0, name);

                if (uniqueName == content.GetCultureName(culture)) continue;

                // update the name, and the publish name if published
                content.SetCultureName(uniqueName, culture);
                if (publishing && content.PublishNames.ContainsKey(culture)) // fixme but what about those cultures we are NOT publishing NOW?! they shouldn't change their name!
                    content.SetPublishInfo(culture, uniqueName, DateTime.Now);
            }
        }

        private class CultureNodeName
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int LanguageId { get; set; }
        }

        #endregion
    }
}
