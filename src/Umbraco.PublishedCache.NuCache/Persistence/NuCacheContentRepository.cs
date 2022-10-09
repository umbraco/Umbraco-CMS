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
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Persistence.SqlExtensionsStatics;

namespace Umbraco.Cms.Infrastructure.PublishedCache.Persistence;

public class NuCacheContentRepository : RepositoryBase, INuCacheContentRepository
{
    private readonly IContentCacheDataSerializerFactory _contentCacheDataSerializerFactory;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<NuCacheContentRepository> _logger;
    private readonly IMediaRepository _mediaRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IOptions<NuCacheSettings> _nucacheSettings;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly UrlSegmentProviderCollection _urlSegmentProviders;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NuCacheContentRepository" /> class.
    /// </summary>
    public NuCacheContentRepository(
        IScopeAccessor scopeAccessor,
        AppCaches appCaches,
        ILogger<NuCacheContentRepository> logger,
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

    public void DeleteContentItem(IContentBase item)
        => Database.Execute("DELETE FROM cmsContentNu WHERE nodeId=@id", new { id = item.Id });

    public void RefreshContent(IContent content)
    {
        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);

        // always refresh the edited data
        OnRepositoryRefreshed(serializer, content, false);

        if (content.PublishedState == PublishedState.Unpublishing)
        {
            // if unpublishing, remove published data from table
            Database.Execute("DELETE FROM cmsContentNu WHERE nodeId=@id AND published=1", new { id = content.Id });
        }
        else if (content.PublishedState == PublishedState.Publishing)
        {
            // if publishing, refresh the published data
            OnRepositoryRefreshed(serializer, content, true);
        }
    }

    public void RefreshMedia(IMedia media)
    {
        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);

