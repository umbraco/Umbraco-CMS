using System;
using System.Collections.Generic;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// This event manager is created for each scope and is aware of if it is nested in an outer scope
    /// </summary>
    /// <remarks>
    /// The outer scope is the only scope that can raise events, the inner scope's will defer to the outer scope
    /// </remarks>
    internal class ScopedEventManager : DisposableObject, IEventManager
    {
        private readonly bool _raiseEvents;
        private readonly IEventManager _outerScope;

        /// <summary>
        /// Outer scope ctor
        /// </summary>
        /// <param name="raiseEvents">
        /// true to raise all tracked events when disposed
        /// </param>
        public ScopedEventManager(bool raiseEvents)
        {
            _raiseEvents = raiseEvents;
            _outerScope = null;
        }

        /// <summary>
        /// Inner scope ctor
        /// </summary>
        /// <param name="outerScope"></param>
        public ScopedEventManager(IEventManager outerScope)
        {
            _outerScope = outerScope;
        }

        public void QueueEvent(EventHandler e, object sender, EventArgs args, string eventName = null)
        {
            if (_outerScope == null)
            {
                _tracked.Add(new EventDefinition(e, sender, args, eventName));
            }
            else
            {
                _outerScope.QueueEvent(e, sender, args, eventName);
            }
        }

        public void QueueEvent<TEventArgs>(EventHandler<TEventArgs> e, object sender, TEventArgs args, string eventName = null)
        {
            if (_outerScope == null)
            {
                _tracked.Add(new EventDefinition<TEventArgs>(e, sender, args, eventName));
            }
            else
            {
                _outerScope.QueueEvent<TEventArgs>(e, sender, args, eventName);
            }
        }

        public void QueueEvent<TSender, TEventArgs>(TypedEventHandler<TSender, TEventArgs> e, TSender sender, TEventArgs args, string eventName = null)
        {
            if (_outerScope == null)
            {
                _tracked.Add(new EventDefinition<TSender, TEventArgs>(e, sender, args, eventName));
            }
            else
            {
                _outerScope.QueueEvent<TSender, TEventArgs>(e, sender, args, eventName);
            }
        }

        public IEnumerable<IEventDefinition> GetEvents()
        {
            return _tracked;
        }

        private readonly List<EventDefinitionBase> _tracked = new List<EventDefinitionBase>();

        public bool SupportsEventCancellation
        {
            get
            {
                //if there is no outer scope then this is the 'root' scope, in which case
                // event cancelation is supported since events are not queued.
                return _outerScope == null;
            }
        }

        /// <summary>
        /// When the outer scope is disposed, events will be raised if configured to do so
        /// </summary>
        protected override void DisposeResources()
        {
            if (_raiseEvents)
            {
                foreach (var e in _tracked)
                {
                    e.RaiseEvent();
                }
            }

            _tracked.Clear();
        }
    }
}