using System.Reflection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Events;

public abstract class EventDefinitionBase : IEventDefinition, IEquatable<EventDefinitionBase>
{
    protected EventDefinitionBase(object? sender, object? args, string? eventName = null)
    {
        Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        Args = args ?? throw new ArgumentNullException(nameof(args));
        EventName = eventName;

        if (EventName.IsNullOrWhiteSpace())
        {
            // don't match "Ing" suffixed names
            Attempt<EventNameExtractorResult?> findResult =
                EventNameExtractor.FindEvent(sender, args, EventNameExtractor.MatchIngNames);

            if (findResult.Success == false)
            {
                throw new AmbiguousMatchException(
                    "Could not automatically find the event name, the event name will need to be explicitly registered for this event definition. "
                    + $"Sender: {sender.GetType()} Args: {args.GetType()}"
                    + " Error: " + findResult.Result?.Error);
            }

            EventName = findResult.Result?.Name;
        }
    }

    public object Sender { get; }

    public bool Equals(EventDefinitionBase? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Args.Equals(other.Args) && string.Equals(EventName, other.EventName) && Sender.Equals(other.Sender);
    }

    public object Args { get; }

    public string? EventName { get; }

    public static bool operator ==(EventDefinitionBase left, EventDefinitionBase right) => Equals(left, right);

    public abstract void RaiseEvent();

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((EventDefinitionBase)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Args.GetHashCode();
            if (EventName is not null)
            {
                hashCode = (hashCode * 397) ^ EventName.GetHashCode();
            }

            hashCode = (hashCode * 397) ^ Sender.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator !=(EventDefinitionBase left, EventDefinitionBase right) => Equals(left, right) == false;
}
