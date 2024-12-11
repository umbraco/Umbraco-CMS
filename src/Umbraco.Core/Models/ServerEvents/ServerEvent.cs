namespace Umbraco.Cms.Core.Models.ServerEvents;

public class ServerEvent
{
    public EventType EventType { get; set; }

    public EventSource EventSource { get; set; }

    public Guid Key { get; set; }
}
