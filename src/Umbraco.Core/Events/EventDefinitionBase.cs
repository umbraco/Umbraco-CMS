using System;
using System.Reflection;

namespace Umbraco.Core.Events
{
    public abstract class EventDefinitionBase : IEventDefinition, IEquatable<EventDefinitionBase>
    {
        protected EventDefinitionBase(object sender, object args, string eventName = null)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Args = args ?? throw new ArgumentNullException(nameof(args));
            EventName = eventName;

            if (EventName.IsNullOrWhiteSpace())
            {
                // don't match "Ing" suffixed names
                var findResult = EventNameExtractor.FindEvent(sender, args, exclude:EventNameExtractor.MatchIngNames);

                if (findResult.Success == false)
                    throw new AmbiguousMatchException("Could not automatically find the event name, the event name will need to be explicitly registered for this event definition. "
                        + $"Sender: {sender.GetType()} Args: {args.GetType()}"
                        + " Error: " + findResult.Result.Error);
                EventName = findResult.Result.Name;
            }
        }

        public object Sender { get; }
        public object Args { get; }
        public string EventName { get; }

        public abstract void RaiseEvent();

        public bool Equals(EventDefinitionBase other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Args.Equals(other.Args) && string.Equals(EventName, other.EventName) && Sender.Equals(other.Sender);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EventDefinitionBase) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Args.GetHashCode();
                hashCode = (hashCode * 397) ^ EventName.GetHashCode();
                hashCode = (hashCode * 397) ^ Sender.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(EventDefinitionBase left, EventDefinitionBase right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EventDefinitionBase left, EventDefinitionBase right)
        {
            return Equals(left, right) == false;
        }
    }
}
