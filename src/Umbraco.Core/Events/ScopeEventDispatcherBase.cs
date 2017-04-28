using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.EntityBase;

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
                    UpdateToLatestEntity(_events);
                    return _events;
                case EventDefinitionFilter.FirstIn:
                    var l1 = new OrderedHashSet<IEventDefinition>();
                    foreach (var e in _events)
                    {
                        l1.Add(e);
                    }
                    UpdateToLatestEntity(l1);
                    return l1;
                case EventDefinitionFilter.LastIn:
                    var l2 = new OrderedHashSet<IEventDefinition>(keepOldest: false);
                    foreach (var e in _events)
                    {
                        l2.Add(e);
                    }
                    UpdateToLatestEntity(l2);
                    return l2;
                default:
                    throw new ArgumentOutOfRangeException("filter", filter, null);
            }
        }

        private void UpdateToLatestEntity(IEnumerable<IEventDefinition> events)
        {
            //used to keep the 'latest' entity
            var allEntities = new List<IEntity>();
            var cancelableArgs = new List<CancellableObjectEventArgs>();
            
            foreach (var eventDefinition in events)
            {
                var args = eventDefinition.Args as CancellableObjectEventArgs;
                if (args != null)
                {
                    cancelableArgs.Add(args);

                    var list = TypeHelper.CreateGenericEnumerableFromOjbect(args.EventObject);
                    
                    if (list == null)
                    {
                        //extract the event object
                        var obj = args.EventObject as IEntity;
                        if (obj != null)
                        {
                            allEntities.Add(obj);
                        }
                    }
                    else
                    {
                        foreach (var entity in list)
                        {
                            //extract the event object
                            var obj = entity as IEntity;
                            if (obj != null)
                            {
                                allEntities.Add(obj);
                            }
                        }
                    }
                }
            }

            var latestEntities = new OrderedHashSet<IEntity>(keepOldest: true);
            foreach (var entity in allEntities.OrderByDescending(entity => entity.UpdateDate))
            {
                latestEntities.Add(entity);
            }

            foreach (var args in cancelableArgs)
            {
                var list = TypeHelper.CreateGenericEnumerableFromOjbect(args.EventObject);
                if (list == null)
                {
                    //try to find the args entity in the latest entity - based on the equality operators, this will 
                    //match by Id since that is the default equality checker for IEntity. If one is found, than it is 
                    //the most recent entity instance so update the args with that instance so we don't emit a stale instance.
                    var foundEntity = latestEntities.FirstOrDefault(x => Equals(x, args.EventObject));
                    if (foundEntity != null)
                    {
                        args.EventObject = foundEntity;
                    }
                }
                else
                {
                    var updated = false;
                    
                    for (int i = 0; i < list.Count; i++)
                    {
                        //try to find the args entity in the latest entity - based on the equality operators, this will 
                        //match by Id since that is the default equality checker for IEntity. If one is found, than it is 
                        //the most recent entity instance so update the args with that instance so we don't emit a stale instance.
                        var foundEntity = latestEntities.FirstOrDefault(x => Equals(x, list[i]));
                        if (foundEntity != null)
                        {
                            list[i] = foundEntity;
                            updated = true;
                        }
                    }

                    if (updated)
                    {
                        args.EventObject = list;
                    }
                }
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