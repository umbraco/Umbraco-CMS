﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using NPoco;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Web.Composing;
using static Umbraco.Core.Persistence.NPocoSqlExtensions.Statics;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    // TODO: use SqlTemplate for these queries else it's going to be horribly slow!

    // provides efficient database access for NuCache
    internal class DatabaseDataSource : IDataSource
    {
        private const int PageSize = 500;
        private readonly IContentCacheDataSerializerFactory _contentCacheDataSerializerFactory;
        private readonly IDocumentRepository _documentRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IScopeProvider _scopeProvider;
        private readonly UrlSegmentProviderCollection _urlSegmentProviders;
        public DatabaseDataSource(IContentCacheDataSerializerFactory contentCacheDataSerializerFactory,
            IDocumentRepository documentRepository,
            IMediaRepository mediaRepository,
            IMemberRepository memberRepository,
            IScopeProvider scopeProvider,
            UrlSegmentProviderCollection urlSegmentProviders)
        {
            _urlSegmentProviders = urlSegmentProviders;
            _contentCacheDataSerializerFactory = contentCacheDataSerializerFactory;
            _documentRepository = documentRepository;
            _mediaRepository = mediaRepository;
            _memberRepository = memberRepository;
            _scopeProvider = scopeProvider;
        }

        // we want arrays, we want them all loaded, not an enumerable

        private Sql<ISqlContext> SqlContentSourcesSelect(IScope scope, Func<ISqlContext, Sql<ISqlContext>> joins = null)
        {
            var sqlTemplate = scope.SqlContext.Templates.Get(Constants.SqlTemplates.NuCacheDatabaseDataSource.ContentSourcesSelect, tsql =>
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
        private Sql<ISqlContext> SqlContentSourcesCount(IScope scope, Func<ISqlContext, Sql<ISqlContext>> joins = null)
        {
            var sqlTemplate = scope.SqlContext.Templates.Get(Constants.SqlTemplates.NuCacheDatabaseDataSource.ContentSourcesCount, tsql =>
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

        private Sql<ISqlContext> SqlMediaSourcesSelect(IScope scope, Func<ISqlContext, Sql<ISqlContext>> joins = null)
        {
            var sqlTemplate = scope.SqlContext.Templates.Get(Constants.SqlTemplates.NuCacheDatabaseDataSource.MediaSourcesSelect, tsql =>
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
        private Sql<ISqlContext> SqlMediaSourcesCount(IScope scope, Func<ISqlContext, Sql<ISqlContext>> joins = null)
        {
            var sqlTemplate = scope.SqlContext.Templates.Get(Constants.SqlTemplates.NuCacheDatabaseDataSource.MediaSourcesCount, tsql =>
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

        public ContentNodeKit GetContentSource(IScope scope, int id)
        {
            var sql = SqlContentSourcesSelect(scope)
                .Append(SqlObjectTypeNotTrashed(scope.SqlContext, Constants.ObjectTypes.Document))
                .Append(SqlWhereNodeId(scope.SqlContext, id))
                .Append(SqlOrderByLevelIdSortOrder(scope.SqlContext));

            var dto = scope.Database.Fetch<ContentSourceDto>(sql).FirstOrDefault();

            if (dto == null) return ContentNodeKit.Empty;

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);
            return CreateContentNodeKit(dto, serializer);
        }

        public IEnumerable<ContentNodeKit> GetAllContentSources(IScope scope)
        {
            var sql = SqlContentSourcesSelect(scope)
                .Append(SqlObjectTypeNotTrashed(scope.SqlContext, Constants.ObjectTypes.Document))
                .Append(SqlOrderByLevelIdSortOrder(scope.SqlContext));

            // Use a more efficient COUNT query
            var sqlCountQuery = SqlContentSourcesCount(scope)
                .Append(SqlObjectTypeNotTrashed(scope.SqlContext, Constants.ObjectTypes.Document));

            var sqlCount = scope.SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in scope.Database.QueryPaged<ContentSourceDto>(PageSize, sql, sqlCount))
            {
                yield return CreateContentNodeKit(row, serializer);
            }
        }

        public IEnumerable<ContentNodeKit> GetBranchContentSources(IScope scope, int id)
        {
            var sql = SqlContentSourcesSelect(scope, SqlContentSourcesSelectUmbracoNodeJoin)
                .Append(SqlObjectTypeNotTrashed(scope.SqlContext, Constants.ObjectTypes.Document))
                .Append(SqlWhereNodeIdX(scope.SqlContext, id))
                .Append(SqlOrderByLevelIdSortOrder(scope.SqlContext));

            // Use a more efficient COUNT query
            var sqlCountQuery = SqlContentSourcesCount(scope, SqlContentSourcesSelectUmbracoNodeJoin)
                .Append(SqlObjectTypeNotTrashed(scope.SqlContext, Constants.ObjectTypes.Document))
                .Append(SqlWhereNodeIdX(scope.SqlContext, id));
            var sqlCount = scope.SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in scope.Database.QueryPaged<ContentSourceDto>(PageSize, sql, sqlCount))
            {
                yield return CreateContentNodeKit(row, serializer);
            }
        }

        public IEnumerable<ContentNodeKit> GetTypeContentSources(IScope scope, IEnumerable<int> ids)
        {
            if (!ids.Any()) yield break;

            var sql = SqlContentSourcesSelect(scope)
                .Append(SqlObjectTypeNotTrashed(scope.SqlContext, Constants.ObjectTypes.Document))
                .WhereIn<ContentDto>(x => x.ContentTypeId, ids)
                .Append(SqlOrderByLevelIdSortOrder(scope.SqlContext));

            // Use a more efficient COUNT query
            var sqlCountQuery = SqlContentSourcesCount(scope)
                .Append(SqlObjectTypeNotTrashed(scope.SqlContext, Constants.ObjectTypes.Document))
                .WhereIn<ContentDto>(x => x.ContentTypeId, ids);
            var sqlCount = scope.SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in scope.Database.QueryPaged<ContentSourceDto>(PageSize, sql, sqlCount))
            {
                yield return CreateContentNodeKit(row, serializer);
            }
        }

        public ContentNodeKit GetMediaSource(IScope scope, int id)
        {
            var sql = SqlMediaSourcesSelect(scope)
                .Append(SqlObjectTypeNotTrashed(scope.SqlContext, Constants.ObjectTypes.Media))
                .Append(SqlWhereNodeId(scope.SqlContext, id))
                .Append(SqlOrderByLevelIdSortOrder(scope.SqlContext));

            var dto = scope.Database.Fetch<ContentSourceDto>(sql).FirstOrDefault();

            if (dto == null) return ContentNodeKit.Empty;

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);
            return CreateMediaNodeKit(dto, serializer);
        }

        public IEnumerable<ContentNodeKit> GetAllMediaSources(IScope scope)
        {
            var sql = SqlMediaSourcesSelect(scope)
                .Append(SqlObjectTypeNotTrashed(scope.SqlContext, Constants.ObjectTypes.Media))
                .Append(SqlOrderByLevelIdSortOrder(scope.SqlContext));

            // Use a more efficient COUNT query
            var sqlCountQuery = SqlMediaSourcesCount(scope)
                .Append(SqlObjectTypeNotTrashed(scope.SqlContext, Constants.ObjectTypes.Media));
            var sqlCount = scope.SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in scope.Database.QueryPaged<ContentSourceDto>(PageSize, sql, sqlCount))
            {
                yield return CreateMediaNodeKit(row, serializer);
            }
        }

        public IEnumerable<ContentNodeKit> GetBranchMediaSources(IScope scope, int id)
        {
            var sql = SqlMediaSourcesSelect(scope, SqlContentSourcesSelectUmbracoNodeJoin)
                .Append(SqlObjectTypeNotTrashed(scope.SqlContext, Constants.ObjectTypes.Media))
                .Append(SqlWhereNodeIdX(scope.SqlContext, id))
                .Append(SqlOrderByLevelIdSortOrder(scope.SqlContext));

            // Use a more efficient COUNT query
            var sqlCountQuery = SqlMediaSourcesCount(scope, SqlContentSourcesSelectUmbracoNodeJoin)
                .Append(SqlObjectTypeNotTrashed(scope.SqlContext, Constants.ObjectTypes.Media))
                .Append(SqlWhereNodeIdX(scope.SqlContext, id));
            var sqlCount = scope.SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in scope.Database.QueryPaged<ContentSourceDto>(PageSize, sql, sqlCount))
            {
                yield return CreateMediaNodeKit(row, serializer);
            }
        }

        public IEnumerable<ContentNodeKit> GetTypeMediaSources(IScope scope, IEnumerable<int> ids)
        {
            if (!ids.Any()) yield break;

            var sql = SqlMediaSourcesSelect(scope)
                    .Append(SqlObjectTypeNotTrashed(scope.SqlContext, Constants.ObjectTypes.Media))
                    .WhereIn<ContentDto>(x => x.ContentTypeId, ids)
                    .Append(SqlOrderByLevelIdSortOrder(scope.SqlContext));

            // Use a more efficient COUNT query
            var sqlCountQuery = SqlMediaSourcesCount(scope)
                .Append(SqlObjectTypeNotTrashed(scope.SqlContext, Constants.ObjectTypes.Media))
                .WhereIn<ContentDto>(x => x.ContentTypeId, ids);
            var sqlCount = scope.SqlContext.Sql("SELECT COUNT(*) FROM (").Append(sqlCountQuery).Append(") npoco_tbl");

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);

            // We need to page here. We don't want to iterate over every single row in one connection cuz this can cause an SQL Timeout.
            // We also want to read with a db reader and not load everything into memory, QueryPaged lets us do that.

            foreach (var row in scope.Database.QueryPaged<ContentSourceDto>(PageSize, sql, sqlCount))
            {
                yield return CreateMediaNodeKit(row, serializer);
            }
        }

        private ContentNodeKit CreateContentNodeKit(ContentSourceDto dto, IContentCacheDataSerializer serializer)
        {
            ContentData d = null;
            ContentData p = null;

            if (dto.Edited)
            {
                if (dto.EditData == null && dto.EditDataRaw == null)
                {
                    if (Debugger.IsAttached)
                        throw new InvalidOperationException("Missing cmsContentNu edited content for node " + dto.Id + ", consider rebuilding.");
                    Current.Logger.Warn<DatabaseDataSource,int>("Missing cmsContentNu edited content for node {NodeId}, consider rebuilding.", dto.Id);
                }
                else
                {
                    bool published = false;
                    var deserializedContent = serializer.Deserialize(dto, dto.EditData, dto.EditDataRaw, published);

                    d = new ContentData
                    {
                        Name = dto.EditName,
                        Published = published,
                        TemplateId = dto.EditTemplateId,
                        VersionId = dto.VersionId,
                        VersionDate = dto.EditVersionDate,
                        WriterId = dto.EditWriterId,
                        Properties = deserializedContent.PropertyData, // TODO: We don't want to allocate empty arrays
                        CultureInfos = deserializedContent.CultureData,
                        UrlSegment = deserializedContent.UrlSegment
                    };
                }
            }

            if (dto.Published)
            {
                if (dto.PubData == null && dto.PubDataRaw == null)
                {
                    if (Debugger.IsAttached)
                        throw new InvalidOperationException("Missing cmsContentNu published content for node " + dto.Id + ", consider rebuilding.");
                    Current.Logger.Warn<DatabaseDataSource,int>("Missing cmsContentNu published content for node {NodeId}, consider rebuilding.", dto.Id);
                }
                else
                {
                    bool published = true;
                    var deserializedContent = serializer.Deserialize(dto, dto.PubData, dto.PubDataRaw, published);

                    p = new ContentData
                    {
                        Name = dto.PubName,
                        UrlSegment = deserializedContent.UrlSegment,
                        Published = published,
                        TemplateId = dto.PubTemplateId,
                        VersionId = dto.VersionId,
                        VersionDate = dto.PubVersionDate,
                        WriterId = dto.PubWriterId,
                        Properties = deserializedContent.PropertyData, // TODO: We don't want to allocate empty arrays
                        CultureInfos = deserializedContent.CultureData
                    };
                }
            }

            var n = new ContentNode(dto.Id, dto.Key,
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

        private ContentNodeKit CreateMediaNodeKit(ContentSourceDto dto, IContentCacheDataSerializer serializer)
        {
            if (dto.EditData == null && dto.EditDataRaw == null)
                throw new InvalidOperationException("No data for media " + dto.Id);

            bool published = true;
            var deserializedMedia = serializer.Deserialize(dto, dto.EditData, dto.EditDataRaw, published);

            var p = new ContentData
            {
                Name = dto.EditName,
                Published = published,
                TemplateId = -1,
                VersionId = dto.VersionId,
                VersionDate = dto.EditVersionDate,
                WriterId = dto.CreatorId, // what-else?
                Properties = deserializedMedia.PropertyData, // TODO: We don't want to allocate empty arrays
                CultureInfos = deserializedMedia.CultureData
            };

            var n = new ContentNode(dto.Id, dto.Key,
                dto.Level, dto.Path, dto.SortOrder, dto.ParentId, dto.CreateDate, dto.CreatorId);

            var s = new ContentNodeKit
            {
                Node = n,
                ContentTypeId = dto.ContentTypeId,
                PublishedData = p
            };

            return s;
        }

        public void RemoveEntity(IScope scope, int id)
        {
            scope.Database.Execute("DELETE FROM cmsContentNu WHERE nodeId=@id", new { id = id });
        }

        public void RemovePublishedEntity(IScope scope, int id)
        {
            scope.Database.Execute("DELETE FROM cmsContentNu WHERE nodeId=@id AND published=1", new { id = id });
        }



        public void UpsertContentEntity(IScope scope, IContentBase contentBase, bool published)
        {
            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);
            var dto = GetDto(contentBase, published, serializer);
            scope.Database.InsertOrUpdate(dto,
                "SET data=@data, dataRaw=@dataRaw, rv=rv+1 WHERE nodeId=@id AND published=@published",
                new
                {
                    dataRaw = dto.RawData ?? Array.Empty<byte>(),
                    data = dto.Data,
                    id = dto.NodeId,
                    published = dto.Published
                });
        }
        public void UpsertMediaEntity(IScope scope, IContentBase contentBase, bool published)
        {
            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);
            var dto = GetDto(contentBase, published, serializer);
            scope.Database.InsertOrUpdate(dto,
                "SET data=@data, dataRaw=@dataRaw, rv=rv+1 WHERE nodeId=@id AND published=@published",
                new
                {
                    dataRaw = dto.RawData ?? Array.Empty<byte>(),
                    data = dto.Data,
                    id = dto.NodeId,
                    published = dto.Published
                });
        }
        public void UpsertMemberEntity(IScope scope, IContentBase contentBase, bool published)
        {
            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Member);
            var dto = GetDto(contentBase, published, serializer);
            scope.Database.InsertOrUpdate(dto,
                "SET data=@data, dataRaw=@dataRaw, rv=rv+1 WHERE nodeId=@id AND published=@published",
                new
                {
                    dataRaw = dto.RawData ?? Array.Empty<byte>(),
                    data = dto.Data,
                    id = dto.NodeId,
                    published = dto.Published
                });
        }


        private ContentNuDto GetDto(IContentBase content, bool published, IContentCacheDataSerializer serializer)
        {
            // should inject these in ctor
            // BUT for the time being we decide not to support ConvertDbToXml/String
            //var propertyEditorResolver = PropertyEditorResolver.Current;
            //var dataTypeService = ApplicationContext.Current.Services.DataTypeService;

            var propertyData = new Dictionary<string, PropertyData[]>();
            foreach (var prop in content.Properties)
            {
                var pdatas = new List<PropertyData>();
                foreach (var pvalue in prop.Values)
                {
                    // sanitize - properties should be ok but ... never knows
                    if (!prop.PropertyType.SupportsVariation(pvalue.Culture, pvalue.Segment))
                        continue;

                    // note: at service level, invariant is 'null', but here invariant becomes 'string.Empty'
                    var value = published ? pvalue.PublishedValue : pvalue.EditedValue;
                    if (value != null)
                        pdatas.Add(new PropertyData { Culture = pvalue.Culture ?? string.Empty, Segment = pvalue.Segment ?? string.Empty, Value = value });

                    //Core.Composing.Current.Logger.Debug<PublishedSnapshotService>($"{content.Id} {prop.Alias} [{pvalue.LanguageId},{pvalue.Segment}] {value} {(published?"pub":"edit")}");

                    //if (value != null)
                    //{
                    //    var e = propertyEditorResolver.GetByAlias(prop.PropertyType.PropertyEditorAlias);

                    //    // We are converting to string, even for database values which are integer or
                    //    // DateTime, which is not optimum. Doing differently would require that we have a way to tell
                    //    // whether the conversion to XML string changes something or not... which we don't, and we
                    //    // don't want to implement it as PropertyValueEditor.ConvertDbToXml/String should die anyway.

                    //    // Don't think about improving the situation here: this is a corner case and the real
                    //    // thing to do is to get rig of PropertyValueEditor.ConvertDbToXml/String.

                    //    // Use ConvertDbToString to keep it simple, although everywhere we use ConvertDbToXml and
                    //    // nothing ensures that the two methods are consistent.

                    //    if (e != null)
                    //        value = e.ValueEditor.ConvertDbToString(prop, prop.PropertyType, dataTypeService);
                    //}
                }
                propertyData[prop.Alias] = pdatas.ToArray();
            }

            var cultureData = new Dictionary<string, CultureVariation>();

            // sanitize - names should be ok but ... never knows
            if (content.ContentType.VariesByCulture())
            {
                var infos = content is IContent document
                    ? (published
                        ? document.PublishCultureInfos
                        : document.CultureInfos)
                    : content.CultureInfos;

                // ReSharper disable once UseDeconstruction
                foreach (var cultureInfo in infos)
                {
                    var cultureIsDraft = !published && content is IContent d && d.IsCultureEdited(cultureInfo.Culture);
                    cultureData[cultureInfo.Culture] = new CultureVariation
                    {
                        Name = cultureInfo.Name,
                        UrlSegment = content.GetUrlSegment(_urlSegmentProviders, cultureInfo.Culture),
                        Date = content.GetUpdateDate(cultureInfo.Culture) ?? DateTime.MinValue,
                        IsDraft = cultureIsDraft
                    };
                }
            }

            //the dictionary that will be serialized
            var contentCacheData = new ContentCacheDataModel
            {
                PropertyData = propertyData,
                CultureData = cultureData,
                UrlSegment = content.GetUrlSegment(_urlSegmentProviders)
            };

            var serialized = serializer.Serialize(ReadOnlyContentBaseAdapter.Create(content), contentCacheData);

            var dto = new ContentNuDto
            {
                NodeId = content.Id,
                Published = published,
                Data = serialized.StringData,
                RawData = serialized.ByteData
            };

            //Core.Composing.Current.Logger.Debug<PublishedSnapshotService>(dto.Data);

            return dto;
        }

        /// <summary>
        /// Loads all Content entites into the datasource
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="serializer"></param>
        /// <param name="contentTypeIds"></param>
        public void LoadAllContentEntities(IScope scope, IEnumerable<int> contentTypeIds = null)
        {

            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Document);
            var contentTypeIdsA = contentTypeIds?.ToArray();
            var db = scope.Database;

            // insert back - if anything fails the transaction will rollback
            var query = scope.SqlContext.Query<IContent>();
            if (contentTypeIds != null && contentTypeIdsA.Length > 0)
                query = query.WhereIn(x => x.ContentTypeId, contentTypeIdsA); // assume number of ctypes won't blow IN(...)

            int groupSize = GetSqlPagingSize();
            long pageIndex = 0;
            long processed = 0;
            long total;
            do
            {
                // the tree is locked, counting and comparing to total is safe
                var descendants = _documentRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
                var items = new List<ContentNuDto>();
                var count = 0;
                foreach (var c in descendants)
                {
                    // always the edited version
                    items.Add(GetDto(c, false, serializer));

                    // and also the published version if it makes any sense
                    if (c.Published)
                        items.Add(GetDto(c, true, serializer));

                    count++;
                }

                db.BulkInsertRecords(items);
                processed += count;
            } while (processed < total);
        }

        /// <summary>
        /// Loads all Content entites into the datasource
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="serializer"></param>
        /// <param name="contentTypeIds"></param>
        public void LoadAllMediaEntities(IScope scope, IEnumerable<int> contentTypeIds = null)
        {
            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Media);
            var contentTypeIdsA = contentTypeIds?.ToArray();
            var query = scope.SqlContext.Query<IMedia>();
            if (contentTypeIds != null && contentTypeIdsA.Length > 0)
                query = query.WhereIn(x => x.ContentTypeId, contentTypeIdsA); // assume number of ctypes won't blow IN(...)

            int groupSize = GetSqlPagingSize();
            long pageIndex = 0;
            long processed = 0;
            long total;
            do
            {
                // the tree is locked, counting and comparing to total is safe
                var descendants = _mediaRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
                var items = descendants.Select(m => GetDto(m, false, serializer)).ToList();
                scope.Database.BulkInsertRecords(items);
                processed += items.Count;
            } while (processed < total);
        }

        /// <summary>
        /// Loads all Content entites into the datasource
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="serializer"></param>
        /// <param name="contentTypeIds"></param>
        public void LoadAllMemberEntities(IScope scope, IEnumerable<int> contentTypeIds = null)
        {
            var serializer = _contentCacheDataSerializerFactory.Create(ContentCacheDataSerializerEntityType.Member);
            var contentTypeIdsA = contentTypeIds?.ToArray();
            var memberObjectType = Constants.ObjectTypes.Member;
            var db = scope.Database;

            // remove all - if anything fails the transaction will rollback

            // insert back - if anything fails the transaction will rollback
            var query = scope.SqlContext.Query<IMember>();
            if (contentTypeIds != null && contentTypeIdsA.Length > 0)
                query = query.WhereIn(x => x.ContentTypeId, contentTypeIdsA); // assume number of ctypes won't blow IN(...)

            int groupSize = GetSqlPagingSize();
            long pageIndex = 0;
            long processed = 0;
            long total;
            do
            {
                var descendants = _memberRepository.GetPage(query, pageIndex++, groupSize, out total, null, Ordering.By("Path"));
                var items = descendants.Select(m => GetDto(m, false, serializer)).ToArray();
                db.BulkInsertRecords(items);
                processed += items.Length;
            } while (processed < total);
        }

        private const int DefaultSqlPagingSize = 1000;

        private static int GetSqlPagingSize()
        {
            var appSetting = ConfigurationManager.AppSettings["Umbraco.Web.PublishedCache.NuCache.PublishedSnapshotService.SqlPageSize"];
            return appSetting != null && int.TryParse(appSetting, out var size) ? size : DefaultSqlPagingSize;
        }

        public void DeleteAllContentEntities(IScope scope, IEnumerable<int> contentTypeIds = null)
        {
            DeleteAllEntities(scope, Constants.ObjectTypes.Document, contentTypeIds);
        }

        public void DeleteAllMediaEntities(IScope scope, IEnumerable<int> contentTypeIds = null)
        {
            DeleteAllEntities(scope, Constants.ObjectTypes.Media, contentTypeIds);
        }

        public void DeleteAllMemberEntities(IScope scope, IEnumerable<int> contentTypeIds = null)
        {
            DeleteAllEntities(scope, Constants.ObjectTypes.Member, contentTypeIds);
        }

        public void RebuildMediaDbCache(IEnumerable<int> contentTypeIds = null)
        {
            using (var scope = _scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
            {
                scope.ReadLock(Constants.Locks.MediaTree);
                // remove all - if anything fails the transaction will rollback
                DeleteAllMediaEntities(scope, contentTypeIds);
                // insert back - if anything fails the transaction will rollback
                LoadAllMediaEntities(scope, contentTypeIds);
                scope.Complete();
            }
        }

        public void RebuildContentDbCache(IEnumerable<int> contentTypeIds = null)
        {
            using (var scope = _scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                // remove all - if anything fails the transaction will rollback
                DeleteAllContentEntities(scope, contentTypeIds);
                LoadAllContentEntities(scope, contentTypeIds);
                scope.Complete();
            }
        }
        public void RebuildMemberDbCache(IEnumerable<int> contentTypeIds = null)
        {
            using (var scope = _scopeProvider.CreateScope(repositoryCacheMode: RepositoryCacheMode.Scoped))
            {
                scope.ReadLock(Constants.Locks.MemberTree);
                // remove all - if anything fails the transaction will rollback
                DeleteAllMemberEntities(scope, contentTypeIds);

                // insert back - if anything fails the transaction will rollback
                LoadAllMemberEntities(scope, contentTypeIds);
                scope.Complete();
            }
        }


        public bool MemberEntitiesValid()
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.MemberTree);


                // every member item should have a corresponding row for edited properties
                var memberObjectType = Constants.ObjectTypes.Member;
                var db = scope.Database;

                var count = db.ExecuteScalar<int>(@"SELECT COUNT(*)
            FROM umbracoNode
            LEFT JOIN cmsContentNu ON (umbracoNode.id=cmsContentNu.nodeId AND cmsContentNu.published=0)
            WHERE umbracoNode.nodeObjectType=@objType
            AND cmsContentNu.nodeId IS NULL
            ", new { objType = memberObjectType });

                scope.Complete();
                return count == 0;
            }
        }

        public bool ContentEntitiesValid()
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.ContentTree);
                // every document should have a corresponding row for edited properties
                // and if published, may have a corresponding row for published properties
                var contentObjectType = Constants.ObjectTypes.Document;
                var db = scope.Database;

                var count = db.ExecuteScalar<int>($@"SELECT COUNT(*)
            FROM umbracoNode
            JOIN {Constants.DatabaseSchema.Tables.Document} ON umbracoNode.id={Constants.DatabaseSchema.Tables.Document}.nodeId
            LEFT JOIN cmsContentNu nuEdited ON (umbracoNode.id=nuEdited.nodeId AND nuEdited.published=0)
            LEFT JOIN cmsContentNu nuPublished ON (umbracoNode.id=nuPublished.nodeId AND nuPublished.published=1)
            WHERE umbracoNode.nodeObjectType=@objType
            AND nuEdited.nodeId IS NULL OR ({Constants.DatabaseSchema.Tables.Document}.published=1 AND nuPublished.nodeId IS NULL);"
                    , new { objType = contentObjectType });
                scope.Complete();
                return count == 0;
            }
        }

        public bool MediaEntitiesValid()
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                scope.ReadLock(Constants.Locks.MediaTree);
                // every media item should have a corresponding row for edited properties
                var mediaObjectType = Constants.ObjectTypes.Media;
                var db = scope.Database;

                var count = db.ExecuteScalar<int>(@"SELECT COUNT(*)
                FROM umbracoNode
                LEFT JOIN cmsContentNu ON (umbracoNode.id=cmsContentNu.nodeId AND cmsContentNu.published=0)
                WHERE umbracoNode.nodeObjectType=@objType
                AND cmsContentNu.nodeId IS NULL
                ", new { objType = mediaObjectType });

                scope.Complete();
                return count == 0;
            }

        }

        private void DeleteAllEntities(IScope scope, Guid objectType, IEnumerable<int> contentTypeIds = null)
        {
            var contentTypeIdsA = contentTypeIds?.ToArray();
            if (contentTypeIds == null || contentTypeIdsA.Length == 0)
            {
                // must support SQL-CE
                scope.Database.Execute(@"DELETE FROM cmsContentNu
                WHERE cmsContentNu.nodeId IN (
                    SELECT id FROM umbracoNode WHERE umbracoNode.nodeObjectType=@objType
                )",
                    new { objType = objectType });
            }
            else
            {
                // assume number of ctypes won't blow IN(...)
                // must support SQL-CE
                scope.Database.Execute($@"DELETE FROM cmsContentNu
                WHERE cmsContentNu.nodeId IN (
                    SELECT id FROM umbracoNode
                    JOIN {Constants.DatabaseSchema.Tables.Content} ON {Constants.DatabaseSchema.Tables.Content}.nodeId=umbracoNode.id
                    WHERE umbracoNode.nodeObjectType=@objType
                    AND {Constants.DatabaseSchema.Tables.Content}.contentTypeId IN (@ctypes)
                )",
                    new { objType = objectType, ctypes = contentTypeIdsA });
            }
        }
    }
}
