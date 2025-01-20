namespace Umbraco.Cms.Core.Models.ServerEvents;

public class ServerEvent
{
    public required string EventType { get; set; }

    public required string EventSource { get; set; }

    public Guid Key { get; set; }
}
