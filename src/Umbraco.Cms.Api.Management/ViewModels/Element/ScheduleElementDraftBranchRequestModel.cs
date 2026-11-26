namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents the API request model for scheduling (or clearing the schedule of) an element draft branch's
/// automatic publish.
/// </summary>
public class ScheduleElementDraftBranchRequestModel
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
