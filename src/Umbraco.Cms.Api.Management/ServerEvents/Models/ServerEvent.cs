namespace Umbraco.Cms.Api.Management.ServerEvents.Models;

public class ServerEvent
{
    public EventType EventType { get; set; }

    public EventSource EventSource { get; set; }

    public Guid Key { get; set; }
}
