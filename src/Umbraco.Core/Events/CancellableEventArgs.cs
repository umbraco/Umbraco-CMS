using System.Collections.ObjectModel;

namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents event data for events that support cancellation.
/// </summary>
public class CancellableEventArgs : EventArgs, IEquatable<CancellableEventArgs>
{
    private static readonly ReadOnlyDictionary<string, object> EmptyAdditionalData = new(new Dictionary<string, object>());

    private bool _cancel;
    private IDictionary<string, object>? _eventState;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableEventArgs" /> class.
    /// </summary>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    /// <param name="messages">The event messages.</param>
    /// <param name="additionalData">Additional data associated with the event.</param>
    public CancellableEventArgs(bool canCancel, EventMessages messages, IDictionary<string, object> additionalData)
    {
        CanCancel = canCancel;
        Messages = messages;
        AdditionalData = new ReadOnlyDictionary<string, object>(additionalData);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableEventArgs" /> class.
    /// </summary>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    /// <param name="eventMessages">The event messages.</param>
    public CancellableEventArgs(bool canCancel, EventMessages eventMessages)
    {
        CanCancel = canCancel;
        Messages = eventMessages ?? throw new ArgumentNullException("eventMessages");
        AdditionalData = EmptyAdditionalData;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableEventArgs" /> class.
    /// </summary>
    /// <param name="canCancel">A value indicating whether the event can be cancelled.</param>
    public CancellableEventArgs(bool canCancel)
    {
        CanCancel = canCancel;

        // create a standalone messages
        Messages = new EventMessages();
        AdditionalData = EmptyAdditionalData;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableEventArgs" /> class with cancellation enabled.
    /// </summary>
    /// <param name="eventMessages">The event messages.</param>
    public CancellableEventArgs(EventMessages eventMessages)
        : this(true, eventMessages)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CancellableEventArgs" /> class with cancellation enabled.
    /// </summary>
    public CancellableEventArgs()
        : this(true)
    {
    }

    /// <summary>
    ///     Flag to determine if this instance will support being cancellable
    /// </summary>
    public bool CanCancel { get; set; }

    /// <summary>
    ///     If this instance supports cancellation, this gets/sets the cancel value
    /// </summary>
    public bool Cancel
    {
        get
        {
            if (CanCancel == false)
            {
                throw new InvalidOperationException("This event argument class does not support canceling.");
            }

            return _cancel;
        }

        set
        {
            if (CanCancel == false)
            {
                throw new InvalidOperationException("This event argument class does not support canceling.");
            }

            _cancel = value;
        }
    }

    /// <summary>
    ///     Returns the EventMessages object which is used to add messages to the message collection for this event
    /// </summary>
    public EventMessages Messages { get; }

    /// <summary>
    ///     In some cases raised evens might need to contain additional arbitrary readonly data which can be read by event
    ///     subscribers
    /// </summary>
    /// <remarks>
    ///     This allows for a bit of flexibility in our event raising - it's not pretty but we need to maintain backwards
    ///     compatibility
    ///     so we cannot change the strongly typed nature for some events.
    /// </remarks>
    public ReadOnlyDictionary<string, object> AdditionalData { get; set; }

    /// <summary>
    ///     This can be used by event subscribers to store state in the event args so they easily deal with custom state data
    ///     between a starting ("ing")
    ///     event and an ending ("ed") event
    /// </summary>
    public IDictionary<string, object> EventState
    {
        get => _eventState ??= new Dictionary<string, object>();
        set => _eventState = value;
    }

    /// <summary>
    ///     Determines whether two <see cref="CancellableEventArgs" /> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(CancellableEventArgs? left, CancellableEventArgs? right) => Equals(left, right);

    /// <summary>
    ///     Determines whether two <see cref="CancellableEventArgs" /> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(CancellableEventArgs left, CancellableEventArgs right) => Equals(left, right) == false;

    /// <inheritdoc />
    public bool Equals(CancellableEventArgs? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Equals(AdditionalData, other.AdditionalData);
    }

    /// <summary>
    ///     if this instance supports cancellation, this will set Cancel to true with an affiliated cancellation message
    /// </summary>
    /// <param name="cancelationMessage"></param>
    public void CancelOperation(EventMessage cancelationMessage)
    {
        Cancel = true;
        cancelationMessage.IsDefaultEventMessage = true;
        Messages.Add(cancelationMessage);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
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

        return Equals((CancellableEventArgs)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => AdditionalData != null ? AdditionalData.GetHashCode() : 0;
}
