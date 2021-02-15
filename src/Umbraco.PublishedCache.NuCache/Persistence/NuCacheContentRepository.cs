using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPoco;
using Umbraco.Cms.Core.Cache;
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
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Persistence.SqlExtensionsStatics;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Infrastructure.PublishedCache.Persistence
{
    public class NuCacheContentRepository : RepositoryBase, INuCacheContentRepository
    {
        private const int PageSize = 500;
        private readonly ILogger<NuCacheContentRepository> _logger;
        private readonly IMemberRepository _memberRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly UrlSegmentProviderCollection _urlSegmentProviders;

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
            UrlSegmentProviderCollection urlSegmentProviders)
            : base(scopeAccessor, appCaches)
        {
            _logger = logger;
            _memberRepository = memberRepository;
            _documentRepository = documentRepository;
            _mediaRepository = mediaRepository;
            _shortStringHelper = shortStringHelper;
            _urlSegmentProviders = urlSegmentProviders;
        }

        public void DeleteContentItem(IContentBase item)
            => Database.Execute("DELETE FROM cmsContentNu WHERE nodeId=@id", new { id = item.Id });

        public void RefreshContent(IContent content)
        {
            // always refresh the edited data
            OnRepositoryRefreshed(content, false);

            if (content.PublishedState == PublishedState.Unpublishing)
            {
                // if unpublishing, remove published data from table
                Database.Execute("DELETE FROM cmsContentNu WHERE nodeId=@id AND published=1", new { id = content.Id });
            }
            else if (content.PublishedState == PublishedState.Publishing)
            {
                // if publishing, refresh the published data
                OnRepositoryRefreshed(content, true);
            }
        }

        public void RefreshEntity(IContentBase content)
            => OnRepositoryRefreshed(content, false);

        private void OnRepositoryRefreshed(IContentBase content, bool published)
        {
            // use a custom SQL to update row version on each update
            // db.InsertOrUpdate(dto);
            ContentNuDto dto = GetDto(content, published);

            Database.InsertOrUpdate(
                dto,
                "SET data=@data, rv=rv+1 WHERE nodeId=@id AND published=@published",
                new
                {
                    data = dto.Data,
                    id = dto.NodeId,
                    published = dto.Published
                });
        }

        public void Rebuild(
            int groupSize = 5000,
            IReadOnlyCollection<int> contentTypeIds = null,
            IReadOnlyCollection<int> mediaTypeIds = null,
            IReadOnlyCollection<int> memberTypeIds = null)
        {
            if (contentTypeIds != null)
            {
                RebuildContentDbCache(groupSize, contentTypeIds);
            }

            if (mediaTypeIds != null)
            {
                RebuildContentDbCache(groupSize, mediaTypeIds);
            }

            if (memberTypeIds != null)
            {
                RebuildContentDbCache(groupSize, memberTypeIds);
            }
        }

        // assumes content tree lock
        private void RebuildContentDbCache(int groupSize, IReadOnlyCollection<int> contentTypeIds)
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
                IEnumerable<IContent> descendants = _documentRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
                var items = new List<ContentNuDto>();
                var count = 0;
                foreach (IContent c in descendants)
                {
                    // always the edited version
                    items.Add(GetDto(c, false));

                    // and also the published version if it makes any sense
                    if (c.Published)
                    {
                        items.Add(GetDto(c, true));
                    }

                    count++;
                }

                Database.BulkInsertRecords(items);
                processed += count;
            } while (processed < total);
        }

        // assumes media tree lock
        private void RebuildMediaDbCache(int groupSize, IReadOnlyCollection<int> contentTypeIds)
        {
            var mediaObjectType = Constants.ObjectTypes.Media;

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
                var items = descendants.Select(m => GetDto(m, false)).ToList();
                Database.BulkInsertRecords(items);
                processed += items.Count;
            } while (processed < total);
        }

        // assumes member tree lock
        private void RebuildMemberDbCache(int groupSize, IReadOnlyCollection<int> contentTypeIds)
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
                ContentNuDto[] items = descendants.Select(m => GetDto(m, false)).ToArray();
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

        private ContentNuDto GetDto(IContentBase content, bool published)
        {
            // should inject these in ctor
            // BUT for the time being we decide not to support ConvertDbToXml/String
            // var propertyEditorResolver = PropertyEditorResolver.Current;
            // var dataTypeService = ApplicationContext.Current.Services.DataTypeService;
            var propertyData = new Dictionary<string, PropertyData[]>();
            foreach (IProperty prop in content.Properties)
            {
                var pdatas = new List<PropertyData>();
                foreach (IPropertyValue pvalue in prop.Values)
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
                ContentCultureInfosCollection infos = content is IContent document
                    ? published
                        ? document.PublishCultureInfos
                        : document.CultureInfos
                    : content.CultureInfos;

                // ReSharper disable once UseDeconstruction
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

            // the dictionary that will be serialized
            var nestedData = new ContentNestedData
            {
                PropertyData = propertyData,
                CultureData = cultureData,
                UrlSegment = content.GetUrlSegment(_shortStringHelper, _urlSegmentProviders)
            };

            var dto = new ContentNuDto
            {
                NodeId = content.Id,
                Published = published,

                // note that numeric values (which are Int32) are serialized without their
                // type (eg "value":1234) and JsonConvert by default deserializes them as Int64
                Data = JsonConvert.SerializeObject(nestedData)
            };

            return dto;
        }

        // we want arrays, we want them all loaded, not an enumerable
        private Sql<ISqlContext> ContentSourcesSelect(Func<Sql<ISqlContext>, Sql<ISqlContext>> joins = null)
        {
            var sql = Sql()

                .Select<NodeDto>(x => Alias(x.NodeId, "Id"), x => Alias(x.UniqueId, "Uid"),
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

                .From<NodeDto>();

            if (joins != null)
            {
                sql = joins(sql);
            }

            sql = sql
                .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((left, right) => left.NodeId == right.NodeId)

                .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
                .InnerJoin<DocumentVersionDto>().On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id)

                .LeftJoin<ContentVersionDto>(j =>
                    j.InnerJoin<DocumentVersionDto>("pdver").On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id && right.Published, "pcver", "pdver"), "pcver")
                .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId, aliasRight: "pcver")

                .LeftJoin<ContentNuDto>("nuEdit").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && !right.Published, aliasRight: "nuEdit")
                .LeftJoin<ContentNuDto>("nuPub").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && right.Published, aliasRight: "nuPub");

            return sql;
        }

        public ContentNodeKit GetContentSource(int id)
        {
            var sql = ContentSourcesSelect()
                .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Document && x.NodeId == id && !x.Trashed)
                .OrderBy<NodeDto>(x => x.Level, x => x.ParentId, x => x.SortOrder);

            var dto = Database.Fetch<ContentSourceDto>(sql).FirstOrDefault();
            return dto == null ? new ContentNodeKit() : CreateContentNodeKit(dto);
        }

        public IEnumerable<ContentNodeKit> GetAllContentSources()
        {
            var sql = ContentSourcesSelect()
                .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Document && !x.Trashed)
                .OrderBy<NodeDto>(x => x.Level, x => x.ParentId, x => x.SortOrder);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in Database.QueryPaged<ContentSourceDto>(PageSize, sql))
            {
                yield return CreateContentNodeKit(row);
            }
        }

        public IEnumerable<ContentNodeKit> GetBranchContentSources(int id)
        {
            var syntax = SqlSyntax;
            var sql = ContentSourcesSelect(
                s => s.InnerJoin<NodeDto>("x").On<NodeDto, NodeDto>((left, right) => left.NodeId == right.NodeId || SqlText<bool>(left.Path, right.Path, (lp, rp) => $"({lp} LIKE {syntax.GetConcat(rp, "',%'")})"), aliasRight: "x"))
                    .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Document && !x.Trashed)
                    .Where<NodeDto>(x => x.NodeId == id, "x")
                    .OrderBy<NodeDto>(x => x.Level, x => x.ParentId, x => x.SortOrder);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in Database.QueryPaged<ContentSourceDto>(PageSize, sql))
            {
                yield return CreateContentNodeKit(row);
            }
        }

        public IEnumerable<ContentNodeKit> GetTypeContentSources(IEnumerable<int> ids)
        {
            if (!ids.Any())
                yield break;

            var sql = ContentSourcesSelect()
                .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Document && !x.Trashed)
                .WhereIn<ContentDto>(x => x.ContentTypeId, ids)
                .OrderBy<NodeDto>(x => x.Level, x => x.ParentId, x => x.SortOrder);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in Database.QueryPaged<ContentSourceDto>(PageSize, sql))
            {
                yield return CreateContentNodeKit(row);
            }
        }

        private Sql<ISqlContext> MediaSourcesSelect(Func<Sql<ISqlContext>, Sql<ISqlContext>> joins = null)
        {
            var sql = Sql()

                .Select<NodeDto>(x => Alias(x.NodeId, "Id"), x => Alias(x.UniqueId, "Uid"),
                    x => Alias(x.Level, "Level"), x => Alias(x.Path, "Path"), x => Alias(x.SortOrder, "SortOrder"), x => Alias(x.ParentId, "ParentId"),
                    x => Alias(x.CreateDate, "CreateDate"), x => Alias(x.UserId, "CreatorId"))
                .AndSelect<ContentDto>(x => Alias(x.ContentTypeId, "ContentTypeId"))
                .AndSelect<ContentVersionDto>(x => Alias(x.Id, "VersionId"), x => Alias(x.Text, "EditName"), x => Alias(x.VersionDate, "EditVersionDate"), x => Alias(x.UserId, "EditWriterId"))
                .AndSelect<ContentNuDto>("nuEdit", x => Alias(x.Data, "EditData"))
                .From<NodeDto>();

            if (joins != null)
            {
                sql = joins(sql);
            }

            sql = sql
                .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
                .LeftJoin<ContentNuDto>("nuEdit").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && !right.Published, aliasRight: "nuEdit");

            return sql;
        }

        public ContentNodeKit GetMediaSource(int id)
        {
            var sql = MediaSourcesSelect()
                .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Media && x.NodeId == id && !x.Trashed)
                .OrderBy<NodeDto>(x => x.Level, x => x.ParentId, x => x.SortOrder);

            var dto = Database.Fetch<ContentSourceDto>(sql).FirstOrDefault();
            return dto == null ? new ContentNodeKit() : CreateMediaNodeKit(dto);
        }

        public IEnumerable<ContentNodeKit> GetAllMediaSources()
        {
            var sql = MediaSourcesSelect()
                .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Media && !x.Trashed)
                .OrderBy<NodeDto>(x => x.Level, x => x.ParentId, x => x.SortOrder);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in Database.QueryPaged<ContentSourceDto>(PageSize, sql))
            {
                yield return CreateMediaNodeKit(row);
            }
        }

        public IEnumerable<ContentNodeKit> GetBranchMediaSources(int id)
        {
            var syntax = SqlSyntax;
            var sql = MediaSourcesSelect(
                s => s.InnerJoin<NodeDto>("x").On<NodeDto, NodeDto>((left, right) => left.NodeId == right.NodeId || SqlText<bool>(left.Path, right.Path, (lp, rp) => $"({lp} LIKE {syntax.GetConcat(rp, "',%'")})"), aliasRight: "x"))
                .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Media && !x.Trashed)
                .Where<NodeDto>(x => x.NodeId == id, "x")
                .OrderBy<NodeDto>(x => x.Level, x => x.ParentId, x => x.SortOrder);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in Database.QueryPaged<ContentSourceDto>(PageSize, sql))
            {
                yield return CreateMediaNodeKit(row);
            }
        }

        public IEnumerable<ContentNodeKit> GetTypeMediaSources(IEnumerable<int> ids)
        {
            if (!ids.Any())
            {
                yield break;
            }

            var sql = MediaSourcesSelect()
                    .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Media && !x.Trashed)
                    .WhereIn<ContentDto>(x => x.ContentTypeId, ids)
                    .OrderBy<NodeDto>(x => x.Level, x => x.ParentId, x => x.SortOrder);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in Database.QueryPaged<ContentSourceDto>(PageSize, sql))
            {
                yield return CreateMediaNodeKit(row);
            }
        }

        private ContentNodeKit CreateContentNodeKit(ContentSourceDto dto)
        {
            ContentData d = null;
            ContentData p = null;

            if (dto.Edited)
            {
                if (dto.EditData == null)
                {
                    if (Debugger.IsAttached)
                    {
                        throw new InvalidOperationException("Missing cmsContentNu edited content for node " + dto.Id + ", consider rebuilding.");
                    }

                    _logger.LogWarning("Missing cmsContentNu edited content for node {NodeId}, consider rebuilding.", dto.Id);
                }
                else
                {
                    var nested = DeserializeNestedData(dto.EditData);

                    d = new ContentData
                    {
                        Name = dto.EditName,
                        Published = false,
                        TemplateId = dto.EditTemplateId,
                        VersionId = dto.VersionId,
                        VersionDate = dto.EditVersionDate,
                        WriterId = dto.EditWriterId,
                        Properties = nested.PropertyData,
                        CultureInfos = nested.CultureData,
                        UrlSegment = nested.UrlSegment
                    };
                }
            }

            if (dto.Published)
            {
                if (dto.PubData == null)
                {
                    if (Debugger.IsAttached)
                    {
                        throw new InvalidOperationException("Missing cmsContentNu published content for node " + dto.Id + ", consider rebuilding.");
                    }

                    _logger.LogWarning("Missing cmsContentNu published content for node {NodeId}, consider rebuilding.", dto.Id);
                }
                else
                {
                    var nested = DeserializeNestedData(dto.PubData);

                    p = new ContentData
                    {
                        Name = dto.PubName,
                        UrlSegment = nested.UrlSegment,
                        Published = true,
                        TemplateId = dto.PubTemplateId,
                        VersionId = dto.VersionId,
                        VersionDate = dto.PubVersionDate,
                        WriterId = dto.PubWriterId,
                        Properties = nested.PropertyData,
                        CultureInfos = nested.CultureData
                    };
                }
            }

            var n = new ContentNode(dto.Id, dto.Uid,
                dto.Level, dto.Path, dto.SortOrder, dto.ParentId, dto.CreateDate, dto.CreatorId);

            var s = new ContentNodeKit
            {
                Node = n,
                ContentTypeId = dto.ContentTypeId,
                DraftData = d,
                PublishedData = p
            };

            return s;
        }

        private static ContentNodeKit CreateMediaNodeKit(ContentSourceDto dto)
        {
            if (dto.EditData == null)
                throw new InvalidOperationException("No data for media " + dto.Id);

            var nested = DeserializeNestedData(dto.EditData);

            var p = new ContentData
            {
                Name = dto.EditName,
                Published = true,
                TemplateId = -1,
                VersionId = dto.VersionId,
                VersionDate = dto.EditVersionDate,
                WriterId = dto.CreatorId, // what-else?
                Properties = nested.PropertyData,
                CultureInfos = nested.CultureData
            };

            var n = new ContentNode(dto.Id, dto.Uid,
                dto.Level, dto.Path, dto.SortOrder, dto.ParentId, dto.CreateDate, dto.CreatorId);

            var s = new ContentNodeKit
            {
                Node = n,
                ContentTypeId = dto.ContentTypeId,
                PublishedData = p
            };

            return s;
        }

        private static ContentNestedData DeserializeNestedData(string data)
        {
            // by default JsonConvert will deserialize our numeric values as Int64
            // which is bad, because they were Int32 in the database - take care

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new ForceInt32Converter() }
            };

            return JsonConvert.DeserializeObject<ContentNestedData>(data, settings);
        }
    }
}
