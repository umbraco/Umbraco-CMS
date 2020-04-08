using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Data
{
    [HealthCheck(
        "73DD0C1C-E0CA-4C31-9564-1DCA509788AF",
        "Database data integrity check",
        Description = "Checks for various data integrity issues in the Umbraco database.",
        Group = "Data Integrity")]
    public class DatabaseIntegrityCheck : HealthCheck
    {
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;
        private const string _fixMediaPaths = "fixMediaPaths";
        private const string _fixContentPaths = "fixContentPaths";
        private const string _fixMediaPathsTitle = "Fix media paths";
        private const string _fixContentPathsTitle = "Fix content paths";

        public DatabaseIntegrityCheck(IContentService contentService, IMediaService mediaService)
        {
            _contentService = contentService;
            _mediaService = mediaService;
        }

        /// <summary>
        /// Get the status for this health check
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[]
            {
                CheckDocuments(false),
                CheckMedia(false)
            };
        }

        private HealthCheckStatus CheckMedia(bool fix)
        {
            return CheckPaths(_fixMediaPaths, _fixMediaPathsTitle, Core.Constants.UdiEntityType.Media, fix,
                () => _mediaService.CheckDataIntegrity(new ContentDataIntegrityReportOptions {FixIssues = fix}));
        }

        private HealthCheckStatus CheckDocuments(bool fix)
        {
            return CheckPaths(_fixContentPaths, _fixContentPathsTitle, Core.Constants.UdiEntityType.Document, fix,
                () => _contentService.CheckDataIntegrity(new ContentDataIntegrityReportOptions {FixIssues = fix}));
        }

        private HealthCheckStatus CheckPaths(string actionAlias, string actionName, string entityType, bool detailedReport, Func<ContentDataIntegrityReport> doCheck)
        {
            var report = doCheck();

            var actions = new List<HealthCheckAction>();
            if (!report.Ok)
            {
                actions.Add(new HealthCheckAction(actionAlias, Id)
                {
                    Name = actionName
                });
            }

            return new HealthCheckStatus(GetReport(report, entityType, detailedReport))
            {
                ResultType = report.Ok ? StatusResultType.Success : StatusResultType.Error,
                Actions = actions
            };
        }

        private static string GetReport(ContentDataIntegrityReport report, string entityType, bool detailed)
        {
            var sb = new StringBuilder();

            if (report.Ok)
            {
                sb.AppendLine($"<p>All {entityType} paths are valid</p>");

                if (!detailed)
                    return sb.ToString();
            }
            else
            {
                sb.AppendLine($"<p>{report.DetectedIssues.Count} invalid {entityType} paths detected.</p>");
            }

            if (detailed && report.DetectedIssues.Count > 0)
            {
                sb.AppendLine("<ul>");
                foreach (var issueGroup in report.DetectedIssues.GroupBy(x => x.Value.IssueType))
                {
                    var countByGroup = issueGroup.Count();
                    var fixedByGroup = issueGroup.Count(x => x.Value.Fixed);
                    sb.AppendLine("<li>");
                    sb.AppendLine($"{countByGroup} issues of type <code>{issueGroup.Key}</code> ... {fixedByGroup} fixed");
                    sb.AppendLine("</li>");
                }
                sb.AppendLine("</ul>");
            }

            return sb.ToString();
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case _fixContentPaths:
                    return CheckDocuments(true);
                case _fixMediaPaths:
                    return CheckMedia(true);
                default:
                    throw new InvalidOperationException("Action not supported");
            }
        }
    }
}
