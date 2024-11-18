namespace Umbraco.Cms.Core.Models.ContentPublishing;

public class CulturePublishScheduleModel
{
    /// <summary>
    /// Gets or sets the culture. Null means invariant.
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    /// Gets or sets the schedule of publishing. Null means immediately.
    /// </summary>
    public ContentScheduleModel? Schedule { get; set; }
}

public class ContentScheduleModel
{
    public DateTimeOffset? PublishDate { get; set; }

    public DateTimeOffset? UnpublishDate { get; set; }
}
