using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using NPoco;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Serialization;
using Umbraco.Web.Composing;
using static Umbraco.Core.Persistence.NPocoSqlExtensions.Statics;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    // fixme - use SqlTemplate for these queries else it's going to be horribly slow!

    // provides efficient database access for NuCache
    internal class Database
    {
        private Sql<ISqlContext> SelectContentSources(IScopeUnitOfWork uow)
        {
            return uow.SqlContext.Sql()

                .Select<NodeDto>(x => Alias(x.NodeId, "Id"), x => Alias(x.UniqueId, "Uid"),
                    x => Alias(x.Level, "Level"), x => Alias(x.Path, "Path"), x => Alias(x.SortOrder, "SortOrder"), x => Alias(x.ParentId, "ParentId"),
                    x => Alias(x.CreateDate, "CreateDate"), x => Alias(x.UserId, "CreatorId"))
                .AndSelect<ContentDto>(x => Alias(x.ContentTypeId, "ContentTypeId"))
                .AndSelect<DocumentDto>(x => Alias(x.Published, "Published"), x => Alias(x.Edited, "Edited"))

                .AndSelect<ContentVersionDto>(x => Alias(x.VersionId, "Version"))
                .AndSelect<ContentVersionDto>(x => Alias(x.Text, "DraftName"), x => Alias(x.VersionDate, "DraftVersionDate"), x => Alias(x.UserId, "DraftWriterId"))
                .AndSelect<DocumentVersionDto>(x => Alias(x.TemplateId, "DraftTemplateId"))

                .AndSelect<ContentVersionDto>("pcver", x => Alias(x.Text, "PubName"), x => Alias(x.VersionDate, "PubVersionDate"), x => Alias(x.UserId, "PubWriterId"))
                .AndSelect<DocumentVersionDto>("pdver", x => Alias(x.TemplateId, "PubTemplateId"))

                .AndSelect<ContentNuDto>("nuDraft", x => Alias(x.Data, "DraftData"))
                .AndSelect<ContentNuDto>("nuPub", x => Alias(x.Data, "PubData"));
        }

        private Sql<ISqlContext> SelectMediaSources(IScopeUnitOfWork uow)
        {
            return uow.SqlContext.Sql()

                .Select<NodeDto>(x => Alias(x.NodeId, "Id"), x => Alias(x.UniqueId, "Uid"),
                    x => Alias(x.Level, "Level"), x => Alias(x.Path, "Path"), x => Alias(x.SortOrder, "SortOrder"), x => Alias(x.ParentId, "ParentId"),
                    x => Alias(x.CreateDate, "CreateDate"), x => Alias(x.UserId, "CreatorId"))
                .AndSelect<ContentDto>(x => Alias(x.ContentTypeId, "ContentTypeId"))
                //.AndSelect<DocumentDto>(x => Alias(x.Published, "Published"), x => Alias(x.Edited, "Edited"))

                .AndSelect<ContentVersionDto>(x => Alias(x.VersionId, "Version"))
                .AndSelect<ContentVersionDto>(x => Alias(x.Text, "PubName"), x => Alias(x.VersionDate, "PubVersionDate"), x => Alias(x.UserId, "PubWriterId"))
                .AndSelect<DocumentVersionDto>(x => Alias(x.TemplateId, "PubTemplateId"))

                //.AndSelect<ContentVersionDto>("pcver", x => Alias(x.Text, "PubName"), x => Alias(x.VersionDate, "PubVersionDate"), x => Alias(x.UserId, "PubWriterId"))
                //.AndSelect<DocumentVersionDto>("pdver", x => Alias(x.TemplateId, "PubTemplateId"))

                .AndSelect<ContentNuDto>("nuDraft", x => Alias(x.Data, "PubData"));
                //.AndSelect<ContentNuDto>("nuPub", x => Alias(x.Data, "PubData"));
        }

        public ContentNodeKit GetContentSource(IScopeUnitOfWork uow, int id)
        {
            var sql = SelectContentSources(uow)

                    .From<NodeDto>()
                    .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                    .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((left, right) => left.NodeId == right.NodeId)

                    .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
                    .InnerJoin<DocumentVersionDto>().On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id)

                    .LeftJoin<ContentVersionDto>(j =>
                        j.InnerJoin<DocumentVersionDto>("pdver").On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id && right.Published, "pcver", "pdver"), "pcver")
                    .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId)

                    .LeftJoin<ContentNuDto>("nuDraft").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && !right.Published, aliasRight: "nuDraft")
                    .LeftJoin<ContentNuDto>("nuPub").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && right.Published, aliasRight: "nuPub")

                    .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Document && x.NodeId == id)

                    .OrderBy<NodeDto>(x => x.Level, x => x.SortOrder)

                ;

            var dto = uow.Database.Fetch<ContentSourceDto>(sql).FirstOrDefault();
            return dto == null ? new ContentNodeKit() : CreateContentNodeKit(dto);
        }

        public ContentNodeKit GetMediaSource(IScopeUnitOfWork uow, int id)
        {
            var sql = SelectMediaSources(uow)

                    .From<NodeDto>()
                    .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                    .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((left, right) => left.NodeId == right.NodeId)

                    .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
                    .InnerJoin<DocumentVersionDto>().On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id)

                    //.LeftJoin<ContentVersionDto>(j =>
                    //    j.InnerJoin<DocumentVersionDto>("pdver").On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id, "pcver", "pdver"), "pcver")
                    //.On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId)

                    .LeftJoin<ContentNuDto>("nuDraft").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && !right.Published, aliasRight: "nuDraft")
                    //.LeftJoin<ContentNuDto>("nuPub").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && right.Published, aliasRight: "nuPub")

                    .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Document && x.NodeId == id)

                    .OrderBy<NodeDto>(x => x.Level, x => x.SortOrder)

                ;

            var dto = uow.Database.Fetch<ContentSourceDto>(sql).FirstOrDefault();
            return dto == null ? new ContentNodeKit() : CreateContentNodeKit(dto);
        }

        // we want arrays, we want them all loaded, not an enumerable

        public IEnumerable<ContentNodeKit> GetAllContentSources(IScopeUnitOfWork uow)
        {
            var sql = SelectContentSources(uow)

                    .From<NodeDto>()
                    .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                    .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((left, right) => left.NodeId == right.NodeId)

                    .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
                    .InnerJoin<DocumentVersionDto>().On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id)

                    .LeftJoin<ContentVersionDto>(j =>
                        j.InnerJoin<DocumentVersionDto>("pdver").On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id && right.Published, "pcver", "pdver"), "pcver")
                    .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId)

                    .LeftJoin<ContentNuDto>("nuDraft").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && !right.Published, aliasRight: "nuDraft")
                    .LeftJoin<ContentNuDto>("nuPub").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && right.Published, aliasRight: "nuPub")

                    .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Document)

                    .OrderBy<NodeDto>(x => x.Level, x => x.SortOrder)

                ;

            return uow.Database.Query<ContentSourceDto>(sql).Select(CreateContentNodeKit);
        }

        public IEnumerable<ContentNodeKit> GetAllMediaSources(IScopeUnitOfWork uow)
        {
            var sql = SelectMediaSources(uow)

                    .From<NodeDto>()
                    .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                    .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((left, right) => left.NodeId == right.NodeId)

                    .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
                    .InnerJoin<DocumentVersionDto>().On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id)

                    //.LeftJoin<ContentVersionDto>(j =>
                    //    j.InnerJoin<DocumentVersionDto>("pdver").On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id, "pcver", "pdver"), "pcver")
                    //.On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId)

                    .LeftJoin<ContentNuDto>("nuDraft").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && !right.Published, aliasRight: "nuDraft")
                    //.LeftJoin<ContentNuDto>("nuPub").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && right.Published, aliasRight: "nuPub")

                    .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Document)

                    .OrderBy<NodeDto>(x => x.Level, x => x.SortOrder)

                ;

            return uow.Database.Query<ContentSourceDto>(sql).Select(CreateMediaNodeKit);
        }

        public IEnumerable<ContentNodeKit> GetBranchContentSources(IScopeUnitOfWork uow, int id)
        {
            var syntax = uow.SqlContext.SqlSyntax;
            var sql = SelectContentSources(uow)

                .From<NodeDto>()
                .InnerJoin<NodeDto>("x").On<NodeDto, NodeDto>((left, right) => left.NodeId == right.NodeId || SqlText<bool>(left.Path, right.Path, (lp, rp) => $"({lp} LIKE {syntax.GetConcat(rp, "',%'")})"), aliasRight: "x")
                .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((left, right) => left.NodeId == right.NodeId)

                .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
                .InnerJoin<DocumentVersionDto>().On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id)

                .LeftJoin<ContentVersionDto>(j =>
                        j.InnerJoin<DocumentVersionDto>("pdver").On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id && right.Published, "pcver", "pdver"), "pcver")
                    .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId)

                .LeftJoin<ContentNuDto>("nuDraft").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && !right.Published, aliasRight: "nuDraft")
                .LeftJoin<ContentNuDto>("nuPub").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && right.Published, aliasRight: "nuPub")

                .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Document)
                .Where<NodeDto>(x => x.NodeId == id, "x")

                .OrderBy<NodeDto>(x => x.Level, x => x.SortOrder)

                ;

            return uow.Database.Query<ContentSourceDto>(sql).Select(CreateContentNodeKit);
        }

        public IEnumerable<ContentNodeKit> GetBranchMediaSources(IScopeUnitOfWork uow, int id)
        {
            var syntax = uow.SqlContext.SqlSyntax;
            var sql = SelectMediaSources(uow)

                    .From<NodeDto>()
                    .InnerJoin<NodeDto>("x").On<NodeDto, NodeDto>((left, right) => left.NodeId == right.NodeId || SqlText<bool>(left.Path, right.Path, (lp, rp) => $"({lp} LIKE {syntax.GetConcat(rp, "',%'")})"), aliasRight: "x")
                    .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                    .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((left, right) => left.NodeId == right.NodeId)

                    .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
                    .InnerJoin<DocumentVersionDto>().On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id)

                    //.LeftJoin<ContentVersionDto>(j =>
                    //    j.InnerJoin<DocumentVersionDto>("pdver").On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id, "pcver", "pdver"), "pcver")
                    //.On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId)

                    .LeftJoin<ContentNuDto>("nuDraft").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && !right.Published, aliasRight: "nuDraft")
                    //.LeftJoin<ContentNuDto>("nuPub").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && right.Published, aliasRight: "nuPub")

                    .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Document)
                    .Where<NodeDto>(x => x.NodeId == id, "x")

                    .OrderBy<NodeDto>(x => x.Level, x => x.SortOrder)

                ;

            return uow.Database.Query<ContentSourceDto>(sql).Select(CreateMediaNodeKit);
        }

        public IEnumerable<ContentNodeKit> GetTypeContentSources(IScopeUnitOfWork uow, IEnumerable<int> ids)
        {
            var sql = SelectContentSources(uow)

                .From<NodeDto>()
                .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((left, right) => left.NodeId == right.NodeId)

                .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
                .InnerJoin<DocumentVersionDto>().On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id)

                .LeftJoin<ContentVersionDto>(j =>
                        j.InnerJoin<DocumentVersionDto>("pdver").On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id && right.Published, "pcver", "pdver"), "pcver")
                    .On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId)

                .LeftJoin<ContentNuDto>("nuDraft").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && !right.Published, aliasRight: "nuDraft")
                .LeftJoin<ContentNuDto>("nuPub").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && right.Published, aliasRight: "nuPub")

                .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Document)
                .WhereIn<ContentDto>(x => x.ContentTypeId, ids)

                .OrderBy<NodeDto>(x => x.Level, x => x.SortOrder)

                ;

            return uow.Database.Query<ContentSourceDto>(sql).Select(CreateContentNodeKit);
        }

        public IEnumerable<ContentNodeKit> GetTypeMediaSources(IScopeUnitOfWork uow, IEnumerable<int> ids)
        {
            var sql = SelectMediaSources(uow)

                    .From<NodeDto>()
                    .InnerJoin<ContentDto>().On<NodeDto, ContentDto>((left, right) => left.NodeId == right.NodeId)
                    .InnerJoin<DocumentDto>().On<NodeDto, DocumentDto>((left, right) => left.NodeId == right.NodeId)

                    .InnerJoin<ContentVersionDto>().On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId && right.Current)
                    .InnerJoin<DocumentVersionDto>().On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id)

                    //.LeftJoin<ContentVersionDto>(j =>
                    //    j.InnerJoin<DocumentVersionDto>("pdver").On<ContentVersionDto, DocumentVersionDto>((left, right) => left.Id == right.Id, "pcver", "pdver"), "pcver")
                    //.On<NodeDto, ContentVersionDto>((left, right) => left.NodeId == right.NodeId)

                    .LeftJoin<ContentNuDto>("nuDraft").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && !right.Published, aliasRight: "nuDraft")
                    //.LeftJoin<ContentNuDto>("nuPub").On<NodeDto, ContentNuDto>((left, right) => left.NodeId == right.NodeId && right.Published, aliasRight: "nuPub")

                    .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Document)
                    .WhereIn<ContentDto>(x => x.ContentTypeId, ids)

                    .OrderBy<NodeDto>(x => x.Level, x => x.SortOrder)

                ;

            return uow.Database.Query<ContentSourceDto>(sql).Select(CreateMediaNodeKit);
        }

        private static ContentNodeKit CreateContentNodeKit(ContentSourceDto dto)
        {
            ContentData d = null;
            ContentData p = null;

            if (dto.Edited)
            {
                if (dto.DraftData == null)
                {
                    if (Debugger.IsAttached)
                        throw new Exception("Missing cmsContentNu edited content for node " + dto.Id + ", consider rebuilding.");
                    Current.Logger.Warn<Database>("Missing cmsContentNu edited content for node " + dto.Id + ", consider rebuilding.");
                }
                else
                {
                    d = new ContentData
                    {
                        Name = dto.DraftName,
                        Published = false,
                        TemplateId = dto.DraftTemplateId,
                        Version = dto.Version,
                        VersionDate = dto.DraftVersionDate,
                        WriterId = dto.DraftWriterId,
                        Properties = DeserializeData(dto.DraftData)
                    };
                }
            }

            if (dto.Published)
            {
                if (dto.PubData == null)
                {
                    if (Debugger.IsAttached)
                        throw new Exception("Missing cmsContentNu published content for node " + dto.Id + ", consider rebuilding.");
                    Current.Logger.Warn<Database>("Missing cmsContentNu published content for node " + dto.Id + ", consider rebuilding.");
                }
                else
                {
                    p = new ContentData
                    {
                        Name = dto.PubName,
                        Published = true,
                        TemplateId = dto.PubTemplateId,
                        Version = dto.Version,
                        VersionDate = dto.PubVersionDate,
                        WriterId = dto.PubWriterId,
                        Properties = DeserializeData(dto.PubData)
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
            if (dto.PubData == null)
                throw new Exception("No data for media " + dto.Id);

            var p = new ContentData
            {
                Name = dto.PubName,
                Published = true,
                TemplateId = -1,
                Version = dto.Version,
                VersionDate = dto.PubVersionDate,
                WriterId = dto.CreatorId, // what-else?
                Properties = DeserializeData(dto.PubData)
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

        private static Dictionary<string, object> DeserializeData(string data)
        {
            // by default JsonConvert will deserialize our numeric values as Int64
            // which is bad, because they were Int32 in the database - take care

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new ForceInt32Converter() }
            };

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(data, settings);
        }
    }
}
