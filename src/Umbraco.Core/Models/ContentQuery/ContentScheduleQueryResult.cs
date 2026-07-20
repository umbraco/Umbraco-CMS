namespace Umbraco.Cms.Core.Models.ContentQuery;

/// <summary>
///     Represents the result of a content schedule query, containing the content item and its associated schedules.
/// </summary>
public class ContentScheduleQueryResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentScheduleQueryResult" /> class.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <param name="schedules">The collection of schedules associated with the content.</param>
    public ContentScheduleQueryResult(IContent content, ContentScheduleCollection schedules)
    {
        Content = content;
        Schedules = schedules;
    }

    /// <summary>
    ///     Gets the content item.
    /// </summary>
    public IContent Content { get; init; }

    /// <summary>
    ///     Gets the collection of schedules associated with the content.
    /// </summary>
    public ContentScheduleCollection Schedules { get; init; }
}
