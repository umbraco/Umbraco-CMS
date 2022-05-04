using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Persistence.SqlExtensionsStatics;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.PublishedCache.Persistence
{
    public class NuCacheContentRepository : RepositoryBase, INuCacheContentRepository
    {
        private readonly ILogger<NuCacheContentRepository> _logger;
        private readonly IMemberRepository _memberRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly UrlSegmentProviderCollection _urlSegmentProviders;
        private readonly IContentCacheDataSerializerFactory _contentCacheDataSerializerFactory;
        private readonly IOptions<NuCacheSettings> _nucacheSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuCacheContentRepository"/> class.
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
            IContentCacheDataSerializer serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);

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
            IContentCacheDataSerializer serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);

            OnRepositoryRefreshed(serializer, media, false);
        }

        public void RefreshMember(IMember member)
        {
            IContentCacheDataSerializer serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Member);

            OnRepositoryRefreshed(serializer, member, false);
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
                    published = dto.Published
                });
        }

        public void Rebuild(
            IReadOnlyCollection<int>? contentTypeIds = null,
            IReadOnlyCollection<int>? mediaTypeIds = null,
            IReadOnlyCollection<int>? memberTypeIds = null)
        {
            IContentCacheDataSerializer serializer = _contentCacheDataSerializerFactory.Create(
                ContentCacheDataSerializerEntityType.Document
                | ContentCacheDataSerializerEntityType.Media
                | ContentCacheDataSerializerEntityType.Member);

            // If contentTypeIds, mediaTypeIds and memberTypeIds are null, truncate table as all records will be deleted (as these 3 are the only types in the table).
            /* TODO: If large % of records would be deleted it may be faster to
             * 1. insert the records that won't be deleted into a temp table
             * 2. truncate cmsContentNu
             * 3. insert the records from the temp table back into cmsContentNu
             * 4. Delete the temp table
            */
            if ((contentTypeIds == null || !contentTypeIds.Any())
                && (mediaTypeIds == null || !mediaTypeIds.Any())
                && (memberTypeIds == null || !memberTypeIds.Any()))
            {
                Database.Execute("TRUNCATE TABLE cmsContentNu");
            }

            RebuildContentDbCache(serializer, _nucacheSettings.Value.SqlPageSize, contentTypeIds);
            RebuildMediaDbCache(serializer, _nucacheSettings.Value.SqlPageSize, mediaTypeIds);
            RebuildMemberDbCache(serializer, _nucacheSettings.Value.SqlPageSize, memberTypeIds);
        }

        // assumes content tree lock
        private void RebuildContentDbCache(IContentCacheDataSerializer serializer, int groupSize, IReadOnlyCollection<int>? contentTypeIds)
        {
            Guid contentObjectType = Constants.ObjectTypes.Document;

            // remove all - if anything fails the transaction will rollback
            if (contentTypeIds == null || contentTypeIds.Count == 0)
            {
                // must support SQL-CE
                if (Database.DatabaseType is NPoco.DatabaseTypes.SqlServerCEDatabaseType)
                {
                    Database.Execute(
                   @"DELETE FROM cmsContentNu
WHERE cmsContentNu.nodeId IN (
    SELECT id FROM umbracoNode WHERE umbracoNode.nodeObjectType=@objType
)",
                   new { objType = contentObjectType });
                }
                else
                {
                    Database.Execute(
                  @"DELETE cmsContentNu FROM cmsContentNu
  INNER JOIN umbracoNode ON cmsContentNu.nodeId = umbracoNode.id
  WHERE umbracoNode.nodeObjectType=@objType",
                  new { objType = contentObjectType });
                }
            }
            else
            {
                // assume number of ctypes won't blow IN(...)
                if (Database.DatabaseType is NPoco.DatabaseTypes.SqlServerCEDatabaseType)
                {
                    // must support SQL-CE
                    Database.Execute(
                    $@"DELETE FROM cmsContentNu
WHERE cmsContentNu.nodeId IN (
    SELECT id FROM umbracoNode
    JOIN {Constants.DatabaseSchema.Tables.Content} ON {Constants.DatabaseSchema.Tables.Content}.nodeId=umbracoNode.id
    WHERE umbracoNode.nodeObjectType=@objType
    AND {Constants.DatabaseSchema.Tables.Content}.contentTypeId IN (@ctypes)",
                    new { objType = contentObjectType, ctypes = contentTypeIds });
                }
                else
                {
                    Database.Execute(
                   $@"DELETE cmsContentNu FROM cmsContentNu
  INNER JOIN umbracoNode ON umbracoNode.id = cmsContentNu.nodeId
  INNER JOIN {Constants.DatabaseSchema.Tables.Content} ON {Constants.DatabaseSchema.Tables.Content}.nodeId=umbracoNode.id
  WHERE {Constants.DatabaseSchema.Tables.Content}.contentTypeId IN (@ctypes)
  AND umbracoNode.nodeObjectType=@objType",
                   new { objType = contentObjectType, ctypes = contentTypeIds });
                }
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
                IEnumerable<IContent> descendants = _documentRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
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
            } while (processed < total);
        }

        // assumes media tree lock
        private void RebuildMediaDbCache(IContentCacheDataSerializer serializer, int groupSize, IReadOnlyCollection<int>? contentTypeIds)
        {
            var mediaObjectType = Constants.ObjectTypes.Media;

            // remove all - if anything fails the transaction will rollback
            if (contentTypeIds == null || contentTypeIds.Count == 0)
            {
                if (Database.DatabaseType is NPoco.DatabaseTypes.SqlServerCEDatabaseType)
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
                    Database.Execute(
                 @"DELETE cmsContentNu FROM cmsContentNu
  INNER JOIN umbracoNode ON cmsContentNu.nodeId = umbracoNode.id
  WHERE umbracoNode.nodeObjectType=@objType",
                 new { objType = mediaObjectType });
                }
            }
            else
            {
                // assume number of ctypes won't blow IN(...)
                if (Database.DatabaseType is NPoco.DatabaseTypes.SqlServerCEDatabaseType)
                {
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
                else
                {
                    Database.Execute(
                  $@"DELETE cmsContentNu FROM cmsContentNu
  INNER JOIN umbracoNode ON umbracoNode.id = cmsContentNu.nodeId
  INNER JOIN {Constants.DatabaseSchema.Tables.Content} ON {Constants.DatabaseSchema.Tables.Content}.nodeId=umbracoNode.id
  WHERE {Constants.DatabaseSchema.Tables.Content}.contentTypeId IN (@ctypes)
  AND umbracoNode.nodeObjectType=@objType",
                  new { objType = mediaObjectType, ctypes = contentTypeIds });
                }
            }

            // insert back - if anything fails the transaction will rollback
            var query = SqlContext.Query<IMedia>();
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
                var descendants = _mediaRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
                var items = descendants.Select(m => GetDto(m, false, serializer)).ToList();
                Database.BulkInsertRecords(items);
                processed += items.Count;
            } while (processed < total);
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
                IEnumerable<IMember> descendants = _memberRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
                ContentNuDto[] items = descendants.Select(m => GetDto(m, false, serializer)).ToArray();
                Database.BulkInsertRecords(items);
                processed += items.Length;
            } while (processed < total);
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
", new { objType = mediaObjectType });

            return count == 0;
        }

        // assumes member tree lock
        public bool VerifyMemberDbCache()
        {
            // every member item should have a corresponding row for edited properties
            var memberObjectType = Constants.ObjectTypes.Member;

            var count = Database.ExecuteScalar<int>(
                @"SELECT COUNT(*)
FROM umbracoNode
LEFT JOIN cmsContentNu ON (umbracoNode.id=cmsContentNu.nodeId AND cmsContentNu.published=0)
WHERE umbracoNode.nodeObjectType=@objType
AND cmsContentNu.nodeId IS NULL
", new { objType = memberObjectType });

            return count == 0;
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
                        pdatas.Add(new PropertyData { Culture = pvalue.Culture ?? string.Empty, Segment = pvalue.Segment ?? string.Empty, Value = value });
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
                            UrlSegment = content.GetUrlSegment(_shortStringHelper, _urlSegmentProviders, cultureInfo.Culture),
                            Date = content.GetUpdateDate(cultureInfo.Culture) ?? DateTime.MinValue,
                            IsDraft = cultureIsDraft
                        };
                    }
                }
            }

            // the dictionary that will be serialized
            var contentCacheData = new ContentCacheDataModel
            {
                PropertyData = propertyData,
                CultureData = cultureData,
                UrlSegment = content.GetUrlSegment(_shortStringHelper, _urlSegmentProviders)
            };

            var serialized = serializer.Serialize(ReadOnlyContentBaseAdapter.Create(content), contentCacheData, published);

            var dto = new ContentNuDto
            {
                NodeId = content.Id,
                Published = published,
                Data = serialized.StringData,
                RawData = serialized.ByteData
            };

            return dto;
        }

        // we want arrays, we want them all loaded, not an enumerable
        private Sql<ISqlContext> SqlContentSourcesSelect(Func<ISqlContext, Sql<ISqlContext>>? joins = null)
        {
            var sqlTemplate = SqlContext.Templates.Get(Constants.SqlTemplates.NuCacheDatabaseDataSource.ContentSourcesSelect, tsql =>
                tsql.Select<NodeDto>(x => Alias(x.NodeId, "Id"), x => Alias(x.UniqueId, "Key"),
                    x => Alias(x.Level, "Level"), x => Alias(x.Path, "Path"), x => Alias(x.SortOrder, "SortOrder"), x => Alias(x.ParentId, "ParentId"),
                    x => Alias(x.CreateDate, "CreateDate"), x => Alias(x.UserId, "CreatorId"))
                .AndSelect<ContentDto>(x => Alias(x.ContentTypeId, "ContentTypeId"))
                .AndSelect<DocumentDto>(x => Alias(x.Published, "Published"), x => Alias(x.Edited, "Edited"))

                .AndSelect<ContentVersionDto>(x => Alias(x.Id, "VersionId"), x => Alias(x.Text, "EditName"), x => Alias(x.VersionDate, "EditVersionDate"), x => Alias(x.UserId, "EditWriterId"))
                .AndSelect<DocumentVersionDto>(x => Alias(x.TemplateId, "EditTemplateId"))

                .AndSelect<ContentVersionDto>("pcver", x => Alias(x.Id, "PublishedVersionId"), x => Alias(x.Text, "PubName"), x => Alias(x.VersionDate, "PubVersionDate"), x => Alias(x.UserId, "PubWriterId"))
                .AndSelect<DocumentVersionDto>("pdver", x => Alias(x.TemplateId, "PubTemplateId"))

                .AndSelect<ContentNuDto>("nuEdit", x => Alias(x.Data, "EditData"))
                .AndSelect<ContentNuDto>("nuPub", x => Alias(x.Data, "PubData"))

                .AndSelect<ContentNuDto>("nuEdit", x => Alias(x.RawData, "EditDataRaw"))
                .AndSelect<ContentNuDto>("nuPub", x => Alias(x.RawData, "PubDataRaw"))

                .From<NodeDto>());

            var sql = sqlTemplate.Sql();

            // TODO: I'm unsure how we can format the below into SQL templates also because right.Current and right.Published end up being parameters

            if (joins != null)
                sql = sql.Append(joins(sql.SqlContext));

            sql = sql
                .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((left, right) => left.NodeId == right.NodeId)

                .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
                .InnerJoin<DocumentVersionDto>().On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id)

                .LeftJoin<ContentVersionDto>(j =>
                    j.InnerJoin<DocumentVersionDto>("pdver").On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id && right.Published == true, "pcver", "pdver"), "pcver")
                .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId, aliasRight: "pcver")

                .LeftJoin<ContentNuDto>("nuEdit").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && right.Published == false, aliasRight: "nuEdit")
                .LeftJoin<ContentNuDto>("nuPub").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && right.Published == true, aliasRight: "nuPub");

            return sql;
        }

        private Sql<ISqlContext> SqlContentSourcesSelectUmbracoNodeJoin(ISqlContext sqlContext)
        {
            var syntax = sqlContext.SqlSyntax;

            var sqlTemplate = sqlContext.Templates.Get(Constants.SqlTemplates.NuCacheDatabaseDataSource.SourcesSelectUmbracoNodeJoin, builder =>
                builder.InnerJoin<NodeDto>("x")
                    .On<NodeDto, NodeDto>((left, right) => left.NodeId == right.NodeId || SqlText<bool>(left.Path, right.Path, (lp, rp) => $"({lp} LIKE {syntax.GetConcat(rp, "',%'")})"), aliasRight: "x"));

            var sql = sqlTemplate.Sql();
            return sql;
        }

        private Sql<ISqlContext> SqlWhereNodeId(ISqlContext sqlContext, int id)
        {
            var syntax = sqlContext.SqlSyntax;

            var sqlTemplate = sqlContext.Templates.Get(Constants.SqlTemplates.NuCacheDatabaseDataSource.WhereNodeId, builder =>
                builder.Where<NodeDto>(x => x.NodeId == SqlTemplate.Arg<int>("id")));

            var sql = sqlTemplate.Sql(id);
            return sql;
        }

        private Sql<ISqlContext> SqlWhereNodeIdX(ISqlContext sqlContext, int id)
        {
            var syntax = sqlContext.SqlSyntax;

            var sqlTemplate = sqlContext.Templates.Get(Constants.SqlTemplates.NuCacheDatabaseDataSource.WhereNodeIdX, s =>
                s.Where<NodeDto>(x => x.NodeId == SqlTemplate.Arg<int>("id"), "x"));

            var sql = sqlTemplate.Sql(id);
            return sql;
        }

        private Sql<ISqlContext> SqlOrderByLevelIdSortOrder(ISqlContext sqlContext)
        {
            var syntax = sqlContext.SqlSyntax;

            var sqlTemplate = sqlContext.Templates.Get(Constants.SqlTemplates.NuCacheDatabaseDataSource.OrderByLevelIdSortOrder, s =>
                s.OrderBy<NodeDto>(x => x.Level, x => x.ParentId, x => x.SortOrder));

            var sql = sqlTemplate.Sql();
            return sql;
        }

        private Sql<ISqlContext> SqlObjectTypeNotTrashed(ISqlContext sqlContext, Guid nodeObjectType)
        {
            var syntax = sqlContext.SqlSyntax;

            var sqlTemplate = sqlContext.Templates.Get(Constants.SqlTemplates.NuCacheDatabaseDataSource.ObjectTypeNotTrashedFilter, s =>
                s.Where<NodeDto>(x => x.NodeObjectType == SqlTemplate.Arg<Guid?>("nodeObjectType") && x.Trashed == SqlTemplate.Arg<bool>("trashed")));

            var sql = sqlTemplate.Sql(nodeObjectType, false);
            return sql;
        }

        /// <summary>
        /// Returns a slightly more optimized query to use for the document counting when paging over the content sources
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        private Sql<ISqlContext> SqlContentSourcesCount(Func<ISqlContext, Sql<ISqlContext>>? joins = null)
        {
            var sqlTemplate = SqlContext.Templates.Get(Constants.SqlTemplates.NuCacheDatabaseDataSource.ContentSourcesCount, tsql =>
                tsql.Select<NodeDto>(x => Alias(x.NodeId, "Id"))
                    .From<NodeDto>()
                    .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                    .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((left, right) => left.NodeId == right.NodeId));

            var sql = sqlTemplate.Sql();

            if (joins != null)
                sql = sql.Append(joins(sql.SqlContext));

            // TODO: We can't use a template with this one because of the 'right.Current' and 'right.Published' ends up being a parameter so not sure how we can do that
            sql = sql
                .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
                .InnerJoin<DocumentVersionDto>().On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id)
                .LeftJoin<ContentVersionDto>(j =>
                        j.InnerJoin<DocumentVersionDto>("pdver").On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id && right.Published, "pcver", "pdver"), "pcver")
                    .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId, aliasRight: "pcver");

            return sql;
        }

        private Sql<ISqlContext> SqlMediaSourcesSelect(Func<ISqlContext, Sql<ISqlContext>>? joins = null)
        {
            var sqlTemplate = SqlContext.Templates.Get(Constants.SqlTemplates.NuCacheDatabaseDataSource.MediaSourcesSelect, tsql =>
                tsql.Select<NodeDto>(x => Alias(x.NodeId, "Id"), x => Alias(x.UniqueId, "Key"),
                    x => Alias(x.Level, "Level"), x => Alias(x.Path, "Path"), x => Alias(x.SortOrder, "SortOrder"), x => Alias(x.ParentId, "ParentId"),
                    x => Alias(x.CreateDate, "CreateDate"), x => Alias(x.UserId, "CreatorId"))
                .AndSelect<ContentDto>(x => Alias(x.ContentTypeId, "ContentTypeId"))
                .AndSelect<ContentVersionDto>(x => Alias(x.Id, "VersionId"), x => Alias(x.Text, "EditName"), x => Alias(x.VersionDate, "EditVersionDate"), x => Alias(x.UserId, "EditWriterId"))
                .AndSelect<ContentNuDto>("nuEdit", x => Alias(x.Data, "EditData"))
                .AndSelect<ContentNuDto>("nuEdit", x => Alias(x.RawData, "EditDataRaw"))
                .From<NodeDto>());

            var sql = sqlTemplate.Sql();

            if (joins != null)
                sql = sql.Append(joins(sql.SqlContext));

            // TODO: We can't use a template with this one because of the 'right.Published' ends up being a parameter so not sure how we can do that
            sql = sql
                .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
                .LeftJoin<ContentNuDto>("nuEdit").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && !right.Published, aliasRight: "nuEdit");

            return sql;
        }

        private Sql<ISqlContext> SqlMediaSourcesCount(Func<ISqlContext, Sql<ISqlContext>>? joins = null)
        {
            var sqlTemplate = SqlContext.Templates.Get(Constants.SqlTemplates.NuCacheDatabaseDataSource.MediaSourcesCount, tsql =>
               tsql.Select<NodeDto>(x => Alias(x.NodeId, "Id")).From<NodeDto>());

            var sql = sqlTemplate.Sql();

            if (joins != null)
                sql = sql.Append(joins(sql.SqlContext));

            // TODO: We can't use a template with this one because of the 'right.Current' ends up being a parameter so not sure how we can do that
            sql = sql
                .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current);

            return sql;
        }

        public ContentNodeKit GetContentSource(int id)
        {
            var sql = SqlContentSourcesSelect()
                .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document))
                .Append(SqlWhereNodeId(SqlContext, id))
                .Append(SqlOrderByLevelIdSortOrder(SqlContext));

            var dto = Database.Fetch<ContentSourceDto>(sql).FirstOrDefault();

            if (dto == null) return ContentNodeKit.Empty;

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);
            return CreateContentNodeKit(dto, serializer);
        }

        public IEnumerable<ContentNodeKit> GetAllContentSources()
        {
            var sql = SqlContentSourcesSelect()
                .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document))
                .Append(SqlOrderByLevelIdSortOrder(SqlContext));

            // Use a more efficient COUNT query
            var sqlCountQuery = SqlContentSourcesCount()
                .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document));

            var sqlCount = SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in Database.QueryPaged<ContentSourceDto>(_nucacheSettings.Value.SqlPageSize, sql, sqlCount))
            {
                yield return CreateContentNodeKit(row, serializer);
            }
        }

        public IEnumerable<ContentNodeKit> GetBranchContentSources(int id)
        {
            var sql = SqlContentSourcesSelect(SqlContentSourcesSelectUmbracoNodeJoin)
                .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document))
                .Append(SqlWhereNodeIdX(SqlContext, id))
                .Append(SqlOrderByLevelIdSortOrder(SqlContext));

            // Use a more efficient COUNT query
            var sqlCountQuery = SqlContentSourcesCount(SqlContentSourcesSelectUmbracoNodeJoin)
                .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document))
                .Append(SqlWhereNodeIdX(SqlContext, id));
            var sqlCount = SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in Database.QueryPaged<ContentSourceDto>(_nucacheSettings.Value.SqlPageSize, sql, sqlCount))
            {
                yield return CreateContentNodeKit(row, serializer);
            }
        }

        public IEnumerable<ContentNodeKit> GetTypeContentSources(IEnumerable<int>? ids)
        {
            if (!ids?.Any() ?? false)
                yield break;

            var sql = SqlContentSourcesSelect()
                .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document))
                .WhereIn<ContentDto>(x => x.ContentTypeId, ids)
                .Append(SqlOrderByLevelIdSortOrder(SqlContext));

            // Use a more efficient COUNT query
            var sqlCountQuery = SqlContentSourcesCount()
                .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Document))
                .WhereIn<ContentDto>(x => x.ContentTypeId, ids);
            var sqlCount = SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in Database.QueryPaged<ContentSourceDto>(_nucacheSettings.Value.SqlPageSize, sql, sqlCount))
            {
                yield return CreateContentNodeKit(row, serializer);
            }
        }

        public ContentNodeKit GetMediaSource(Scoping.IScope scope, int id)
        {
            var sql = SqlMediaSourcesSelect()
                .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
                .Append(SqlWhereNodeId(SqlContext, id))
                .Append(SqlOrderByLevelIdSortOrder(scope.SqlContext));

            var dto = scope.Database.Fetch<ContentSourceDto>(sql).FirstOrDefault();

            if (dto == null)
                return ContentNodeKit.Empty;

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);
            return CreateMediaNodeKit(dto, serializer);
        }

        public ContentNodeKit GetMediaSource(int id)
        {
            var sql = SqlMediaSourcesSelect()
                .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
                .Append(SqlWhereNodeId(SqlContext, id))
                .Append(SqlOrderByLevelIdSortOrder(SqlContext));

            var dto = Database.Fetch<ContentSourceDto>(sql).FirstOrDefault();

            if (dto == null)
                return ContentNodeKit.Empty;

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);
            return CreateMediaNodeKit(dto, serializer);
        }

        public IEnumerable<ContentNodeKit> GetAllMediaSources()
        {
            var sql = SqlMediaSourcesSelect()
                .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
                .Append(SqlOrderByLevelIdSortOrder(SqlContext));

            // Use a more efficient COUNT query
            var sqlCountQuery = SqlMediaSourcesCount()
                .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media));
            var sqlCount = SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in Database.QueryPaged<ContentSourceDto>(_nucacheSettings.Value.SqlPageSize, sql, sqlCount))
            {
                yield return CreateMediaNodeKit(row, serializer);
            }
        }

        public IEnumerable<ContentNodeKit> GetBranchMediaSources(int id)
        {
            var sql = SqlMediaSourcesSelect(SqlContentSourcesSelectUmbracoNodeJoin)
                .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
                .Append(SqlWhereNodeIdX(SqlContext, id))
                .Append(SqlOrderByLevelIdSortOrder(SqlContext));

            // Use a more efficient COUNT query
            var sqlCountQuery = SqlMediaSourcesCount(SqlContentSourcesSelectUmbracoNodeJoin)
                .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
                .Append(SqlWhereNodeIdX(SqlContext, id));
            var sqlCount = SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in Database.QueryPaged<ContentSourceDto>(_nucacheSettings.Value.SqlPageSize, sql, sqlCount))
            {
                yield return CreateMediaNodeKit(row, serializer);
            }
        }

        public IEnumerable<ContentNodeKit> GetTypeMediaSources(IEnumerable<int> ids)
        {
            if (!ids.Any())
                yield break;

            var sql = SqlMediaSourcesSelect()
                    .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
                    .WhereIn<ContentDto>(x => x.ContentTypeId, ids)
                    .Append(SqlOrderByLevelIdSortOrder(SqlContext));

            // Use a more efficient COUNT query
            var sqlCountQuery = SqlMediaSourcesCount()
                .Append(SqlObjectTypeNotTrashed(SqlContext, Constants.ObjectTypes.Media))
                .WhereIn<ContentDto>(x => x.ContentTypeId, ids);
            var sqlCount = SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in Database.QueryPaged<ContentSourceDto>(_nucacheSettings.Value.SqlPageSize, sql, sqlCount))
            {
                yield return CreateMediaNodeKit(row, serializer);
            }
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
                        throw new InvalidOperationException("Missing cmsContentNu edited content for node " + dto.Id + ", consider rebuilding.");
                    }

                    _logger.LogWarning("Missing cmsContentNu edited content for node {NodeId}, consider rebuilding.", dto.Id);
                }
                else
                {
                    bool published = false;
                    var deserializedContent = serializer.Deserialize(dto, dto.EditData, dto.EditDataRaw, published);

                    d = new ContentData(
                        dto.EditName,
                        deserializedContent?.UrlSegment,
                        dto.VersionId,
                        dto.EditVersionDate,
                        dto.EditWriterId,
                        dto.EditTemplateId,
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
                        throw new InvalidOperationException("Missing cmsContentNu published content for node " + dto.Id + ", consider rebuilding.");
                    }

                    _logger.LogWarning("Missing cmsContentNu published content for node {NodeId}, consider rebuilding.", dto.Id);
                }
                else
                {
                    bool published = true;
                    var deserializedContent = serializer.Deserialize(dto, dto.PubData, dto.PubDataRaw, published);

                    p = new ContentData(
                        dto.PubName,
                        deserializedContent?.UrlSegment,
                        dto.VersionId,
                        dto.PubVersionDate,
                        dto.PubWriterId,
                        dto.PubTemplateId,
                        published,
                        deserializedContent?.PropertyData,
                        deserializedContent?.CultureData);
                }
            }

            var n = new ContentNode(dto.Id, dto.Key,
                dto.Level, dto.Path, dto.SortOrder, dto.ParentId, dto.CreateDate, dto.CreatorId);

            var s = new ContentNodeKit(n, dto.ContentTypeId, d, p);

            return s;
        }

        private ContentNodeKit CreateMediaNodeKit(ContentSourceDto dto, IContentCacheDataSerializer serializer)
        {
            if (dto.EditData == null && dto.EditDataRaw == null)
                throw new InvalidOperationException("No data for media " + dto.Id);

            bool published = true;
            var deserializedMedia = serializer.Deserialize(dto, dto.EditData, dto.EditDataRaw, published);

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

            var n = new ContentNode(dto.Id, dto.Key,
                dto.Level, dto.Path, dto.SortOrder, dto.ParentId, dto.CreateDate, dto.CreatorId);

            var s = new ContentNodeKit(n, dto.ContentTypeId, null, p);

            return s;
        }
    }
}
