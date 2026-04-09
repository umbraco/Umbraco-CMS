namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a single entry in a content data integrity report.
/// </summary>
public class ContentDataIntegrityReportEntry
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentDataIntegrityReportEntry" /> class.
    /// </summary>
    /// <param name="issueType">The type of data integrity issue detected.</param>
    public ContentDataIntegrityReportEntry(ContentDataIntegrityReport.IssueType issueType) => IssueType = issueType;

    /// <summary>
    ///     Gets the type of data integrity issue that was detected.
    /// </summary>
    public ContentDataIntegrityReport.IssueType IssueType { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the issue has been fixed.
    /// </summary>
    public bool Fixed { get; set; }
}
