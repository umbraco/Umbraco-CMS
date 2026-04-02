namespace Umbraco.Cms.Core.Models.ContentPublishing;

/// <summary>
///     Represents a model containing cultures to publish immediately and scheduled publishing information.
/// </summary>
public class CultureAndScheduleModel
{
    /// <summary>
    ///     Gets or sets the set of cultures that should be published immediately.
    /// </summary>
    public required ISet<string> CulturesToPublishImmediately { get; set; }

    /// <summary>
    ///     Gets or sets the collection of content schedules for scheduled publishing and unpublishing.
    /// </summary>
    public required ContentScheduleCollection Schedules { get; set; }
}