        OnRepositoryRefreshed(serializer, media, false);
    }

    public void RefreshMember(IMember member)
    {
        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Member);

        OnRepositoryRefreshed(serializer, member, false);
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

        if(contentTypeIds != null)
        {
            RebuildContentDbCache(serializer, _nucacheSettings.Value.SqlPageSize, contentTypeIds);
        }

        if (mediaTypeIds != null)
        {
            RebuildMediaDbCache(serializer, _nucacheSettings.Value.SqlPageSize, mediaTypeIds);
        }

        if (memberTypeIds != null)
        {
            RebuildMemberDbCache(serializer, _nucacheSettings.Value.SqlPageSize, memberTypeIds);
        }
    }

    // assumes content tree lock
    public bool VerifyContentDbCache()
    {
        // every document should have a corresponding row for edited properties
        // and if published, may have a corresponding row for published properties
        Guid contentObjectType = Constants.ObjectTypes.Document;

        var count = Database.ExecuteScalar<int>(
            $@"SELECT COUNT(*)
FROM umbracoNode
JOIN {Constants.DatabaseSchema.Tables.Document} ON umbracoNode.id={Constants.DatabaseSchema.Tables.Document}.nodeId
LEFT JOIN cmsContentNu nuEdited ON (umbracoNode.id=nuEdited.nodeId AND nuEdited.published=0)
LEFT JOIN cmsContentNu nuPublished ON (umbracoNode.id=nuPublished.nodeId AND nuPublished.published=1)
WHERE umbracoNode.nodeObjectType=@objType
AND nuEdited.nodeId IS NULL OR ({Constants.DatabaseSchema.Tables.Document}.published=1 AND nuPublished.nodeId IS NULL);",
            new { objType = contentObjectType });

        return count == 0;
    }

    // assumes media tree lock
    public bool VerifyMediaDbCache()
    {
        // every media item should have a corresponding row for edited properties
        Guid mediaObjectType = Constants.ObjectTypes.Media;

        var count = Database.ExecuteScalar<int>(
            @"SELECT COUNT(*)
FROM umbracoNode
LEFT JOIN cmsContentNu ON (umbracoNode.id=cmsContentNu.nodeId AND cmsContentNu.published=0)
WHERE umbracoNode.nodeObjectType=@objType
AND cmsContentNu.nodeId IS NULL
",
            new { objType = mediaObjectType });

        return count == 0;
    }

    // assumes member tree lock
    public bool VerifyMemberDbCache()
    {
        // every member item should have a corresponding row for edited properties
        Guid memberObjectType = Constants.ObjectTypes.Member;

        var count = Database.ExecuteScalar<int>(
            @"SELECT COUNT(*)
FROM umbracoNode
LEFT JOIN cmsContentNu ON (umbracoNode.id=cmsContentNu.nodeId AND cmsContentNu.published=0)
WHERE umbracoNode.nodeObjectType=@objType
AND cmsContentNu.nodeId IS NULL
",
            new { objType = memberObjectType });

        return count == 0;
    }

    public ContentNodeKit GetContentSource(int id)
    {
        Sql<ISqlContext>? sql = SqlContentSourcesSelect()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document))
            .Append(SqlWhereNodeId(SqlContext, id))
            .Append(SqlOrderByLevelIdSortOrder(SqlContext));

        ContentSourceDto? dto = Database.Fetch<ContentSourceDto>(sql).FirstOrDefault();

        if (dto == null)
        {
            return ContentNodeKit.Empty;
        }

        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);
        return CreateContentNodeKit(dto, serializer);
    }

    public IEnumerable<ContentNodeKit> GetAllContentSources()
    {
        Sql<ISqlContext>? sql = SqlContentSourcesSelect()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document))
            .Append(SqlOrderByLevelIdSortOrder(SqlContext));

        // Use a more efficient COUNT query
        Sql<ISqlContext>? sqlCountQuery = SqlContentSourcesCount()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document));

        Sql<ISqlContext>? sqlCount =
            SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);

        // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
        // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.
        foreach (ContentSourceDto row in Database.QueryPaged<ContentSourceDto>(_nucacheSettings.Value.SqlPageSize, sql, sqlCount))
        {
            yield return CreateContentNodeKit(row, serializer);
        }
    }

    public IEnumerable<ContentNodeKit> GetBranchContentSources(int id)
    {
        Sql<ISqlContext>? sql = SqlContentSourcesSelect(SqlContentSourcesSelectUmbracoNodeJoin)
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document))
            .Append(SqlWhereNodeIdX(SqlContext, id))
            .Append(SqlOrderByLevelIdSortOrder(SqlContext));

        // Use a more efficient COUNT query
        Sql<ISqlContext>? sqlCountQuery = SqlContentSourcesCount(SqlContentSourcesSelectUmbracoNodeJoin)
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document))
            .Append(SqlWhereNodeIdX(SqlContext, id));
        Sql<ISqlContext>? sqlCount =
            SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);

        // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
        // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.
        foreach (ContentSourceDto row in Database.QueryPaged<ContentSourceDto>(_nucacheSettings.Value.SqlPageSize, sql, sqlCount))
        {
            yield return CreateContentNodeKit(row, serializer);
        }
    }

    public IEnumerable<ContentNodeKit> GetTypeContentSources(IEnumerable<int>? ids)
    {
        if (!ids?.Any() ?? false)
        {
            yield break;
        }

        Sql<ISqlContext>? sql = SqlContentSourcesSelect()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document))
            .WhereIn<ContentDto>(x => x.ContentTypeId, ids)
            .Append(SqlOrderByLevelIdSortOrder(SqlContext));

        // Use a more efficient COUNT query
        Sql<ISqlContext> sqlCountQuery = SqlContentSourcesCount()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document))
            .WhereIn<ContentDto>(x => x.ContentTypeId, ids);
        Sql<ISqlContext>? sqlCount =
            SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);

        // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
        // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.
        foreach (ContentSourceDto row in Database.QueryPaged<ContentSourceDto>(_nucacheSettings.Value.SqlPageSize, sql, sqlCount))
        {
            yield return CreateContentNodeKit(row, serializer);
        }
    }

    public ContentNodeKit GetMediaSource(int id)
    {
        Sql<ISqlContext>? sql = SqlMediaSourcesSelect()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
            .Append(SqlWhereNodeId(SqlContext, id))
            .Append(SqlOrderByLevelIdSortOrder(SqlContext));

        ContentSourceDto? dto = Database.Fetch<ContentSourceDto>(sql).FirstOrDefault();

        if (dto == null)
        {
            return ContentNodeKit.Empty;
        }

        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);
        return CreateMediaNodeKit(dto, serializer);
    }

    public IEnumerable<ContentNodeKit> GetAllMediaSources()
    {
        Sql<ISqlContext>? sql = SqlMediaSourcesSelect()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
            .Append(SqlOrderByLevelIdSortOrder(SqlContext));

        // Use a more efficient COUNT query
        Sql<ISqlContext>? sqlCountQuery = SqlMediaSourcesCount()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media));
        Sql<ISqlContext>? sqlCount =
            SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);

        // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
        // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.
        foreach (ContentSourceDto row in Database.QueryPaged<ContentSourceDto>(_nucacheSettings.Value.SqlPageSize, sql, sqlCount))
        {
            yield return CreateMediaNodeKit(row, serializer);
        }
    }

    public IEnumerable<ContentNodeKit> GetBranchMediaSources(int id)
    {
        Sql<ISqlContext>? sql = SqlMediaSourcesSelect(SqlContentSourcesSelectUmbracoNodeJoin)
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
            .Append(SqlWhereNodeIdX(SqlContext, id))
            .Append(SqlOrderByLevelIdSortOrder(SqlContext));

        // Use a more efficient COUNT query
        Sql<ISqlContext>? sqlCountQuery = SqlMediaSourcesCount(SqlContentSourcesSelectUmbracoNodeJoin)
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
            .Append(SqlWhereNodeIdX(SqlContext, id));
        Sql<ISqlContext>? sqlCount =
            SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);

        // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
        // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.
        foreach (ContentSourceDto row in Database.QueryPaged<ContentSourceDto>(_nucacheSettings.Value.SqlPageSize, sql, sqlCount))
        {
            yield return CreateMediaNodeKit(row, serializer);
        }
    }

    public IEnumerable<ContentNodeKit> GetTypeMediaSources(IEnumerable<int> ids)
    {
        if (!ids.Any())
        {
            yield break;
        }

        Sql<ISqlContext>? sql = SqlMediaSourcesSelect()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
            .WhereIn<ContentDto>(x => x.ContentTypeId, ids)
            .Append(SqlOrderByLevelIdSortOrder(SqlContext));

        // Use a more efficient COUNT query
        Sql<ISqlContext> sqlCountQuery = SqlMediaSourcesCount()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
            .WhereIn<ContentDto>(x => x.ContentTypeId, ids);
        Sql<ISqlContext>? sqlCount =
            SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);

        // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
        // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.
        foreach (ContentSourceDto row in Database.QueryPaged<ContentSourceDto>(_nucacheSettings.Value.SqlPageSize, sql, sqlCount))
        {
            yield return CreateMediaNodeKit(row, serializer);
        }
    }

    public ContentNodeKit GetMediaSource(IScope scope, int id)
    {
        Sql<ISqlContext>? sql = SqlMediaSourcesSelect()
            .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
            .Append(SqlWhereNodeId(SqlContext, id))
            .Append(SqlOrderByLevelIdSortOrder(scope.SqlContext));

        ContentSourceDto? dto = scope.Database.Fetch<ContentSourceDto>(sql).FirstOrDefault();

        if (dto == null)
        {
            return ContentNodeKit.Empty;
        }

        IContentCacheDataSerializer serializer =
            _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);
        return CreateMediaNodeKit(dto, serializer);
    }

    private void OnRepositoryRefreshed(IContentCacheDataSerializer serializer, IContentBase content, bool published)
    {
        // use a custom SQL to update row version on each update
        // db.InsertOrUpdate(dto);
        ContentNuDto dto = GetDto(content, published, serializer);

        Database.InsertOrUpdate(
            dto,
            "SET data=@data, dataRaw=@dataRaw, rv=rv+1 WHERE nodeId=@id AND published=@published",
            new
            {
                dataRaw = dto.RawData ?? Array.Empty<byte>(),
                data = dto.Data,
                id = dto.NodeId,
                published = dto.Published,
            });
    }

    // assumes content tree lock
    private void RebuildContentDbCache(IContentCacheDataSerializer serializer, int groupSize, IReadOnlyCollection<int>? contentTypeIds)
    {
        Guid contentObjectType = Constants.ObjectTypes.Document;

        // remove all - if anything fails the transaction will rollback
        if (contentTypeIds == null || contentTypeIds.Count == 0)
        {
            // must support SQL-CE
            Database.Execute(
                @"DELETE FROM cmsContentNu
WHERE cmsContentNu.nodeId IN (
    SELECT id FROM umbracoNode WHERE umbracoNode.nodeObjectType=@objType
)",
                new { objType = contentObjectType });
        }
        else
        {
            // assume number of ctypes won't blow IN(...)
            // must support SQL-CE
            Database.Execute(
                $@"DELETE FROM cmsContentNu
WHERE cmsContentNu.nodeId IN (
    SELECT id FROM umbracoNode
    JOIN {Constants.DatabaseSchema.Tables.Content} ON {Constants.DatabaseSchema.Tables.Content}.nodeId=umbracoNode.id
    WHERE umbracoNode.nodeObjectType=@objType
    AND {Constants.DatabaseSchema.Tables.Content}.contentTypeId IN (@ctypes)
)",
                new { objType = contentObjectType, ctypes = contentTypeIds });
        }

        // insert back - if anything fails the transaction will rollback
        IQuery<IContent> query = SqlContext.Query<IContent>();
        if (contentTypeIds != null && contentTypeIds.Count > 0)
        {
            query = query.WhereIn(x => x.ContentTypeId, contentTypeIds); // assume number of ctypes won't blow IN(...)
        }

        long pageIndex = 0;
        long processed = 0;
        long total;
        do
        {
            // the tree is locked, counting and comparing to total is safe
            IEnumerable<IContent> descendants =
                _documentRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
            var items = new List<ContentNuDto>();
            var count = 0;
            foreach (IContent c in descendants)
            {
                // always the edited version
                items.Add(GetDto(c, false, serializer));

                // and also the published version if it makes any sense
                if (c.Published)
                {
                    items.Add(GetDto(c, true, serializer));
                }

                count++;
            }

            Database.BulkInsertRecords(items);
            processed += count;
        }
        while (processed < total);
    }

    // assumes media tree lock
    private void RebuildMediaDbCache(IContentCacheDataSerializer serializer, int groupSize, IReadOnlyCollection<int>? contentTypeIds)
    {
        Guid mediaObjectType = Constants.ObjectTypes.Media;

        // remove all - if anything fails the transaction will rollback
        if (contentTypeIds == null || contentTypeIds.Count == 0)
        {
            // must support SQL-CE
            Database.Execute(
                @"DELETE FROM cmsContentNu
WHERE cmsContentNu.nodeId IN (
    SELECT id FROM umbracoNode WHERE umbracoNode.nodeObjectType=@objType
)",
                new { objType = mediaObjectType });
        }
        else
        {
            // assume number of ctypes won't blow IN(...)
            // must support SQL-CE
            Database.Execute(
                $@"DELETE FROM cmsContentNu
WHERE cmsContentNu.nodeId IN (
    SELECT id FROM umbracoNode
    JOIN {Constants.DatabaseSchema.Tables.Content} ON {Constants.DatabaseSchema.Tables.Content}.nodeId=umbracoNode.id
    WHERE umbracoNode.nodeObjectType=@objType
    AND {Constants.DatabaseSchema.Tables.Content}.contentTypeId IN (@ctypes)
)",
                new { objType = mediaObjectType, ctypes = contentTypeIds });
        }

        // insert back - if anything fails the transaction will rollback
        IQuery<IMedia> query = SqlContext.Query<IMedia>();
        if (contentTypeIds != null && contentTypeIds.Count > 0)
        {
            query = query.WhereIn(x => x.ContentTypeId, contentTypeIds); // assume number of ctypes won't blow IN(...)
        }

        long pageIndex = 0;
        long processed = 0;
        long total;
        do
        {
            // the tree is locked, counting and comparing to total is safe
            IEnumerable<IMedia> descendants =
                _mediaRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
            var items = descendants.Select(m => GetDto(m, false, serializer)).ToList();
            Database.BulkInsertRecords(items);
            processed += items.Count;
        }
        while (processed < total);
    }

    // assumes member tree lock
    private void RebuildMemberDbCache(IContentCacheDataSerializer serializer, int groupSize, IReadOnlyCollection<int>? contentTypeIds)
    {
        Guid memberObjectType = Constants.ObjectTypes.Member;

        // remove all - if anything fails the transaction will rollback
        if (contentTypeIds == null || contentTypeIds.Count == 0)
        {
            // must support SQL-CE
            Database.Execute(
                @"DELETE FROM cmsContentNu
WHERE cmsContentNu.nodeId IN (
    SELECT id FROM umbracoNode WHERE umbracoNode.nodeObjectType=@objType
)",
                new { objType = memberObjectType });
        }
        else
        {
            // assume number of ctypes won't blow IN(...)
            // must support SQL-CE
            Database.Execute(
                $@"DELETE FROM cmsContentNu
WHERE cmsContentNu.nodeId IN (
    SELECT id FROM umbracoNode
    JOIN {Constants.DatabaseSchema.Tables.Content} ON {Constants.DatabaseSchema.Tables.Content}.nodeId=umbracoNode.id
    WHERE umbracoNode.nodeObjectType=@objType
    AND {Constants.DatabaseSchema.Tables.Content}.contentTypeId IN (@ctypes)
)",
                new { objType = memberObjectType, ctypes = contentTypeIds });
        }

        // insert back - if anything fails the transaction will rollback
        IQuery<IMember> query = SqlContext.Query<IMember>();
        if (contentTypeIds != null && contentTypeIds.Count > 0)
        {
            query = query.WhereIn(x => x.ContentTypeId, contentTypeIds); // assume number of ctypes won't blow IN(...)
        }

        long pageIndex = 0;
        long processed = 0;
        long total;
        do
        {
            IEnumerable<IMember> descendants =
                _memberRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
            ContentNuDto[] items = descendants.Select(m => GetDto(m, false, serializer)).ToArray();
            Database.BulkInsertRecords(items);
            processed += items.Length;
        }
        while (processed < total);
    }

    private ContentNuDto GetDto(IContentBase content, bool published, IContentCacheDataSerializer serializer)
    {
        // should inject these in ctor
        // BUT for the time being we decide not to support ConvertDbToXml/String
        // var propertyEditorResolver = PropertyEditorResolver.Current;
        // var dataTypeService = ApplicationContext.Current.Services.DataTypeService;
        var propertyData = new Dictionary<string, PropertyData[]>();
        foreach (IProperty prop in content.Properties)
        {
            var pdatas = new List<PropertyData>();
            foreach (IPropertyValue pvalue in prop.Values.OrderBy(x => x.Culture))
            {
                // sanitize - properties should be ok but ... never knows
                if (!prop.PropertyType.SupportsVariation(pvalue.Culture, pvalue.Segment))
                {
                    continue;
                }

                // note: at service level, invariant is 'null', but here invariant becomes 'string.Empty'
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

        // sanitize - names should be ok but ... never knows
        if (content.ContentType.VariesByCulture())
        {
            ContentCultureInfosCollection? infos = content is IContent document
                ? published
                    ? document.PublishCultureInfos
                    : document.CultureInfos
                : content.CultureInfos;

            // ReSharper disable once UseDeconstruction
            if (infos is not null)
            {
                foreach (ContentCultureInfos cultureInfo in infos)
                {
                    var cultureIsDraft = !published && content is IContent d && d.IsCultureEdited(cultureInfo.Culture);
                    cultureData[cultureInfo.Culture] = new CultureVariation
                    {
                        Name = cultureInfo.Name,
                        UrlSegment =
                            content.GetUrlSegment(_shortStringHelper, _urlSegmentProviders, cultureInfo.Culture),
                        Date = content.GetUpdateDate(cultureInfo.Culture) ?? DateTime.MinValue,
                        IsDraft = cultureIsDraft,
                    };
                }
            }
        }

        // the dictionary that will be serialized
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
            NodeId = content.Id,
            Published = published,
            Data = serialized.StringData,
            RawData = serialized.ByteData,
        };

        return dto;
    }

    // we want arrays, we want them all loaded, not an enumerable
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

    private Sql<ISqlContext> SqlContentSourcesSelectUmbracoNodeJoin(ISqlContext sqlContext)
    {
        ISqlSyntaxProvider syntax = sqlContext.SqlSyntax;

        SqlTemplate sqlTemplate = sqlContext.Templates.Get(
            Constants.SqlTemplates.NuCacheDatabaseDataSource.SourcesSelectUmbracoNodeJoin, builder =>
                builder.InnerJoin<NodeDto>("x")
                    .On<NodeDto, NodeDto>(
                        (left, right) => left.NodeId == right.NodeId ||
                                         SqlText<bool>(left.Path, right.Path, (lp, rp) => $"({lp} LIKE {syntax.GetConcat(rp, "',%'")})"),
                        aliasRight: "x"));

        Sql<ISqlContext> sql = sqlTemplate.Sql();
        return sql;
    }

    private Sql<ISqlContext> SqlWhereNodeId(ISqlContext sqlContext, int id)
    {
        ISqlSyntaxProvider syntax = sqlContext.SqlSyntax;

        SqlTemplate sqlTemplate = sqlContext.Templates.Get(
            Constants.SqlTemplates.NuCacheDatabaseDataSource.WhereNodeId,
            builder =>
                builder.Where<NodeDto>(x => x.NodeId == SqlTemplate.Arg<int>("id")));

        Sql<ISqlContext> sql = sqlTemplate.Sql(id);
        return sql;
    }

    private Sql<ISqlContext> SqlWhereNodeIdX(ISqlContext sqlContext, int id)
    {
        ISqlSyntaxProvider syntax = sqlContext.SqlSyntax;

        SqlTemplate sqlTemplate = sqlContext.Templates.Get(
            Constants.SqlTemplates.NuCacheDatabaseDataSource.WhereNodeIdX, s =>
                s.Where<NodeDto>(x => x.NodeId == SqlTemplate.Arg<int>("id"), "x"));

        Sql<ISqlContext> sql = sqlTemplate.Sql(id);
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
    ///     Returns a slightly more optimized query to use for the document counting when paging over the content sources
    /// </summary>
    /// <param name="joins"></param>
    /// <returns></returns>
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

    private Sql<ISqlContext> SqlMediaSourcesCount(Func<ISqlContext, Sql<ISqlContext>>? joins = null)
    {
        SqlTemplate sqlTemplate = SqlContext.Templates.Get(
            Constants.SqlTemplates.NuCacheDatabaseDataSource.MediaSourcesCount, tsql =>
                tsql.Select<NodeDto>(x => Alias(x.NodeId, "Id")).From<NodeDto>());

        Sql<ISqlContext>? sql = sqlTemplate.Sql();

        if (joins != null)
        {
            sql = sql.Append(joins(sql.SqlContext));
        }

        // TODO: We can't use a template with this one because of the 'right.Current' ends up being a parameter so not sure how we can do that
        sql = sql
            .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
            .InnerJoin<ContentVersionDto>()
            .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current);

        return sql;
    }

    private ContentNodeKit CreateContentNodeKit(ContentSourceDto dto, IContentCacheDataSerializer serializer)
    {
        ContentData? d = null;
        ContentData? p = null;

        if (dto.Edited)
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
                var published = false;
                ContentCacheDataModel? deserializedContent =
                    serializer.Deserialize(dto, dto.EditData, dto.EditDataRaw, published);

                d = new ContentData(
                    dto.EditName,
                    deserializedContent?.UrlSegment,
                    dto.VersionId,
                    dto.EditVersionDate,
                    dto.EditWriterId,
                    dto.EditTemplateId == 0 ? null : dto.EditTemplateId,
                    published,
                    deserializedContent?.PropertyData,
                    deserializedContent?.CultureData);
            }
        }

        if (dto.Published)
        {
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
            else
            {
                var published = true;
                ContentCacheDataModel? deserializedContent =
                    serializer.Deserialize(dto, dto.PubData, dto.PubDataRaw, published);

                p = new ContentData(
                    dto.PubName,
                    deserializedContent?.UrlSegment,
                    dto.VersionId,
                    dto.PubVersionDate,
                    dto.PubWriterId,
                    dto.PubTemplateId == 0 ? null : dto.PubTemplateId,
                    published,
                    deserializedContent?.PropertyData,
                    deserializedContent?.CultureData);
            }
        }

        var n = new ContentNode(dto.Id, dto.Key, dto.Level, dto.Path, dto.SortOrder, dto.ParentId, dto.CreateDate, dto.CreatorId);

        var s = new ContentNodeKit(n, dto.ContentTypeId, d, p);

        return s;
    }

    private ContentNodeKit CreateMediaNodeKit(ContentSourceDto dto, IContentCacheDataSerializer serializer)
    {
        if (dto.EditData == null && dto.EditDataRaw == null)
        {
            throw new InvalidOperationException("No data for media " + dto.Id);
        }

        var published = true;
        ContentCacheDataModel? deserializedMedia =
            serializer.Deserialize(dto, dto.EditData, dto.EditDataRaw, published);

        var p = new ContentData(
            dto.EditName,
            null,
            dto.VersionId,
            dto.EditVersionDate,
            dto.CreatorId,
            -1,
            published,
            deserializedMedia?.PropertyData,
            deserializedMedia?.CultureData);

        var n = new ContentNode(dto.Id, dto.Key, dto.Level, dto.Path, dto.SortOrder, dto.ParentId, dto.CreateDate, dto.CreatorId);

        var s = new ContentNodeKit(n, dto.ContentTypeId, null, p);

        return s;
    }
}
