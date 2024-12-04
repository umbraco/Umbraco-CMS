namespace Umbraco.Cms.Api.Management.ViewModels.Events;

public class SeverEventViewModel
{
    public EventType EventType { get; set; }

    public required string SourceType { get; set; }

    public Guid Key { get; set; }
}
