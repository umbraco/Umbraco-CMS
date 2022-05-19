using System.Collections;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Events;

/// <summary>
///     An IEventDispatcher that queues events.
/// </summary>
/// <remarks>
///     <para>Can raise, or ignore, cancelable events, depending on option.</para>
///     <para>
///         Implementations must override ScopeExitCompleted to define what
///         to do with the events when the scope exits and has been completed.
///     </para>
///     <para>If the scope exits without being completed, events are ignored.</para>
/// </remarks>
public abstract class QueuingEventDispatcherBase : IEventDispatcher
{
    private readonly bool _raiseCancelable;

    // events will be enlisted in the order they are raised
    private List<IEventDefinition>? _events;

    protected QueuingEventDispatcherBase(bool raiseCancelable) => _raiseCancelable = raiseCancelable;

    private List<IEventDefinition> Events => _events ??= new List<IEventDefinition>();

    public bool DispatchCancelable(EventHandler eventHandler, object sender, CancellableEventArgs args, string? eventName = null)
    {
        if (eventHandler == null)
        {
            return args.Cancel;
        }

        if (_raiseCancelable == false)
        {
            return args.Cancel;
        }

        eventHandler(sender, args);
        return args.Cancel;
    }

    public bool DispatchCancelable<TArgs>(EventHandler<TArgs> eventHandler, object sender, TArgs args, string? eventName = null)
        where TArgs : CancellableEventArgs
    {
        if (eventHandler == null)
        {
            return args.Cancel;
        }

        if (_raiseCancelable == false)
        {
            return args.Cancel;
        }

        eventHandler(sender, args);
        return args.Cancel;
    }

    public bool DispatchCancelable<TSender, TArgs>(TypedEventHandler<TSender, TArgs> eventHandler, TSender sender, TArgs args, string? eventName = null)
        where TArgs : CancellableEventArgs
    {
        if (eventHandler == null)
        {
            return args.Cancel;
        }

        if (_raiseCancelable == false)
        {
            return args.Cancel;
        }

        eventHandler(sender, args);
        return args.Cancel;
    }

    public void Dispatch(EventHandler eventHandler, object sender, EventArgs args, string? eventName = null)
    {
        if (eventHandler == null)
        {
            return;
        }

        Events.Add(new EventDefinition(eventHandler, sender, args, eventName));
    }

    public void Dispatch<TArgs>(EventHandler<TArgs> eventHandler, object sender, TArgs args, string? eventName = null)
    {
        if (eventHandler == null)
        {
            return;
        }

        Events.Add(new EventDefinition<TArgs>(eventHandler, sender, args, eventName));
    }

    public void Dispatch<TSender, TArgs>(TypedEventHandler<TSender, TArgs> eventHandler, TSender sender, TArgs args, string? eventName = null)
    {
        if (eventHandler == null)
        {
            return;
        }

        Events.Add(new EventDefinition<TSender, TArgs>(eventHandler, sender, args, eventName));
    }

    public IEnumerable<IEventDefinition> GetEvents(EventDefinitionFilter filter)
    {
        if (_events == null)
        {
            return Enumerable.Empty<IEventDefinition>();
        }

        IReadOnlyList<IEventDefinition> events;
        switch (filter)
        {
            case EventDefinitionFilter.All:
                events = _events;
                break;
            case EventDefinitionFilter.FirstIn:
                var l1 = new OrderedHashSet<IEventDefinition>();
                foreach (IEventDefinition e in _events)
                {
                    l1.Add(e);
                }

                events = l1;
                break;
            case EventDefinitionFilter.LastIn:
                var l2 = new OrderedHashSet<IEventDefinition>(false);
                foreach (IEventDefinition e in _events)
                {
                    l2.Add(e);
                }

                events = l2;
                break;
            default:
                throw new ArgumentOutOfRangeException("filter", filter, null);
        }

        return FilterSupersededAndUpdateToLatestEntity(events);
    }

    public void ScopeExit(bool completed)
    {
        if (_events == null)
        {
            return;
        }

        if (completed)
        {
            ScopeExitCompleted();
        }

        _events.Clear();
    }

