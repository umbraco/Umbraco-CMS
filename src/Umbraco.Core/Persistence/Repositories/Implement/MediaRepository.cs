using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using static Umbraco.Core.Persistence.NPocoSqlExtensions.Statics;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represents a repository for doing CRUD operations for <see cref="IMedia"/>
    /// </summary>
    internal class MediaRepository : ContentRepositoryBase<int, IMedia, MediaRepository>, IMediaRepository
    {
        private readonly AppCaches _cache;
        private readonly IMediaTypeRepository _mediaTypeRepository;
        private readonly ITagRepository _tagRepository;
        private readonly MediaByGuidReadRepository _mediaByGuidReadRepository;

        public MediaRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger, IMediaTypeRepository mediaTypeRepository, ITagRepository tagRepository, ILanguageRepository languageRepository, IRelationRepository relationRepository, IRelationTypeRepository relationTypeRepository,
            Lazy<PropertyEditorCollection> propertyEditorCollection, DataValueReferenceFactoryCollection dataValueReferenceFactories)
            : base(scopeAccessor, cache, logger, languageRepository, relationRepository, relationTypeRepository, propertyEditorCollection, dataValueReferenceFactories)
        {
            _cache = cache;
            _mediaTypeRepository = mediaTypeRepository ?? throw new ArgumentNullException(nameof(mediaTypeRepository));
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
            _mediaByGuidReadRepository = new MediaByGuidReadRepository(this, scopeAccessor, cache, logger);
        }

        protected override MediaRepository This => this;


        #region Repository Base

        protected override Guid NodeObjectTypeId => Constants.ObjectTypes.Media;

        protected override IMedia PerformGet(int id)
        {
            var sql = GetBaseQuery(QueryType.Single)
                .Where<NodeDto>(x => x.NodeId == id)
                .SelectTop(1);

            var dto = Database.Fetch<ContentDto>(sql).FirstOrDefault();
            return dto == null
                ? null
                : MapDtoToContent(dto);
        }

        protected override IEnumerable<IMedia> PerformGetAll(params int[] ids)
        {
            var sql = GetBaseQuery(QueryType.Many);

            if (ids.Any())
                sql.WhereIn<NodeDto>(x => x.NodeId, ids);

            return MapDtosToContent(Database.Fetch<ContentDto>(sql));
        }

        protected override IEnumerable<IMedia> PerformGetByQuery(IQuery<IMedia> query)
        {
            var sqlClause = GetBaseQuery(QueryType.Many);

            var translator = new SqlTranslator<IMedia>(sqlClause, query);
            var sql = translator.Translate();

            sql
                .OrderBy<NodeDto>(x => x.Level)
                .OrderBy<NodeDto>(x => x.SortOrder);

            return MapDtosToContent(Database.Fetch<ContentDto>(sql));
        }

        protected override Sql<ISqlContext> GetBaseQuery(QueryType queryType)
        {
            return GetBaseQuery(queryType);
        }

        protected virtual Sql<ISqlContext> GetBaseQuery(QueryType queryType, bool current = true, bool joinMediaVersion = false)
        {
            var sql = SqlContext.Sql();

            switch (queryType)
            {
                case QueryType.Count:
                    sql = sql.SelectCount();
                    break;
                case QueryType.Ids:
                    sql = sql.Select<ContentDto>(x => x.NodeId);
                    break;
                case QueryType.Single:
                case QueryType.Many:
                    sql = sql.Select<ContentDto>(r =>
                        r.Select(x => x.NodeDto)
                         .Select(x => x.ContentVersionDto))

                        // ContentRepositoryBase expects a variantName field to order by name
                        // for now, just return the plain invariant node name
                        .AndSelect<NodeDto>(x => Alias(x.Text, "variantName"));
                    break;
            }

            sql
                .From<ContentDto>()
                .InnerJoin<NodeDto>().On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<ContentVersionDto>().On<ContentDto, ContentVersionDto>(left => left.NodeId, right => right.NodeId);

            if (joinMediaVersion)
                sql.InnerJoin<MediaVersionDto>().On<ContentVersionDto, MediaVersionDto>((left, right) => left.Id == right.Id);

            sql.Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

            if (current)
                sql.Where<ContentVersionDto>(x => x.Current); // always get the current version

            return sql;
        }

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
        {
            return GetBaseQuery(isCount ? QueryType.Count : QueryType.Single);
        }

        // ah maybe not, that what's used for eg Exists in base repo
        protected override string GetBaseWhereClause()
        {
            return $"{Constants.DatabaseSchema.Tables.Node}.id = @id";
        }

        protected override IEnumerable<string> GetDeleteClauses()
        {
            var list = new List<string>
            {
                "DELETE FROM " + Constants.DatabaseSchema.Tables.User2NodeNotify + " WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.UserGroup2NodePermission + " WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.UserStartNode + " WHERE startNode = @id",
                "UPDATE " + Constants.DatabaseSchema.Tables.UserGroup + " SET startContentId = NULL WHERE startContentId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.Relation + " WHERE parentId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.Relation + " WHERE childId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.TagRelationship + " WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.Document + " WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.MediaVersion + " WHERE id IN (SELECT id FROM " + Constants.DatabaseSchema.Tables.ContentVersion + " WHERE nodeId = @id)",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.PropertyData + " WHERE versionId IN (SELECT id FROM " + Constants.DatabaseSchema.Tables.ContentVersion + " WHERE nodeId = @id)",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.ContentVersion + " WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.Content + " WHERE nodeId = @id",
                "DELETE FROM " + Constants.DatabaseSchema.Tables.Node + " WHERE id = @id"
            };
            return list;
        }

        #endregion

        #region Versions

        public override IEnumerable<IMedia> GetAllVersions(int nodeId)
        {
            var sql = GetBaseQuery(QueryType.Many, current: false)
                .Where<NodeDto>(x => x.NodeId == nodeId)
                .OrderByDescending<ContentVersionDto>(x => x.Current)
                .AndByDescending<ContentVersionDto>(x => x.VersionDate);

            return MapDtosToContent(Database.Fetch<ContentDto>(sql), true);
        }

        public override IMedia GetVersion(int versionId)
        {
            var sql = GetBaseQuery(QueryType.Single)
                .Where<ContentVersionDto>(x => x.Id == versionId);

            var dto = Database.Fetch<ContentDto>(sql).FirstOrDefault();
            return dto == null ? null : MapDtoToContent(dto);
        }

        public IMedia GetMediaByPath(string mediaPath)
        {
            var umbracoFileValue = mediaPath;
            const string pattern = ".*[_][0-9]+[x][0-9]+[.].*";
            var isResized = Regex.IsMatch(mediaPath, pattern);

            // If the image has been resized we strip the "_403x328" of the original "/media/1024/koala_403x328.jpg" URL.
            if (isResized)
            {
                var underscoreIndex = mediaPath.LastIndexOf('_');
                var dotIndex = mediaPath.LastIndexOf('.');
                umbracoFileValue = string.Concat(mediaPath.Substring(0, underscoreIndex), mediaPath.Substring(dotIndex));
            }

            var sql = GetBaseQuery(QueryType.Single, joinMediaVersion: true)
                .Where<MediaVersionDto>(x => x.Path == umbracoFileValue)
                .SelectTop(1);

            var dto = Database.Fetch<ContentDto>(sql).FirstOrDefault();
            return dto == null
                ? null
                : MapDtoToContent(dto);
        }

        protected override void PerformDeleteVersion(int id, int versionId)
        {
            // raise event first else potential FK issues
            OnUowRemovingVersion(new ScopedVersionEventArgs(AmbientScope, id, versionId));

            Database.Delete<PropertyDataDto>("WHERE versionId = @versionId", new { versionId });
            Database.Delete<ContentVersionDto>("WHERE versionId = @versionId", new { versionId });
        }

        #endregion

        #region Persist

        protected override void PersistNewItem(IMedia entity)
        {
            entity.AddingEntity();

            // ensure unique name on the same level
            entity.Name = EnsureUniqueNodeName(entity.ParentId, entity.Name);

            // ensure that strings don't contain characters that are invalid in xml
            // TODO: do we really want to keep doing this here?
            entity.SanitizeEntityPropertiesForXmlStorage();

            // create the dto
            var dto = ContentBaseFactory.BuildDto(PropertyEditors, entity);

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
            // assumes a new version id and version date (modified date) has been set
            var contentVersionDto = dto.MediaVersionDto.ContentVersionDto;
            contentVersionDto.NodeId = nodeDto.NodeId;
            contentVersionDto.Current = true;
            Database.Insert(contentVersionDto);
            entity.VersionId = contentVersionDto.Id;

            // persist the media version dto
            var mediaVersionDto = dto.MediaVersionDto;
            mediaVersionDto.Id = entity.VersionId;
            Database.Insert(mediaVersionDto);

            // persist the property data
            InsertPropertyValues(entity, 0, out _, out _);

            // set tags
            SetEntityTags(entity, _tagRepository);

            PersistRelations(entity);

            OnUowRefreshedEntity(new ScopedEntityEventArgs(AmbientScope, entity));

            entity.ResetDirtyProperties();
        }

        protected override void PersistUpdatedItem(IMedia entity)
        {
            // update
            entity.UpdatingEntity();

            // Check if this entity is being moved as a descendant as part of a bulk moving operations.
            // In this case we can bypass a lot of the below operations which will make this whole operation go much faster.
            // When moving we don't need to create new versions, etc... because we cannot roll this operation back anyways.
            var isMoving = entity.IsMoving();

            if (!isMoving)
            {
                // ensure unique name on the same level
                entity.Name = EnsureUniqueNodeName(entity.ParentId, entity.Name, entity.Id);

                // ensure that strings don't contain characters that are invalid in xml
                // TODO: do we really want to keep doing this here?
                entity.SanitizeEntityPropertiesForXmlStorage();

                // if parent has changed, get path, level and sort order
                if (entity.IsPropertyDirty(nameof(entity.ParentId)))
                {
                    var parent = GetParentNodeDto(entity.ParentId);

                    entity.Path = string.Concat(parent.Path, ",", entity.Id);
                    entity.Level = parent.Level + 1;
                    entity.SortOrder = GetNewChildSortOrder(entity.ParentId, 0);
                }
            }

            // create the dto
            var dto = ContentBaseFactory.BuildDto(PropertyEditors, entity);

            // update the node dto
            var nodeDto = dto.ContentDto.NodeDto;
            nodeDto.ValidatePathWithException();
            Database.Update(nodeDto);

            if (!isMoving)
            {
                // update the content dto
                Database.Update(dto.ContentDto);

                // update the content & media version dtos
                var contentVersionDto = dto.MediaVersionDto.ContentVersionDto;
                var mediaVersionDto = dto.MediaVersionDto;
                contentVersionDto.Current = true;
                Database.Update(contentVersionDto);
                Database.Update(mediaVersionDto);

                // replace the property data
                ReplacePropertyValues(entity, entity.VersionId, 0, out _, out _);

                SetEntityTags(entity, _tagRepository);

                PersistRelations(entity);
            }

            OnUowRefreshedEntity(new ScopedEntityEventArgs(AmbientScope, entity));

            entity.ResetDirtyProperties();
        }

        protected override void PersistDeletedItem(IMedia entity)
        {
            // raise event first else potential FK issues
            OnUowRemovingEntity(new ScopedEntityEventArgs(AmbientScope, entity));
            base.PersistDeletedItem(entity);
        }

        #endregion

        #region Recycle Bin

        public override int RecycleBinId => Constants.System.RecycleBinMedia;

        public bool RecycleBinSmells()
        {
            var cache = _cache.RuntimeCache;
            var cacheKey = CacheKeys.MediaRecycleBinCacheKey;

            // always cache either true or false
            return cache.GetCacheItem<bool>(cacheKey, () => CountChildren(RecycleBinId) > 0);
        }

        #endregion

        #region Read Repository implementation for Guid keys

        public IMedia Get(Guid id)
        {
            return _mediaByGuidReadRepository.Get(id);
        }

        IEnumerable<IMedia> IReadRepository<Guid, IMedia>.GetMany(params Guid[] ids)
        {
            return _mediaByGuidReadRepository.GetMany(ids);
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
        private class MediaByGuidReadRepository : NPocoRepositoryBase<Guid, IMedia>
        {
            private readonly MediaRepository _outerRepo;

            public MediaByGuidReadRepository(MediaRepository outerRepo, IScopeAccessor scopeAccessor, AppCaches cache, ILogger logger)
                : base(scopeAccessor, cache, logger)
            {
                _outerRepo = outerRepo;
            }

            protected override Guid NodeObjectTypeId => _outerRepo.NodeObjectTypeId;

            protected override IMedia PerformGet(Guid id)
            {
                var sql = _outerRepo.GetBaseQuery(QueryType.Single)
                    .Where<NodeDto>(x => x.UniqueId == id);

                var dto = Database.Fetch<ContentDto>(sql.SelectTop(1)).FirstOrDefault();

                if (dto == null)
                    return null;

                var content = _outerRepo.MapDtoToContent(dto);

                return content;
            }

            protected override IEnumerable<IMedia> PerformGetAll(params Guid[] ids)
            {
                var sql = _outerRepo.GetBaseQuery(QueryType.Many);
                if (ids.Length > 0)
                    sql.WhereIn<NodeDto>(x => x.UniqueId, ids);

                return _outerRepo.MapDtosToContent(Database.Fetch<ContentDto>(sql));
            }

            protected override IEnumerable<IMedia> PerformGetByQuery(IQuery<IMedia> query)
            {
                throw new InvalidOperationException("This method won't be implemented.");
            }

            protected override IEnumerable<string> GetDeleteClauses()
            {
                throw new InvalidOperationException("This method won't be implemented.");
            }

            protected override void PersistNewItem(IMedia entity)
            {
                throw new InvalidOperationException("This method won't be implemented.");
            }

            protected override void PersistUpdatedItem(IMedia entity)
            {
                throw new InvalidOperationException("This method won't be implemented.");
            }

            protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
            {
                throw new InvalidOperationException("This method won't be implemented.");
            }

            protected override string GetBaseWhereClause()
            {
                throw new InvalidOperationException("This method won't be implemented.");
            }
        }

        #endregion

        /// <inheritdoc />
        public override IEnumerable<IMedia> GetPage(IQuery<IMedia> query,
            long pageIndex, int pageSize, out long totalRecords,
            IQuery<IMedia> filter, Ordering ordering)
        {
            Sql<ISqlContext> filterSql = null;

            if (filter != null)
            {
                filterSql = Sql();
                foreach (var clause in filter.GetWhereClauses())
                    filterSql = filterSql.Append($"AND ({clause.Item1})", clause.Item2);
            }

            return GetPage<ContentDto>(query, pageIndex, pageSize, out totalRecords,
                x => MapDtosToContent(x),
                filterSql,
                ordering);
        }

        private IEnumerable<IMedia> MapDtosToContent(List<ContentDto> dtos, bool withCache = false)
        {
            var temps = new List<TempContent<Models.Media>>();
            var contentTypes = new Dictionary<int, IMediaType>();
            var content = new Models.Media[dtos.Count];

            for (var i = 0; i < dtos.Count; i++)
            {
                var dto = dtos[i];

                if (withCache)
                {
                    // if the cache contains the (proper version of the) item, use it
                    var cached = IsolatedCache.GetCacheItem<IMedia>(RepositoryCacheKeys.GetKey<IMedia, int>(dto.NodeId));
                    if (cached != null && cached.VersionId == dto.ContentVersionDto.Id)
                    {
                        content[i] = (Models.Media)cached;
                        continue;
                    }
                }

                // else, need to build it

                // get the content type - the repository is full cache *but* still deep-clones
                // whatever comes out of it, so use our own local index here to avoid this
                var contentTypeId = dto.ContentTypeId;
                if (contentTypes.TryGetValue(contentTypeId, out IMediaType contentType) == false)
                    contentTypes[contentTypeId] = contentType = _mediaTypeRepository.Get(contentTypeId);

                var c = content[i] = ContentBaseFactory.BuildEntity(dto, contentType);

                // need properties
                var versionId = dto.ContentVersionDto.Id;
                temps.Add(new TempContent<Models.Media>(dto.NodeId, versionId, 0, contentType, c));
            }

            // load all properties for all documents from database in 1 query - indexed by version id
            var properties = GetPropertyCollections(temps);

            // assign properties
            foreach (var temp in temps)
            {
                temp.Content.Properties = properties[temp.VersionId];

                // reset dirty initial properties (U4-1946)
                temp.Content.ResetDirtyProperties(false);
            }

            return content;
        }

        private IMedia MapDtoToContent(ContentDto dto)
        {
            var contentType = _mediaTypeRepository.Get(dto.ContentTypeId);
            var media = ContentBaseFactory.BuildEntity(dto, contentType);

            // get properties - indexed by version id
            var versionId = dto.ContentVersionDto.Id;
            var temp = new TempContent<Models.Media>(dto.NodeId, versionId, 0, contentType);
            var properties = GetPropertyCollections(new List<TempContent<Models.Media>> { temp });
            media.Properties = properties[versionId];

            // reset dirty initial properties (U4-1946)
            media.ResetDirtyProperties(false);
            return media;
        }

    }
}
