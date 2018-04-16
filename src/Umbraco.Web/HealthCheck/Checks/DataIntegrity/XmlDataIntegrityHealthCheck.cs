using System;
using System.Collections.Generic;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;

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
    [HideFromTypeFinder] // only if running the Xml cache! added by XmlCacheComponent!
    public class XmlDataIntegrityHealthCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        private readonly PublishedCache.XmlPublishedCache.PublishedSnapshotService _publishedSnapshotService;

        private const string CheckContentXmlTableAction = "checkContentXmlTable";
        private const string CheckMediaXmlTableAction = "checkMediaXmlTable";
        private const string CheckMembersXmlTableAction = "checkMembersXmlTable";

        public XmlDataIntegrityHealthCheck(ILocalizedTextService textService, IPublishedSnapshotService publishedSnapshotService)
        {
            _textService = textService;

            _publishedSnapshotService = publishedSnapshotService as PublishedCache.XmlPublishedCache.PublishedSnapshotService;
            if (_publishedSnapshotService == null)
                throw new NotSupportedException("Unsupported IPublishedSnapshotService, only the Xml one is supported.");
        }

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
                    _publishedSnapshotService.RebuildContentAndPreviewXml();
                    return CheckContent();
                case CheckMediaXmlTableAction:
                    _publishedSnapshotService.RebuildMediaXml();
                    return CheckMedia();
                case CheckMembersXmlTableAction:
                    _publishedSnapshotService.RebuildMemberXml();
                    return CheckMembers();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckMembers()
        {
            return Check(_publishedSnapshotService.VerifyMemberXml(), CheckMembersXmlTableAction, "healthcheck/xmlDataIntegrityCheckMembers");
        }

        private HealthCheckStatus CheckMedia()
        {
            return Check(_publishedSnapshotService.VerifyMediaXml(), CheckMediaXmlTableAction, "healthcheck/xmlDataIntegrityCheckMedia");
        }

        private HealthCheckStatus CheckContent()
        {
            return Check(_publishedSnapshotService.VerifyContentAndPreviewXml(), CheckContentXmlTableAction, "healthcheck/xmlDataIntegrityCheckContent");
        }

        private HealthCheckStatus Check(bool ok, string action, string text)
        {
            var actions = new List<HealthCheckAction>();
            if (ok == false)
                actions.Add(new HealthCheckAction(action, Id));

            return new HealthCheckStatus(_textService.Localize(text, new[] { ok ? "ok" : "not ok" }))
            {
                ResultType = ok ? StatusResultType.Success : StatusResultType.Error,
                Actions = actions
            };
        }
    }
}
