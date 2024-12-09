namespace Umbraco.Cms.Api.Management.ServerEvents.Models;

public class ServerEvent
{
    public EventType EventType { get; set; }

    public required string SourceType { get; set; }

    public Guid Key { get; set; }
}
