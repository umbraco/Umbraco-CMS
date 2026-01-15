namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class PublishDocumentRequestModel
{
    public required IEnumerable<CultureAndScheduleRequestModel> PublishSchedules { get; set; }
}


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


public class ScheduleRequestModel
{
    public DateTimeOffset? PublishTime { get; set; }

    public DateTimeOffset? UnpublishTime { get; set; }
}
