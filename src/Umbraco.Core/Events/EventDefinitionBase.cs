using System;

namespace Umbraco.Core.Events
{
    public abstract class EventDefinitionBase : IEventDefinition, IEquatable<EventDefinitionBase>
    {
        protected EventDefinitionBase()
        {
            EventId = Guid.NewGuid();
        }
        public Guid EventId { get; private set; }
        public abstract void RaiseEvent();

        public bool Equals(EventDefinitionBase other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EventId.Equals(other.EventId);
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
            return EventId.GetHashCode();
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