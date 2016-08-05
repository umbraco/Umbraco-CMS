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
        Description = "This checks the data integrity for the xml structures for content, media and members that are stored in the cmsContentXml table. This does not check the data integrity of the xml cache file, only the xml structures stored in the database used to create the xml cache file.",
        Group = "Data Integrity")]
    public class XmlDataIntegrityHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        private const string CheckContentXmlTableAction = "checkContentXmlTable";
        private const string CheckMediaXmlTableAction = "checkMediaXmlTable";
        private const string CheckMembersXmlTableAction = "checkMembersXmlTable";

        public XmlDataIntegrityHealthCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _sqlSyntax = HealthCheckContext.ApplicationContext.DatabaseContext.SqlSyntax;
            _services = HealthCheckContext.ApplicationContext.Services;
            _database = HealthCheckContext.ApplicationContext.DatabaseContext.Database;
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
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
            return new[] { CheckContent(), CheckMedia(), CheckMembers() };
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
                case CheckContentXmlTableAction:
                    _services.ContentService.RebuildXmlStructures();
                    return CheckContent();
                case CheckMediaXmlTableAction:
                    _services.MediaService.RebuildXmlStructures();
                    return CheckMedia();
                case CheckMembersXmlTableAction:
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

            var actions = new List<HealthCheckAction>();
            if (totalXml != total)
                actions.Add(new HealthCheckAction(CheckMembersXmlTableAction, Id));
            
            return new HealthCheckStatus(_textService.Localize("healthcheck/xmlDataIntegrityCheckMembers", new[] { totalXml.ToString(), total.ToString(), (total - totalXml).ToString() }))
            {
                ResultType = totalXml == total ? StatusResultType.Success : StatusResultType.Error,
                Actions = actions
            };
        }

        /// <summary>
        /// Checks the cmsContentXml table to see if the number of entries for media matches the number of media entities
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Further to just counting the data integrity, this also checks if the XML stored in the cmsContentXml table contains the correct
        /// GUID identifier. In older versions of Umbraco, the GUID is not persisted to the content cache, that will cause problems in newer
        /// versions of Umbraco, so the xml table would need a rebuild.
        /// </remarks>
        private HealthCheckStatus CheckMedia()
        {
            var total = _services.MediaService.Count();
            var mediaObjectType = Guid.Parse(Constants.ObjectTypes.Media);

            //count entries
            var countTotalSubQuery = new Sql()
                .Select("Count(*)")
                .From<ContentXmlDto>(_sqlSyntax)
                .InnerJoin<NodeDto>(_sqlSyntax)
                .On<ContentXmlDto, NodeDto>(_sqlSyntax, left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(dto => dto.NodeObjectType == mediaObjectType);
            var totalXml = _database.ExecuteScalar<int>(countTotalSubQuery);

            //count invalid entries
            var countNonGuidQuery = new Sql()
                .Select("Count(*)")
                .From<ContentXmlDto>(_sqlSyntax)
                .InnerJoin<NodeDto>(_sqlSyntax)
                .On<ContentXmlDto, NodeDto>(_sqlSyntax, left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(dto => dto.NodeObjectType == mediaObjectType)
                .Where(string.Format("{0}.{1} NOT LIKE '% key=\"%'", _sqlSyntax.GetQuotedTableName("cmsContentXml"), _sqlSyntax.GetQuotedColumnName("xml")));
            var totalNonGuidXml = _database.ExecuteScalar<int>(countNonGuidQuery);

            var hasError = false;
            var actions = new List<HealthCheckAction>();
            if (totalXml != total || totalNonGuidXml > 0)
            {
                //if the counts don't match
                actions.Add(new HealthCheckAction(CheckMediaXmlTableAction, Id));
                hasError = true;
            }         

            return new HealthCheckStatus(_textService.Localize("healthcheck/xmlDataIntegrityCheckMedia",
                new[] { totalXml.ToString(), total.ToString(), totalNonGuidXml.ToString() }))
            {
                ResultType = hasError == false
                    ? StatusResultType.Success
                    : StatusResultType.Error,
                Actions = actions
            };
        }

        /// <summary>
        /// Checks the cmsContentXml table to see if the number of entries for content matches the number of published content entities
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Further to just counting the data integrity, this also checks if the XML stored in the cmsContentXml table contains the correct
        /// GUID identifier. In older versions of Umbraco, the GUID is not persisted to the content cache, that will cause problems in newer
        /// versions of Umbraco, so the xml table would need a rebuild.
        /// </remarks>
        private HealthCheckStatus CheckContent()
        {
            var total = _services.ContentService.CountPublished();
            var documentObjectType = Guid.Parse(Constants.ObjectTypes.Document);

            //count entires
            var countTotalSubQuery = new Sql()
                .Select(string.Format("DISTINCT {0}.{1}", _sqlSyntax.GetQuotedTableName("cmsContentXml"), _sqlSyntax.GetQuotedColumnName("nodeId")))
                .From<ContentXmlDto>(_sqlSyntax)
                .InnerJoin<DocumentDto>(_sqlSyntax)
                .On<DocumentDto, ContentXmlDto>(_sqlSyntax, left => left.NodeId, right => right.NodeId);
            var totalXml = _database.ExecuteScalar<int>("SELECT COUNT(*) FROM (" + countTotalSubQuery.SQL + ") as tmp");
            
            //count invalid entries
            var countNonGuidQuery = new Sql()
                .Select("Count(*)")
                .From<ContentXmlDto>(_sqlSyntax)
                .InnerJoin<NodeDto>(_sqlSyntax)
                .On<ContentXmlDto, NodeDto>(_sqlSyntax, left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(dto => dto.NodeObjectType == documentObjectType)
                .Where(string.Format("{0}.{1} NOT LIKE '% key=\"%'", _sqlSyntax.GetQuotedTableName("cmsContentXml"), _sqlSyntax.GetQuotedColumnName("xml")));
            var totalNonGuidXml = _database.ExecuteScalar<int>(countNonGuidQuery);

            var hasError = false;
            var actions = new List<HealthCheckAction>();
            if (totalXml != total || totalNonGuidXml > 0)
            {
                //if the counts don't match
                actions.Add(new HealthCheckAction(CheckContentXmlTableAction, Id));
                hasError = true;
            }            

            return new HealthCheckStatus(_textService.Localize("healthcheck/xmlDataIntegrityCheckContent", 
                new[] { totalXml.ToString(), total.ToString(), totalNonGuidXml.ToString() }))
            {
                ResultType = hasError == false
                    ? StatusResultType.Success 
                    : StatusResultType.Error,
                Actions = actions
            };
        }
    }
}