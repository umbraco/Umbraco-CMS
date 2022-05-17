namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Event messages factory
/// </summary>
public interface IEventMessagesFactory
{
    EventMessages Get();

    EventMessages? GetOrDefault();
}
