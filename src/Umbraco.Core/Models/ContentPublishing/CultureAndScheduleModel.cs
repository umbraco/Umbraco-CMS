namespace Umbraco.Cms.Core.Models.ContentPublishing;

public class CultureAndScheduleModel
{
    public required ISet<string> CulturesToPublishImmediately { get; set; }
    public required ContentScheduleCollection Schedules { get; set; }
}
