using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;

namespace Umbraco.Core.Events
{
    [HostProtection(SecurityAction.LinkDemand, SharedState = true)]
    public class CancellableEnumerableObjectEventArgs<T> : CancellableObjectEventArgs<IEnumerable<T>>, IEquatable<CancellableEnumerableObjectEventArgs<T>>
    {
        public CancellableEnumerableObjectEventArgs(IEnumerable<T> eventObject, bool canCancel, EventMessages messages, IDictionary<string, object> additionalData)
            : base(eventObject, canCancel, messages, additionalData)
        { }

        public CancellableEnumerableObjectEventArgs(IEnumerable<T> eventObject, bool canCancel, EventMessages eventMessages)
            : base(eventObject, canCancel, eventMessages)
        { }

        public CancellableEnumerableObjectEventArgs(IEnumerable<T> eventObject, EventMessages eventMessages)
            : base(eventObject, eventMessages)
        { }

        public CancellableEnumerableObjectEventArgs(IEnumerable<T> eventObject, bool canCancel)
            : base(eventObject, canCancel)
        { }

        public CancellableEnumerableObjectEventArgs(IEnumerable<T> eventObject)
            : base(eventObject)
        { }

        public bool Equals(CancellableEnumerableObjectEventArgs<T> other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return EventObject.SequenceEqual(other.EventObject);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CancellableEnumerableObjectEventArgs<T>)obj);
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.GetHashCode(EventObject);
        }
    }
}