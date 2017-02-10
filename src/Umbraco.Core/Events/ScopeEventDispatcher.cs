using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Umbraco.Core.IO;

namespace Umbraco.Core.Events
{   

    /// <summary>
    /// This event manager is created for each scope and is aware of if it is nested in an outer scope
    /// </summary>
    /// <remarks>
    /// The outer scope is the only scope that can raise events, the inner scope's will defer to the outer scope
    /// </remarks>
    internal class ScopeEventDispatcher : IEventDispatcher
    {
        private readonly EventsDispatchMode _mode;
        private List<IEventDefinition> _events;

        public ScopeEventDispatcher(EventsDispatchMode mode)
        {
            _mode = mode;
        }

        private List<IEventDefinition> Events { get { return _events ?? (_events = new List<IEventDefinition>()); } }

        private bool PassThroughCancelable { get { return _mode == EventsDispatchMode.PassThrough || _mode == EventsDispatchMode.Scope; } }

        private bool PassThrough { get { return _mode == EventsDispatchMode.PassThrough; } }

        private bool RaiseEvents { get { return _mode == EventsDispatchMode.Scope; } }

        public bool DispatchCancelable(EventHandler eventHandler, object sender, CancellableEventArgs args, string eventName = null)
        {
            if (eventHandler == null) return args.Cancel;
            if (PassThroughCancelable == false) return args.Cancel;
            eventHandler(sender, args);
            return args.Cancel;
        }

        public bool DispatchCancelable<TArgs>(EventHandler<TArgs> eventHandler, object sender, TArgs args, string eventName = null) 
            where TArgs : CancellableEventArgs
        {
            if (eventHandler == null) return args.Cancel;
            if (PassThroughCancelable == false) return args.Cancel;
            eventHandler(sender, args);
            return args.Cancel;
        }

        public bool DispatchCancelable<TSender, TArgs>(TypedEventHandler<TSender, TArgs> eventHandler, TSender sender, TArgs args, string eventName = null) 
            where TArgs : CancellableEventArgs
        {
            if (eventHandler == null) return args.Cancel;
            if (PassThroughCancelable == false) return args.Cancel;
            eventHandler(sender, args);
            return args.Cancel;
        }

        public void Dispatch(EventHandler eventHandler, object sender, EventArgs args, string eventName = null)
        {
            if (eventHandler == null) return;
            if (PassThrough)
                eventHandler(sender, args);
            else
                Events.Add(new EventDefinition(eventHandler, sender, args, eventName));
        }

        public void Dispatch<TArgs>(EventHandler<TArgs> eventHandler, object sender, TArgs args, string eventName = null)
        {
            if (eventHandler == null) return;
            if (PassThrough)
                eventHandler(sender, args);
            else
                Events.Add(new EventDefinition<TArgs>(eventHandler, sender, args, eventName));
        }

        public void Dispatch<TSender, TArgs>(TypedEventHandler<TSender, TArgs> eventHandler, TSender sender, TArgs args, string eventName = null)
        {
            if (eventHandler == null) return;
            if (PassThrough)
                eventHandler(sender, args);
            else
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
                    var l2 = new OrderedHashSet<IEventDefinition>(keepOldest:false);
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
            // fixme - we'd need to de-duplicate events somehow, etc - and the deduplication should be last in wins

            if (_events == null) return;

            var mediaFileSystem = FileSystemProviderManager.Current.MediaFileSystem;

            if (RaiseEvents && completed)
            {
                foreach (var e in _events)
                {
                    e.RaiseEvent();

                    // fixme - not sure I like doing it here - but then where? how?
                    var delete = e.Args as IDeletingMediaFilesEventArgs;
                    if (delete != null && delete.MediaFilesToDelete.Count > 0)
                        mediaFileSystem.DeleteMediaFiles(delete.MediaFilesToDelete);
                }
            }

            _events.Clear();
        }
    }
}