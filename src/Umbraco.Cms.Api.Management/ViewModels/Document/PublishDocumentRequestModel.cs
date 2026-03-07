namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the data required to request the publishing of a document in the API.
/// </summary>
public class PublishDocumentRequestModel
{
    /// <summary>
    /// Gets or sets the collection of publish schedules for different cultures.
    /// </summary>
    public required IEnumerable<CultureAndScheduleRequestModel> PublishSchedules { get; set; }
}


/// <summary>
/// Request model for specifying culture and scheduling settings for a document.
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


    /// <summary>
    /// Request model for scheduling the publication of a document.
    /// </summary>
public class ScheduleRequestModel
{
    /// <summary>Gets or sets the scheduled publish time for the document.</summary>
    public DateTimeOffset? PublishTime { get; set; }

    /// <summary>
    /// Gets or sets the time when the document should be unpublished.
    /// </summary>
    public DateTimeOffset? UnpublishTime { get; set; }
}
