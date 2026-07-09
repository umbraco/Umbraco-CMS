namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents an event definition that wraps a standard <see cref="EventHandler" />.
/// </summary>
public class EventDefinition : EventDefinitionBase
{
    private readonly EventArgs _args;
    private readonly object _sender;
    private readonly EventHandler _trackedEvent;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EventDefinition" /> class.
    /// </summary>
    /// <param name="trackedEvent">The event handler to track.</param>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The event arguments.</param>
    /// <param name="eventName">The optional name of the event.</param>
    public EventDefinition(EventHandler trackedEvent, object sender, EventArgs args, string? eventName = null)
        : base(sender, args, eventName)
    {
        _trackedEvent = trackedEvent;
        _sender = sender;
        _args = args;
    }

    /// <inheritdoc />
    public override void RaiseEvent()
    {
        _trackedEvent?.Invoke(_sender, _args);
    }
}

/// <summary>
///     Represents an event definition that wraps a generic <see cref="EventHandler{TEventArgs}" />.
/// </summary>
/// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
public class EventDefinition<TEventArgs> : EventDefinitionBase
{
    private readonly TEventArgs _args;
    private readonly object _sender;
    private readonly EventHandler<TEventArgs> _trackedEvent;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EventDefinition{TEventArgs}" /> class.
    /// </summary>
    /// <param name="trackedEvent">The event handler to track.</param>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The event arguments.</param>
    /// <param name="eventName">The optional name of the event.</param>
    public EventDefinition(EventHandler<TEventArgs> trackedEvent, object sender, TEventArgs args, string? eventName = null)
        : base(sender, args, eventName)
    {
        _trackedEvent = trackedEvent;
        _sender = sender;
        _args = args;
    }

    /// <inheritdoc />
    public override void RaiseEvent()
    {
        _trackedEvent?.Invoke(_sender, _args);
    }
}

/// <summary>
///     Represents an event definition that wraps a <see cref="TypedEventHandler{TSender, TEventArgs}" />.
/// </summary>
/// <typeparam name="TSender">The type of the event sender.</typeparam>
/// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
public class EventDefinition<TSender, TEventArgs> : EventDefinitionBase
{
    private readonly TEventArgs _args;
    private readonly TSender _sender;
    private readonly TypedEventHandler<TSender, TEventArgs> _trackedEvent;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EventDefinition{TSender, TEventArgs}" /> class.
    /// </summary>
    /// <param name="trackedEvent">The event handler to track.</param>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The event arguments.</param>
    /// <param name="eventName">The optional name of the event.</param>
    public EventDefinition(TypedEventHandler<TSender, TEventArgs> trackedEvent, TSender sender, TEventArgs args, string? eventName = null)
        : base(sender, args, eventName)
    {
        _trackedEvent = trackedEvent;
        _sender = sender;
        _args = args;
    }

    /// <inheritdoc />
    public override void RaiseEvent()
    {
        _trackedEvent?.Invoke(_sender, _args);
    }
}
