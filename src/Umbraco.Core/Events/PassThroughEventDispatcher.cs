using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// This event manager supports event cancellation and will raise the events as soon as they are tracked, it does not store tracked events
    /// </summary>
    internal class PassThroughEventDispatcher : IEventDispatcher
    {
        public bool DispatchCancelable(EventHandler eventHandler, object sender, CancellableEventArgs args, string eventName = null)
        {
            if (eventHandler == null) return args.Cancel;
            eventHandler(sender, args);
            return args.Cancel;
        }

        public bool DispatchCancelable<TArgs>(EventHandler<TArgs> eventHandler, object sender, TArgs args, string eventName = null)
            where TArgs : CancellableEventArgs
        {
            if (eventHandler == null) return args.Cancel;
            eventHandler(sender, args);
            return args.Cancel;
        }

        public bool DispatchCancelable<TSender, TArgs>(TypedEventHandler<TSender, TArgs> eventHandler, TSender sender, TArgs args, string eventName = null)
            where TArgs : CancellableEventArgs
        {
            if (eventHandler == null) return args.Cancel;
            eventHandler(sender, args);
            return args.Cancel;
        }

        public void Dispatch(EventHandler eventHandler, object sender, EventArgs args, string eventName = null)
        {
            if (eventHandler == null) return;
            eventHandler(sender, args);
        }

        public void Dispatch<TArgs>(EventHandler<TArgs> eventHandler, object sender, TArgs args, string eventName = null)
        {
            if (eventHandler == null) return;
            eventHandler(sender, args);
        }

        public void Dispatch<TSender, TArgs>(TypedEventHandler<TSender, TArgs> eventHandler, TSender sender, TArgs args, string eventName = null)
        {
            if (eventHandler == null) return;
            eventHandler(sender, args);
        }

        public IEnumerable<IEventDefinition> GetEvents(EventDefinitionFilter filter)
        {
            return Enumerable.Empty<IEventDefinition>();
        }

        public void ScopeExit(bool completed)
        { }
    }
}