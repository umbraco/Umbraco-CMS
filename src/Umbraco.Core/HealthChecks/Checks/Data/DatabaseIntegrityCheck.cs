// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Data;

/// <summary>
///     Health check for the integrity of the data in the database.
/// </summary>
[HealthCheck(
    "73DD0C1C-E0CA-4C31-9564-1DCA509788AF",
    "Database data integrity check",
    Description = "Checks for various data integrity issues in the Umbraco database.",
    Group = "Data Integrity")]
public class DatabaseIntegrityCheck : HealthCheck
{
    private const string SSsFixMediaPaths = "fixMediaPaths";
    private const string SFixContentPaths = "fixContentPaths";
    private const string SFixMediaPathsTitle = "Fix media paths";
    private const string SFixContentPathsTitle = "Fix content paths";
    private readonly IContentService _contentService;
    private readonly IMediaService _mediaService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseIntegrityCheck" /> class.
    /// </summary>
    public DatabaseIntegrityCheck(
        IContentService contentService,
        IMediaService mediaService)
    {
        _contentService = contentService;
        _mediaService = mediaService;
    }

    /// <summary>
    ///     Get the status for this health check
    /// </summary>
    public override Task<IEnumerable<HealthCheckStatus>> GetStatus() =>
        Task.FromResult((IEnumerable<HealthCheckStatus>)new[] { CheckDocuments(false), CheckMedia(false) });

    /// <inheritdoc />
    public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
    {
        switch (action.Alias)
        {
            case SFixContentPaths:
                return CheckDocuments(true);
            case SSsFixMediaPaths:
                return CheckMedia(true);
            default:
                throw new InvalidOperationException("Action not supported");
        }
    }

    private static string GetReport(ContentDataIntegrityReport report, string entityType, bool detailed)
    {
        var sb = new StringBuilder();

        if (report.Ok)
        {
            sb.AppendLine($"<p>All {entityType} paths are valid</p>");

            if (!detailed)
            {
                return sb.ToString();
            }
        }
        else
        {
            sb.AppendLine($"<p>{report.DetectedIssues.Count} invalid {entityType} paths detected.</p>");
        }

        if (detailed && report.DetectedIssues.Count > 0)
        {
            sb.AppendLine("<ul>");
            foreach (IGrouping<ContentDataIntegrityReport.IssueType, KeyValuePair<int, ContentDataIntegrityReportEntry>>
                         issueGroup in report.DetectedIssues.GroupBy(x => x.Value.IssueType))
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

    private HealthCheckStatus CheckMedia(bool fix) =>
        CheckPaths(
            SSsFixMediaPaths,
            SFixMediaPathsTitle,
            Constants.UdiEntityType.Media,
            fix,
            () => _mediaService.CheckDataIntegrity(new ContentDataIntegrityReportOptions { FixIssues = fix }));

    private HealthCheckStatus CheckDocuments(bool fix) =>
        CheckPaths(
            SFixContentPaths,
            SFixContentPathsTitle,
            Constants.UdiEntityType.Document,
            fix,
            () => _contentService.CheckDataIntegrity(new ContentDataIntegrityReportOptions { FixIssues = fix }));

    private HealthCheckStatus CheckPaths(string actionAlias, string actionName, string entityType, bool detailedReport, Func<ContentDataIntegrityReport> doCheck)
    {
        ContentDataIntegrityReport report = doCheck();

        var actions = new List<HealthCheckAction>();
        if (!report.Ok)
        {
            actions.Add(new HealthCheckAction(actionAlias, Id) { Name = actionName });
        }

        return new HealthCheckStatus(GetReport(report, entityType, detailedReport))
        {
            ResultType = report.Ok ? StatusResultType.Success : StatusResultType.Error,
            Actions = actions,
        };
    }
}
