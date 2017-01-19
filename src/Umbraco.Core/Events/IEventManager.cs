using System;
using System.Collections.Generic;

namespace Umbraco.Core.Events
{
    public interface IEventManager
    {
        IEnumerable<IEventDefinition> GetEvents();
        void TrackEvent(EventHandler e, object sender, EventArgs args);
        void TrackEvent<TEventArgs>(EventHandler<TEventArgs> e, object sender, TEventArgs args);
        void TrackEvent<TSender, TEventArgs>(TypedEventHandler<TSender, TEventArgs> e, TSender sender, TEventArgs args);

        /// <summary>
        /// True or false depending on if the event manager supports event cancellation
        /// </summary>
        bool SupportsEventCancellation { get; }
    }
}