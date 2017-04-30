using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// An IEventDispatcher that queues events.
    /// </summary>
    /// <remarks>
    /// <para>Can raise, or ignore, cancelable events, depending on option.</para>
    /// <para>Implementations must override ScopeExitCompleted to define what
    /// to do with the events when the scope exits and has been completed.</para>
    /// <para>If the scope exits without being completed, events are ignored.</para>
    /// </remarks>
    public abstract class ScopeEventDispatcherBase : IEventDispatcher
    {
        //events will be enlisted in the order they are raised
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
                    return FilterSupersededAndUpdateToLatestEntity(_events);
                case EventDefinitionFilter.FirstIn:
                    var l1 = new OrderedHashSet<IEventDefinition>();
                    foreach (var e in _events)
                    {
                        l1.Add(e);
                    }
                    return FilterSupersededAndUpdateToLatestEntity(l1);
                case EventDefinitionFilter.LastIn:
                    var l2 = new OrderedHashSet<IEventDefinition>(keepOldest: false);
                    foreach (var e in _events)
                    {
                        l2.Add(e);
                    }
                    return FilterSupersededAndUpdateToLatestEntity(l2);
                default:
                    throw new ArgumentOutOfRangeException("filter", filter, null);
            }
        }
        
        private class EventDefinitionTypeData
        {
            public IEventDefinition EventDefinition { get; set; }
            public Type EventArgType { get; set; }
            public SupersedeEventAttribute[] SupersedeAttributes { get; set; }
        }
        
        /// <summary>
        /// This will iterate over the events (latest first) and filter out any events or entities in event args that are included 
        /// in more recent events that Supersede previous ones. For example, If an Entity has been Saved and then Deleted, we don't want
        /// to raise the Saved event (well actually we just don't want to include it in the args for that saved event)
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        private static IEnumerable<IEventDefinition> FilterSupersededAndUpdateToLatestEntity(IReadOnlyList<IEventDefinition> events)
        {
            //used to keep the 'latest' entity and associated event definition data
            var allEntities = new List<Tuple<IEntity, EventDefinitionTypeData>>();

            //tracks all CancellableObjectEventArgs instances in the events which is the only type of args we can work with
            var cancelableArgs = new List<CancellableObjectEventArgs>();

            var result = new List<IEventDefinition>();

            //This will eagerly load all of the event arg types and their attributes so we don't have to continuously look this data up
            var allArgTypesWithAttributes = events.Select(x => x.Args.GetType())
                .Distinct()
                .ToDictionary(x => x, x => x.GetCustomAttributes<SupersedeEventAttribute>(false).ToArray());
               
            //Iterate all events and collect the actual entities in them and relates them to their corresponding EventDefinitionTypeData
            //we'll process the list in reverse because events are added in the order they are raised and we want to filter out
            //any entities from event args that are not longer relevant 
            //(i.e. if an item is Deleted after it's Saved, we won't include the item in the Saved args)
            for (var index = events.Count - 1; index >= 0; index--)
            {
                var eventDefinition = events[index];

                var argType = eventDefinition.Args.GetType();
                var attributes = allArgTypesWithAttributes[eventDefinition.Args.GetType()];

                var meta = new EventDefinitionTypeData
                {
                    EventDefinition = eventDefinition,
                    EventArgType = argType,
                    SupersedeAttributes = attributes
                };

                var args = eventDefinition.Args as CancellableObjectEventArgs;
                if (args != null)
                {
                    var list = TypeHelper.CreateGenericEnumerableFromObject(args.EventObject);

                    if (list == null)
                    {
                        //extract the event object
                        var obj = args.EventObject as IEntity;
                        if (obj != null)
                        {
                            //Now check if this entity already exists in other event args that supersede this current event arg type
                            if (IsFiltered(obj, meta, allEntities) == false)
                            {
                                //if it's not filtered we can adde these args to the response
                                cancelableArgs.Add(args);
                                result.Add(eventDefinition);
                                //track the entity
                                allEntities.Add(Tuple.Create(obj, meta));
                            }
                        }
                        else
                        {
                            //Can't retrieve the entity so cant' filter or inspect, just add to the output
                            result.Add(eventDefinition);                            
                        }
                    }
                    else
                    {
                        var toRemove = new List<IEntity>();
                        foreach (var entity in list)
                        {
                            //extract the event object
                            var obj = entity as IEntity;
                            if (obj != null)
                            {
                                //Now check if this entity already exists in other event args that supersede this current event arg type
                                if (IsFiltered(obj, meta, allEntities))
                                {
                                    //track it to be removed
                                    toRemove.Add(obj);
                                }
                                else
                                {
                                    //track the entity, it's not filtered
                                    allEntities.Add(Tuple.Create(obj, meta));
                                }
                            }
                            else
                            {
                                //we don't need to do anything here, we can't cast to IEntity so we cannot filter, so it will just remain in the list
                            }
                        }

                        //remove anything that has been filtered
                        foreach (var entity in toRemove)
                        {
                            list.Remove(entity);
                        }

                        //track the event and include in the response if there's still entities remaining in the list
                        if (list.Count > 0)
                        {
                            if (toRemove.Count > 0)
                            {
                                //re-assign if the items have changed
                                args.EventObject = list;
                            }
                            cancelableArgs.Add(args);
                            result.Add(eventDefinition);
                        }
                    }
                }
                else
                {
                    //it's not a cancelable event arg so we just include it in the result
                    result.Add(eventDefinition);
                }
            }

            //Now we'll deal with ensuring that only the latest(non stale) entities are used throughout all event args
            UpdateToLatestEntities(allEntities, cancelableArgs);

            //we need to reverse the result since we've been adding by latest added events first!
            result.Reverse();

            return result;
        }

        private static void UpdateToLatestEntities(IEnumerable<Tuple<IEntity, EventDefinitionTypeData>> allEntities, IEnumerable<CancellableObjectEventArgs> cancelableArgs)
        {
            //Now we'll deal with ensuring that only the latest(non stale) entities are used throughout all event args

            var latestEntities = new OrderedHashSet<IEntity>(keepOldest: true);
            foreach (var entity in allEntities.OrderByDescending(entity => entity.Item1.UpdateDate))
            {
                latestEntities.Add(entity.Item1);
            }

            foreach (var args in cancelableArgs)
            {
                var list = TypeHelper.CreateGenericEnumerableFromObject(args.EventObject);
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

        /// <summary>
        /// This will check against all of the processed entity/events (allEntities) to see if this entity already exists in 
        /// event args that supersede the event args being passed in and if so returns true.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="eventDef"></param>
        /// <param name="allEntities"></param>
        /// <returns></returns>
        private static bool IsFiltered(
            IEntity entity,
            EventDefinitionTypeData eventDef,
            List<Tuple<IEntity, EventDefinitionTypeData>> allEntities)
        {
            var argType = eventDef.EventDefinition.Args.GetType();

            //check if the entity is found in any processed event data that could possible supersede this one
            var foundByEntity = allEntities
                .Where(x => x.Item2.SupersedeAttributes.Length > 0
                            //if it's the same arg type than it cannot supersede                
                            && x.Item2.EventArgType != argType
                            && Equals(x.Item1, entity))
                .ToArray();

            //no args have been processed with this entity so it should not be filtered
            if (foundByEntity.Length == 0)
                return false;

            if (argType.IsGenericType)
            {
                var supercededBy = foundByEntity
                    .FirstOrDefault(x =>
                        x.Item2.SupersedeAttributes.Any(y =>
                            //if the attribute type is a generic type def then compare with the generic type def of the event arg
                                (y.SupersededEventArgsType.IsGenericTypeDefinition && y.SupersededEventArgsType == argType.GetGenericTypeDefinition())
                                //if the attribute type is not a generic type def then compare with the normal type of the event arg
                                || (y.SupersededEventArgsType.IsGenericTypeDefinition == false && y.SupersededEventArgsType == argType)));
                return supercededBy != null;
            }
            else
            {
                var supercededBy = foundByEntity
                    .FirstOrDefault(x =>
                        x.Item2.SupersedeAttributes.Any(y =>
                            //since the event arg type is not a generic type, then we just compare type 1:1
                                y.SupersededEventArgsType == argType));
                return supercededBy != null;
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