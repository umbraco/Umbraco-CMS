namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represent event data, for events that support cancellation, and expose an impacted object.
/// </summary>
/// <typeparam name="TEventObject">The type of the exposed, impacted object.</typeparam>
public class CancellableObjectEventArgs<TEventObject> : CancellableObjectEventArgs,
    IEquatable<CancellableObjectEventArgs<TEventObject>>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableObjectEventArgs{TEventObject}" /> class.
    /// </summary>
    /// <param name="eventObject">The impacted object.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    /// <param name="messages">The event messages.</param>
    /// <param name="additionalData">Additional data associated with the event.</param>
    public CancellableObjectEventArgs(TEventObject? eventObject, bool canCancel, EventMessages messages, IDictionary<string, object> additionalData)
        : base(eventObject, canCancel, messages, additionalData)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableObjectEventArgs{TEventObject}" /> class.
    /// </summary>
    /// <param name="eventObject">The impacted object.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    /// <param name="eventMessages">The event messages.</param>
    public CancellableObjectEventArgs(TEventObject? eventObject, bool canCancel, EventMessages eventMessages)
        : base(eventObject, canCancel, eventMessages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableObjectEventArgs{TEventObject}" /> class with cancellation enabled.
    /// </summary>
    /// <param name="eventObject">The impacted object.</param>
    /// <param name="eventMessages">The event messages.</param>
    public CancellableObjectEventArgs(TEventObject? eventObject, EventMessages eventMessages)
        : base(eventObject, eventMessages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableObjectEventArgs{TEventObject}" /> class.
    /// </summary>
    /// <param name="eventObject">The impacted object.</param>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    public CancellableObjectEventArgs(TEventObject? eventObject, bool canCancel)
        : base(eventObject, canCancel)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableObjectEventArgs{TEventObject}" /> class with cancellation enabled.
    /// </summary>
    /// <param name="eventObject">The impacted object.</param>
    public CancellableObjectEventArgs(TEventObject? eventObject)
        : base(eventObject)
    {
    }

    /// <summary>
    ///     Gets or sets the impacted object.
    /// </summary>
    /// <remarks>
    ///     This is protected so that inheritors can expose it with their own name
    /// </remarks>
    protected new TEventObject? EventObject
    {
        get => (TEventObject?)base.EventObject;
        set => base.EventObject = value;
    }

    /// <summary>
    ///     Determines whether two <see cref="CancellableObjectEventArgs{TEventObject}" /> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(
        CancellableObjectEventArgs<TEventObject> left,
        CancellableObjectEventArgs<TEventObject> right) => Equals(left, right);

    /// <summary>
    ///     Determines whether two <see cref="CancellableObjectEventArgs{TEventObject}" /> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(
        CancellableObjectEventArgs<TEventObject> left,
        CancellableObjectEventArgs<TEventObject> right) => !Equals(left, right);

    /// <inheritdoc />
    public bool Equals(CancellableObjectEventArgs<TEventObject>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && EqualityComparer<TEventObject>.Default.Equals(EventObject, other.EventObject);
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

        return Equals((CancellableObjectEventArgs<TEventObject>)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            if (EventObject is not null)
            {
                return (base.GetHashCode() * 397) ^ EqualityComparer<TEventObject>.Default.GetHashCode(EventObject);
            }

            return base.GetHashCode() * 397;
        }
    }
}
