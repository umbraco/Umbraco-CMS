namespace Umbraco.Cms.Api.Management.ViewModels.Content;

/// <summary>
/// Request model for specifying culture and scheduling settings for publishable content.
/// </summary>
public class CultureAndScheduleRequestModel
{
    /// <summary>
    /// Gets or sets the culture. Null means invariant.
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    /// Gets or sets the schedule of publishing. Null means immediately.
    /// </summary>
    public ScheduleRequestModel? Schedule { get; set; }
}
