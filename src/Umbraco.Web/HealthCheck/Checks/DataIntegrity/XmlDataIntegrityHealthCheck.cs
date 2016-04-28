using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.DataIntegrity
{
    /// <summary>
    /// This moves the functionality from the XmlIntegrity check dashboard into a health check
    /// </summary>
    [HealthCheck(
        "D999EB2B-64C2-400F-B50C-334D41F8589A",
        "XML Data Integrity", 
        Description = "Checks the integrity of the XML data in Umbraco.", 
        Group = "DataIntegrity")]
    public class XmlDataIntegrityHealthCheck : HealthCheck
    {
        public XmlDataIntegrityHealthCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _sqlSyntax = HealthCheckContext.ApplicationContext.DatabaseContext.SqlSyntax;
            _services = HealthCheckContext.ApplicationContext.Services;
            _database = HealthCheckContext.ApplicationContext.DatabaseContext.Database;
        }

        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly ServiceContext _services;
        private readonly UmbracoDatabase _database;

        /// <summary>
        /// Get the status for this health check
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[] {CheckContent(), CheckMedia(), CheckMembers()};
        }

        /// <summary>
        /// Executes the action and returns it's status
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "checkContentXmlTable":
                    _services.ContentService.RebuildXmlStructures();
                    return CheckContent();
                case "checkMediaXmlTable":
                    _services.MediaService.RebuildXmlStructures();
                    return CheckMedia();
                case "checkMembersXmlTable":
                    _services.MemberService.RebuildXmlStructures();
                    return CheckMembers();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckMembers()
        {
            var total = _services.MemberService.Count();
            var memberObjectType = Guid.Parse(Constants.ObjectTypes.Member);
            var subQuery = new Sql()
                .Select("Count(*)")
                .From<ContentXmlDto>(_sqlSyntax)
                .InnerJoin<NodeDto>(_sqlSyntax)
                .On<ContentXmlDto, NodeDto>(_sqlSyntax, left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(dto => dto.NodeObjectType == memberObjectType);
            var totalXml = _database.ExecuteScalar<int>(subQuery);

            return new HealthCheckStatus(string.Format("Total XML: {0}, Total: {1}", totalXml, total))
            {
                ResultType = totalXml == total ? StatusResultType.Success : StatusResultType.Error,
                Actions = new[]
                {
                    new HealthCheckAction("checkMembersXmlTable", Id)
                }
            };
        }

        private HealthCheckStatus CheckMedia()
        {
            var total = _services.MediaService.Count();
            var mediaObjectType = Guid.Parse(Constants.ObjectTypes.Media);
            var subQuery = new Sql()
                .Select("Count(*)")
                .From<ContentXmlDto>(_sqlSyntax)
                .InnerJoin<NodeDto>(_sqlSyntax)
                .On<ContentXmlDto, NodeDto>(_sqlSyntax, left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(dto => dto.NodeObjectType == mediaObjectType);
            var totalXml = _database.ExecuteScalar<int>(subQuery);

            return new HealthCheckStatus(string.Format("Total XML: {0}, Total: {1}", totalXml, total))
            {
                ResultType = totalXml == total ? StatusResultType.Success : StatusResultType.Error,
                Actions = new[]
                {
                    new HealthCheckAction("checkMediaXmlTable", Id)
                }
            };
        }

        private HealthCheckStatus CheckContent()
        {
            var total = _services.ContentService.CountPublished();
            var subQuery = new Sql()
                .Select("DISTINCT cmsContentXml.nodeId")
                .From<ContentXmlDto>(_sqlSyntax)
                .InnerJoin<DocumentDto>(_sqlSyntax)
                .On<DocumentDto, ContentXmlDto>(_sqlSyntax, left => left.NodeId, right => right.NodeId);
            var totalXml = _database.ExecuteScalar<int>("SELECT COUNT(*) FROM (" + subQuery.SQL + ") as tmp");

            return new HealthCheckStatus(string.Format("Total XML: {0}, Total Published: {1}", totalXml, total))
            {
                ResultType = totalXml == total ? StatusResultType.Success : StatusResultType.Error,
                Actions = new[]
                {
                    new HealthCheckAction("checkContentXmlTable", Id)
                }
            };
        }
    }
}