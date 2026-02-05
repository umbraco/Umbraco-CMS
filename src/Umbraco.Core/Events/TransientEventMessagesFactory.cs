namespace Umbraco.Cms.Core.Events;

/// <summary>
///     A simple/default transient messages factory
/// </summary>
public class TransientEventMessagesFactory : IEventMessagesFactory
{
    /// <inheritdoc />
    public EventMessages Get() => new EventMessages();

    /// <inheritdoc />
    public EventMessages? GetOrDefault() => null;
}
