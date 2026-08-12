namespace Umbraco.Cms.Api.Management.ViewModels.Content;

/// <summary>
/// Request model for scheduling the publication of publishable content.
/// </summary>
public class ScheduleRequestModel
{
    /// <summary>
    /// Gets or sets the scheduled publish time for the content.
    /// </summary>
    public DateTimeOffset? PublishTime { get; set; }

    /// <summary>
    /// Gets or sets the time when the content should be unpublished.
    /// </summary>
    public DateTimeOffset? UnpublishTime { get; set; }
}
