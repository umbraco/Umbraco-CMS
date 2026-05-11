using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.MediaRepository"/> class,
    /// providing access to media persistence and related services in the Umbraco CMS infrastructure layer.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for transactional operations.</param>
    /// <param name="cache">The application-level caches used for optimizing data retrieval and storage.</param>
    /// <param name="logger">The logger instance for logging repository operations and errors.</param>
    /// <param name="loggerFactory">Factory for creating logger instances.</param>
    /// <param name="mediaTypeRepository">Repository for accessing and managing media types.</param>
    /// <param name="tagRepository">Repository for managing tags associated with media items.</param>
    /// <param name="languageRepository">Repository for accessing language information.</param>
    /// <param name="relationRepository">Repository for managing relationships between entities.</param>
    /// <param name="relationTypeRepository">Repository for managing types of relations between entities.</param>
    /// <param name="propertyEditorCollection">Collection of property editors used for media properties.</param>
    /// <param name="mediaUrlGenerators">Collection of generators for producing media URLs.</param>
    /// <param name="dataValueReferenceFactories">Collection of factories for resolving data value references.</param>
    /// <param name="dataTypeService">Service for managing data types used by media properties.</param>
    /// <param name="serializer">The JSON serializer for serializing and deserializing data.</param>
    /// <param name="eventAggregator">Publishes and subscribes to domain events.</param>
    /// <param name="repositoryCacheVersionService">Service for managing cache versioning for repositories.</param>
    /// <param name="cacheSyncService">Service for synchronizing cache across distributed environments.</param>
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
        IEventAggregator eventAggregator,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            cache,
            logger,
            languageRepository,
            relationRepository,
            relationTypeRepository,
            propertyEditorCollection,
            dataValueReferenceFactories,
            dataTypeService,
            eventAggregator,
            repositoryCacheVersionService,
            cacheSyncService)
    {
        _cache = cache;
        _mediaTypeRepository = mediaTypeRepository ?? throw new ArgumentNullException(nameof(mediaTypeRepository));
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        _mediaUrlGenerators = mediaUrlGenerators;
        _serializer = serializer;
        _mediaByGuidReadRepository = new MediaByGuidReadRepository(
            this,
            scopeAccessor,
            cache,
            loggerFactory.CreateLogger<MediaByGuidReadRepository>(),
            repositoryCacheVersionService,
            cacheSyncService);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaRepository"/> class with the specified dependencies.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope.</param>
    /// <param name="cache">The application-level caches used for performance optimization.</param>
    /// <param name="logger">The logger instance for logging repository operations.</param>
    /// <param name="loggerFactory">Factory for creating logger instances.</param>
    /// <param name="mediaTypeRepository">Repository for accessing media types.</param>
    /// <param name="tagRepository">Repository for managing tags associated with media.</param>
    /// <param name="languageRepository">Repository for managing languages.</param>
    /// <param name="relationRepository">Repository for managing entity relations.</param>
    /// <param name="relationTypeRepository">Repository for managing relation types.</param>
    /// <param name="propertyEditorCollection">Collection of property editors for media properties.</param>
    /// <param name="mediaUrlGenerators">Collection of generators for creating media URLs.</param>
    /// <param name="dataValueReferenceFactories">Collection of factories for resolving data value references.</param>
    /// <param name="dataTypeService">Service for managing data types.</param>
    /// <param name="serializer">The JSON serializer for serializing and deserializing data.</param>
    /// <param name="eventAggregator">Publishes and subscribes to domain events.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18.")]
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
        : this(
            scopeAccessor,
            cache,
            logger,
            loggerFactory,
            mediaTypeRepository,
            tagRepository,
            languageRepository,
            relationRepository,
            relationTypeRepository,
            propertyEditorCollection,
            mediaUrlGenerators,
            dataValueReferenceFactories,
            dataTypeService,
            serializer,
            eventAggregator,
            StaticServiceProvider.Instance.GetRequiredService<IRepositoryCacheVersionService>(),
            StaticServiceProvider.Instance.GetRequiredService<ICacheSyncService>())
    {
    }

    protected override MediaRepository This => this;

    /// <inheritdoc />
    [Obsolete("Please use the method overload with all parameters. Scheduled for removal in Umbraco 19.")]
    public override IEnumerable<IMedia> GetPage(
        IQuery<IMedia>? query,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        IQuery<IMedia>? filter,
        Ordering? ordering)
        => GetPage(query, pageIndex, pageSize, out totalRecords, propertyAliases: null, filter: filter, ordering: ordering);

    /// <inheritdoc />
    public override IEnumerable<IMedia> GetPage(
        IQuery<IMedia>? query,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        string[]? propertyAliases,
        IQuery<IMedia>? filter,
        Ordering? ordering)
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

        return GetPage<ContentDto>(
            query,
            pageIndex,
            pageSize,
            out totalRecords,
            x => MapDtosToContent(x, propertyAliases: propertyAliases),
            filterSql,
            ordering);
    }

    private IEnumerable<IMedia> MapDtosToContent(List<ContentDto> dtos, bool withCache = false, string[]? propertyAliases = null)
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
        IDictionary<int, PropertyCollection> properties = GetPropertyCollections(temps, propertyAliases);

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

    /// <inheritdoc />
    public override void Save(IMedia entity)
    {
        base.Save(entity);

        // Also populate the GUID cache so subsequent lookups by GUID don't hit the database
        _mediaByGuidReadRepository.PopulateCacheByKey(entity);
    }

    protected override IMedia? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Single)
            .Where<NodeDto>(x => x.NodeId == id);

        ContentDto? dto = Database.FirstOrDefault<ContentDto>(sql);
        if (dto == null)
        {
            return null;
        }

        IMedia media = MapDtoToContent(dto);

        // Also populate the GUID cache so subsequent lookups by GUID don't hit the database
        _mediaByGuidReadRepository.PopulateCacheByKey(media);

        return media;
    }

    protected override IEnumerable<IMedia> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many);

        if (ids?.Any() ?? false)
        {
            sql.WhereIn<NodeDto>(x => x.NodeId, ids);
        }

        // MapDtosToContent returns a materialized array, so this is safe to enumerate multiple times
        IEnumerable<IMedia> media = MapDtosToContent(Database.Fetch<ContentDto>(sql));

        // Also populate the GUID cache so subsequent lookups by GUID don't hit the database
        _mediaByGuidReadRepository.PopulateCacheByKey(media);

        return media;
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
    protected override string GetBaseWhereClause() => $"{QuoteTableName(NodeDto.TableName)}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var nodeId = QuoteColumnName("nodeId");
        var uniqueId = QuoteColumnName("uniqueId");
        var umbracoNode = QuoteTableName(NodeDto.TableName);
        var list = new List<string>
        {
            $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.User2NodeNotify)} WHERE {nodeId} = @id",
            $@"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.UserGroup2GranularPermission)} WHERE {uniqueId} IN
                (SELECT {uniqueId} FROM {umbracoNode} WHERE id = @id)",
            $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.UserStartNode)} WHERE {QuoteColumnName("startNode")} = @id",
            $@"UPDATE {QuoteTableName(Constants.DatabaseSchema.Tables.UserGroup)}
                SET {QuoteColumnName("startMediaId")} = NULL WHERE {QuoteColumnName("startMediaId")} = @id",
            $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Relation)} WHERE {QuoteColumnName("parentId")} = @id",
            $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Relation)} WHERE {QuoteColumnName("childId")} = @id",
            $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.TagRelationship)} WHERE {nodeId} = @id",
            $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Document)} WHERE {nodeId} = @id",
            $@"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.MediaVersion)} WHERE id IN
                (SELECT id FROM {QuoteTableName(Constants.DatabaseSchema.Tables.ContentVersion)} WHERE {nodeId} = @id)",
            $@"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.PropertyData)} WHERE {QuoteColumnName("versionId")} IN
                (SELECT id FROM {QuoteTableName(Constants.DatabaseSchema.Tables.ContentVersion)} WHERE {nodeId} = @id)",
            $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.ContentVersion)} WHERE {nodeId} = @id",
            $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.Content)} WHERE {nodeId} = @id",
            $"DELETE FROM {umbracoNode} WHERE id = @id",
        };
        return list;
    }

    #endregion

    #region Versions

    /// <summary>
    /// Retrieves all versions of a media item identified by the specified node ID, ordered by current status and version date.
    /// </summary>
    /// <param name="nodeId">The unique identifier of the media node whose versions are to be retrieved.</param>
    /// <returns>An <see cref="IEnumerable{IMedia}"/> containing all versions of the specified media item.</returns>
    public override IEnumerable<IMedia> GetAllVersions(int nodeId)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Many, false)
            .Where<NodeDto>(x => x.NodeId == nodeId)
            .OrderByDescending<ContentVersionDto>(x => x.Current)
            .AndByDescending<ContentVersionDto>(x => x.VersionDate);

        return MapDtosToContent(Database.Fetch<ContentDto>(sql), true);
    }

    /// <summary>
    /// Retrieves a specific version of a media item by its version identifier.
    /// </summary>
    /// <param name="versionId">The unique identifier of the media version to retrieve.</param>
    /// <returns>The <see cref="IMedia"/> instance representing the specified version, or <c>null</c> if no such version exists.</returns>
    public override IMedia? GetVersion(int versionId)
    {
        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Single)
            .Where<ContentVersionDto>(x => x.Id == versionId);

        ContentDto? dto = Database.Fetch<ContentDto>(sql).FirstOrDefault();
        return dto == null ? null : MapDtoToContent(dto);
    }

    /// <summary>
    /// Retrieves a media item by its media path, normalizing the path if it refers to a resized image.
    /// </summary>
    /// <param name="mediaPath">The media path to search for. If the path refers to a resized image (e.g., contains dimensions like _403x328), the method will normalize it to the original file path before searching.</param>
    /// <returns>The <see cref="IMedia"/> item if found; otherwise, <c>null</c>.</returns>
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
            umbracoFileValue = string.Concat(mediaPath[..underscoreIndex], mediaPath[dotIndex..]);
        }

        Sql<ISqlContext> sql = GetBaseQuery(QueryType.Single, joinMediaVersion: true)
            .Where<MediaVersionDto>(x => x.Path == umbracoFileValue);

        ContentDto? dto = Database.FirstOrDefault<ContentDto>(sql);
        return dto == null
            ? null
            : MapDtoToContent(dto);
    }

    /// <summary>
    /// Nothing to do here, media has only one version which must not be deleted.
    /// Base method is abstract so this must be implemented.
    /// </summary>
    protected override void PerformDeleteVersion(int id, int versionId)
    {
        // Nothing to do here, media has only one version which must not be deleted.
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
        }

        OnUowRefreshedEntity(new MediaRefreshNotification(entity, new EventMessages()));

        entity.ResetDirtyProperties();

        // We need to flush the isolated cache by key explicitly here.
        // The MediaCacheRefresher does the same thing, but by the time it's invoked, custom notification handlers
        // might have already consumed the cached version (which at this point is the previous version).
        // GUID-keyed read repository uses a separate "uRepoGuid_" prefix.
        IsolatedCache.Clear(RepositoryCacheKeys.GetGuidKey<IMedia>(entity.Key));
    }

    protected override void PersistDeletedItem(IMedia entity)
    {
        // Raise event first else potential FK issues
        OnUowRemovingEntity(entity);
        base.PersistDeletedItem(entity);
    }

    #endregion

    #region Recycle Bin

    /// <summary>
    /// Gets the unique identifier for the media recycle bin in Umbraco.
    /// </summary>
    public override int RecycleBinId => Constants.System.RecycleBinMedia;

    /// <summary>
    /// Determines whether the media recycle bin contains any items.
    /// </summary>
    /// <returns><c>true</c> if the recycle bin contains any media items; otherwise, <c>false</c>.</returns>
    public bool RecycleBinSmells()
    {
        IAppPolicyCache cache = _cache.RuntimeCache;
        var cacheKey = CacheKeys.MediaRecycleBinCacheKey;

        // always cache either true or false
        return cache.GetCacheItem(cacheKey, () => CountChildren(RecycleBinId) > 0);
    }

    #endregion

    #region Read Repository implementation for Guid keys

    /// <summary>
    /// Gets a media item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the media item.</param>
    /// <returns>The media item if found; otherwise, null.</returns>
    public IMedia? Get(Guid id) => _mediaByGuidReadRepository.Get(id);

    IEnumerable<IMedia> IReadRepository<Guid, IMedia>.GetMany(params Guid[]? ids) =>
        _mediaByGuidReadRepository.GetMany(ids);

    /// <summary>
    /// Determines whether a media item with the specified identifier exists.
    /// </summary>
    /// <param name="id">The unique identifier of the media item.</param>
    /// <returns>True if the media item exists; otherwise, false.</returns>
    public bool Exists(Guid id) => _mediaByGuidReadRepository.Exists(id);

    /// <summary>
    /// Populates the int-keyed cache with the given entity.
    /// This allows entities retrieved by GUID to also be cached for int ID lookups.
    /// </summary>
    private void PopulateCacheById(IMedia entity)
    {
        if (entity.HasIdentity)
        {
            var cacheKey = GetCacheKey(entity.Id);
            IsolatedCache.Insert(cacheKey, () => entity, RepositoryCacheConstants.DefaultCacheDuration, true);
        }
    }

    /// <summary>
    /// Populates the int-keyed cache with the given entities.
    /// This allows entities retrieved by GUID to also be cached for int ID lookups.
    /// </summary>
    private void PopulateCacheById(IEnumerable<IMedia> entities)
    {
        foreach (IMedia entity in entities)
        {
            PopulateCacheById(entity);
        }
    }

    private static string GetCacheKey(int id) => RepositoryCacheKeys.GetKey<IMedia>() + id;

    // A reading repository purely for looking up by GUID
    // TODO: This is ugly and to fix we need to decouple the IRepositoryQueryable -> IRepository -> IReadRepository which should all be separate things!
    // This sub-repository pattern is super old and totally unecessary anymore, caching can be handled in much nicer ways without this
    private sealed class MediaByGuidReadRepository : EntityRepositoryBase<Guid, IMedia>
    {
        private readonly MediaRepository _outerRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaByGuidReadRepository"/> class.
        /// </summary>
        /// <param name="outerRepo">The parent <see cref="MediaRepository"/> used for delegating media operations.</param>
        /// <param name="scopeAccessor">Provides access to the current database scope.</param>
        /// <param name="cache">The application-level caches for storing and retrieving media data.</param>
        /// <param name="logger">The logger used for logging repository operations and errors.</param>
        /// <param name="repositoryCacheVersionService">Service for managing cache versioning within the repository.</param>
        /// <param name="cacheSyncService">Service responsible for synchronizing cache across distributed environments.</param>
        public MediaByGuidReadRepository(
            MediaRepository outerRepo,
            IScopeAccessor scopeAccessor,
            AppCaches cache,
            ILogger<MediaByGuidReadRepository> logger,
            IRepositoryCacheVersionService repositoryCacheVersionService,
            ICacheSyncService cacheSyncService)
            : base(
                scopeAccessor,
                cache,
                logger,
                repositoryCacheVersionService,
                cacheSyncService) =>
            _outerRepo = outerRepo;

        // Use a GUID-specific cache policy with a distinct prefix ("uRepoGuid_IMedia_")
        // so that GUID-keyed cache entries don't interfere with the parent int-keyed repository's
        // prefix-based search and count validation in DefaultRepositoryCachePolicy.
        protected override IRepositoryCachePolicy<IMedia, Guid> CreateCachePolicy()
            => new GuidReadRepositoryCachePolicy<IMedia>(
                GlobalIsolatedCache,
                ScopeAccessor,
                RepositoryCacheVersionService,
                CacheSyncService);

        protected override IMedia? PerformGet(Guid id)
        {
            Sql<ISqlContext> sql = _outerRepo.GetBaseQuery(QueryType.Single)
                .Where<NodeDto>(x => x.UniqueId == id);

            ContentDto? dto = Database.FirstOrDefault<ContentDto>(sql);

            if (dto == null)
            {
                return null;
            }

            IMedia media = _outerRepo.MapDtoToContent(dto);

            // Also populate the int-keyed cache so subsequent lookups by int ID don't hit the database
            _outerRepo.PopulateCacheById(media);

            return media;
        }

        protected override IEnumerable<IMedia> PerformGetAll(params Guid[]? ids)
        {
            Sql<ISqlContext> sql = _outerRepo.GetBaseQuery(QueryType.Many);
            if (ids?.Length > 0)
            {
                sql.WhereIn<NodeDto>(x => x.UniqueId, ids);
            }

            // MapDtosToContent returns a materialized array, so this is safe to enumerate multiple times
            IEnumerable<IMedia> media = _outerRepo.MapDtosToContent(Database.Fetch<ContentDto>(sql));

            // Also populate the int-keyed cache so subsequent lookups by int ID don't hit the database
            _outerRepo.PopulateCacheById(media);

            return media;
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

        /// <summary>
        /// Populates the GUID-keyed cache with the given entity.
        /// This allows entities retrieved by int ID to also be cached for GUID lookups.
        /// </summary>
        /// <param name="entity">The media entity to cache by its GUID key.</param>
        public void PopulateCacheByKey(IMedia entity)
        {
            if (entity.HasIdentity)
            {
                var cacheKey = GetCacheKey(entity.Key);
                IsolatedCache.Insert(cacheKey, () => entity, RepositoryCacheConstants.DefaultCacheDuration, true);
            }
        }

        /// <summary>
        /// Populates the GUID-keyed cache with the given entities.
        /// This allows entities retrieved by int ID to also be cached for GUID lookups.
        /// </summary>
        /// <param name="entities">The collection of media entities to cache by their GUID keys.</param>
        public void PopulateCacheByKey(IEnumerable<IMedia> entities)
        {
            foreach (IMedia entity in entities)
            {
                PopulateCacheByKey(entity);
            }
        }

        private static string GetCacheKey(Guid key) => GuidReadRepositoryCachePolicy<IMedia>.GetCacheKey(key);
    }

    #endregion
}
