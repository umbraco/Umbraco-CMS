namespace Umbraco.Cms.Core.Models;

public class ContentDataIntegrityReportEntry
{
    public ContentDataIntegrityReportEntry(ContentDataIntegrityReport.IssueType issueType) => IssueType = issueType;

    public ContentDataIntegrityReport.IssueType IssueType { get; }

    public bool Fixed { get; set; }
}
