namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents event data, for events that support cancellation, and expose impacted objects.
/// </summary>
/// <typeparam name="TEventObject">The type of the exposed, impacted objects.</typeparam>
public class CancellableEnumerableObjectEventArgs<TEventObject> : CancellableObjectEventArgs<IEnumerable<TEventObject>>,
    IEquatable<CancellableEnumerableObjectEventArgs<TEventObject>>
{
    public CancellableEnumerableObjectEventArgs(IEnumerable<TEventObject> eventObject, bool canCancel, EventMessages messages, IDictionary<string, object> additionalData)
        : base(eventObject, canCancel, messages, additionalData)
    {
    }

    public CancellableEnumerableObjectEventArgs(IEnumerable<TEventObject> eventObject, bool canCancel, EventMessages eventMessages)
        : base(eventObject, canCancel, eventMessages)
    {
    }

    public CancellableEnumerableObjectEventArgs(IEnumerable<TEventObject> eventObject, EventMessages eventMessages)
        : base(eventObject, eventMessages)
    {
    }

    public CancellableEnumerableObjectEventArgs(IEnumerable<TEventObject> eventObject, bool canCancel)
        : base(eventObject, canCancel)
    {
    }

    public CancellableEnumerableObjectEventArgs(IEnumerable<TEventObject> eventObject)
        : base(eventObject)
    {
    }

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

    public override int GetHashCode()
    {
        if (EventObject is not null)
        {
            return HashCodeHelper.GetHashCode(EventObject);
        }

        return base.GetHashCode();
    }
}
