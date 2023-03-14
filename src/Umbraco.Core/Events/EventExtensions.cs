namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Extension methods for cancellable event operations
/// </summary>
public static class EventExtensions
{
    // keep these two for backward compatibility reasons but understand that
    // they are *not* part of any scope / event dispatcher / anything...

    /// <summary>
    ///     Raises a cancelable event and returns a value indicating whether the event should be cancelled.
    /// </summary>
    /// <typeparam name="TSender">The type of the event source.</typeparam>
    /// <typeparam name="TArgs">The type of the event data.</typeparam>
    /// <param name="eventHandler">The event handler.</param>
    /// <param name="args">The event source.</param>
    /// <param name="sender">The event data.</param>
    /// <returns>A value indicating whether the cancelable event should be cancelled</returns>
    /// <remarks>A cancelable event is raised by a component when it is about to perform an action that can be canceled.</remarks>
    public static bool IsRaisedEventCancelled<TSender, TArgs>(this TypedEventHandler<TSender, TArgs> eventHandler, TArgs args, TSender sender)
        where TArgs : CancellableEventArgs
    {
        if (eventHandler == null)
        {
            return args.Cancel;
        }

        eventHandler(sender, args);
        return args.Cancel;
    }

    /// <summary>
    ///     Raises an event.
    /// </summary>
    /// <typeparam name="TSender">The type of the event source.</typeparam>
    /// <typeparam name="TArgs">The type of the event data.</typeparam>
    /// <param name="eventHandler">The event handler.</param>
    /// <param name="args">The event source.</param>
    /// <param name="sender">The event data.</param>
    public static void RaiseEvent<TSender, TArgs>(this TypedEventHandler<TSender, TArgs> eventHandler, TArgs args, TSender sender)
        where TArgs : EventArgs
    {
        if (eventHandler == null)
        {
            return;
        }

        eventHandler(sender, args);
    }
}
