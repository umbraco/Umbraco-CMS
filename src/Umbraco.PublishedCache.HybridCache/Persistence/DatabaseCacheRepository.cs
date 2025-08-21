using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Persistence.SqlExtensionsStatics;

namespace Umbraco.Cms.Infrastructure.HybridCache.Persistence;

/// <inheritdoc/>
internal sealed class DatabaseCacheRepository : RepositoryBase, IDatabaseCacheRepository
{
    private readonly IContentCacheDataSerializerFactory _contentCacheDataSerializerFactory;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<DatabaseCacheRepository> _logger;
    private readonly IMediaRepository _mediaRepository;
    private readonly IMemberRepository _memberRepository;
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
        IMemberRepository memberRepository,
        IDocumentRepository documentRepository,
        IMediaRepository mediaRepository,
        IShortStringHelper shortStringHelper,
        UrlSegmentProviderCollection urlSegmentProviders,
        IContentCacheDataSerializerFactory contentCacheDataSerializerFactory,
        IOptions<NuCacheSettings> nucacheSettings)
        : base(scopeAccessor, appCaches)
    {
        _logger = logger;
        _memberRepository = memberRepository;
        _documentRepository = documentRepository;
        _mediaRepository = mediaRepository;
        _shortStringHelper = shortStringHelper;
        _urlSegmentProviders = urlSegmentProviders;
        _contentCacheDataSerializerFactory = contentCacheDataSerializerFactory;
        _nucacheSettings = nucacheSettings;
    }

    /// <inheritdoc/>
    public async Task DeleteContentItemAsync(int id)
        => await Database.ExecuteAsync("DELETE FROM cmsContentNu WHERE nodeId = @id", new { id });

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
                await Database.ExecuteAsync("DELETE FROM cmsContentNu WHERE nodeId = @id AND published = 1", new { id = contentCacheNode.Id });
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
        Sql<ISqlContext>? sql = SqlContentSourcesSelect()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document))
            .Append(SqlWhereNodeKey(SqlContext, key))
            .Append(SqlOrderByLevelIdSortOrder(SqlContext));

        ContentSourceDto? dto = await Database.FirstOrDefaultAsync<ContentSourceDto>(sql);

        if (dto == null)
        {
            return null;
        }

        if (preview is false && dto.PubDataRaw is null && dto.PubData is null)
        {
            return null;
        }

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
                .WhereIn<NodeDto>(x => x.UniqueId, keys,"n")
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

        await Database.InsertOrUpdateAsync(
            dto,
            "SET data = @data, dataRaw = @dataRaw, rv = rv + 1 WHERE nodeId = @id AND published = @published",
            new
            {
                dataRaw = dto.RawData ?? Array.Empty<byte>(),
                data = dto.Data,
                id = dto.NodeId,
                published = dto.Published,
            });
    }

    /// <summary>
    /// Rebuilds the content database cache for documents.
    /// </summary>
    /// <remarks>
    /// Assumes content tree lock.
    /// </remarks>
    private void RebuildContentDbCache(IContentCacheDataSerializer serializer, int groupSize, IReadOnlyCollection<int>? contentTypeIds)
    {
        if (contentTypeIds is null)
        {
            return;
        }

        Guid contentObjectType = Constants.ObjectTypes.Document;

        // Remove all - if anything fails the transaction will rollback.
        if (contentTypeIds.Count == 0)
        {
            DeleteForObjectType(contentObjectType);
        }
        else
        {
            DeleteForObjectTypeAndContentTypes(contentObjectType, contentTypeIds);
        }

        // Insert back - if anything fails the transaction will rollback.
        IQuery<IContent> query = GetInsertQuery<IContent>(contentTypeIds);

        long pageIndex = 0;
        long processed = 0;
        long total;
        do
        {
            // The tree is locked, counting and comparing to total is safe.
            IEnumerable<IContent> descendants =
                _documentRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
            var items = new List<ContentNuDto>();
            var count = 0;
            foreach (IContent c in descendants)
            {
                // Always include the edited version.
                items.Add(GetDtoFromContent(c, false, serializer));

                // And also the published version if the document is published.
                if (c.Published)
                {
                    items.Add(GetDtoFromContent(c, true, serializer));
                }

                count++;
            }

            Database.BulkInsertRecords(items);
            processed += count;
        }
        while (processed < total);
    }

    /// <summary>
    /// Rebuilds the content database cache for media.
    /// </summary>
    /// <remarks>
    /// Assumes media tree lock.
    /// </remarks>
    private void RebuildMediaDbCache(IContentCacheDataSerializer serializer, int groupSize, IReadOnlyCollection<int>? contentTypeIds)
    {
        if (contentTypeIds is null)
        {
            return;
        }

        Guid mediaObjectType = Constants.ObjectTypes.Media;

        // Remove all - if anything fails the transaction will rollback.
        if (contentTypeIds.Count == 0)
        {
            DeleteForObjectType(mediaObjectType);
        }
        else
        {
            DeleteForObjectTypeAndContentTypes(mediaObjectType, contentTypeIds);
        }

        // Insert back - if anything fails the transaction will rollback.
        IQuery<IMedia> query = GetInsertQuery<IMedia>(contentTypeIds);

        long pageIndex = 0;
        long processed = 0;
        long total;
        do
        {
            // The tree is locked, counting and comparing to total is safe.
            IEnumerable<IMedia> descendants =
                _mediaRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
            ContentNuDto[] items = descendants.Select(m => GetDtoFromContent(m, false, serializer)).ToArray();
            Database.BulkInsertRecords(items);
            processed += items.Length;
        }
        while (processed < total);
    }

    /// <summary>
    /// Rebuilds the content database cache for members.
    /// </summary>
    /// <remarks>
    /// Assumes member tree lock.
    /// </remarks>
    private void RebuildMemberDbCache(IContentCacheDataSerializer serializer, int groupSize, IReadOnlyCollection<int>? contentTypeIds)
    {
        if (contentTypeIds is null)
        {
            return;
        }

        Guid memberObjectType = Constants.ObjectTypes.Member;

        // Remove all - if anything fails the transaction will rollback.
        if (contentTypeIds.Count == 0)
        {
            DeleteForObjectType(memberObjectType);
        }
        else
        {
            DeleteForObjectTypeAndContentTypes(memberObjectType, contentTypeIds);
        }

        // Insert back - if anything fails the transaction will rollback.
        IQuery<IMember> query = GetInsertQuery<IMember>(contentTypeIds);

        long pageIndex = 0;
        long processed = 0;
        long total;
        do
        {
            IEnumerable<IMember> descendants =
                _memberRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
            ContentNuDto[] items = descendants.Select(m => GetDtoFromContent(m, false, serializer)).ToArray();
            Database.BulkInsertRecords(items);
            processed += items.Length;
        }
        while (processed < total);
    }

    private void DeleteForObjectType(Guid nodeObjectType) =>
        Database.Execute(
            @"
                DELETE FROM cmsContentNu
                WHERE cmsContentNu.nodeId IN (
                    SELECT id FROM umbracoNode WHERE umbracoNode.nodeObjectType = @objType
                )",
            new { objType = nodeObjectType });

    private void DeleteForObjectTypeAndContentTypes(Guid nodeObjectType, IReadOnlyCollection<int> contentTypeIds) =>
        Database.Execute(
            $@"
                DELETE FROM cmsContentNu
                WHERE cmsContentNu.nodeId IN (
                    SELECT id FROM umbracoNode
                    JOIN {Constants.DatabaseSchema.Tables.Content} ON {Constants.DatabaseSchema.Tables.Content}.nodeId=umbracoNode.id
                    WHERE umbracoNode.nodeObjectType = @objType
                    AND {Constants.DatabaseSchema.Tables.Content}.contentTypeId IN (@ctypes)
                )",
            new { objType = nodeObjectType, ctypes = contentTypeIds });

    private IQuery<TContent> GetInsertQuery<TContent>(IReadOnlyCollection<int> contentTypeIds)
        where TContent : IContentBase
    {
        IQuery<TContent> query = SqlContext.Query<TContent>();
        if (contentTypeIds.Count > 0)
        {
            query = query.WhereIn(x => x.ContentTypeId, contentTypeIds);
        }

        return query;
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
            NodeId = cacheNode.Id, Published = published, Data = serialized.StringData, RawData = serialized.ByteData,
        };

        return dto;
    }

    private ContentNuDto GetDtoFromContent(IContentBase content, bool published, IContentCacheDataSerializer serializer)
    {
        var propertyData = new Dictionary<string, PropertyData[]>();
        foreach (IProperty prop in content.Properties)
        {
            var pdatas = new List<PropertyData>();
            foreach (IPropertyValue pvalue in prop.Values.OrderBy(x => x.Culture))
            {
                // Sanitize - properties should be ok but ... never knows.
                if (!prop.PropertyType.SupportsVariation(pvalue.Culture, pvalue.Segment))
                {
                    continue;
                }

                // Note: at service level, invariant is 'null', but here invariant becomes 'string.Empty'
                var value = published ? pvalue.PublishedValue : pvalue.EditedValue;
                if (value != null)
                {
                    pdatas.Add(new PropertyData
                    {
                        Culture = pvalue.Culture ?? string.Empty,
                        Segment = pvalue.Segment ?? string.Empty,
                        Value = value,
                    });
                }
            }

            propertyData[prop.Alias] = pdatas.ToArray();
        }

        var cultureData = new Dictionary<string, CultureVariation>();

        // Sanitize - names should be ok but ... never knows.
        if (content.ContentType.VariesByCulture())
        {
            ContentCultureInfosCollection? infos = content is IContent document
                ? published
                    ? document.PublishCultureInfos
                    : document.CultureInfos
                : content.CultureInfos;

            if (infos is not null)
            {
                foreach (ContentCultureInfos cultureInfo in infos)
                {
                    var cultureIsDraft = !published && content is IContent d && d.IsCultureEdited(cultureInfo.Culture);
                    cultureData[cultureInfo.Culture] = new CultureVariation
                    {
                        Name = cultureInfo.Name,
                        UrlSegment = content.GetUrlSegment(_shortStringHelper, _urlSegmentProviders, cultureInfo.Culture),
                        Date = content.GetUpdateDate(cultureInfo.Culture) ?? DateTime.MinValue,
                        IsDraft = cultureIsDraft,
                    };
                }
            }
        }

        // Prepare the data structure that will be serialized.
        var contentCacheData = new ContentCacheDataModel
        {
            PropertyData = propertyData,
            CultureData = cultureData,
            UrlSegment = content.GetUrlSegment(_shortStringHelper, _urlSegmentProviders),
        };

        ContentCacheDataSerializationResult serialized =
            serializer.Serialize(ReadOnlyContentBaseAdapter.Create(content), contentCacheData, published);

        var dto = new ContentNuDto
        {
            NodeId = content.Id, Published = published, Data = serialized.StringData, RawData = serialized.ByteData,
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
        ISqlSyntaxProvider syntax = sqlContext.SqlSyntax;

        SqlTemplate sqlTemplate = sqlContext.Templates.Get(
            Constants.SqlTemplates.NuCacheDatabaseDataSource.WhereNodeKey,
            builder =>
                builder.Where<NodeDto>(x => x.UniqueId == SqlTemplate.Arg<Guid>("key")));

        Sql<ISqlContext> sql = sqlTemplate.Sql(key);
        return sql;
    }

    private Sql<ISqlContext> SqlOrderByLevelIdSortOrder(ISqlContext sqlContext)
    {
        ISqlSyntaxProvider syntax = sqlContext.SqlSyntax;

        SqlTemplate sqlTemplate = sqlContext.Templates.Get(
            Constants.SqlTemplates.NuCacheDatabaseDataSource.OrderByLevelIdSortOrder, s =>
                s.OrderBy<NodeDto>(x => x.Level, x => x.ParentId, x => x.SortOrder));

        Sql<ISqlContext> sql = sqlTemplate.Sql();
        return sql;
    }

    private Sql<ISqlContext> SqlObjectTypeNotTrashed(ISqlContext sqlContext, Guid nodeObjectType)
    {
        ISqlSyntaxProvider syntax = sqlContext.SqlSyntax;

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
}
