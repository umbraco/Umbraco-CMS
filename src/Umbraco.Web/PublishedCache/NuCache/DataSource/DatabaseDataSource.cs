using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NPoco;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Scoping;
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

        public DatabaseDataSource(IContentCacheDataSerializerFactory contentCacheDataSerializerFactory)
        {
            _contentCacheDataSerializerFactory = contentCacheDataSerializerFactory;
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


    }
}
