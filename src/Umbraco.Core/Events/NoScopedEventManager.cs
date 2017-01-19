using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Events
{
    internal class NoScopedEventManager : IEventManager
    {
        public void TrackEvent(EventHandler e, object sender, EventArgs args)
        {
            if (e != null) e(sender, args);
        }

        public void TrackEvent<TEventArgs>(EventHandler<TEventArgs> e, object sender, TEventArgs args)
        {
            if (e != null) e(sender, args);
        }

        public void TrackEvent<TSender, TEventArgs>(TypedEventHandler<TSender, TEventArgs> e, TSender sender, TEventArgs args)
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
    }
}