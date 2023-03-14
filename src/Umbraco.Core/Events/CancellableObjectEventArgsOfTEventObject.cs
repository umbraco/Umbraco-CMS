namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represent event data, for events that support cancellation, and expose an impacted object.
/// </summary>
/// <typeparam name="TEventObject">The type of the exposed, impacted object.</typeparam>
public class CancellableObjectEventArgs<TEventObject> : CancellableObjectEventArgs,
    IEquatable<CancellableObjectEventArgs<TEventObject>>
{
    public CancellableObjectEventArgs(TEventObject? eventObject, bool canCancel, EventMessages messages, IDictionary<string, object> additionalData)
        : base(eventObject, canCancel, messages, additionalData)
    {
    }

    public CancellableObjectEventArgs(TEventObject? eventObject, bool canCancel, EventMessages eventMessages)
        : base(eventObject, canCancel, eventMessages)
    {
    }

    public CancellableObjectEventArgs(TEventObject? eventObject, EventMessages eventMessages)
        : base(eventObject, eventMessages)
    {
    }

    public CancellableObjectEventArgs(TEventObject? eventObject, bool canCancel)
        : base(eventObject, canCancel)
    {
    }

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

    public static bool operator ==(
        CancellableObjectEventArgs<TEventObject> left,
        CancellableObjectEventArgs<TEventObject> right) => Equals(left, right);

    public static bool operator !=(
        CancellableObjectEventArgs<TEventObject> left,
        CancellableObjectEventArgs<TEventObject> right) => !Equals(left, right);

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
