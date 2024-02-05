namespace Umbraco.Cms.Core.Models.ContentPublishing;

public class CultureAndScheduleModel
{
    public required IEnumerable<string> CulturesToPublishImmediately { get; set; }
    public required ContentScheduleCollection Schedules { get; set; }
}


