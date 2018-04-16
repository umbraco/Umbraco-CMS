using System;

namespace Umbraco.Core.Events
{
    internal class EventDefinition : EventDefinitionBase
    {
        private readonly EventHandler _trackedEvent;
        private readonly object _sender;
        private readonly EventArgs _args;

        public EventDefinition(EventHandler trackedEvent, object sender, EventArgs args, string eventName = null)
            : base(sender, args, eventName)
        {
            _trackedEvent = trackedEvent;
            _sender = sender;
            _args = args;
        }

        public override void RaiseEvent()
        {
            if (_trackedEvent != null)
            {
                _trackedEvent(_sender, _args);
            }
        }
    }

    internal class EventDefinition<TEventArgs> : EventDefinitionBase
    {
        private readonly EventHandler<TEventArgs> _trackedEvent;
        private readonly object _sender;
        private readonly TEventArgs _args;

        public EventDefinition(EventHandler<TEventArgs> trackedEvent, object sender, TEventArgs args, string eventName = null)
            : base(sender, args, eventName)
        {
            _trackedEvent = trackedEvent;
            _sender = sender;
            _args = args;
        }

        public override void RaiseEvent()
        {
            if (_trackedEvent != null)
            {
                _trackedEvent(_sender, _args);
            }
        }
    }

    internal class EventDefinition<TSender, TEventArgs> : EventDefinitionBase
    {
        private readonly TypedEventHandler<TSender, TEventArgs> _trackedEvent;
        private readonly TSender _sender;
        private readonly TEventArgs _args;

        public EventDefinition(TypedEventHandler<TSender, TEventArgs> trackedEvent, TSender sender, TEventArgs args, string eventName = null)
            : base(sender, args, eventName)
        {
            _trackedEvent = trackedEvent;
            _sender = sender;
            _args = args;
        }

        public override void RaiseEvent()
        {
            if (_trackedEvent != null)
            {
                _trackedEvent(_sender, _args);
            }
        }
    }
}
