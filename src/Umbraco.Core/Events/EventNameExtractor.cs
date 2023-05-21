using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Umbraco.Cms.Core.Events;

/// <summary>
///     There is actually no way to discover an event name in c# at the time of raising the event. It is possible
///     to get the event name from the handler that is being executed based on the event being raised, however that is not
///     what we want in this case. We need to find the event name before it is being raised - you would think that it's
///     possible
///     with reflection or anything but that is not the case, the delegate that defines an event has no info attached to
///     it, it
///     is literally just an event.
///     So what this does is take the sender and event args objects, looks up all public/static events on the sender that
///     have
///     a generic event handler with generic arguments (but only) one, then we match the type of event arguments with the
///     ones
///     being passed in. As it turns out, in our services this will work for the majority of our events! In some cases it
///     may not
///     work and we'll have to supply a string but hopefully this saves a bit of magic strings.
///     We can also write tests to validate these are all working correctly for all services.
/// </summary>
public class EventNameExtractor
{
    /// <summary>
    ///     Used to cache all candidate events for a given type so we don't re-look them up
    /// </summary>
    private static readonly ConcurrentDictionary<Type, EventInfoArgs[]> CandidateEvents = new();

    /// <summary>
    ///     Used to cache all matched event names by (sender type + arg type) so we don't re-look them up
    /// </summary>
    private static readonly ConcurrentDictionary<Tuple<Type, Type>, string[]> MatchedEventNames = new();

    /// <summary>
    ///     Finds the event name on the sender that matches the args type
    /// </summary>
    /// <param name="senderType"></param>
    /// <param name="argsType"></param>
    /// <param name="exclude">
    ///     A filter to exclude matched event names, this filter should return true to exclude the event name from being
    ///     matched
    /// </param>
    /// <returns>
    ///     null if not found or an ambiguous match
    /// </returns>
    public static Attempt<EventNameExtractorResult?> FindEvent(Type senderType, Type argsType, Func<string, bool> exclude)
    {
        var events = FindEvents(senderType, argsType, exclude);

        switch (events.Length)
        {
            case 0:
                return Attempt.Fail(new EventNameExtractorResult(EventNameExtractorError.NoneFound));

            case 1:
                return Attempt.Succeed(new EventNameExtractorResult(events[0]));

            default:
                // there's more than one left so it's ambiguous!
                return Attempt.Fail(new EventNameExtractorResult(EventNameExtractorError.Ambiguous));
        }
    }

    public static string[] FindEvents(Type senderType, Type argsType, Func<string, bool> exclude)
    {
        var found = MatchedEventNames.GetOrAdd(new Tuple<Type, Type>(senderType, argsType), tuple =>
        {
            EventInfoArgs[] events = CandidateEvents.GetOrAdd(senderType, t =>
            {
                return t.GetEvents(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.FlattenHierarchy)

                    // we can only look for events handlers with generic types because that is the only
                    // way that we can try to find a matching event based on the arg type passed in
                    .Where(x => x.EventHandlerType?.IsGenericType ?? false)
                    .Select(x => new EventInfoArgs(x, x.EventHandlerType!.GetGenericArguments()))

                    // we are only looking for event handlers that have more than one generic argument
                    .Where(x =>
                    {
                        if (x.GenericArgs.Length == 1)
                        {
                            return true;
                        }

                        // special case for our own TypedEventHandler
                        if (x.EventInfo.EventHandlerType?.GetGenericTypeDefinition() == typeof(TypedEventHandler<,>) &&
                            x.GenericArgs.Length == 2)
                        {
                            return true;
                        }

                        return false;
                    })
                    .ToArray();
            });

            return events.Where(x =>
            {
                if (x.GenericArgs.Length == 1 && x.GenericArgs[0] == tuple.Item2)
                {
                    return true;
                }

                // special case for our own TypedEventHandler
                if (x.EventInfo.EventHandlerType?.GetGenericTypeDefinition() == typeof(TypedEventHandler<,>)
                    && x.GenericArgs.Length == 2
                    && x.GenericArgs[1] == tuple.Item2)
                {
                    return true;
                }

                return false;
            }).Select(x => x.EventInfo.Name).ToArray();
        });

        return found.Where(x => exclude(x) == false).ToArray();
    }

    /// <summary>
    ///     Finds the event name on the sender that matches the args type
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    /// <param name="exclude">
    ///     A filter to exclude matched event names, this filter should return true to exclude the event name from being
    ///     matched
    /// </param>
    /// <returns>
    ///     null if not found or an ambiguous match
    /// </returns>
    public static Attempt<EventNameExtractorResult?>
        FindEvent(object sender, object args, Func<string, bool> exclude) =>
        FindEvent(sender.GetType(), args.GetType(), exclude);

    /// <summary>
    ///     Return true if the event is named with an ING name such as "Saving" or "RollingBack"
    /// </summary>
    /// <param name="eventName"></param>
    /// <returns></returns>
    public static bool MatchIngNames(string eventName)
    {
        var splitter = new Regex(@"(?<!^)(?=[A-Z])");
        var words = splitter.Split(eventName);
        if (words.Length == 0)
        {
            return false;
        }

        return words[0].EndsWith("ing");
    }

    /// <summary>
    ///     Return true if the event is not named with an ING name such as "Saving" or "RollingBack"
    /// </summary>
    /// <param name="eventName"></param>
    /// <returns></returns>
    public static bool MatchNonIngNames(string eventName)
    {
        var splitter = new Regex(@"(?<!^)(?=[A-Z])");
        var words = splitter.Split(eventName);
        if (words.Length == 0)
        {
            return false;
        }

        return words[0].EndsWith("ing") == false;
    }

    private class EventInfoArgs
    {
        public EventInfoArgs(EventInfo eventInfo, Type[] genericArgs)
        {
            EventInfo = eventInfo;
            GenericArgs = genericArgs;
        }

        public EventInfo EventInfo { get; }

        public Type[] GenericArgs { get; }
    }
}
