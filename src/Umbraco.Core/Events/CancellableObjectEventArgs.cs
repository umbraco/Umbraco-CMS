namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Provides a base class for classes representing event data, for events that support cancellation, and expose an
///     impacted object.
/// </summary>
public abstract class CancellableObjectEventArgs : CancellableEventArgs
{
    protected CancellableObjectEventArgs(object? eventObject, bool canCancel, EventMessages messages, IDictionary<string, object> additionalData)
        : base(canCancel, messages, additionalData) =>
        EventObject = eventObject;

    protected CancellableObjectEventArgs(object? eventObject, bool canCancel, EventMessages eventMessages)
        : base(canCancel, eventMessages) =>
        EventObject = eventObject;

    protected CancellableObjectEventArgs(object? eventObject, EventMessages eventMessages)
        : this(eventObject, true, eventMessages)
    {
    }

    protected CancellableObjectEventArgs(object? eventObject, bool canCancel)
        : base(canCancel) =>
        EventObject = eventObject;

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