    // this is way too convoluted, the supersede attribute is used only on DeleteEventargs to specify
    // that it supersedes save, publish, move and copy - BUT - publish event args is also used for
    // unpublishing and should NOT be superseded - so really it should not be managed at event args
    // level but at event level
    //
    // what we want is:
    // if an entity is deleted, then all Saved, Moved, Copied, Published events prior to this should
    // not trigger for the entity - and even though, does it make any sense? making a copy of an entity
    // should ... trigger?
    //
    // not going to refactor it all - we probably want to *always* trigger event but tell people that
    // due to scopes, they should not expected eg a saved entity to still be around - however, now,
    // going to write a ugly condition to deal with U4-10764

    // iterates over the events (latest first) and filter out any events or entities in event args that are included
    // in more recent events that Supersede previous ones. For example, If an Entity has been Saved and then Deleted, we don't want
    // to raise the Saved event (well actually we just don't want to include it in the args for that saved event)
    internal static IEnumerable<IEventDefinition> FilterSupersededAndUpdateToLatestEntity(
        IReadOnlyList<IEventDefinition> events)
    {
        // keeps the 'latest' entity and associated event data
        var entities = new List<Tuple<IEntity, EventDefinitionInfos>>();

        // collects the event definitions
        // collects the arguments in result, that require their entities to be updated
        var result = new List<IEventDefinition>();
        var resultArgs = new List<CancellableObjectEventArgs>();

        // eagerly fetch superseded arg types for each arg type
        var argTypeSuperceeding = events.Select(x => x.Args.GetType())
            .Distinct()
            .ToDictionary(
                x => x,
                x => x.GetCustomAttributes<SupersedeEventAttribute>(false).Select(y => y.SupersededEventArgsType)
                    .ToArray());

        // iterate over all events and filter
        //
        // process the list in reverse, because events are added in the order they are raised and we want to keep
        // the latest (most recent) entities and filter out what is not relevant anymore (too old), eg if an entity
        // is Deleted after being Saved, we want to filter out the Saved event
        for (var index = events.Count - 1; index >= 0; index--)
        {
            IEventDefinition def = events[index];

            var infos = new EventDefinitionInfos
            {
                EventDefinition = def,
                SupersedeTypes = argTypeSuperceeding[def.Args.GetType()],
            };

            var args = def.Args as CancellableObjectEventArgs;
            if (args == null)
            {
                // not a cancellable event arg, include event definition in result
                result.Add(def);
            }
            else
            {
                // event object can either be a single object or an enumerable of objects
                // try to get as an enumerable, get null if it's not
                IList? eventObjects = TypeHelper.CreateGenericEnumerableFromObject(args.EventObject);
                if (eventObjects == null)
                {
                    // single object, cast as an IEntity
                    // if cannot cast, cannot filter, nothing - just include event definition in result
                    if (args.EventObject is not IEntity eventEntity)
                    {
                        result.Add(def);
                        continue;
                    }

                    // look for this entity in superseding event args
                    // found = must be removed (ie not added), else track
                    if (IsSuperceeded(eventEntity, infos, entities) == false)
                    {
                        // track
                        entities.Add(Tuple.Create(eventEntity, infos));

                        // track result arguments
                        // include event definition in result
                        resultArgs.Add(args);
                        result.Add(def);
                    }
                }
                else
                {
                    // enumerable of objects
                    var toRemove = new List<IEntity>();
                    foreach (var eventObject in eventObjects)
                    {
                        // extract the event object, cast as an IEntity
                        // if cannot cast, cannot filter, nothing to do - just leave it in the list & continue
                        if (eventObject is not IEntity eventEntity)
                        {
                            continue;
                        }

                        // look for this entity in superseding event args
                        // found = must be removed, else track
                        if (IsSuperceeded(eventEntity, infos, entities))
                        {
                            toRemove.Add(eventEntity);
                        }
                        else
                        {
                            entities.Add(Tuple.Create(eventEntity, infos));
                        }
                    }

                    // remove superseded entities
                    foreach (IEntity entity in toRemove)
                    {
                        eventObjects.Remove(entity);
                    }

                    // if there are still entities in the list, keep the event definition
                    if (eventObjects.Count > 0)
                    {
                        if (toRemove.Count > 0)
                        {
                            // re-assign if changed
                            args.EventObject = eventObjects;
                        }

                        // track result arguments
                        // include event definition in result
                        resultArgs.Add(args);
                        result.Add(def);
                    }
                }
            }
        }

        // go over all args in result, and update them with the latest instanceof each entity
        UpdateToLatestEntities(entities, resultArgs);

        // reverse, since we processed the list in reverse
        result.Reverse();

        return result;
    }

