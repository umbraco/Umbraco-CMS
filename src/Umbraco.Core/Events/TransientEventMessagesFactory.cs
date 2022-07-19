namespace Umbraco.Cms.Core.Events;

/// <summary>
///     A simple/default transient messages factory
/// </summary>
public class TransientEventMessagesFactory : IEventMessagesFactory
{
    public EventMessages Get() => new EventMessages();

    public EventMessages? GetOrDefault() => null;
}
