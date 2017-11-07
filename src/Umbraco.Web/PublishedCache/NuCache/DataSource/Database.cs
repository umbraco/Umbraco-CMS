using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NPoco;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Serialization;
using Umbraco.Web.Composing;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    // provides efficient database access for NuCache
    internal class Database
    {
        public ContentNodeKit GetContentSource(IScopeUnitOfWork uow, int id)
        {
            var dto = uow.Database.Fetch<ContentSourceDto>(new Sql(@"SELECT
n.id Id, n.uniqueId Uid,
uContent.contentTypeId ContentTypeId,
n.level Level, n.path Path, n.sortOrder SortOrder, n.parentId ParentId,
n.createDate CreateDate, n.nodeUser CreatorId,
docDraft.text DraftName, docDraft.versionId DraftVersion, docDraft.updateDate DraftVersionDate, docDraft.writerUserId DraftWriterId, docDraft.templateId DraftTemplateId,
nuDraft.data DraftData,
docPub.text PubName, docPub.versionId PubVersion, docPub.updateDate PubVersionDate, docPub.writerUserId PubWriterId, docPub.templateId PubTemplateId,
nuPub.data PubData
FROM umbracoNode n
JOIN uContent ON (uContent.nodeId=n.id)
LEFT JOIN cmsDocument docDraft ON (docDraft.nodeId=n.id AND docDraft.newest=1 AND docDraft.published=0)
LEFT JOIN cmsDocument docPub ON (docPub.nodeId=n.id AND docPub.published=1)
LEFT JOIN cmsContentNu nuDraft ON (nuDraft.nodeId=n.id AND nuDraft.published=0)
LEFT JOIN cmsContentNu nuPub ON (nuPub.nodeId=n.id AND nuPub.published=1)
WHERE n.nodeObjectType=@objType AND n.id=@id
", new { objType = Constants.ObjectTypes.Document, /*id =*/ id })).FirstOrDefault();
            return dto == null ? new ContentNodeKit() : CreateContentNodeKit(dto);
        }

        public ContentNodeKit GetMediaSource(IScopeUnitOfWork uow, int id)
        {
            // should be only 1 version for medias

            var dto = uow.Database.Fetch<ContentSourceDto>(new Sql(@"SELECT
n.id Id, n.uniqueId Uid,
uContent.contentTypeId ContentTypeId,
n.level Level, n.path Path, n.sortOrder SortOrder, n.parentId ParentId,
n.createDate CreateDate, n.nodeUser CreatorId,
n.text PubName, ver.versionId PubVersion, ver.versionDate PubVersionDate,
nuPub.data PubData
FROM umbracoNode n
JOIN uContent ON (uContent.nodeId=n.id)
JOIN cmsContentVersion ver ON (ver.contentId=n.id)
LEFT JOIN cmsContentNu nuPub ON (nuPub.nodeId=n.id AND nuPub.published=1)
WHERE n.nodeObjectType=@objType AND n.id=@id
", new { objType = Constants.ObjectTypes.Media, /*id =*/ id })).FirstOrDefault();
            return dto == null ? new ContentNodeKit() : CreateMediaNodeKit(dto);
        }

        // we want arrays, we want them all loaded, not an enumerable

        public IEnumerable<ContentNodeKit> GetAllContentSources(IScopeUnitOfWork uow)
        {
            return uow.Database.Query<ContentSourceDto>(new Sql(@"SELECT
n.id Id, n.uniqueId Uid,
uContent.contentTypeId ContentTypeId,
n.level Level, n.path Path, n.sortOrder SortOrder, n.parentId ParentId,
n.createDate CreateDate, n.nodeUser CreatorId,
docDraft.text DraftName, docDraft.versionId DraftVersion, docDraft.updateDate DraftVersionDate, docDraft.writerUserId DraftWriterId, docDraft.templateId DraftTemplateId,
nuDraft.data DraftData,
docPub.text PubName, docPub.versionId PubVersion, docPub.updateDate PubVersionDate, docPub.writerUserId PubWriterId, docPub.templateId PubTemplateId,
nuPub.data PubData
FROM umbracoNode n
JOIN uContent ON (uContent.nodeId=n.id)
LEFT JOIN cmsDocument docDraft ON (docDraft.nodeId=n.id AND docDraft.newest=1 AND docDraft.published=0)
LEFT JOIN cmsDocument docPub ON (docPub.nodeId=n.id AND docPub.published=1)
LEFT JOIN cmsContentNu nuDraft ON (nuDraft.nodeId=n.id AND nuDraft.published=0)
LEFT JOIN cmsContentNu nuPub ON (nuPub.nodeId=n.id AND nuPub.published=1)
WHERE n.nodeObjectType=@objType
ORDER BY n.level, n.sortOrder
", new { objType = Constants.ObjectTypes.Document })).Select(CreateContentNodeKit);
        }

        public IEnumerable<ContentNodeKit> GetAllMediaSources(IScopeUnitOfWork uow)
        {
            // should be only 1 version for medias

            return uow.Database.Query<ContentSourceDto>(new Sql(@"SELECT
n.id Id, n.uniqueId Uid,
uContent.contentTypeId ContentTypeId,
n.level Level, n.path Path, n.sortOrder SortOrder, n.parentId ParentId,
n.createDate CreateDate, n.nodeUser CreatorId,
n.text PubName, ver.versionId PubVersion, ver.versionDate PubVersionDate,
nuPub.data PubData
FROM umbracoNode n
JOIN uContent ON (uContent.nodeId=n.id)
JOIN cmsContentVersion ver ON (ver.contentId=n.id)
LEFT JOIN cmsContentNu nuPub ON (nuPub.nodeId=n.id AND nuPub.published=1)
WHERE n.nodeObjectType=@objType
ORDER BY n.level, n.sortOrder
", new { objType = Constants.ObjectTypes.Media })).Select(CreateMediaNodeKit);
        }

        public IEnumerable<ContentNodeKit> GetBranchContentSources(IScopeUnitOfWork uow, int id)
        {
            return uow.Database.Query<ContentSourceDto>(new Sql(@"SELECT
n.id Id, n.uniqueId Uid,
uContent.contentTypeId ContentTypeId,
n.level Level, n.path Path, n.sortOrder SortOrder, n.parentId ParentId,
n.createDate CreateDate, n.nodeUser CreatorId,
docDraft.text DraftName, docDraft.versionId DraftVersion, docDraft.updateDate DraftVersionDate, docDraft.writerUserId DraftWriterId, docDraft.templateId DraftTemplateId,
nuDraft.data DraftData,
docPub.text PubName, docPub.versionId PubVersion, docPub.updateDate PubVersionDate, docPub.writerUserId PubWriterId, docPub.templateId PubTemplateId,
nuPub.data PubData
FROM umbracoNode n
JOIN umbracoNode x ON (n.id=x.id OR n.path LIKE " + uow.SqlContext.SqlSyntax.GetConcat("x.path", "',%'") + @")
JOIN uContent ON (uContent.nodeId=n.id)
LEFT JOIN cmsDocument docDraft ON (docDraft.nodeId=n.id AND docDraft.newest=1 AND docDraft.published=0)
LEFT JOIN cmsDocument docPub ON (docPub.nodeId=n.id AND docPub.published=1)
LEFT JOIN cmsContentNu nuDraft ON (nuDraft.nodeId=n.id AND nuDraft.published=0)
LEFT JOIN cmsContentNu nuPub ON (nuPub.nodeId=n.id AND nuPub.published=1)
WHERE n.nodeObjectType=@objType AND x.id=@id
ORDER BY n.level, n.sortOrder
", new { objType = Constants.ObjectTypes.Document, /*id =*/ id })).Select(CreateContentNodeKit);
        }

        public IEnumerable<ContentNodeKit> GetBranchMediaSources(IScopeUnitOfWork uow, int id)
        {
            // should be only 1 version for medias

            return uow.Database.Query<ContentSourceDto>(new Sql(@"SELECT
n.id Id, n.uniqueId Uid,
uContent.contentTypeId ContentTypeId,
n.level Level, n.path Path, n.sortOrder SortOrder, n.parentId ParentId,
n.createDate CreateDate, n.nodeUser CreatorId,
n.text PubName, ver.versionId PubVersion, ver.versionDate PubVersionDate,
nuPub.data PubData
FROM umbracoNode n
JOIN umbracoNode x ON (n.id=x.id OR n.path LIKE " + uow.SqlContext.SqlSyntax.GetConcat("x.path", "',%'") + @")
JOIN uContent ON (uContent.nodeId=n.id)
JOIN cmsContentVersion ver ON (ver.contentId=n.id)
LEFT JOIN cmsContentNu nuPub ON (nuPub.nodeId=n.id AND nuPub.published=1)
WHERE n.nodeObjectType=@objType AND x.id=@id
ORDER BY n.level, n.sortOrder
", new { objType = Constants.ObjectTypes.Media, /*id =*/ id })).Select(CreateMediaNodeKit);
        }

        public IEnumerable<ContentNodeKit> GetTypeContentSources(IScopeUnitOfWork uow, IEnumerable<int> ids)
        {
            return uow.Database.Query<ContentSourceDto>(new Sql(@"SELECT
n.id Id, n.uniqueId Uid,
uContent.contentTypeId ContentTypeId,
n.level Level, n.path Path, n.sortOrder SortOrder, n.parentId ParentId,
n.createDate CreateDate, n.nodeUser CreatorId,
docDraft.text DraftName, docDraft.versionId DraftVersion, docDraft.updateDate DraftVersionDate, docDraft.writerUserId DraftWriterId, docDraft.templateId DraftTemplateId,
nuDraft.data DraftData,
docPub.text PubName, docPub.versionId PubVersion, docPub.updateDate PubVersionDate, docPub.writerUserId PubWriterId, docPub.templateId PubTemplateId,
nuPub.data PubData
FROM umbracoNode n
JOIN uContent ON (uContent.nodeId=n.id)
LEFT JOIN cmsDocument docDraft ON (docDraft.nodeId=n.id AND docDraft.newest=1 AND docDraft.published=0)
LEFT JOIN cmsDocument docPub ON (docPub.nodeId=n.id AND docPub.published=1)
LEFT JOIN cmsContentNu nuDraft ON (nuDraft.nodeId=n.id AND nuDraft.published=0)
LEFT JOIN cmsContentNu nuPub ON (nuPub.nodeId=n.id AND nuPub.published=1)
WHERE n.nodeObjectType=@objType AND uContent.contentTypeId IN (@ids)
ORDER BY n.level, n.sortOrder
", new { objType = Constants.ObjectTypes.Document, /*id =*/ ids })).Select(CreateContentNodeKit);
        }

        public IEnumerable<ContentNodeKit> GetTypeMediaSources(IScopeUnitOfWork uow, IEnumerable<int> ids)
        {
            // should be only 1 version for medias

            return uow.Database.Query<ContentSourceDto>(new Sql(@"SELECT
n.id Id, n.uniqueId Uid,
uContent.contentTypeId ContentTypeId,
n.level Level, n.path Path, n.sortOrder SortOrder, n.parentId ParentId,
n.createDate CreateDate, n.nodeUser CreatorId,
n.text PubName, ver.versionId PubVersion, ver.versionDate PubVersionDate,
nuPub.data PubData
FROM umbracoNode n
JOIN uContent ON (uContent.nodeId=n.id)
JOIN cmsContentVersion ver ON (ver.contentId=n.id)
LEFT JOIN cmsContentNu nuPub ON (nuPub.nodeId=n.id AND nuPub.published=1)
WHERE n.nodeObjectType=@objType AND uContent.contentTypeId IN (@ids)
ORDER BY n.level, n.sortOrder
", new { objType = Constants.ObjectTypes.Media, /*id =*/ ids })).Select(CreateMediaNodeKit);
        }

        private static ContentNodeKit CreateContentNodeKit(ContentSourceDto dto)
        {
            ContentData d = null;
            ContentData p = null;

            if (dto.DraftVersion != Guid.Empty)
            {
                if (dto.DraftData == null)
                {
                    //throw new Exception("Missing cmsContentNu content for node " + dto.Id + ", consider rebuilding.");
                    Current.Logger.Warn<Database>("Missing cmsContentNu content for node " + dto.Id + ", consider rebuilding.");
                }
                else
                {
                    d = new ContentData
                    {
                        Name = dto.DraftName,
                        Published = false,
                        TemplateId = dto.DraftTemplateId,
                        Version = dto.DraftVersion,
                        VersionDate = dto.DraftVersionDate,
                        WriterId = dto.DraftWriterId,
                        Properties = DeserializeData(dto.DraftData)
                    };
                }
            }

            if (dto.PubVersion != Guid.Empty)
            {
                if (dto.PubData == null)
                {
                    //throw new Exception("Missing cmsContentNu content for node " + dto.Id + ", consider rebuilding.");
                    Current.Logger.Warn<Database>("Missing cmsContentNu content for node " + dto.Id + ", consider rebuilding.");
                }
                else
                {
                    p = new ContentData
                    {
                        Name = dto.PubName,
                        Published = true,
                        TemplateId = dto.PubTemplateId,
                        Version = dto.PubVersion,
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
                Version = dto.PubVersion,
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
