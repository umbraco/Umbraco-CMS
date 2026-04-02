namespace Umbraco.Cms.Core.Models.ContentPublishing;

/// <summary>
///     Represents a model for scheduling content publishing for a specific culture.
/// </summary>
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

/// <summary>
///     Represents the scheduling model for content publishing and unpublishing dates.
/// </summary>
public class ContentScheduleModel
{
    /// <summary>
    ///     Gets or sets the date and time when the content should be published.
    /// </summary>
    public DateTimeOffset? PublishDate { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the content should be unpublished.
    /// </summary>
    public DateTimeOffset? UnpublishDate { get; set; }
}
