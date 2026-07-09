namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Provides a base class for classes representing event data, for events that support cancellation, and expose an
///     impacted object.
/// </summary>
public abstract class CancellableObjectEventArgs : CancellableEventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableObjectEventArgs" /> class.
    /// </summary>
    /// <param name="eventObject">The impacted object.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    /// <param name="messages">The event messages.</param>
    /// <param name="additionalData">Additional data associated with the event.</param>
    protected CancellableObjectEventArgs(object? eventObject, bool canCancel, EventMessages messages, IDictionary<string, object> additionalData)
        : base(canCancel, messages, additionalData) =>
        EventObject = eventObject;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableObjectEventArgs" /> class.
    /// </summary>
    /// <param name="eventObject">The impacted object.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    /// <param name="eventMessages">The event messages.</param>
    protected CancellableObjectEventArgs(object? eventObject, bool canCancel, EventMessages eventMessages)
        : base(canCancel, eventMessages) =>
        EventObject = eventObject;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableObjectEventArgs" /> class with cancellation enabled.
    /// </summary>
    /// <param name="eventObject">The impacted object.</param>
    /// <param name="eventMessages">The event messages.</param>
    protected CancellableObjectEventArgs(object? eventObject, EventMessages eventMessages)
        : this(eventObject, true, eventMessages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableObjectEventArgs" /> class.
    /// </summary>
    /// <param name="eventObject">The impacted object.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    protected CancellableObjectEventArgs(object? eventObject, bool canCancel)
        : base(canCancel) =>
        EventObject = eventObject;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableObjectEventArgs" /> class with cancellation enabled.
    /// </summary>
    /// <param name="eventObject">The impacted object.</param>
    protected CancellableObjectEventArgs(object? eventObject)
        : this(eventObject, true)
    {
    }

    /// <summary>
    ///     Gets or sets the impacted object.
    /// </summary>
    /// <remarks>
    ///     This is protected so that inheritors can expose it with their own name
    /// </remarks>
    public object? EventObject { get; set; }
}
