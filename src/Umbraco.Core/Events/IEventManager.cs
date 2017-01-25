using System;
using System.Collections.Generic;

namespace Umbraco.Core.Events
{
    public interface IEventManager : IDisposable
    {
        IEnumerable<IEventDefinition> GetEvents();
        void QueueEvent(EventHandler e, object sender, EventArgs args, string eventName = null);
        void QueueEvent<TEventArgs>(EventHandler<TEventArgs> e, object sender, TEventArgs args, string eventName = null);
        void QueueEvent<TSender, TEventArgs>(TypedEventHandler<TSender, TEventArgs> e, TSender sender, TEventArgs args, string eventName = null);

        /// <summary>
        /// True or false depending on if the event manager supports event cancellation
        /// </summary>
        bool SupportsEventCancellation { get; }
    }
}