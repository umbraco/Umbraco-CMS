using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// This event manager supports event cancellation and will raise the events as soon as they are tracked, it does not store tracked events
    /// </summary>
    internal class NoScopedEventManager : DisposableObject, IEventManager
    {
        public void TrackEvent(EventHandler e, object sender, EventArgs args, string eventName = null)
        {
            if (e != null) e(sender, args);
        }

        public void TrackEvent<TEventArgs>(EventHandler<TEventArgs> e, object sender, TEventArgs args, string eventName = null)
        {
            if (e != null) e(sender, args);
        }

        public void TrackEvent<TSender, TEventArgs>(TypedEventHandler<TSender, TEventArgs> e, TSender sender, TEventArgs args, string eventName = null)
        {
            if (e != null) e(sender, args);
        }

        public IEnumerable<IEventDefinition> GetEvents()
        {
            return Enumerable.Empty<IEventDefinition>();
        }

        public bool SupportsEventCancellation
        {
            get { return true; }
        }

        protected override void DisposeResources()
        {
            //noop
        }
    }
}