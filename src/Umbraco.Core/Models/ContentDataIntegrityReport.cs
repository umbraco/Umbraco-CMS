namespace Umbraco.Cms.Core.Models;

public class ContentDataIntegrityReport
{
    public ContentDataIntegrityReport(IReadOnlyDictionary<int, ContentDataIntegrityReportEntry> detectedIssues) =>
        DetectedIssues = detectedIssues;

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

    public bool Ok => DetectedIssues.Count == 0 || DetectedIssues.Count == DetectedIssues.Values.Count(x => x.Fixed);

    public IReadOnlyDictionary<int, ContentDataIntegrityReportEntry> DetectedIssues { get; }

    public IReadOnlyDictionary<int, ContentDataIntegrityReportEntry> FixedIssues
        => DetectedIssues.Where(x => x.Value.Fixed).ToDictionary(x => x.Key, x => x.Value);
}
