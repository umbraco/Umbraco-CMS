namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Event messages factory
/// </summary>
public interface IEventMessagesFactory
{
    /// <summary>
    ///     Gets the current <see cref="EventMessages" /> instance, creating a new one if necessary.
    /// </summary>
    /// <returns>The current event messages instance.</returns>
    EventMessages Get();

    /// <summary>
    ///     Gets the current <see cref="EventMessages" /> instance if one exists.
    /// </summary>
    /// <returns>The current event messages instance, or <c>null</c> if none exists.</returns>
    EventMessages? GetOrDefault();
}
