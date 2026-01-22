using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Persistence.SqlExtensionsStatics;

namespace Umbraco.Cms.Infrastructure.HybridCache.Persistence;

/// <inheritdoc/>
internal sealed class DatabaseCacheRepository : RepositoryBase, IDatabaseCacheRepository
{
    private readonly IContentCacheDataSerializerFactory _contentCacheDataSerializerFactory;
    private readonly ILogger<DatabaseCacheRepository> _logger;
    private readonly IOptions<NuCacheSettings> _nucacheSettings;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly UrlSegmentProviderCollection _urlSegmentProviders;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseCacheRepository" /> class.
    /// </summary>
    public DatabaseCacheRepository(
        IScopeAccessor scopeAccessor,
        AppCaches appCaches,
        ILogger<DatabaseCacheRepository> logger,
        IShortStringHelper shortStringHelper,
        UrlSegmentProviderCollection urlSegmentProviders,
        IContentCacheDataSerializerFactory contentCacheDataSerializerFactory,
        IOptions<NuCacheSettings> nucacheSettings)
        : base(scopeAccessor, appCaches)
    {
        _logger = logger;
        _shortStringHelper = shortStringHelper;
        _urlSegmentProviders = urlSegmentProviders;
        _contentCacheDataSerializerFactory = contentCacheDataSerializerFactory;
        _nucacheSettings = nucacheSettings;
    }

    /// <inheritdoc/>
    public async Task DeleteContentItemAsync(int id)
    {
        Sql<ISqlContext> sql = Sql()
            .Delete<ContentNuDto>()
            .Where<ContentNuDto>(x => x.NodeId == id);
        await Database.ExecuteAsync(sql);
    }

    /// <inheritdoc/>
    public async Task RefreshContentAsync(ContentCacheNode contentCacheNode, PublishedState publishedState)
    {
        IContentCacheDataSerializer serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);

        // We always cache draft and published separately, so we only want to cache drafts if the node is a draft type.
        if (contentCacheNode.IsDraft)
        {
            await OnRepositoryRefreshed(serializer, contentCacheNode, true);

            // If it's a draft node we don't need to worry about the published state.
            return;
        }

