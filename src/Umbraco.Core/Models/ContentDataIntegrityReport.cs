namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a report of data integrity issues detected in content data.
/// </summary>
public class ContentDataIntegrityReport
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentDataIntegrityReport" /> class.
    /// </summary>
    /// <param name="detectedIssues">A dictionary of detected issues keyed by content ID.</param>
    public ContentDataIntegrityReport(IReadOnlyDictionary<int, ContentDataIntegrityReportEntry> detectedIssues) =>
        DetectedIssues = detectedIssues;

    /// <summary>
    ///     Defines the types of data integrity issues that can be detected.
    /// </summary>
    public enum IssueType
    {
        /// <summary>
        ///     The item's level and path are inconsistent with it's parent's path and level
        /// </summary>
        InvalidPathAndLevelByParentId,

        /// <summary>
        ///     The item's path doesn't contain all required parts
        /// </summary>
        InvalidPathEmpty,

        /// <summary>
        ///     The item's path parts are inconsistent with it's level value
        /// </summary>
        InvalidPathLevelMismatch,

        /// <summary>
        ///     The item's path does not end with it's own ID
        /// </summary>
        InvalidPathById,

        /// <summary>
        ///     The item's path does not have it's parent Id as the 2nd last entry
        /// </summary>
        InvalidPathByParentId,
    }

    /// <summary>
    ///     Gets a value indicating whether all detected issues have been resolved or no issues were found.
    /// </summary>
    public bool Ok => DetectedIssues.Count == 0 || DetectedIssues.Count == DetectedIssues.Values.Count(x => x.Fixed);

    /// <summary>
    ///     Gets a dictionary of all detected data integrity issues keyed by content ID.
    /// </summary>
    public IReadOnlyDictionary<int, ContentDataIntegrityReportEntry> DetectedIssues { get; }

    /// <summary>
    ///     Gets a dictionary of issues that have been successfully fixed, keyed by content ID.
    /// </summary>
    public IReadOnlyDictionary<int, ContentDataIntegrityReportEntry> FixedIssues
        => DetectedIssues.Where(x => x.Value.Fixed).ToDictionary(x => x.Key, x => x.Value);
}
