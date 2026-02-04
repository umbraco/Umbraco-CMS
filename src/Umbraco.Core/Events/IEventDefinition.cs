namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents an event definition that can be tracked and raised by an event dispatcher.
/// </summary>
public interface IEventDefinition
{
    /// <summary>
    ///     Gets the source of the event.
    /// </summary>
    object Sender { get; }

    /// <summary>
    ///     Gets the event arguments.
    /// </summary>
    object Args { get; }

    /// <summary>
    ///     Gets the name of the event.
    /// </summary>
    string? EventName { get; }

    /// <summary>
    ///     Raises the event by invoking the tracked event handler.
    /// </summary>
    void RaiseEvent();
}
