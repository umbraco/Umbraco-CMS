namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Provides access to the current <see cref="Events.EventMessages" /> instance.
/// </summary>
public interface IEventMessagesAccessor
{
    /// <summary>
    ///     Gets or sets the current event messages instance.
    /// </summary>
    EventMessages? EventMessages { get; set; }
}
