namespace Umbraco.Cms.Core.Models.ContentQuery;

public class ContentScheduleQueryResult
{
    public ContentScheduleQueryResult(IContent content, ContentScheduleCollection schedules)
    {
        Content = content;
        Schedules = schedules;
    }

    public IContent Content { get; init; }

    public ContentScheduleCollection Schedules { get; init; }
}
