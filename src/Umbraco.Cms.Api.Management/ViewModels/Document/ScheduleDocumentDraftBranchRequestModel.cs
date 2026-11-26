namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the API request model for scheduling (or clearing the schedule of) a document draft branch's
/// automatic publish.
/// </summary>
public class ScheduleDocumentDraftBranchRequestModel
{
    /// <summary>
    /// Gets or sets the date and time (UTC) to automatically publish the draft branch, or <c>null</c> to clear
    /// an existing schedule.
    /// </summary>
    public DateTimeOffset? PublishDate { get; set; }

    /// <summary>
    /// Gets or sets the cultures to promote/publish when the schedule fires. For culture-variant content, must
    /// be non-empty and every entry must exist on the branch's own content. For invariant content, must be
    /// empty.
    /// </summary>
    public ISet<string> Cultures { get; set; } = new HashSet<string>();
}
