namespace Umbraco.Cms.Core.Events;

public class EventDefinition : EventDefinitionBase
{
    private readonly EventArgs _args;
    private readonly object _sender;
    private readonly EventHandler _trackedEvent;

    public EventDefinition(EventHandler trackedEvent, object sender, EventArgs args, string? eventName = null)
        : base(sender, args, eventName)
    {
        _trackedEvent = trackedEvent;
        _sender = sender;
        _args = args;
    }

    public override void RaiseEvent()
    {
        _trackedEvent?.Invoke(_sender, _args);
    }
}

public class EventDefinition<TEventArgs> : EventDefinitionBase
{
    private readonly TEventArgs _args;
    private readonly object _sender;
    private readonly EventHandler<TEventArgs> _trackedEvent;

    public EventDefinition(EventHandler<TEventArgs> trackedEvent, object sender, TEventArgs args, string? eventName = null)
        : base(sender, args, eventName)
    {
        _trackedEvent = trackedEvent;
        _sender = sender;
        _args = args;
    }

    public override void RaiseEvent()
    {
        _trackedEvent?.Invoke(_sender, _args);
    }
}

public class EventDefinition<TSender, TEventArgs> : EventDefinitionBase
{
    private readonly TEventArgs _args;
    private readonly TSender _sender;
    private readonly TypedEventHandler<TSender, TEventArgs> _trackedEvent;

    public EventDefinition(TypedEventHandler<TSender, TEventArgs> trackedEvent, TSender sender, TEventArgs args, string? eventName = null)
        : base(sender, args, eventName)
    {
        _trackedEvent = trackedEvent;
        _sender = sender;
        _args = args;
    }

    public override void RaiseEvent()
    {
        _trackedEvent?.Invoke(_sender, _args);
    }
}
