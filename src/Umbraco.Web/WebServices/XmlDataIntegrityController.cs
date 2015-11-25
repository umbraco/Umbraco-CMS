using System;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.WebServices
{
    [ValidateAngularAntiForgeryToken]
    public class XmlDataIntegrityController : UmbracoAuthorizedApiController
    {
        [HttpPost]
        public bool FixContentXmlTable()
        {
            Services.ContentService.RebuildXmlStructures();
            return CheckContentXmlTable();
        }

        [HttpPost]
        public bool FixMediaXmlTable()
        {
            Services.MediaService.RebuildXmlStructures();
            return CheckMediaXmlTable();
        }

        [HttpPost]
        public bool FixMembersXmlTable()
        {
            Services.MemberService.RebuildXmlStructures();
            return CheckMembersXmlTable();
        }

        [HttpGet]
        public bool CheckContentXmlTable()
        {
            var totalPublished = Services.ContentService.CountPublished();
            
            var subQuery = new Sql()
                .Select("DISTINCT cmsContentXml.nodeId")
                .From<ContentXmlDto>()
                .InnerJoin<DocumentDto>()
                .On<DocumentDto, ContentXmlDto>(left => left.NodeId, right => right.NodeId);

            var totalXml = ApplicationContext.DatabaseContext.Database.ExecuteScalar<int>("SELECT COUNT(*) FROM (" + subQuery.SQL + ") as tmp");

            return totalXml == totalPublished;
        }
        
        [HttpGet]
        public bool CheckMediaXmlTable()
        {
            var total = Services.MediaService.Count();
            var mediaObjectType = Guid.Parse(Constants.ObjectTypes.Media);
            var subQuery = new Sql()
                .Select("Count(*)")
                .From<ContentXmlDto>()
                .InnerJoin<NodeDto>()
                .On<ContentXmlDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(dto => dto.NodeObjectType == mediaObjectType);
            var totalXml = ApplicationContext.DatabaseContext.Database.ExecuteScalar<int>(subQuery);

            return totalXml == total;
        }

        [HttpGet]
        public bool CheckMembersXmlTable()
        {
            var total = Services.MemberService.Count();
            var memberObjectType = Guid.Parse(Constants.ObjectTypes.Member);
            var subQuery = new Sql()
                .Select("Count(*)")
                .From<ContentXmlDto>()
                .InnerJoin<NodeDto>()
                .On<ContentXmlDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(dto => dto.NodeObjectType == memberObjectType);
            var totalXml = ApplicationContext.DatabaseContext.Database.ExecuteScalar<int>(subQuery);

            return totalXml == total;
        }

        
    }
}