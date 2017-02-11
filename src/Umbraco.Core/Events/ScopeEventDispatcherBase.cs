using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Events
{
    public abstract class ScopeEventDispatcherBase : IEventDispatcher
    {
        private List<IEventDefinition> _events;
        private readonly bool _raiseCancelable;

        protected ScopeEventDispatcherBase(bool raiseCancelable)
        {
            _raiseCancelable = raiseCancelable;
        }

        private List<IEventDefinition> Events { get { return _events ?? (_events = new List<IEventDefinition>()); } }

        public bool DispatchCancelable(EventHandler eventHandler, object sender, CancellableEventArgs args, string eventName = null)
        {
            if (eventHandler == null) return args.Cancel;
            if (_raiseCancelable == false) return args.Cancel;
            eventHandler(sender, args);
            return args.Cancel;
        }

        public bool DispatchCancelable<TArgs>(EventHandler<TArgs> eventHandler, object sender, TArgs args, string eventName = null)
            where TArgs : CancellableEventArgs
        {
            if (eventHandler == null) return args.Cancel;
            if (_raiseCancelable == false) return args.Cancel;
            eventHandler(sender, args);
            return args.Cancel;
        }

        public bool DispatchCancelable<TSender, TArgs>(TypedEventHandler<TSender, TArgs> eventHandler, TSender sender, TArgs args, string eventName = null)
            where TArgs : CancellableEventArgs
        {
            if (eventHandler == null) return args.Cancel;
            if (_raiseCancelable == false) return args.Cancel;
            eventHandler(sender, args);
            return args.Cancel;
        }

        public void Dispatch(EventHandler eventHandler, object sender, EventArgs args, string eventName = null)
        {
            if (eventHandler == null) return;
            Events.Add(new EventDefinition(eventHandler, sender, args, eventName));
        }

        public void Dispatch<TArgs>(EventHandler<TArgs> eventHandler, object sender, TArgs args, string eventName = null)
        {
            if (eventHandler == null) return;
            Events.Add(new EventDefinition<TArgs>(eventHandler, sender, args, eventName));
        }

        public void Dispatch<TSender, TArgs>(TypedEventHandler<TSender, TArgs> eventHandler, TSender sender, TArgs args, string eventName = null)
        {
            if (eventHandler == null) return;
            Events.Add(new EventDefinition<TSender, TArgs>(eventHandler, sender, args, eventName));
        }

        public IEnumerable<IEventDefinition> GetEvents(EventDefinitionFilter filter)
        {
            if (_events == null)
                return Enumerable.Empty<IEventDefinition>();

            switch (filter)
            {
                case EventDefinitionFilter.All:
                    return _events;
                case EventDefinitionFilter.FirstIn:
                    var l1 = new OrderedHashSet<IEventDefinition>();
                    foreach (var e in _events)
                    {
                        l1.Add(e);
                    }
                    return l1;
                case EventDefinitionFilter.LastIn:
                    var l2 = new OrderedHashSet<IEventDefinition>(keepOldest: false);
                    foreach (var e in _events)
                    {
                        l2.Add(e);
                    }
                    return l2;
                default:
                    throw new ArgumentOutOfRangeException("filter", filter, null);
            }
        }

        public void ScopeExit(bool completed)
        {
            if (_events == null) return;
            if (completed)
                ScopeExitCompleted();
            _events.Clear();
        }

        protected abstract void ScopeExitCompleted();
    }
}