    protected abstract void ScopeExitCompleted();

    // edits event args to use the latest instance of each entity
    private static void UpdateToLatestEntities(
        IEnumerable<Tuple<IEntity, EventDefinitionInfos>> entities,
        IEnumerable<CancellableObjectEventArgs> args)
    {
        // get the latest entities
        // ordered hash set + keepOldest will keep the latest inserted entity (in case of duplicates)
        var latestEntities = new OrderedHashSet<IEntity>(true);
        foreach (Tuple<IEntity, EventDefinitionInfos> entity in entities.OrderByDescending(entity =>
                     entity.Item1.UpdateDate))
        {
            latestEntities.Add(entity.Item1);
        }

        foreach (CancellableObjectEventArgs arg in args)
        {
            // event object can either be a single object or an enumerable of objects
            // try to get as an enumerable, get null if it's not
            IList? eventObjects = TypeHelper.CreateGenericEnumerableFromObject(arg.EventObject);
            if (eventObjects == null)
            {
                // single object
                // look for a more recent entity for that object, and replace if any
                // works by "equalling" entities ie the more recent one "equals" this one (though different object)
                IEntity? foundEntity = latestEntities.FirstOrDefault(x => Equals(x, arg.EventObject));
                if (foundEntity != null)
                {
                    arg.EventObject = foundEntity;
                }
            }
            else
            {
                // enumerable of objects
                // same as above but for each object
                var updated = false;
                for (var i = 0; i < eventObjects.Count; i++)
                {
                    IEntity? foundEntity = latestEntities.FirstOrDefault(x => Equals(x, eventObjects[i]));
                    if (foundEntity == null)
                    {
                        continue;
                    }

                    eventObjects[i] = foundEntity;
                    updated = true;
                }

                if (updated)
                {
                    arg.EventObject = eventObjects;
                }
            }
        }
    }

    // determines if a given entity, appearing in a given event definition, should be filtered out,
    // considering the entities that have already been visited - an entity is filtered out if it
    // appears in another even definition, which supersedes this event definition.
    private static bool IsSuperceeded(IEntity entity, EventDefinitionInfos infos, List<Tuple<IEntity, EventDefinitionInfos>> entities)
    {
        // var argType = meta.EventArgsType;
        Type? argType = infos.EventDefinition?.Args.GetType();

        // look for other instances of the same entity, coming from an event args that supersedes other event args,
        // ie is marked with the attribute, and is not this event args (cannot supersede itself)
        Tuple<IEntity, EventDefinitionInfos>[] superceeding = entities
            .Where(x => x.Item2.SupersedeTypes?.Length > 0 // has the attribute
                        && x.Item2.EventDefinition?.Args.GetType() != argType // is not the same
                        && Equals(x.Item1, entity)) // same entity
            .ToArray();

        // first time we see this entity = not filtered
        if (superceeding.Length == 0)
        {
            return false;
        }

        // delete event args does NOT supersedes 'unpublished' event
        if ((argType?.IsGenericType ?? false) && argType.GetGenericTypeDefinition() == typeof(PublishEventArgs<>) &&
            infos.EventDefinition?.EventName == "Unpublished")
        {
            return false;
        }

        // found occurrences, need to determine if this event args is superseded
        if (argType?.IsGenericType ?? false)
        {
            // generic, must compare type arguments
            Tuple<IEntity, EventDefinitionInfos>? supercededBy = superceeding.FirstOrDefault(x =>
                x.Item2.SupersedeTypes?.Any(y =>

                    // superseding a generic type which has the same generic type definition
                    // (but ... no matter the generic type parameters? could be different?)
                    (y.IsGenericTypeDefinition && y == argType.GetGenericTypeDefinition())

                    // or superceeding a non-generic type which is ... (but... how is this ever possible? argType *is* generic?
                    || (y.IsGenericTypeDefinition == false && y == argType)) ?? false);
            return supercededBy != null;
        }
        else
        {
            // non-generic, can compare types 1:1
            Tuple<IEntity, EventDefinitionInfos>? supercededBy = superceeding.FirstOrDefault(x =>
                x.Item2.SupersedeTypes?.Any(y => y == argType) ?? false);
            return supercededBy != null;
        }
    }

    private class EventDefinitionInfos
    {
        public IEventDefinition? EventDefinition { get; set; }

        public Type[]? SupersedeTypes { get; set; }
    }
}