        switch (publishedState)
        {
            case PublishedState.Publishing:
                await OnRepositoryRefreshed(serializer, contentCacheNode, false);
                break;
            case PublishedState.Unpublishing:
                Sql<ISqlContext> sql = Sql()
                    .Delete<ContentNuDto>()
                    .Where<ContentNuDto>(x => x.NodeId == contentCacheNode.Id && x.Published);
                await Database.ExecuteAsync(sql);
                break;
        }
    }

    /// <inheritdoc/>
    public async Task RefreshMediaAsync(ContentCacheNode contentCacheNode)
    {
        IContentCacheDataSerializer serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);
        await OnRepositoryRefreshed(serializer, contentCacheNode, true);
    }

    /// <inheritdoc/>
    public void Rebuild(
        IReadOnlyCollection<int>? contentTypeIds = null,
        IReadOnlyCollection<int>? mediaTypeIds = null,
        IReadOnlyCollection<int>? memberTypeIds = null)
    {
        IContentCacheDataSerializer serializer = _contentCacheDataSerializerFactory.Create(
            ContentCacheDataSerializerEntityType.Document
            | ContentCacheDataSerializerEntityType.Media
            | ContentCacheDataSerializerEntityType.Member);

        // If contentTypeIds, mediaTypeIds and memberTypeIds are all non-null but empty,
        // truncate the table as all records will be deleted (as these 3 are the only types in the table).
        if (contentTypeIds is not null && contentTypeIds.Count == 0 &&
            mediaTypeIds is not null && mediaTypeIds.Count == 0 &&
            memberTypeIds is not null && memberTypeIds.Count == 0)
        {
            TruncateContent();
        }

        RebuildContentDbCache(serializer, _nucacheSettings.Value.SqlPageSize, contentTypeIds);
        RebuildMediaDbCache(serializer, _nucacheSettings.Value.SqlPageSize, mediaTypeIds);
        RebuildMemberDbCache(serializer, _nucacheSettings.Value.SqlPageSize, memberTypeIds);
    }

    private void TruncateContent()
    {
        if (Database.DatabaseType == DatabaseType.SqlServer2012)
        {
            Database.Execute($"TRUNCATE TABLE cmsContentNu");
        }

        if (Database.DatabaseType == DatabaseType.SQLite)
        {
            Database.Execute($"DELETE FROM cmsContentNu");
        }
    }

    /// <inheritdoc/>
    public async Task<ContentCacheNode?> GetContentSourceAsync(Guid key, bool preview = false)
    {
        ContentSourceDto? dto = await GetContentSourceDto(key);

        if (dto is null)
        {
            return null;
        }

        if (preview is false && dto.PubDataRaw is null && dto.PubData is null)
        {
            return null;
        }

        return CreateContentNodeKit(preview, dto);
    }

    /// <inheritdoc/>
    public async Task<(ContentCacheNode? Draft, ContentCacheNode? Published)> GetContentSourceForPublishStatesAsync(Guid key)
    {
        ContentSourceDto? dto = await GetContentSourceDto(key);

        if (dto is null)
        {
            return (null, null);
        }

        ContentCacheNode draftNode = CreateContentNodeKit(true, dto);
        ContentCacheNode? publishedNode = dto.PubDataRaw is null && dto.PubData is null
            ? null
            : CreateContentNodeKit(false, dto);

        return (draftNode, publishedNode);
    }

    private async Task<ContentSourceDto?> GetContentSourceDto(Guid key)
    {
        Sql<ISqlContext>? sql = SqlContentSourcesSelect()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document))
            .Append(SqlWhereNodeKey(SqlContext, key))
            .Append(SqlOrderByLevelIdSortOrder(SqlContext));

        return await Database.FirstOrDefaultAsync<ContentSourceDto>(sql);
    }

    private ContentCacheNode CreateContentNodeKit(bool preview, ContentSourceDto dto)
    {
        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);
        return CreateContentNodeKit(dto, serializer, preview);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ContentCacheNode>> GetContentSourcesAsync(IEnumerable<Guid> keys, bool preview = false)
    {
        Sql<ISqlContext>? sql = SqlContentSourcesSelect()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document))
            .WhereIn<NodeDto>(x => x.UniqueId, keys)
            .Append(SqlOrderByLevelIdSortOrder(SqlContext));

        List<ContentSourceDto> dtos = await Database.FetchAsync<ContentSourceDto>(sql);

        dtos = dtos
            .Where(x => x is not null)
            .Where(x => preview || x.PubDataRaw is not null || x.PubData is not null)
            .ToList();

        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);
        return dtos
            .Select(x => CreateContentNodeKit(x, serializer, preview));
    }

    private IEnumerable<ContentSourceDto> GetContentSourceByDocumentTypeKey(IEnumerable<Guid> documentTypeKeys, Guid objectType)
    {
        Guid[] keys = documentTypeKeys.ToArray();
        if (keys.Length == 0)
        {
            return [];
        }

        Sql<ISqlContext> sql = objectType == Constants.ObjectTypes.Document
            ? SqlContentSourcesSelect()
            : objectType == Constants.ObjectTypes.Media
                ? SqlMediaSourcesSelect()
                : throw new ArgumentOutOfRangeException(nameof(objectType), objectType, null);

        sql.InnerJoin<NodeDto>("n")
            .On<NodeDto, ContentDto>((n, c) => n.NodeId == c.ContentTypeId, "n", "umbracoContent")
            .Append(SqlObjectTypeNotTrashed(SqlContext, objectType))
            .WhereIn<NodeDto>(x => x.UniqueId, keys, "n")
            .Append(SqlOrderByLevelIdSortOrder(SqlContext));

        return GetContentNodeDtos(sql);
    }

    /// <inheritdoc/>
    public IEnumerable<ContentCacheNode> GetContentByContentTypeKey(IEnumerable<Guid> keys, ContentCacheDataSerializerEntityType entityType)
    {
        Guid objectType = entityType switch
        {
            ContentCacheDataSerializerEntityType.Document => Constants.ObjectTypes.Document,
            ContentCacheDataSerializerEntityType.Media => Constants.ObjectTypes.Media,
            _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null),
        };

        IEnumerable<ContentSourceDto> dtos = GetContentSourceByDocumentTypeKey(keys, objectType);

        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(entityType);

        foreach (ContentSourceDto row in dtos)
        {
            if (entityType == ContentCacheDataSerializerEntityType.Document)
            {
                yield return CreateContentNodeKit(row, serializer, row.Published is false);
            }
            else
            {
                yield return CreateMediaNodeKit(row, serializer);
            }

        }
    }

    /// <inheritdoc />
    public IEnumerable<Guid> GetDocumentKeysByContentTypeKeys(IEnumerable<Guid> keys, bool published = false)
        => GetContentSourceByDocumentTypeKey(keys, Constants.ObjectTypes.Document).Where(x => x.Published == published).Select(x => x.Key);

    /// <inheritdoc />
    public IEnumerable<(Guid Key, bool IsDraft)> GetDocumentKeysWithPublishedStatus(IEnumerable<Guid> contentTypeKeys)
    {
        Guid[] keys = contentTypeKeys.ToArray();
        if (keys.Length == 0)
        {
            yield break;
        }

        // Lightweight query that only gets keys and published status - no serialized data
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Select<NodeDto>(x => x.UniqueId)
            .AndSelect<ContentNuDto>(x => x.Published)
            .From<ContentNuDto>()
            .InnerJoin<NodeDto>().On<ContentNuDto, NodeDto>((nu, n) => nu.NodeId == n.NodeId)
            .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((n, c) => n.NodeId == c.NodeId)
            .InnerJoin<NodeDto>("ct").On<ContentDto, NodeDto>((c, ct) => c.ContentTypeId == ct.NodeId, aliasRight: "ct")
            .Where<NodeDto>(n => n.NodeObjectType == Constants.ObjectTypes.Document)
            .WhereIn<NodeDto>(x => x.UniqueId, keys, "ct");

        foreach (DocumentKeysWithPublishedStatusDto row in Database.Fetch<DocumentKeysWithPublishedStatusDto>(sql))
        {
            yield return (row.UniqueId, row.Published is false);
        }
    }

    /// <summary>
    /// Lightweight DTO for cache refresh key queries.
    /// </summary>
    private class DocumentKeysWithPublishedStatusDto
    {
        public Guid UniqueId { get; set; }

        public bool Published { get; set; }
    }

    /// <inheritdoc />
    public IEnumerable<Guid> GetMediaKeysByContentTypeKeys(IEnumerable<Guid> mediaTypeKeys)
    {
        Guid[] keys = mediaTypeKeys.ToArray();
        if (keys.Length == 0)
        {
            yield break;
        }

        // Lightweight query that only gets keys - no serialized data.
        // Media items don't have published state, so we just need the keys.
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Select<NodeDto>(x => x.UniqueId)
            .From<ContentNuDto>()
            .InnerJoin<NodeDto>().On<ContentNuDto, NodeDto>((nu, n) => nu.NodeId == n.NodeId)
            .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((n, c) => n.NodeId == c.NodeId)
            .InnerJoin<NodeDto>("ct").On<ContentDto, NodeDto>((c, ct) => c.ContentTypeId == ct.NodeId, aliasRight: "ct")
            .Where<NodeDto>(n => n.NodeObjectType == Constants.ObjectTypes.Media)
            .WhereIn<NodeDto>(x => x.UniqueId, keys, "ct");

        foreach (MediaKeyDto row in Database.Fetch<MediaKeyDto>(sql))
        {
            yield return row.UniqueId;
        }
    }

    /// <summary>
    /// Lightweight DTO for media key queries.
    /// </summary>
    private class MediaKeyDto
    {
        public Guid UniqueId { get; set; }
    }

    /// <inheritdoc/>
    public async Task<ContentCacheNode?> GetMediaSourceAsync(Guid key)
    {
        Sql<ISqlContext>? sql = SqlMediaSourcesSelect()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
            .Append(SqlWhereNodeKey(SqlContext, key))
            .Append(SqlOrderByLevelIdSortOrder(SqlContext));

        ContentSourceDto? dto = await Database.FirstOrDefaultAsync<ContentSourceDto>(sql);

        if (dto is null)
        {
            return null;
        }

        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);
        return CreateMediaNodeKit(dto, serializer);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ContentCacheNode>> GetMediaSourcesAsync(IEnumerable<Guid> keys)
    {
        Sql<ISqlContext>? sql = SqlMediaSourcesSelect()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
            .WhereIn<NodeDto>(x => x.UniqueId, keys)
            .Append(SqlOrderByLevelIdSortOrder(SqlContext));

        List<ContentSourceDto> dtos = await Database.FetchAsync<ContentSourceDto>(sql);

        dtos = dtos
            .Where(x => x is not null)
            .ToList();

        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);
        return dtos
            .Select(x => CreateMediaNodeKit(x, serializer));
    }

    private async Task OnRepositoryRefreshed(IContentCacheDataSerializer serializer, ContentCacheNode content, bool preview)
    {
        ContentNuDto dto = GetDtoFromCacheNode(content, !preview, serializer);

        string c(string s) => SqlSyntax.GetQuotedColumnName(s);

        await Database.InsertOrUpdateAsync(
            dto,
            $"SET data = @data, {c("dataRaw")} = @dataRaw, rv = rv + 1 WHERE {c("nodeId")} = @id AND published = @published",
            new
            {
                dataRaw = dto.RawData ?? Array.Empty<byte>(),
                data = dto.Data,
                id = dto.NodeId,
                published = dto.Published,
            });
    }

    /// <summary>
    /// Rebuilds the content database cache for documents by clearing and repopulating the cache with the latest document data.
    /// </summary>
    /// <remarks>
    /// Assumes content tree lock.
    /// Uses an optimized query approach that bypasses IContent entity hydration for better performance (uses JOINs instead of
    /// WHERE IN clauses for better SQL Server query plan efficiency).
    /// </remarks>
    private void RebuildContentDbCache(IContentCacheDataSerializer serializer, int groupSize, IReadOnlyCollection<int>? contentTypeIds)
    {
        if (contentTypeIds is null)
        {
            return;
        }

        Guid contentObjectType = Constants.ObjectTypes.Document;

        // Remove all - if anything fails the transaction will rollback.
        RemoveByObjectType(contentObjectType, contentTypeIds);

        // Get total count for paging
        Sql<ISqlContext> countSql = Sql()
            .SelectCount()
            .From<NodeDto>()
            .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((n, c) => n.NodeId == c.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == contentObjectType);

        if (contentTypeIds.Count > 0)
        {
            countSql = countSql.WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);
        }

        var total = Database.ExecuteScalar<long>(countSql);

        if (total == 0)
        {
            return;
        }

        // Pre-fetch content type variations for all relevant content types.
        Dictionary<int, byte> contentTypeVariations = GetContentTypeVariations(contentTypeIds);

        // Pre-fetch all languages for culture code lookup.
        Dictionary<short, string> languageMap = GetLanguageMap();

        // Pre-fetch property info (aliases and variations) for all relevant content types (including compositions).
        Dictionary<int, List<PropertyTypeInfo>> propertyInfoByContentType = GetPropertyInfoByContentType(contentTypeIds);

        long processed = 0;
        long pageIndex = 0;

        while (processed < total)
        {
            // Get paged content node IDs.
            List<int> nodeIds = GetPagedContentNodeIds(contentObjectType, contentTypeIds, pageIndex, groupSize);
            if (nodeIds.Count == 0)
            {
                break;
            }

            // Fetch all data for these specific nodes (using more efficient JOINs than the WHERE IN approach used in ContentRepositoryBase).
            List<CacheRebuildDocumentDto> contentDtos = GetDocumentMetadataForNodes(nodeIds);
            List<CacheRebuildPropertyDto> propertyDtos = GetPropertyDataForNodes(nodeIds);
            List<CacheRebuildCultureDto> cultureDtos = GetCultureDataForNodes(nodeIds);
            List<CacheRebuildDocumentCultureDto> documentCultureDtos = GetDocumentCultureDataForNodes(nodeIds);

            // Build cache DTOs (without IContent entity hydration).
            // Parallelize the serialization for better performance on multi-core systems (saw 5-10% improvement doing this with a large test site).
            var items = contentDtos
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .SelectMany(content => BuildCacheDtosForDocument(
                    content,
                    propertyDtos,
                    cultureDtos,
                    documentCultureDtos,
                    contentTypeVariations,
                    languageMap,
                    propertyInfoByContentType,
                    serializer))
                .ToList();

            // Bulk insert the DTOs.
            Database.BulkInsertRecords(items);

            processed += nodeIds.Count;
            pageIndex++;
        }
    }

    /// <summary>
    /// Gets content type variations for the specified content type IDs.
    /// </summary>
    private Dictionary<int, byte> GetContentTypeVariations(IReadOnlyCollection<int> contentTypeIds)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<ContentTypeDto>(x => x.NodeId, x => x.Variations)
            .From<ContentTypeDto>();

        if (contentTypeIds.Count > 0)
        {
            sql = sql.WhereIn<ContentTypeDto>(x => x.NodeId, contentTypeIds);
        }

        return Database.Fetch<ContentTypeVariationDto>(sql)
            .ToDictionary(x => x.NodeId, x => x.Variations);
    }

    /// <summary>
    /// Gets a mapping of language ID to ISO code.
    /// </summary>
    private Dictionary<short, string> GetLanguageMap()
    {
        Sql<ISqlContext> sql = Sql()
            .Select<LanguageDto>(x => x.Id, x => x.IsoCode)
            .From<LanguageDto>();

        return Database.Fetch<LanguageDto>(sql)
            .ToDictionary(x => x.Id, x => x.IsoCode ?? string.Empty);
    }

    /// <summary>
    /// Gets paged content node IDs ordered by path. Used for documents, media, and members.
    /// </summary>
    private List<int> GetPagedContentNodeIds(Guid objectType, IReadOnlyCollection<int> contentTypeIds, long pageIndex, int pageSize)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<NodeDto>(x => x.NodeId)
            .From<NodeDto>()
            .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((n, c) => n.NodeId == c.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == objectType);

        if (contentTypeIds.Count > 0)
        {
            sql = sql.WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);
        }

        sql = sql.OrderBy<NodeDto>(x => x.Path);

        return Database.Page<int>(pageIndex + 1, pageSize, sql).Items;
    }

    /// <summary>
    /// Gets document metadata for the specified node IDs using efficient JOIN.
    /// </summary>
    private List<CacheRebuildDocumentDto> GetDocumentMetadataForNodes(List<int> nodeIds)
    {
        // Query content metadata with both edit and published version info
        // Uses nested join pattern to ensure we only get the published ContentVersion
        // (where a DocumentVersionDto with Published=true exists)
        Sql<ISqlContext> sql = Sql()
            .Select<NodeDto>(
                x => x.NodeId,
                x => x.UniqueId,
                x => x.Text,
                x => x.Path,
                x => x.Level,
                x => x.ParentId,
                x => x.SortOrder,
                x => x.CreateDate,
                x => Alias(x.UserId, "CreatorId"))
            .AndSelect<ContentDto>(x => x.ContentTypeId)
            .AndSelect<DocumentDto>(x => x.Published)
            .AndSelect<ContentVersionDto>(
                x => Alias(x.Id, "EditVersionId"),
                x => Alias(x.Text, "EditName"),
                x => Alias(x.VersionDate, "EditVersionDate"),
                x => Alias(x.UserId, "EditWriterId"))
            .AndSelect<ContentVersionDto>(
                "pcv",
                x => Alias(x.Id, "PublishedVersionId"),
                x => Alias(x.Text, "PublishedName"),
                x => Alias(x.VersionDate, "PublishedVersionDate"),
                x => Alias(x.UserId, "PublishedWriterId"))
            .From<NodeDto>()
            .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((n, c) => n.NodeId == c.NodeId)
            .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((n, d) => n.NodeId == d.NodeId)
            .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((n, cv) => n.NodeId == cv.NodeId && cv.Current)

            // Nested join: ContentVersionDto "pcv" INNER JOIN DocumentVersionDto "pdv" ON published=true
            // This ensures pcv only includes rows where there's a published DocumentVersion
            .LeftJoin<ContentVersionDto>(
                j => j.InnerJoin<DocumentVersionDto>("pdv")
                      .On<ContentVersionDto, DocumentVersionDto>(
                          (left, right) => left.Id == right.Id && right.Published == true, "pcv", "pdv"),
                "pcv")

            .On<NodeDto, ContentVersionDto>((n, cv) => n.NodeId == cv.NodeId, aliasRight: "pcv")
            .WhereIn<NodeDto>(x => x.NodeId, nodeIds);

        return Database.Fetch<CacheRebuildDocumentDto>(sql);
    }

    /// <summary>
    /// Gets property data for the specified node IDs using efficient JOIN on nodeId.
    /// This avoids the expensive WHERE IN on versionId that causes index scans.
    /// </summary>
    private List<CacheRebuildPropertyDto> GetPropertyDataForNodes(List<int> nodeIds)
    {
        // JOIN through nodeId → versionId path for efficient query plan
        Sql<ISqlContext> sql = Sql()
            .Select<PropertyDataDto>(
                x => x.VersionId,
                x => x.LanguageId,
                x => x.Segment,
                x => x.IntegerValue,
                x => x.DecimalValue,
                x => x.DateValue,
                x => x.VarcharValue,
                x => x.TextValue)
            .AndSelect<PropertyTypeDto>(x => Alias(x.Alias, "PropertyAlias"))
            .From<PropertyDataDto>()
            .InnerJoin<PropertyTypeDto>().On<PropertyDataDto, PropertyTypeDto>((pd, pt) => pd.PropertyTypeId == pt.Id)
            .InnerJoin<ContentVersionDto>().On<PropertyDataDto, ContentVersionDto>((pd, cv) => pd.VersionId == cv.Id)
            .WhereIn<ContentVersionDto>(x => x.NodeId, nodeIds);

        return Database.Fetch<CacheRebuildPropertyDto>(sql);
    }

    /// <summary>
    /// Gets culture variation data for the specified node IDs.
    /// </summary>
    private List<CacheRebuildCultureDto> GetCultureDataForNodes(List<int> nodeIds)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<ContentVersionCultureVariationDto>(x => x.VersionId, x => x.Name, x => x.UpdateDate)
            .AndSelect<LanguageDto>(x => Alias(x.IsoCode, "IsoCode"))
            .From<ContentVersionCultureVariationDto>()
            .InnerJoin<LanguageDto>().On<ContentVersionCultureVariationDto, LanguageDto>((cv, l) => cv.LanguageId == l.Id)
            .InnerJoin<ContentVersionDto>().On<ContentVersionCultureVariationDto, ContentVersionDto>((ccv, cv) => ccv.VersionId == cv.Id)
            .WhereIn<ContentVersionDto>(x => x.NodeId, nodeIds);

        return Database.Fetch<CacheRebuildCultureDto>(sql);
    }

    /// <summary>
    /// Gets document culture variation data (edited status per culture) for the specified node IDs.
    /// </summary>
    private List<CacheRebuildDocumentCultureDto> GetDocumentCultureDataForNodes(List<int> nodeIds)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<DocumentCultureVariationDto>(x => x.NodeId, x => x.Edited)
            .AndSelect<LanguageDto>(x => Alias(x.IsoCode, "IsoCode"))
            .From<DocumentCultureVariationDto>()
            .InnerJoin<LanguageDto>().On<DocumentCultureVariationDto, LanguageDto>((dcv, l) => dcv.LanguageId == l.Id)
            .WhereIn<DocumentCultureVariationDto>(x => x.NodeId, nodeIds);

        return Database.Fetch<CacheRebuildDocumentCultureDto>(sql);
    }

    /// <summary>
    /// Builds ContentNuDto entries for a single content item (both draft and published if applicable).
    /// </summary>
    private IEnumerable<ContentNuDto> BuildCacheDtosForDocument(
        CacheRebuildDocumentDto content,
        List<CacheRebuildPropertyDto> allPropertyDtos,
        List<CacheRebuildCultureDto> allCultureDtos,
        List<CacheRebuildDocumentCultureDto> allDocumentCultureDtos,
        Dictionary<int, byte> contentTypeVariations,
        Dictionary<short, string> languageMap,
        Dictionary<int, List<PropertyTypeInfo>> propertyInfoByContentType,
        IContentCacheDataSerializer serializer)
    {
        var results = new List<ContentNuDto>(2);

        // Get content type variation.
        byte contentTypeVariation = contentTypeVariations.TryGetValue(content.ContentTypeId, out var variations)
            ? variations
            : (byte)0;
        var variesByCulture = (contentTypeVariation & (byte)ContentVariation.Culture) > 0;

        // Get property info for this content type (including compositions).
        List<PropertyTypeInfo> propertyTypes = propertyInfoByContentType.TryGetValue(content.ContentTypeId, out List<PropertyTypeInfo>? props)
            ? props
            : [];

        // Filter data for this content's versions.
        var editVersionId = content.EditVersionId;
        var publishedVersionId = content.PublishedVersionId;

        // Build draft/edit version DTO.
        Dictionary<string, PropertyData[]> editPropertyData = BuildPropertyDataDictionary(
            allPropertyDtos.Where(p => p.VersionId == editVersionId),
            languageMap,
            propertyTypes,
            contentTypeVariation);

        // Create culture data dictionary.
        Dictionary<string, CultureVariation> editCultureData = variesByCulture
            ? BuildCultureDataDictionary(
                allCultureDtos.Where(c => c.VersionId == editVersionId),
                allDocumentCultureDtos.Where(d => d.NodeId == content.NodeId),
                published: false)
            : [];

        var editCacheData = new ContentCacheDataModel
        {
            PropertyData = editPropertyData,
            CultureData = editCultureData,
            UrlSegment = GenerateUrlSegment(content.EditName),
        };

        ContentCacheDataSerializationResult editSerialized = serializer.Serialize(
            new CacheRebuildDocumentAdapter(content, false),
            editCacheData,
            published: false);

        results.Add(new ContentNuDto
        {
            NodeId = content.NodeId,
            Published = false,
            Data = editSerialized.StringData,
            RawData = editSerialized.ByteData,
        });

        // Build published version DTO if published.
        if (content.Published && publishedVersionId.HasValue)
        {
            Dictionary<string, PropertyData[]> pubPropertyData = BuildPropertyDataDictionary(
                allPropertyDtos.Where(p => p.VersionId == publishedVersionId.Value),
                languageMap,
                propertyTypes,
                contentTypeVariation);

            // Create culture data dictionary.
            Dictionary<string, CultureVariation> pubCultureData = variesByCulture
                ? BuildCultureDataDictionary(
                    allCultureDtos.Where(c => c.VersionId == publishedVersionId.Value),
                    null, // No "edited" status for published
                    published: true)
                : [];

            var pubCacheData = new ContentCacheDataModel
            {
                PropertyData = pubPropertyData,
                CultureData = pubCultureData,
                UrlSegment = GenerateUrlSegment(content.PublishedName ?? content.EditName),
            };

            ContentCacheDataSerializationResult pubSerialized = serializer.Serialize(
                new CacheRebuildDocumentAdapter(content, true),
                pubCacheData,
                published: true);

            results.Add(new ContentNuDto
            {
                NodeId = content.NodeId,
                Published = true,
                Data = pubSerialized.StringData,
                RawData = pubSerialized.ByteData,
            });
        }

        return results;
    }

    /// <summary>
    /// Builds property data dictionary from property DTOs for invariant content types (media/members).
    /// Includes empty arrays for all defined property aliases to match original serialization behavior.
    /// </summary>
    private Dictionary<string, PropertyData[]> BuildPropertyDataDictionary(
        IEnumerable<CacheRebuildPropertyDto> propertyDtos,
        Dictionary<short, string> languageMap,
        List<string> propertyAliases)
    {
        // For media and members (invariant), create PropertyTypeInfo with no variations
        var propertyTypes = propertyAliases
            .Select(alias => new PropertyTypeInfo { Alias = alias, Variations = 0 })
            .ToList();
        return BuildPropertyDataDictionary(propertyDtos, languageMap, propertyTypes, contentTypeVariation: 0);
    }

    /// <summary>
    /// Builds property data dictionary from property DTOs.
    /// Includes empty arrays for all defined property aliases to match original serialization behavior.
    /// Filters property values based on effective variation (content type variation AND property type variation).
    /// </summary>
    private static Dictionary<string, PropertyData[]> BuildPropertyDataDictionary(
        IEnumerable<CacheRebuildPropertyDto> propertyDtos,
        Dictionary<short, string> languageMap,
        List<PropertyTypeInfo> propertyTypes,
        byte contentTypeVariation)
    {
        var result = new Dictionary<string, PropertyData[]>(StringComparer.OrdinalIgnoreCase);

        // Build a lookup for property variations.
        var propertyVariations = propertyTypes.ToDictionary(
            p => p.Alias,
            p => p.Variations,
            StringComparer.OrdinalIgnoreCase);

        // Initialize all property aliases with empty arrays to match original serialization behavior.
        foreach (PropertyTypeInfo prop in propertyTypes)
        {
            result[prop.Alias] = [];
        }

        IEnumerable<IGrouping<string, CacheRebuildPropertyDto>> grouped = propertyDtos.GroupBy(p => p.PropertyAlias);

        foreach (IGrouping<string, CacheRebuildPropertyDto> group in grouped)
        {
            // Calculate effective variation for this property.
            // A property only varies by culture if BOTH the content type AND the property type support it.
            byte propertyVariation = propertyVariations.TryGetValue(group.Key, out var pv) ? pv : (byte)0;
            var effectiveVariation = (byte)(contentTypeVariation & propertyVariation);
            var effectivelyVariesByCulture = (effectiveVariation & (byte)ContentVariation.Culture) != 0;

            var propertyDataList = new List<PropertyData>();

            foreach (CacheRebuildPropertyDto? prop in group.OrderBy(p => GetCultureCode((short?)p.LanguageId, languageMap)))
            {
                var value = GetPropertyValue(prop);
                if (value != null)
                {
                    var cultureCode = GetCultureCode((short?)prop.LanguageId, languageMap);

                    // If property is effectively invariant, only include values without a culture.
                    // If property is effectively variant, include all values.
                    if (effectivelyVariesByCulture || string.IsNullOrEmpty(cultureCode))
                    {
                        propertyDataList.Add(new PropertyData
                        {
                            Culture = effectivelyVariesByCulture ? cultureCode : string.Empty,
                            Segment = prop.Segment ?? string.Empty,
                            Value = value,
                        });
                    }
                }
            }

            if (propertyDataList.Count > 0)
            {
                result[group.Key] = propertyDataList.ToArray();
            }
        }

        return result;
    }

    /// <summary>
    /// Builds culture data dictionary from culture DTOs.
    /// </summary>
    private Dictionary<string, CultureVariation> BuildCultureDataDictionary(
        IEnumerable<CacheRebuildCultureDto> cultureDtos,
        IEnumerable<CacheRebuildDocumentCultureDto>? documentCultureDtos,
        bool published)
    {
        var cultureList = cultureDtos.ToList();
        if (cultureList.Count == 0)
        {
            return new Dictionary<string, CultureVariation>();
        }

        Dictionary<string, bool> editedByCulture = documentCultureDtos?.ToDictionary(d => d.IsoCode, d => d.Edited) ?? [];

        var result = new Dictionary<string, CultureVariation>(StringComparer.OrdinalIgnoreCase);

        foreach (CacheRebuildCultureDto culture in cultureList)
        {
            var isoCode = culture.IsoCode;
            var isDraft = !published && editedByCulture.TryGetValue(isoCode, out var edited) && edited;

            result[isoCode] = new CultureVariation
            {
                Name = culture.Name,
                UrlSegment = GenerateUrlSegment(culture.Name),
                Date = culture.UpdateDate,
                IsDraft = isDraft,
            };
        }

        return result;
    }

    /// <summary>
    /// Gets the culture code from language ID.
    /// </summary>
    private static string GetCultureCode(short? languageId, Dictionary<short, string> languageMap)
    {
        if (!languageId.HasValue)
        {
            return string.Empty;
        }

        return languageMap.TryGetValue(languageId.Value, out var code) ? code : string.Empty;
    }

    /// <summary>
    /// Gets the property value from the DTO, selecting the appropriate column.
    /// </summary>
    private static object? GetPropertyValue(CacheRebuildPropertyDto prop)
    {
        // Return the first non-null value in priority order
        if (prop.TextValue != null)
        {
            return prop.TextValue;
        }

        if (prop.VarcharValue != null)
        {
            return prop.VarcharValue;
        }

        if (prop.IntegerValue.HasValue)
        {
            return prop.IntegerValue.Value;
        }

        if (prop.DecimalValue.HasValue)
        {
            return prop.DecimalValue.Value;
        }

        if (prop.DateValue.HasValue)
        {
            return prop.DateValue.Value;
        }

        return null;
    }

    /// <summary>
    /// Generates a URL segment from the content name using the default algorithm.
    /// </summary>
    private string? GenerateUrlSegment(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return name.ToUrlSegment(_shortStringHelper);
    }

    /// <summary>
    /// Rebuilds the content database cache for media by clearing and repopulating the cache with the latest media data.
    /// </summary>
    /// <remarks>
    /// Assumes content tree lock.
    /// Uses an optimized query approach that bypasses IMedia entity hydration for better performance.
    /// </remarks>
    private void RebuildMediaDbCache(IContentCacheDataSerializer serializer, int groupSize, IReadOnlyCollection<int>? contentTypeIds)
    {
        if (contentTypeIds is null)
        {
            return;
        }

        Guid mediaObjectType = Constants.ObjectTypes.Media;

        // Remove all - if anything fails the transaction will rollback.
        RemoveByObjectType(mediaObjectType, contentTypeIds);

        // Get total count for paging.
        Sql<ISqlContext> countSql = Sql()
            .SelectCount()
            .From<NodeDto>()
            .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((n, c) => n.NodeId == c.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == mediaObjectType);

        if (contentTypeIds.Count > 0)
        {
            countSql = countSql.WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);
        }

        var total = Database.ExecuteScalar<long>(countSql);

        if (total == 0)
        {
            return;
        }

        // Pre-fetch property aliases for all content types to include empty arrays for properties without values.
        Dictionary<int, List<string>> propertyAliasesByContentType = GetPropertyAliasesByContentType(contentTypeIds);

        long processed = 0;
        long pageIndex = 0;

        while (processed < total)
        {
            // Get paged media node IDs.
            List<int> nodeIds = GetPagedContentNodeIds(mediaObjectType, contentTypeIds, pageIndex, groupSize);
            if (nodeIds.Count == 0)
            {
                break;
            }

            // Fetch all data for these specific nodes.
            List<CacheRebuildContentDto> mediaDtos = GetContentMetadataForNodes(nodeIds);
            List<CacheRebuildPropertyDto> propertyDtos = GetPropertyDataForNodes(nodeIds);

            // Build cache DTOs (without IMedia entity hydration).
            var items = mediaDtos
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .Select(media => BuildCacheDtoForContent(media, propertyDtos, propertyAliasesByContentType, serializer))
                .ToList();

            // Bulk insert the DTOs.
            Database.BulkInsertRecords(items);

            processed += nodeIds.Count;
            pageIndex++;
        }
    }

    /// <summary>
    /// Gets property type info (alias and variations) grouped by content type ID, including inherited and composed properties.
    /// </summary>
    private Dictionary<int, List<PropertyTypeInfo>> GetPropertyInfoByContentType(IReadOnlyCollection<int> contentTypeIds)
    {
        if (contentTypeIds.Count == 0)
        {
            return new Dictionary<int, List<PropertyTypeInfo>>();
        }

        // Get the composition hierarchy: which content types compose which other content types.
        // The cmsContentType2ContentType table stores: ParentId = composed type, ChildId = composing type.
        Dictionary<int, HashSet<int>> compositionsByChild = GetCompositionHierarchy(contentTypeIds);

        // Build set of all content type IDs we need properties for (including all compositions).
        var allContentTypeIds = new HashSet<int>(contentTypeIds);
        foreach (HashSet<int> compositions in compositionsByChild.Values)
        {
            allContentTypeIds.UnionWith(compositions);
        }

        // Get all property info (alias and variations) for all relevant content types.
        Sql<ISqlContext> sql = Sql()
            .Select<PropertyTypeDto>(x => x.Alias, x => x.Variations)
            .AndSelect<ContentTypeDto>(x => x.NodeId)
            .From<PropertyTypeDto>()
            .InnerJoin<ContentTypeDto>().On<PropertyTypeDto, ContentTypeDto>((pt, ct) => pt.ContentTypeId == ct.NodeId)
            .WhereIn<ContentTypeDto>(x => x.NodeId, allContentTypeIds);

        List<PropertyTypeAliasDto> results = Database.Fetch<PropertyTypeAliasDto>(sql);

        // Group properties by the content type that defines them.
        var propertiesByDefiningType = results
            .GroupBy(x => x.ContentTypeId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => new PropertyTypeInfo { Alias = x.Alias, Variations = x.Variations }).ToList());

        // Build the final result: for each requested content type, include its own properties
        // plus all properties from its compositions.
        var result = new Dictionary<int, List<PropertyTypeInfo>>();
        foreach (var contentTypeId in contentTypeIds)
        {
            var allProperties = new Dictionary<string, PropertyTypeInfo>(StringComparer.OrdinalIgnoreCase);

            // Add direct properties.
            if (propertiesByDefiningType.TryGetValue(contentTypeId, out List<PropertyTypeInfo>? directProperties))
            {
                foreach (PropertyTypeInfo prop in directProperties)
                {
                    allProperties[prop.Alias] = prop;
                }
            }

            // Add properties from compositions.
            if (compositionsByChild.TryGetValue(contentTypeId, out HashSet<int>? compositionIds))
            {
                foreach (var compositionId in compositionIds)
                {
                    if (propertiesByDefiningType.TryGetValue(compositionId, out List<PropertyTypeInfo>? composedProperties))
                    {
                        foreach (PropertyTypeInfo prop in composedProperties)
                        {
                            allProperties[prop.Alias] = prop;
                        }
                    }
                }
            }

            result[contentTypeId] = allProperties.Values.ToList();
        }

        return result;
    }

    /// <summary>
    /// Gets property type aliases grouped by content type ID, including inherited and composed properties.
    /// Used for media and members which don't need variation info.
    /// </summary>
    private Dictionary<int, List<string>> GetPropertyAliasesByContentType(IReadOnlyCollection<int> contentTypeIds)
    {
        Dictionary<int, List<PropertyTypeInfo>> propertyInfo = GetPropertyInfoByContentType(contentTypeIds);
        return propertyInfo.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Select(p => p.Alias).ToList());
    }

    /// <summary>
    /// Gets the composition hierarchy for the given content type IDs.
    /// Returns a dictionary where the key is the child content type ID and the value is the set of all
    /// parent/composed content type IDs (recursively resolved).
    /// </summary>
    private Dictionary<int, HashSet<int>> GetCompositionHierarchy(IReadOnlyCollection<int> contentTypeIds)
    {
        // Query all composition relationships.
        Sql<ISqlContext> sql = Sql()
            .Select<ContentType2ContentTypeDto>()
            .From<ContentType2ContentTypeDto>();

        List<ContentType2ContentTypeDto> allCompositions = Database.Fetch<ContentType2ContentTypeDto>(sql);

        // Build a lookup: childId -> list of direct parent IDs.
        var directParents = allCompositions
            .GroupBy(x => x.ChildId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ParentId).ToHashSet());

        // For each requested content type, recursively resolve all compositions.
        var result = new Dictionary<int, HashSet<int>>();
        foreach (var contentTypeId in contentTypeIds)
        {
            result[contentTypeId] = GetAllCompositionsRecursive(contentTypeId, directParents);
        }

        return result;
    }

    /// <summary>
    /// Recursively gets all composition content type IDs for a given content type.
    /// </summary>
    private HashSet<int> GetAllCompositionsRecursive(int contentTypeId, Dictionary<int, HashSet<int>> directParents)
    {
        var result = new HashSet<int>();

        if (!directParents.TryGetValue(contentTypeId, out HashSet<int>? parents))
        {
            return result;
        }

        foreach (var parentId in parents)
        {
            result.Add(parentId);

            // Recursively add compositions of compositions.
            result.UnionWith(GetAllCompositionsRecursive(parentId, directParents));
        }

        return result;
    }

    /// <summary>
    /// Gets content metadata for the specified node IDs using efficient JOIN. Used for media and members.
    /// </summary>
    private List<CacheRebuildContentDto> GetContentMetadataForNodes(List<int> nodeIds)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<NodeDto>(
                x => x.NodeId,
                x => x.UniqueId,
                x => x.Text,
                x => x.Path,
                x => x.Level,
                x => x.ParentId,
                x => x.SortOrder,
                x => x.CreateDate,
                x => Alias(x.UserId, "CreatorId"))
            .AndSelect<ContentDto>(x => x.ContentTypeId)
            .AndSelect<ContentVersionDto>(
                x => Alias(x.Id, "VersionId"),
                x => Alias(x.VersionDate, "VersionDate"),
                x => Alias(x.UserId, "WriterId"))
            .From<NodeDto>()
            .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((n, c) => n.NodeId == c.NodeId)
            .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((n, cv) => n.NodeId == cv.NodeId && cv.Current)
            .WhereIn<NodeDto>(x => x.NodeId, nodeIds);

        return Database.Fetch<CacheRebuildContentDto>(sql);
    }

    /// <summary>
    /// Builds a cache DTO for a single content item (media or member) without entity hydration.
    /// </summary>
    private ContentNuDto BuildCacheDtoForContent(
        CacheRebuildContentDto content,
        List<CacheRebuildPropertyDto> allPropertyDtos,
        Dictionary<int, List<string>> propertyAliasesByContentType,
        IContentCacheDataSerializer serializer)
    {
        // Get property aliases for this content type (including compositions).
        List<string> propertyAliases = propertyAliasesByContentType.TryGetValue(content.ContentTypeId, out List<string>? aliases)
            ? aliases
            : [];

        // Build property data dictionary for this content's version.
        // Includes empty arrays for all defined property aliases to match original serialization behavior.
        Dictionary<string, PropertyData[]> propertyData = BuildPropertyDataDictionary(
            allPropertyDtos.Where(p => p.VersionId == content.VersionId),
            new Dictionary<short, string>(),
            propertyAliases);

        // Media and members don't have culture data, but we include empty dict for consistency.
        var cultureData = new Dictionary<string, CultureVariation>();

        var cacheData = new ContentCacheDataModel
        {
            PropertyData = propertyData,
            CultureData = cultureData,
            UrlSegment = GenerateUrlSegment(content.Name),
        };

        ContentCacheDataSerializationResult serialized = serializer.Serialize(
            new CacheRebuildContentAdapter(content),
            cacheData,
            published: false);

        return new ContentNuDto
        {
            NodeId = content.NodeId,
            Published = false, // Media and members don't have published state
            Data = serialized.StringData,
            RawData = serialized.ByteData,
        };
    }

    /// <summary>
    /// Rebuilds the content database cache for members by clearing and repopulating the cache with the latest member data.
    /// </summary>
    /// <remarks>
    /// Assumes content tree lock.
    /// Uses an optimized query approach that bypasses IMember entity hydration for better performance.
    /// </remarks>
    private void RebuildMemberDbCache(IContentCacheDataSerializer serializer, int groupSize, IReadOnlyCollection<int>? contentTypeIds)
    {
        if (contentTypeIds is null)
        {
            return;
        }

        Guid memberObjectType = Constants.ObjectTypes.Member;

        // Remove all - if anything fails the transaction will rollback.
        RemoveByObjectType(memberObjectType, contentTypeIds);

        // Get total count for paging.
        Sql<ISqlContext> countSql = Sql()
            .SelectCount()
            .From<NodeDto>()
            .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((n, c) => n.NodeId == c.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == memberObjectType);

        if (contentTypeIds.Count > 0)
        {
            countSql = countSql.WhereIn<ContentDto>(x => x.ContentTypeId, contentTypeIds);
        }

        var total = Database.ExecuteScalar<long>(countSql);
        if (total == 0)
        {
            return;
        }

        // Pre-fetch property aliases for all content types to include empty arrays for properties without values.
        Dictionary<int, List<string>> propertyAliasesByContentType = GetPropertyAliasesByContentType(contentTypeIds);

        long processed = 0;
        long pageIndex = 0;

        while (processed < total)
        {
            // Get paged member node IDs.
            List<int> nodeIds = GetPagedContentNodeIds(memberObjectType, contentTypeIds, pageIndex, groupSize);
            if (nodeIds.Count == 0)
            {
                break;
            }

            // Fetch all data for these specific nodes - reuse the content metadata method since structure is identical.
            List<CacheRebuildContentDto> memberDtos = GetContentMetadataForNodes(nodeIds);
            List<CacheRebuildPropertyDto> propertyDtos = GetPropertyDataForNodes(nodeIds);

            // Build cache DTOs (without IMember entity hydration) - reuse the content builder since structure is identical.
            var items = memberDtos
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .Select(member => BuildCacheDtoForContent(member, propertyDtos, propertyAliasesByContentType, serializer))
                .ToList();

            // Bulk insert the DTOs.
            Database.BulkInsertRecords(items);

            processed += nodeIds.Count;
            pageIndex++;
        }
    }

    private void RemoveByObjectType(Guid objectType, IReadOnlyCollection<int> contentTypeIds)
    {
        // If the provided contentTypeIds collection is empty, remove all records for the provided object type.
        Sql<ISqlContext> sql;
        if (contentTypeIds.Count == 0)
        {
            sql = Sql()
                .Delete<ContentNuDto>()
                .WhereIn<ContentNuDto>(
                    x => x.NodeId,
                    Sql().Select<NodeDto>(n => n.NodeId)
                        .From<NodeDto>()
                        .Where<NodeDto>(n => n.NodeObjectType == objectType));
            Database.Execute(sql);
        }
        else
        {
            // Otherwise, if contentTypeIds are provided remove only those records that match the object type and one of the content types.
            sql = Sql()
                .Delete<ContentNuDto>()
                .WhereIn<ContentNuDto>(
                    x => x.NodeId,
                    Sql()
                    .Select<NodeDto>(n => n.NodeId)
                    .From<NodeDto>()
                    .InnerJoin<ContentDto>()
                    .On<NodeDto, ContentDto>((n, c) => n.NodeId == c.NodeId)
                    .Where<NodeDto, ContentDto>((n, c) =>
                        n.NodeObjectType == objectType &&
                        contentTypeIds.Contains(c.ContentTypeId)));
            Database.Execute(sql);
        }
    }

    private ContentNuDto GetDtoFromCacheNode(ContentCacheNode cacheNode, bool published, IContentCacheDataSerializer serializer)
    {
        // Prepare the data structure that will be serialized.
        var contentCacheData = new ContentCacheDataModel
        {
            PropertyData = cacheNode.Data?.Properties,
            CultureData = cacheNode.Data?.CultureInfos?.ToDictionary(),
            UrlSegment = cacheNode.Data?.UrlSegment,
        };

        // TODO: We should probably fix all serialization to only take ContentTypeId, for now it takes an IReadOnlyContentBase
        // but it is only the content type id that is needed.
        ContentCacheDataSerializationResult serialized = serializer.Serialize(new ContentSourceDto { ContentTypeId = cacheNode.ContentTypeId, }, contentCacheData, published);

        var dto = new ContentNuDto
        {
            NodeId = cacheNode.Id,
            Published = published,
            Data = serialized.StringData,
            RawData = serialized.ByteData,
        };

        return dto;
    }

    private Sql<ISqlContext> SqlContentSourcesSelect(Func<ISqlContext, Sql<ISqlContext>>? joins = null)
    {
        SqlTemplate sqlTemplate = SqlContext.Templates.Get(
            Constants.SqlTemplates.NuCacheDatabaseDataSource.ContentSourcesSelect,
            tsql =>
                tsql.Select<NodeDto>(
                        x => Alias(x.NodeId, "Id"),
                        x => Alias(x.UniqueId, "Key"),
                        x => Alias(x.Level, "Level"),
                        x => Alias(x.Path, "Path"),
                        x => Alias(x.SortOrder, "SortOrder"),
                        x => Alias(x.ParentId, "ParentId"),
                        x => Alias(x.CreateDate, "CreateDate"),
                        x => Alias(x.UserId, "CreatorId"))
                    .AndSelect<ContentDto>(x => Alias(x.ContentTypeId, "ContentTypeId"))
                    .AndSelect<DocumentDto>(x => Alias(x.Published, "Published"), x => Alias(x.Edited, "Edited"))
                    .AndSelect<ContentVersionDto>(
                        x => Alias(x.Id, "VersionId"),
                        x => Alias(x.Text, "EditName"),
                        x => Alias(x.VersionDate, "EditVersionDate"),
                        x => Alias(x.UserId, "EditWriterId"))
                    .AndSelect<DocumentVersionDto>(x => Alias(x.TemplateId, "EditTemplateId"))
                    .AndSelect<ContentVersionDto>(
                        "pcver",
                        x => Alias(x.Id, "PublishedVersionId"),
                        x => Alias(x.Text, "PubName"),
                        x => Alias(x.VersionDate, "PubVersionDate"),
                        x => Alias(x.UserId, "PubWriterId"))
                    .AndSelect<DocumentVersionDto>("pdver", x => Alias(x.TemplateId, "PubTemplateId"))
                    .AndSelect<ContentNuDto>("nuEdit", x => Alias(x.Data, "EditData"))
                    .AndSelect<ContentNuDto>("nuPub", x => Alias(x.Data, "PubData"))
                    .AndSelect<ContentNuDto>("nuEdit", x => Alias(x.RawData, "EditDataRaw"))
                    .AndSelect<ContentNuDto>("nuPub", x => Alias(x.RawData, "PubDataRaw"))
                    .From<NodeDto>());

        Sql<ISqlContext>? sql = sqlTemplate.Sql();

        // TODO: I'm unsure how we can format the below into SQL templates also because right.Current and right.Published end up being parameters
        if (joins != null)
        {
            sql = sql.Append(joins(sql.SqlContext));
        }

        sql = sql
            .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
            .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((left, right) => left.NodeId == right.NodeId)
            .InnerJoin<ContentVersionDto>()
            .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
            .InnerJoin<DocumentVersionDto>()
            .On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id)
            .LeftJoin<ContentVersionDto>(
                j =>
                    j.InnerJoin<DocumentVersionDto>("pdver")
                        .On<ContentVersionDto, DocumentVersionDto>(
                            (left, right) => left.Id == right.Id && right.Published == true, "pcver", "pdver"),
                "pcver")
            .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId, aliasRight: "pcver")
            .LeftJoin<ContentNuDto>("nuEdit").On<NodeDto, ContentNuDto>(
                (left, right) => left.NodeId == right.NodeId && right.Published == false, aliasRight: "nuEdit")
            .LeftJoin<ContentNuDto>("nuPub").On<NodeDto, ContentNuDto>(
                (left, right) => left.NodeId == right.NodeId && right.Published == true, aliasRight: "nuPub");

        return sql;
    }

    private Sql<ISqlContext> SqlWhereNodeKey(ISqlContext sqlContext, Guid key)
    {
        SqlTemplate sqlTemplate = sqlContext.Templates.Get(
            Constants.SqlTemplates.NuCacheDatabaseDataSource.WhereNodeKey,
            builder =>
                builder.Where<NodeDto>(x => x.UniqueId == SqlTemplate.Arg<Guid>("key")));
        Sql<ISqlContext> sql = sqlTemplate.Sql(key);
        return sql;
    }

    private Sql<ISqlContext> SqlOrderByLevelIdSortOrder(ISqlContext sqlContext)
    {
        SqlTemplate sqlTemplate = sqlContext.Templates.Get(
            Constants.SqlTemplates.NuCacheDatabaseDataSource.OrderByLevelIdSortOrder, s =>
                s.OrderBy<NodeDto>(x => x.Level, x => x.ParentId, x => x.SortOrder));
        Sql<ISqlContext> sql = sqlTemplate.Sql();
        return sql;
    }

    private Sql<ISqlContext> SqlObjectTypeNotTrashed(ISqlContext sqlContext, Guid nodeObjectType)
    {
        SqlTemplate sqlTemplate = sqlContext.Templates.Get(
            Constants.SqlTemplates.NuCacheDatabaseDataSource.ObjectTypeNotTrashedFilter, s =>
                s.Where<NodeDto>(x =>
                    x.NodeObjectType == SqlTemplate.Arg<Guid?>("nodeObjectType") &&
                    x.Trashed == SqlTemplate.Arg<bool>("trashed")));
        Sql<ISqlContext> sql = sqlTemplate.Sql(nodeObjectType, false);
        return sql;
    }

    /// <summary>
    /// Returns a slightly more optimized query to use for the document counting when paging over the content sources.
    /// </summary>
    private Sql<ISqlContext> SqlContentSourcesCount(Func<ISqlContext, Sql<ISqlContext>>? joins = null)
    {
        SqlTemplate sqlTemplate = SqlContext.Templates.Get(
            Constants.SqlTemplates.NuCacheDatabaseDataSource.ContentSourcesCount, tsql =>
                tsql.Select<NodeDto>(x => Alias(x.NodeId, "Id"))
                    .From<NodeDto>()
                    .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                    .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((left, right) => left.NodeId == right.NodeId));
        Sql<ISqlContext>? sql = sqlTemplate.Sql();

        if (joins != null)
        {
            sql = sql.Append(joins(sql.SqlContext));
        }

        // TODO: We can't use a template with this one because of the 'right.Current' and 'right.Published' ends up being a parameter so not sure how we can do that
        sql = sql
            .InnerJoin<ContentVersionDto>()
            .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
            .InnerJoin<DocumentVersionDto>()
            .On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id)
            .LeftJoin<ContentVersionDto>(
                j =>
                    j.InnerJoin<DocumentVersionDto>("pdver")
                        .On<ContentVersionDto, DocumentVersionDto>(
                            (left, right) => left.Id == right.Id && right.Published,
                            "pcver",
                            "pdver"),
                "pcver")
            .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId, aliasRight: "pcver");

        return sql;
    }

    private Sql<ISqlContext> SqlMediaSourcesSelect(Func<ISqlContext, Sql<ISqlContext>>? joins = null)
    {
        SqlTemplate sqlTemplate = SqlContext.Templates.Get(
            Constants.SqlTemplates.NuCacheDatabaseDataSource.MediaSourcesSelect, tsql =>
                tsql.Select<NodeDto>(
                        x => Alias(x.NodeId, "Id"),
                        x => Alias(x.UniqueId, "Key"),
                        x => Alias(x.Level, "Level"),
                        x => Alias(x.Path, "Path"),
                        x => Alias(x.SortOrder, "SortOrder"),
                        x => Alias(x.ParentId, "ParentId"),
                        x => Alias(x.CreateDate, "CreateDate"),
                        x => Alias(x.UserId, "CreatorId"))
                    .AndSelect<ContentDto>(x => Alias(x.ContentTypeId, "ContentTypeId"))
                    .AndSelect<ContentVersionDto>(
                        x => Alias(x.Id, "VersionId"),
                        x => Alias(x.Text, "EditName"),
                        x => Alias(x.VersionDate, "EditVersionDate"),
                        x => Alias(x.UserId, "EditWriterId"))
                    .AndSelect<ContentNuDto>("nuEdit", x => Alias(x.Data, "EditData"))
                    .AndSelect<ContentNuDto>("nuEdit", x => Alias(x.RawData, "EditDataRaw"))
                    .From<NodeDto>());

        Sql<ISqlContext>? sql = sqlTemplate.Sql();

        if (joins != null)
        {
            sql = sql.Append(joins(sql.SqlContext));
        }

        // TODO: We can't use a template with this one because of the 'right.Published' ends up being a parameter so not sure how we can do that
        sql = sql
            .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
            .InnerJoin<ContentVersionDto>()
            .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
            .LeftJoin<ContentNuDto>("nuEdit")
            .On<NodeDto, ContentNuDto>(
                (left, right) => left.NodeId == right.NodeId && !right.Published,
                aliasRight: "nuEdit");

        return sql;
    }

    private ContentCacheNode CreateContentNodeKit(ContentSourceDto dto, IContentCacheDataSerializer serializer, bool preview)
    {
        if (preview)
        {
            if (dto.EditData == null && dto.EditDataRaw == null)
            {
                if (Debugger.IsAttached)
                {
                    throw new InvalidOperationException("Missing cmsContentNu edited content for node " + dto.Id +
                                                        ", consider rebuilding.");
                }

                _logger.LogWarning(
                    "Missing cmsContentNu edited content for node {NodeId}, consider rebuilding.",
                    dto.Id);
            }
            else
            {
                bool published = false;
                ContentCacheDataModel? deserializedDraftContent =
                    serializer.Deserialize(dto, dto.EditData, dto.EditDataRaw, published);
                var draftContentData = new ContentData(
                    dto.EditName,
                    deserializedDraftContent?.UrlSegment,
                    dto.VersionId,
                    dto.EditVersionDate,
                    dto.CreatorId,
                    dto.EditTemplateId == 0 ? null : dto.EditTemplateId,
                    published,
                    deserializedDraftContent?.PropertyData,
                    deserializedDraftContent?.CultureData);

                return new ContentCacheNode
                {
                    Id = dto.Id,
                    Key = dto.Key,
                    SortOrder = dto.SortOrder,
                    CreateDate = dto.CreateDate,
                    CreatorId = dto.CreatorId,
                    ContentTypeId = dto.ContentTypeId,
                    Data = draftContentData,
                    IsDraft = true,
                };
            }
        }

        if (dto.PubData == null && dto.PubDataRaw == null)
        {
            if (Debugger.IsAttached)
            {
                throw new InvalidOperationException("Missing cmsContentNu published content for node " + dto.Id +
                                                    ", consider rebuilding.");
            }

            _logger.LogWarning(
                "Missing cmsContentNu published content for node {NodeId}, consider rebuilding.",
                dto.Id);
        }

        ContentCacheDataModel? deserializedContent = serializer.Deserialize(dto, dto.PubData, dto.PubDataRaw, true);
        var publishedContentData = new ContentData(
            dto.PubName,
            deserializedContent?.UrlSegment,
            dto.VersionId,
            dto.PubVersionDate,
            dto.CreatorId,
            dto.PubTemplateId == 0 ? null : dto.PubTemplateId,
            true,
            deserializedContent?.PropertyData,
            deserializedContent?.CultureData);

        return new ContentCacheNode
        {
            Id = dto.Id,
            Key = dto.Key,
            SortOrder = dto.SortOrder,
            CreateDate = dto.CreateDate,
            CreatorId = dto.CreatorId,
            ContentTypeId = dto.ContentTypeId,
            Data = publishedContentData,
            IsDraft = false,
        };
    }

    private ContentCacheNode CreateMediaNodeKit(ContentSourceDto dto, IContentCacheDataSerializer serializer)
    {
        if (dto.EditData == null && dto.EditDataRaw == null)
        {
            throw new InvalidOperationException("No data for media " + dto.Id);
        }

        ContentCacheDataModel? deserializedMedia = serializer.Deserialize(dto, dto.EditData, dto.EditDataRaw, true);

        var publishedContentData = new ContentData(
            dto.EditName,
            null,
            dto.VersionId,
            dto.EditVersionDate,
            dto.CreatorId,
            dto.EditTemplateId == 0 ? null : dto.EditTemplateId,
            true,
            deserializedMedia?.PropertyData,
            deserializedMedia?.CultureData);

        return new ContentCacheNode
        {
            Id = dto.Id,
            Key = dto.Key,
            SortOrder = dto.SortOrder,
            CreateDate = dto.CreateDate,
            CreatorId = dto.CreatorId,
            ContentTypeId = dto.ContentTypeId,
            Data = publishedContentData,
            IsDraft = false,
        };
    }

    private IEnumerable<ContentSourceDto> GetContentNodeDtos(Sql<ISqlContext> sql)
    {
        // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
        // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.
        // QueryPaged is very slow on large sites however, so use fetch if UsePagedSqlQuery is disabled.
        IEnumerable<ContentSourceDto> dtos;
        if (_nucacheSettings.Value.UsePagedSqlQuery)
        {
            // Use a more efficient COUNT query
            Sql<ISqlContext>? sqlCountQuery = SqlContentSourcesCount()
                .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document));

            Sql<ISqlContext>? sqlCount =
                SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

            dtos = Database.QueryPaged<ContentSourceDto>(_nucacheSettings.Value.SqlPageSize, sql, sqlCount);
        }
        else
        {
            dtos = Database.Fetch<ContentSourceDto>(sql);
        }

        return dtos;
    }

    #region Private DTOs for optimized cache rebuild

    /// <summary>
    /// Lightweight DTO for content type variation lookup.
    /// </summary>
    private sealed class ContentTypeVariationDto
    {
        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("variations")]
        public byte Variations { get; set; }
    }

    /// <summary>
    /// Lightweight DTO for content metadata during cache rebuild.
    /// </summary>
    private sealed class CacheRebuildDocumentDto
    {
        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("uniqueId")]
        public Guid Key { get; set; }

        [Column("text")]
        public string? Name { get; set; }

        [Column("path")]
        public string? Path { get; set; }

        [Column("level")]
        public int Level { get; set; }

        [Column("parentId")]
        public int ParentId { get; set; }

        [Column("sortOrder")]
        public int SortOrder { get; set; }

        [Column("createDate")]
        public DateTime CreateDate { get; set; }

        [Column("creatorId")]
        public int CreatorId { get; set; }

        [Column("contentTypeId")]
        public int ContentTypeId { get; set; }

        [Column("published")]
        public bool Published { get; set; }

        [Column("EditVersionId")]
        public int EditVersionId { get; set; }

        [Column("EditName")]
        public string? EditName { get; set; }

        [Column("EditVersionDate")]
        public DateTime EditVersionDate { get; set; }

        [Column("EditWriterId")]
        public int EditWriterId { get; set; }

        [Column("PublishedVersionId")]
        public int? PublishedVersionId { get; set; }

        [Column("PublishedName")]
        public string? PublishedName { get; set; }

        [Column("PublishedVersionDate")]
        public DateTime? PublishedVersionDate { get; set; }

        [Column("PublishedWriterId")]
        public int? PublishedWriterId { get; set; }
    }

    /// <summary>
    /// Lightweight DTO for property data during cache rebuild.
    /// </summary>
    private sealed class CacheRebuildPropertyDto
    {
        [Column("versionId")]
        public int VersionId { get; set; }

        [Column("PropertyAlias")]
        public string PropertyAlias { get; set; } = string.Empty;

        [Column("languageId")]
        public int? LanguageId { get; set; }

        [Column("segment")]
        public string? Segment { get; set; }

        [Column("intValue")]
        public int? IntegerValue { get; set; }

        [Column("decimalValue")]
        public decimal? DecimalValue { get; set; }

        [Column("dateValue")]
        public DateTime? DateValue { get; set; }

        [Column("varcharValue")]
        public string? VarcharValue { get; set; }

        [Column("textValue")]
        public string? TextValue { get; set; }
    }

    /// <summary>
    /// Lightweight DTO for culture variation data during cache rebuild.
    /// </summary>
    private sealed class CacheRebuildCultureDto
    {
        [Column("versionId")]
        public int VersionId { get; set; }

        [Column("IsoCode")]
        public string IsoCode { get; set; } = string.Empty;

        [Column("name")]
        public string? Name { get; set; }

        [Column("date")]
        public DateTime UpdateDate { get; set; }
    }

    /// <summary>
    /// Lightweight DTO for document culture variation (edited status per culture).
    /// </summary>
    private sealed class CacheRebuildDocumentCultureDto
    {
        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("IsoCode")]
        public string IsoCode { get; set; } = string.Empty;

        [Column("edited")]
        public bool Edited { get; set; }
    }

    /// <summary>
    /// Lightweight DTO for property type alias lookup.
    /// </summary>
    private sealed class PropertyTypeAliasDto
    {
        [Column("alias")]
        public string Alias { get; set; } = string.Empty;

        [Column("nodeId")]
        public int ContentTypeId { get; set; }

        [Column("variations")]
        public byte Variations { get; set; }
    }

    /// <summary>
    /// Property type information including alias and variation settings.
    /// </summary>
    private sealed class PropertyTypeInfo
    {
        public required string Alias { get; init; }

        public byte Variations { get; init; }
    }

    /// <summary>
    /// Minimal adapter for IReadOnlyContentBase to satisfy serializer requirements for documents.
    /// </summary>
    private sealed class CacheRebuildDocumentAdapter : IReadOnlyContentBase
    {
        private readonly CacheRebuildDocumentDto _content;
        private readonly bool _published;

        public CacheRebuildDocumentAdapter(CacheRebuildDocumentDto content, bool published)
        {
            _content = content;
            _published = published;
        }

        public int Id => _content.NodeId;
        public Guid Key => _content.Key;

        public DateTime CreateDate => _content.CreateDate;

        public DateTime UpdateDate => _published
            ? _content.PublishedVersionDate ?? _content.EditVersionDate
            : _content.EditVersionDate;

        public string? Name => _published
            ? _content.PublishedName ?? _content.EditName
            : _content.EditName;

        public int CreatorId => _content.CreatorId;

        public int ParentId => _content.ParentId;

        public int Level => _content.Level;

        public string? Path => _content.Path;

        public int SortOrder => _content.SortOrder;

        public int ContentTypeId => _content.ContentTypeId;

        public int WriterId => _published
            ? _content.PublishedWriterId ?? _content.EditWriterId
            : _content.EditWriterId;

        public int VersionId => _published
            ? _content.PublishedVersionId ?? _content.EditVersionId
            : _content.EditVersionId;
    }

    /// <summary>
    /// Lightweight DTO for content metadata during cache rebuild. Used for media and members.
    /// </summary>
    private sealed class CacheRebuildContentDto
    {
        [Column("nodeId")]
        public int NodeId { get; set; }

        [Column("uniqueId")]
        public Guid Key { get; set; }

        [Column("text")]
        public string? Name { get; set; }

        [Column("path")]
        public string? Path { get; set; }

        [Column("level")]
        public int Level { get; set; }

        [Column("parentId")]
        public int ParentId { get; set; }

        [Column("sortOrder")]
        public int SortOrder { get; set; }

        [Column("createDate")]
        public DateTime CreateDate { get; set; }

        [Column("creatorId")]
        public int CreatorId { get; set; }

        [Column("contentTypeId")]
        public int ContentTypeId { get; set; }

        [Column("VersionId")]
        public int VersionId { get; set; }

        [Column("VersionDate")]
        public DateTime VersionDate { get; set; }

        [Column("WriterId")]
        public int WriterId { get; set; }
    }

    /// <summary>
    /// Minimal adapter for IReadOnlyContentBase to satisfy serializer requirements for media and members.
    /// </summary>
    private sealed class CacheRebuildContentAdapter : IReadOnlyContentBase
    {
        private readonly CacheRebuildContentDto _content;

        public CacheRebuildContentAdapter(CacheRebuildContentDto content) => _content = content;

        public int Id => _content.NodeId;

        public Guid Key => _content.Key;

        public DateTime CreateDate => _content.CreateDate;

        public DateTime UpdateDate => _content.VersionDate;

        public string? Name => _content.Name;

        public int CreatorId => _content.CreatorId;

        public int ParentId => _content.ParentId;

        public int Level => _content.Level;

        public string? Path => _content.Path;

        public int SortOrder => _content.SortOrder;

        public int ContentTypeId => _content.ContentTypeId;

        public int WriterId => _content.WriterId;

        public int VersionId => _content.VersionId;
    }

    #endregion
}
