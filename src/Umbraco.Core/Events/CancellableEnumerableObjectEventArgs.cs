namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents event data, for events that support cancellation, and expose impacted objects.
/// </summary>
/// <typeparam name="TEventObject">The type of the exposed, impacted objects.</typeparam>
public class CancellableEnumerableObjectEventArgs<TEventObject> : CancellableObjectEventArgs<IEnumerable<TEventObject>>,
    IEquatable<CancellableEnumerableObjectEventArgs<TEventObject>>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableEnumerableObjectEventArgs{TEventObject}" /> class.
    /// </summary>
    /// <param name="eventObject">The collection of impacted objects.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    /// <param name="messages">The event messages.</param>
    /// <param name="additionalData">Additional data associated with the event.</param>
    public CancellableEnumerableObjectEventArgs(IEnumerable<TEventObject> eventObject, bool canCancel, EventMessages messages, IDictionary<string, object> additionalData)
        : base(eventObject, canCancel, messages, additionalData)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableEnumerableObjectEventArgs{TEventObject}" /> class.
    /// </summary>
    /// <param name="eventObject">The collection of impacted objects.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    /// <param name="eventMessages">The event messages.</param>
    public CancellableEnumerableObjectEventArgs(IEnumerable<TEventObject> eventObject, bool canCancel, EventMessages eventMessages)
        : base(eventObject, canCancel, eventMessages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableEnumerableObjectEventArgs{TEventObject}" /> class with cancellation enabled.
    /// </summary>
    /// <param name="eventObject">The collection of impacted objects.</param>
    /// <param name="eventMessages">The event messages.</param>
    public CancellableEnumerableObjectEventArgs(IEnumerable<TEventObject> eventObject, EventMessages eventMessages)
        : base(eventObject, eventMessages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableEnumerableObjectEventArgs{TEventObject}" /> class.
    /// </summary>
    /// <param name="eventObject">The collection of impacted objects.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    public CancellableEnumerableObjectEventArgs(IEnumerable<TEventObject> eventObject, bool canCancel)
        : base(eventObject, canCancel)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableEnumerableObjectEventArgs{TEventObject}" /> class with cancellation enabled.
    /// </summary>
    /// <param name="eventObject">The collection of impacted objects.</param>
    public CancellableEnumerableObjectEventArgs(IEnumerable<TEventObject> eventObject)
        : base(eventObject)
    {
    }

    /// <inheritdoc />
    public bool Equals(CancellableEnumerableObjectEventArgs<TEventObject>? other)
    {
        if (other is null || other.EventObject is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return EventObject?.SequenceEqual(other.EventObject) ?? false;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((CancellableEnumerableObjectEventArgs<TEventObject>)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (EventObject is not null)
        {
            return HashCodeHelper.GetHashCode(EventObject);
        }

        return base.GetHashCode();
    }
}
