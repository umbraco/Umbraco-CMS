using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Persistence.SqlExtensionsStatics;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents a repository for doing CRUD operations for <see cref="IMedia" />
/// </summary>
public class MediaRepository : ContentRepositoryBase<int, IMedia, MediaRepository>, IMediaRepository
{
    private readonly AppCaches _cache;
    private readonly MediaByGuidReadRepository _mediaByGuidReadRepository;
    private readonly IMediaTypeRepository _mediaTypeRepository;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;
    private readonly IJsonSerializer _serializer;
    private readonly ITagRepository _tagRepository;

    public MediaRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<MediaRepository> logger,
        ILoggerFactory loggerFactory,
        IMediaTypeRepository mediaTypeRepository,
        ITagRepository tagRepository,
        ILanguageRepository languageRepository,
        IRelationRepository relationRepository,
        IRelationTypeRepository relationTypeRepository,
        PropertyEditorCollection propertyEditorCollection,
        MediaUrlGeneratorCollection mediaUrlGenerators,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        IDataTypeService dataTypeService,
        IJsonSerializer serializer,
        IEventAggregator eventAggregator)
        : base(scopeAccessor, cache, logger, languageRepository, relationRepository, relationTypeRepository,
            propertyEditorCollection, dataValueReferenceFactories, dataTypeService, eventAggregator)
    {
        _cache = cache;
        _mediaTypeRepository = mediaTypeRepository ?? throw new ArgumentNullException(nameof(mediaTypeRepository));
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _mediaUrlGenerators = mediaUrlGenerators;
        _serializer = serializer;
        _mediaByGuidReadRepository = new MediaByGuidReadRepository(this, scopeAccessor, cache,
            loggerFactory.CreateLogger<MediaByGuidReadRepository>());
    }

    protected override MediaRepository This => this;

    /// <inheritdoc />
    public override IEnumerable<IMedia> GetPage(IQuery<IMedia>? query,
        long pageIndex, int pageSize, out long totalRecords,
        IQuery<IMedia>? filter, Ordering? ordering)
    {
        Sql<ISqlContext>? filterSql = null;

        if (filter != null)
        {
            filterSql = Sql();
            foreach (Tuple<string, object[]> clause in filter.GetWhereClauses())
            {
                filterSql = filterSql.Append($"AND ({clause.Item1})", clause.Item2);
            }
        }

        return GetPage<ContentDto>(query, pageIndex, pageSize, out totalRecords,
            x => MapDtosToContent(x),
            filterSql,
            ordering);
    }

    private IEnumerable<IMedia> MapDtosToContent(List<ContentDto> dtos, bool withCache = false)
    {
        var temps = new List<TempContent<Core.Models.Media>>();
        var contentTypes = new Dictionary<int, IMediaType?>();
        var content = new Core.Models.Media[dtos.Count];

        for (var i = 0; i < dtos.Count; i++)
        {
            ContentDto dto = dtos[i];

            if (withCache)
            {
                // if the cache contains the (proper version of the) item, use it
                IMedia? cached =
                    IsolatedCache.GetCacheItem<IMedia>(RepositoryCacheKeys.GetKey<IMedia, int>(dto.NodeId));
                if (cached != null && cached.VersionId == dto.ContentVersionDto.Id)
                {
                    content[i] = (Core.Models.Media)cached;
                    continue;
                }
            }

            // else, need to build it

            // get the content type - the repository is full cache *but* still deep-clones
            // whatever comes out of it, so use our own local index here to avoid this
            var contentTypeId = dto.ContentTypeId;
            if (contentTypes.TryGetValue(contentTypeId, out IMediaType? contentType) == false)
            {
                contentTypes[contentTypeId] = contentType = _mediaTypeRepository.Get(contentTypeId);
            }

            Core.Models.Media c = content[i] = ContentBaseFactory.BuildEntity(dto, contentType);

            // need properties
            var versionId = dto.ContentVersionDto.Id;
            temps.Add(new TempContent<Core.Models.Media>(dto.NodeId, versionId, 0, contentType, c));
        }

        // load all properties for all documents from database in 1 query - indexed by version id
        IDictionary<int, PropertyCollection> properties = GetPropertyCollections(temps);

        // assign properties
        foreach (TempContent<Core.Models.Media> temp in temps)
        {
            if (temp.Content is not null)
            {
                temp.Content.Properties = properties[temp.VersionId];

                // reset dirty initial properties (U4-1946)
                temp.Content.ResetDirtyProperties(false);
            }
        }

        return content;
    }

    private IMedia MapDtoToContent(ContentDto dto)
    {
        IMediaType? contentType = _mediaTypeRepository.Get(dto.ContentTypeId);
        Core.Models.Media media = ContentBaseFactory.BuildEntity(dto, contentType);

        // get properties - indexed by version id
        var versionId = dto.ContentVersionDto.Id;
        var temp = new TempContent<Core.Models.Media>(dto.NodeId, versionId, 0, contentType);
        IDictionary<int, PropertyCollection> properties =
            GetPropertyCollections(new List<TempContent<Core.Models.Media>> { temp });
        media.Properties = properties[versionId];

        // reset dirty initial properties (U4-1946)
        media.ResetDirtyProperties(false);
        return media;
    }

    #region Repository Base

    protected override Guid NodeObjectTypeId => Constants.ObjectTypes.Media;

    protected override IMedia? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Single)
            .Where<NodeDto>(x => x.NodeId == id)
            .SelectTop(1);

        ContentDto? dto = Database.Fetch<ContentDto>(sql).FirstOrDefault();
        return dto == null
            ? null
            : MapDtoToContent(dto);
    }

    protected override IEnumerable<IMedia> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many);

        if (ids?.Any() ?? false)
        {
            sql.WhereIn<NodeDto>(x => x.NodeId, ids);
        }

        return MapDtosToContent(Database.Fetch<ContentDto>(sql));
    }

    protected override IEnumerable<IMedia> PerformGetByQuery(IQuery<IMedia> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(QueryType.Many);

        var translator = new SqlTranslator<IMedia>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        sql
            .OrderBy<NodeDto>(x => x.Level)
            .OrderBy<NodeDto>(x => x.SortOrder);

        return MapDtosToContent(Database.Fetch<ContentDto>(sql));
    }

    protected override Sql<ISqlContext> GetBaseQuery(QueryType queryType) => GetBaseQuery(queryType);

    protected virtual Sql<ISqlContext> GetBaseQuery(QueryType queryType, bool current = true, bool joinMediaVersion = false)
    {
        Sql<ISqlContext> sql = SqlContext.Sql();

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
            .InnerJoin<ContentVersionDto>()
            .On<ContentDto, ContentVersionDto>(left => left.NodeId, right => right.NodeId);

        if (joinMediaVersion)
        {
            sql.InnerJoin<MediaVersionDto>()
                .On<ContentVersionDto, MediaVersionDto>((left, right) => left.Id == right.Id);
        }

        sql.Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

        if (current)
        {
            sql.Where<ContentVersionDto>(x => x.Current); // always get the current version
        }

        return sql;
    }

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount) =>
        GetBaseQuery(isCount ? QueryType.Count : QueryType.Single);

    // ah maybe not, that what's used for eg Exists in base repo
    protected override string GetBaseWhereClause() => $"{Constants.DatabaseSchema.Tables.Node}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string>
        {
            "DELETE FROM " + Constants.DatabaseSchema.Tables.User2NodeNotify + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.UserGroup2Node + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.UserGroup2NodePermission + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.UserStartNode + " WHERE startNode = @id",
            "UPDATE " + Constants.DatabaseSchema.Tables.UserGroup +
            " SET startContentId = NULL WHERE startContentId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.Relation + " WHERE parentId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.Relation + " WHERE childId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.TagRelationship + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.Document + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.MediaVersion + " WHERE id IN (SELECT id FROM " +
            Constants.DatabaseSchema.Tables.ContentVersion + " WHERE nodeId = @id)",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.PropertyData + " WHERE versionId IN (SELECT id FROM " +
            Constants.DatabaseSchema.Tables.ContentVersion + " WHERE nodeId = @id)",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.ContentVersion + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.Content + " WHERE nodeId = @id",
            "DELETE FROM " + Constants.DatabaseSchema.Tables.Node + " WHERE id = @id",
        };
        return list;
    }

    #endregion

    #region Versions

    public override IEnumerable<IMedia> GetAllVersions(int nodeId)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many, false)
            .Where<NodeDto>(x => x.NodeId == nodeId)
            .OrderByDescending<ContentVersionDto>(x => x.Current)
            .AndByDescending<ContentVersionDto>(x => x.VersionDate);

        return MapDtosToContent(Database.Fetch<ContentDto>(sql), true);
    }

    public override IMedia? GetVersion(int versionId)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Single)
            .Where<ContentVersionDto>(x => x.Id == versionId);

        ContentDto? dto = Database.Fetch<ContentDto>(sql).FirstOrDefault();
        return dto == null ? null : MapDtoToContent(dto);
    }

    public IMedia? GetMediaByPath(string mediaPath)
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

        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Single, joinMediaVersion: true)
            .Where<MediaVersionDto>(x => x.Path == umbracoFileValue)
            .SelectTop(1);

        ContentDto? dto = Database.Fetch<ContentDto>(sql).FirstOrDefault();
        return dto == null
            ? null
            : MapDtoToContent(dto);
    }

    protected override void PerformDeleteVersion(int id, int versionId)
    {
        Database.Delete<PropertyDataDto>("WHERE versionId = @versionId", new { versionId });
        Database.Delete<ContentVersionDto>("WHERE versionId = @versionId", new { versionId });
    }

    #endregion

    #region Persist

    protected override void PersistNewItem(IMedia entity)
    {
        entity.AddingEntity();

        // ensure unique name on the same level
        entity.Name = EnsureUniqueNodeName(entity.ParentId, entity.Name)!;

        // ensure that strings don't contain characters that are invalid in xml
        // TODO: do we really want to keep doing this here?
        entity.SanitizeEntityPropertiesForXmlStorage();

        // create the dto
        MediaDto dto = ContentBaseFactory.BuildDto(_mediaUrlGenerators, entity);

        // derive path and level from parent
        NodeDto parent = GetParentNodeDto(entity.ParentId);
        var level = parent.Level + 1;

        // get sort order
        var sortOrder = GetNewChildSortOrder(entity.ParentId, 0);

        // persist the node dto
        NodeDto nodeDto = dto.ContentDto.NodeDto;
        nodeDto.Path = parent.Path;
        nodeDto.Level = Convert.ToInt16(level);
        nodeDto.SortOrder = sortOrder;

        // see if there's a reserved identifier for this unique id
        // and then either update or insert the node dto
        var id = GetReservedId(nodeDto.UniqueId);
        if (id > 0)
        {
            nodeDto.NodeId = id;
        }
        else
        {
            Database.Insert(nodeDto);
        }

        nodeDto.Path = string.Concat(parent.Path, ",", nodeDto.NodeId);
        nodeDto.ValidatePathWithException();
        Database.Update(nodeDto);

        // update entity
        entity.Id = nodeDto.NodeId;
        entity.Path = nodeDto.Path;
        entity.SortOrder = sortOrder;
        entity.Level = level;

        // persist the content dto
        ContentDto contentDto = dto.ContentDto;
        contentDto.NodeId = nodeDto.NodeId;
        Database.Insert(contentDto);

        // persist the content version dto
        // assumes a new version id and version date (modified date) has been set
        ContentVersionDto contentVersionDto = dto.MediaVersionDto.ContentVersionDto;
        contentVersionDto.NodeId = nodeDto.NodeId;
        contentVersionDto.Current = true;
        Database.Insert(contentVersionDto);
        entity.VersionId = contentVersionDto.Id;

        // persist the media version dto
        MediaVersionDto mediaVersionDto = dto.MediaVersionDto;
        mediaVersionDto.Id = entity.VersionId;
        Database.Insert(mediaVersionDto);

        // persist the property data
        InsertPropertyValues(entity, 0, out _, out _);

        // set tags
        SetEntityTags(entity, _tagRepository, _serializer);

        PersistRelations(entity);

        OnUowRefreshedEntity(new MediaRefreshNotification(entity, new EventMessages()));

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
            entity.Name = EnsureUniqueNodeName(entity.ParentId, entity.Name, entity.Id)!;

            // ensure that strings don't contain characters that are invalid in xml
            // TODO: do we really want to keep doing this here?
            entity.SanitizeEntityPropertiesForXmlStorage();

            // if parent has changed, get path, level and sort order
            if (entity.IsPropertyDirty(nameof(entity.ParentId)))
            {
                NodeDto parent = GetParentNodeDto(entity.ParentId);

                entity.Path = string.Concat(parent.Path, ",", entity.Id);
                entity.Level = parent.Level + 1;
                entity.SortOrder = GetNewChildSortOrder(entity.ParentId, 0);
            }
        }

        // create the dto
        MediaDto dto = ContentBaseFactory.BuildDto(_mediaUrlGenerators, entity);

        // update the node dto
        NodeDto nodeDto = dto.ContentDto.NodeDto;
        nodeDto.ValidatePathWithException();
        Database.Update(nodeDto);

        if (!isMoving)
        {
            // update the content dto
            Database.Update(dto.ContentDto);

            // update the content & media version dtos
            ContentVersionDto contentVersionDto = dto.MediaVersionDto.ContentVersionDto;
            MediaVersionDto mediaVersionDto = dto.MediaVersionDto;
            contentVersionDto.Current = true;
            Database.Update(contentVersionDto);
            Database.Update(mediaVersionDto);

            // replace the property data
            ReplacePropertyValues(entity, entity.VersionId, 0, out _, out _);

            SetEntityTags(entity, _tagRepository, _serializer);

            PersistRelations(entity);
        }

        OnUowRefreshedEntity(new MediaRefreshNotification(entity, new EventMessages()));

        entity.ResetDirtyProperties();
    }

    protected override void PersistDeletedItem(IMedia entity)
    {
        // Raise event first else potential FK issues
        OnUowRemovingEntity(entity);
        base.PersistDeletedItem(entity);
    }

    #endregion

    #region Recycle Bin

    public override int RecycleBinId => Constants.System.RecycleBinMedia;

    public bool RecycleBinSmells()
    {
        IAppPolicyCache cache = _cache.RuntimeCache;
        var cacheKey = CacheKeys.MediaRecycleBinCacheKey;

        // always cache either true or false
        return cache.GetCacheItem(cacheKey, () => CountChildren(RecycleBinId) > 0);
    }

    #endregion

    #region Read Repository implementation for Guid keys

    public IMedia? Get(Guid id) => _mediaByGuidReadRepository.Get(id);

    IEnumerable<IMedia> IReadRepository<Guid, IMedia>.GetMany(params Guid[]? ids) =>
        _mediaByGuidReadRepository.GetMany(ids);

    public bool Exists(Guid id) => _mediaByGuidReadRepository.Exists(id);

    // A reading repository purely for looking up by GUID
    // TODO: This is ugly and to fix we need to decouple the IRepositoryQueryable -> IRepository -> IReadRepository which should all be separate things!
    // This sub-repository pattern is super old and totally unecessary anymore, caching can be handled in much nicer ways without this
    private class MediaByGuidReadRepository : EntityRepositoryBase<Guid, IMedia>
    {
        private readonly MediaRepository _outerRepo;

        public MediaByGuidReadRepository(MediaRepository outerRepo, IScopeAccessor scopeAccessor, AppCaches cache, ILogger<MediaByGuidReadRepository> logger)
            : base(scopeAccessor, cache, logger) =>
            _outerRepo = outerRepo;

        protected override IMedia? PerformGet(Guid id)
        {
            Sql<ISqlContext> sql = _outerRepo.GetBaseQuery(QueryType.Single)
                .Where<NodeDto>(x => x.UniqueId == id);

            ContentDto? dto = Database.Fetch<ContentDto>(sql.SelectTop(1)).FirstOrDefault();

            if (dto == null)
            {
                return null;
            }

            IMedia content = _outerRepo.MapDtoToContent(dto);

            return content;
        }

        protected override IEnumerable<IMedia> PerformGetAll(params Guid[]? ids)
        {
            Sql<ISqlContext> sql = _outerRepo.GetBaseQuery(QueryType.Many);
            if (ids?.Length > 0)
            {
                sql.WhereIn<NodeDto>(x => x.UniqueId, ids);
            }

            return _outerRepo.MapDtosToContent(Database.Fetch<ContentDto>(sql));
        }

        protected override IEnumerable<IMedia> PerformGetByQuery(IQuery<IMedia> query) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override IEnumerable<string> GetDeleteClauses() =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override void PersistNewItem(IMedia entity) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override void PersistUpdatedItem(IMedia entity) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override Sql<ISqlContext> GetBaseQuery(bool isCount) =>
            throw new InvalidOperationException("This method won't be implemented.");

        protected override string GetBaseWhereClause() =>
            throw new InvalidOperationException("This method won't be implemented.");
    }

    #endregion
}
