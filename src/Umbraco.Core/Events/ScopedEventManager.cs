using System;
using System.Collections.Generic;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// This event manager does not support event cancellation and will track all raised events in a list which can be retrieved
    /// </summary>
    internal class ScopedEventManager : DisposableObject, IEventManager
    {
        public void TrackEvent(EventHandler e, object sender, EventArgs args, string eventName = null)
        {
            _tracked.Add(new EventDefinition(e, sender, args, eventName));
        }

        public void TrackEvent<TEventArgs>(EventHandler<TEventArgs> e, object sender, TEventArgs args, string eventName = null)
        {
            _tracked.Add(new EventDefinition<TEventArgs>(e, sender, args, eventName));
        }

        public void TrackEvent<TSender, TEventArgs>(TypedEventHandler<TSender, TEventArgs> e, TSender sender, TEventArgs args, string eventName = null)
        {
            _tracked.Add(new EventDefinition<TSender, TEventArgs>(e, sender, args, eventName));
        }

        public IEnumerable<IEventDefinition> GetEvents()
        {
            return _tracked;
        }

        private readonly List<EventDefinitionBase> _tracked = new List<EventDefinitionBase>();

        public bool SupportsEventCancellation
        {
            get { return false; }
        }

        protected override void DisposeResources()
        {
            _tracked.Clear();
        }
    }
}