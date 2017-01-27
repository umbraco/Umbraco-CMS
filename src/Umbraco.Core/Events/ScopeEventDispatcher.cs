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
    internal class ScopeEventDispatcher : DisposableObject, IEventDispatcher
    {
        private readonly List<EventDefinitionBase> _events = new List<EventDefinitionBase>();

        // fixme - this is completely broken
        // rename ScopeEventDispatcher
        // needs to have MODES to indicate whether it is
        // - pass-through (just execute immediately) = default, same as today
        // - scoped (execute when scope disposes) = for deploy
        // + in scoped mode, shall we pass-through cancellable events or not?
        // + *which* events should execute and which should not
        //
        // we have to refactor the decision-making and it should take place HERE
        // and NOT in EventExtensions! in fact EventExtensions should be obsoleted!

        // fixme temp
        public bool PassThroughCancelable = true;
        public bool PassThrough = true; 
        public bool RaiseEvents = true; // fixme temp

        public bool DispatchCancelable(EventHandler eventHandler, object sender, CancellableEventArgs args)
        {
            if (eventHandler == null) return args.Cancel;
            if (PassThroughCancelable == false) return args.Cancel;
            eventHandler(sender, args);
            return args.Cancel;
        }

        public bool DispatchCancelable<TArgs>(EventHandler<TArgs> eventHandler, object sender, TArgs args) 
            where TArgs : CancellableEventArgs
        {
            if (eventHandler == null) return args.Cancel;
            if (PassThroughCancelable == false) return args.Cancel;
            eventHandler(sender, args);
            return args.Cancel;
        }

        public bool DispatchCancelable<TSender, TArgs>(TypedEventHandler<TSender, TArgs> eventHandler, TSender sender, TArgs args) 
            where TArgs : CancellableEventArgs
        {
            if (eventHandler == null) return args.Cancel;
            if (PassThroughCancelable == false) return args.Cancel;
            eventHandler(sender, args);
            return args.Cancel;
        }

        public void Dispatch(EventHandler eventHandler, object sender, EventArgs args)
        {
            if (eventHandler == null) return;
            if (PassThrough)
                eventHandler(sender, args);
            else
                _events.Add(new EventDefinition(eventHandler, sender, args));
        }

        public void Dispatch<TArgs>(EventHandler<TArgs> eventHandler, object sender, TArgs args)
        {
            if (eventHandler == null) return;
            if (PassThrough)
                eventHandler(sender, args);
            else
                _events.Add(new EventDefinition<TArgs>(eventHandler, sender, args));
        }

        public void Dispatch<TSender, TArgs>(TypedEventHandler<TSender, TArgs> eventHandler, TSender sender, TArgs args)
        {
            if (eventHandler == null) return;
            if (PassThrough)
                eventHandler(sender, args);
            else
                _events.Add(new EventDefinition<TSender, TArgs>(eventHandler, sender, args));
        }

        public void QueueEvent(EventHandler e, object sender, EventArgs args, string eventName = null)
        {
            if (e == null) return;
            if (PassThrough)
                e(sender, args);
            else
                _events.Add(new EventDefinition(e, sender, args, eventName));
        }

        public void QueueEvent<TEventArgs>(EventHandler<TEventArgs> e, object sender, TEventArgs args, string eventName = null)
        {
            if (e == null) return;
            if (PassThrough)
                e(sender, args);
            else
                _events.Add(new EventDefinition<TEventArgs>(e, sender, args, eventName));
        }

        public void QueueEvent<TSender, TEventArgs>(TypedEventHandler<TSender, TEventArgs> e, TSender sender, TEventArgs args, string eventName = null)
        {
            if (e == null) return;
            if (PassThrough)
                e(sender, args);
            else
                _events.Add(new EventDefinition<TSender, TEventArgs>(e, sender, args, eventName));
        }

        public IEnumerable<IEventDefinition> GetEvents()
        {
            return _events;
        }

        // fixme - this makes no sense at all
        // used by EventExtensions to determine whether the cancel event should trigger
        // because... these events are immediate or nothing, they cannot be queued
        public bool SupportsEventCancellation
        {
            get
            {
                return PassThrough;

                ////if there is no outer scope then this is the 'root' scope, in which case
                //// event cancelation is supported since events are not queued.
                //return _outerScope == null;
            }
        }

        /// <summary>
        /// When the outer scope is disposed, events will be raised if configured to do so
        /// </summary>
        protected override void DisposeResources()
        {
            // fixme
            // still, we need to de-duplicate events somehow, etc
            // lots to do!!

            if (RaiseEvents)
                foreach (var e in _events)
                    e.RaiseEvent();

            _events.Clear();
        }
    }
}